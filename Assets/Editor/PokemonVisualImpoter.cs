// 파일: Assets/Editor/PokemonVisualImporter.cs
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace PokeClicker.EditorTools
{
    /// <summary>
    /// 지정된 폴더의 이미지 에셋을 기반으로 PokemonVisualSO를 생성하고 FormSO에 연결하는 에디터 툴.
    /// Resources 폴더에 대한 의존성을 제거하고 AssetDatabase API만 사용하도록 수정했습니다.
    /// </summary>
    public class PokemonVisualImporter : EditorWindow
    {
        [Header("Asset Paths")]
        public string rootAssetPath = "Assets/PokeArt/Graphics/Pokemon";
        public string saveFolder = "Assets/PokemonVisualData";

        private SpeciesDB speciesDB;

        [MenuItem("Tools/Pokemon Visual Importer")]
        public static void ShowWindow()
        {
            GetWindow<PokemonVisualImporter>("Pokemon Visual Importer");
        }

        private Vector2 _scroll;

        void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.LabelField("Pokemon Visual Importer", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            rootAssetPath = EditorGUILayout.TextField("Root Asset Path", rootAssetPath);
            saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);

            EditorGUILayout.Space();

            speciesDB = (SpeciesDB)EditorGUILayout.ObjectField("Species DB", speciesDB, typeof(SpeciesDB), true);

            if (GUILayout.Button("Import Visual Assets"))
            {
                if (speciesDB == null)
                {
                    EditorUtility.DisplayDialog("Error", "Species DB를 할당해주세요.", "OK");
                    return;
                }

                if (!Directory.Exists(rootAssetPath))
                {
                    EditorUtility.DisplayDialog("Error", $"{rootAssetPath} 폴더가 존재하지 않습니다.", "OK");
                    return;
                }

                ImportAllVisuals();
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 모든 SpeciesSO를 순회하며 VisualSO를 생성/업데이트하고 FormSO에 연결합니다.
        /// </summary>
        private void ImportAllVisuals()
        {
            if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);

            foreach (var species in speciesDB.allSpecies)
            {
                if (species == null) continue;

                foreach (var form in species.Forms)
                {
                    if (form == null) continue;

                    var visual = CreateOrUpdateVisualSO(species, form);
                    if (visual != null)
                    {
                        form.visual = visual;
                        EditorUtility.SetDirty(form);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Visual Importer] VisualSO import finished.");
        }

        /// <summary>
        /// VisualSO를 생성하거나 기존 것을 찾아 업데이트하고, 에셋을 할당합니다.
        /// </summary>
        private PokemonVisualSO CreateOrUpdateVisualSO(SpeciesSO species, FormSO form)
        {
            string fileName = $"{species.speciesId:0000}_{species.nameKeyEng}_{form.formKey}_Visual.asset";
            string path = Path.Combine(saveFolder, fileName);

            var visualSO = AssetDatabase.LoadAssetAtPath<PokemonVisualSO>(path);
            if (visualSO == null)
            {
                visualSO = ScriptableObject.CreateInstance<PokemonVisualSO>();
                AssetDatabase.CreateAsset(visualSO, path);
                Debug.Log($"[Visual Importer] Created new VisualSO: {fileName}");
            }

            string assetName = $"{species.speciesId:0000}_{species.nameKeyEng}";
            if (!string.IsNullOrWhiteSpace(form.formKey) && form.formKey != "Default")
            {
                assetName += $"_{form.formKey}";
            }

            // 아이콘 에셋 로드 및 분리 (128x64 스프라이트 시트)
            var iconFrames = LoadSpritesFromSheet(Path.Combine(rootAssetPath, "Icons"), assetName, 64, 64);
            var shinyIconFrames = LoadSpritesFromSheet(Path.Combine(rootAssetPath, "Icons"), $"{assetName}_shiny", 64, 64);

            visualSO.icon = iconFrames?.FirstOrDefault();
            visualSO.shinyIcon = shinyIconFrames?.FirstOrDefault();

            // Front 애니메이션 에셋 로드 및 분리
            var frames = LoadSpritesFromSheet(Path.Combine(rootAssetPath, "Fronts"), assetName, null, null);
            var shinyFrames = LoadSpritesFromSheet(Path.Combine(rootAssetPath, "Fronts"), $"{assetName}_shiny", null, null);

            visualSO.frontFrames = frames?.ToArray();
            visualSO.shinyFrontFrames = shinyFrames?.ToArray();

            EditorUtility.SetDirty(visualSO);
            return visualSO;
        }

        /// <summary>
        /// 지정된 폴더에서 스프라이트 시트를 찾아 프레임을 분리합니다.
        /// </summary>
        private List<Sprite> LoadSpritesFromSheet(string folderPath, string sheetName, int? frameWidth, int? frameHeight)
        {
            var sprites = new List<Sprite>();
            var fullPath = Path.Combine(folderPath, $"{sheetName}.png");

            if (!File.Exists(fullPath)) return null;

            var importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.filterMode = FilterMode.Point;
                importer.spritePixelsPerUnit = 64; // 아이콘과 프론트 프레임 크기에 맞춰 설정
                importer.maxTextureSize = 16384;
                importer.isReadable = true;
                importer.spriteImportMode = SpriteImportMode.Multiple;

                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
                if (texture == null) return null;

                int fw = frameWidth ?? texture.height;
                int fh = frameHeight ?? texture.height;
                int framesCount = texture.width / fw;

                var spriteSheet = new List<SpriteMetaData>();
                for (int i = 0; i < framesCount; i++)
                {
                    var metaData = new SpriteMetaData
                    {
                        name = $"{sheetName}_{i}",
                        rect = new Rect(i * fw, 0, fw, fh)
                    };
                    spriteSheet.Add(metaData);
                }

                importer.spritesheet = spriteSheet.ToArray();
                importer.SaveAndReimport();
            }

            // AssetDatabase.LoadAllAssetsAtPath로 분리된 스프라이트를 로드
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(fullPath);
            foreach (var asset in allAssets)
            {
                if (asset is Sprite sprite)
                {
                    sprites.Add(sprite);
                }
            }
            // 분리된 스프라이트의 이름 규칙에 따라 정렬 (예: sheetName_0, sheetName_1...)
            return sprites.OrderBy(s => int.TryParse(s.name.Split('_').Last(), out var n) ? n : int.MaxValue).ToList();
        }
    }
}