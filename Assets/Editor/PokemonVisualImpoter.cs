// ����: Assets/Editor/PokemonVisualImporter.cs
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace PokeClicker.EditorTools
{
    /// <summary>
    /// ������ ������ �̹��� ������ ������� PokemonVisualSO�� �����ϰ� FormSO�� �����ϴ� ������ ��.
    /// Resources ������ ���� �������� �����ϰ� AssetDatabase API�� ����ϵ��� �����߽��ϴ�.
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
                    EditorUtility.DisplayDialog("Error", "Species DB�� �Ҵ����ּ���.", "OK");
                    return;
                }

                if (!Directory.Exists(rootAssetPath))
                {
                    EditorUtility.DisplayDialog("Error", $"{rootAssetPath} ������ �������� �ʽ��ϴ�.", "OK");
                    return;
                }

                ImportAllVisuals();
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// ��� SpeciesSO�� ��ȸ�ϸ� VisualSO�� ����/������Ʈ�ϰ� FormSO�� �����մϴ�.
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
        /// VisualSO�� �����ϰų� ���� ���� ã�� ������Ʈ�ϰ�, ������ �Ҵ��մϴ�.
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

            // ������ ���� �ε� �� �и� (128x64 ��������Ʈ ��Ʈ)
            var iconFrames = LoadSpritesFromSheet(Path.Combine(rootAssetPath, "Icons"), assetName, 64, 64);
            var shinyIconFrames = LoadSpritesFromSheet(Path.Combine(rootAssetPath, "Icons"), $"{assetName}_shiny", 64, 64);

            visualSO.icon = iconFrames?.FirstOrDefault();
            visualSO.shinyIcon = shinyIconFrames?.FirstOrDefault();

            // Front �ִϸ��̼� ���� �ε� �� �и�
            var frames = LoadSpritesFromSheet(Path.Combine(rootAssetPath, "Fronts"), assetName, null, null);
            var shinyFrames = LoadSpritesFromSheet(Path.Combine(rootAssetPath, "Fronts"), $"{assetName}_shiny", null, null);

            visualSO.frontFrames = frames?.ToArray();
            visualSO.shinyFrontFrames = shinyFrames?.ToArray();

            EditorUtility.SetDirty(visualSO);
            return visualSO;
        }

        /// <summary>
        /// ������ �������� ��������Ʈ ��Ʈ�� ã�� �������� �и��մϴ�.
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
                importer.spritePixelsPerUnit = 64; // �����ܰ� ����Ʈ ������ ũ�⿡ ���� ����
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

            // AssetDatabase.LoadAllAssetsAtPath�� �и��� ��������Ʈ�� �ε�
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(fullPath);
            foreach (var asset in allAssets)
            {
                if (asset is Sprite sprite)
                {
                    sprites.Add(sprite);
                }
            }
            // �и��� ��������Ʈ�� �̸� ��Ģ�� ���� ���� (��: sheetName_0, sheetName_1...)
            return sprites.OrderBy(s => int.TryParse(s.name.Split('_').Last(), out var n) ? n : int.MaxValue).ToList();
        }
    }
}