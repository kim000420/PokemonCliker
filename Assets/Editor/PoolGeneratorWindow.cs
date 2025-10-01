// 파일: Assets/Editor/PoolGeneratorWindow.cs
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace PokeClicker.EditorTools
{
    public class PoolGeneratorWindow : EditorWindow
    {
        // --- 설정 ---
        private const string POKEMON_DB_CSV_PATH = "Assets/Data/PokemonDB_v006.csv"; // PokemonDB.csv 파일의 실제 경로로 수정해주세요.
        private const string POOL_DB_PATH = "Assets/Data/PokemonPoolDB.asset";
        private const string POOLS_FOLDER_PATH = "Assets/Data/PokemonPools";

        // --- UI용 변수 ---
        private string _outputFileName = "NewPool";
        private bool[] _generationToggles = new bool[9]; // 1세대부터 9세대까지
        private bool[] _rarityToggles = new bool[Enum.GetNames(typeof(RarityCategory)).Length];
        private readonly string[] _formFilterNames = { "Default", "Alolan", "Galarian", "Hisuian", "Paldean" };
        private bool[] _formToggles = new bool[5];

        [MenuItem("Tools/Pool Generator")]
        public static void OpenWindow()
        {
            GetWindow<PoolGeneratorWindow>("Pool Generator");
        }

        void OnGUI()
        {
            GUILayout.Label("Pool Generation Settings", EditorStyles.boldLabel);
            _outputFileName = EditorGUILayout.TextField("Output File Name", _outputFileName);

            EditorGUILayout.HelpBox("각 카테고리 내에서는 OR, 카테고리 간에는 AND로 작동합니다. 카테고리에서 아무것도 선택하지 않으면 해당 카테고리는 필터링하지 않습니다.", MessageType.Info);

            EditorGUILayout.Space();

            // --- 필터 UI 부분 (기존과 동일) ---
            GUILayout.Label("최초 출시 세대 필터", EditorStyles.boldLabel);
            for (int i = 0; i < _generationToggles.Length; i++) _generationToggles[i] = EditorGUILayout.Toggle($"Gen {i + 1}", _generationToggles[i]);
            EditorGUILayout.Space();

            GUILayout.Label("희귀도 카테고리 필터", EditorStyles.boldLabel);
            string[] rarityNames = Enum.GetNames(typeof(RarityCategory));
            for (int i = 0; i < _rarityToggles.Length; i++) _rarityToggles[i] = EditorGUILayout.Toggle(rarityNames[i], _rarityToggles[i]);
            EditorGUILayout.Space();

            GUILayout.Label("리전폼 필터", EditorStyles.boldLabel);
            for (int i = 0; i < _formToggles.Length; i++) _formToggles[i] = EditorGUILayout.Toggle(_formFilterNames[i], _formToggles[i]);

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Pool")) GeneratePool();
        }

        private void GeneratePool()
        {
            if (!File.Exists(POKEMON_DB_CSV_PATH))
            {
                Debug.LogError($"PokemonDB.csv not found at {POKEMON_DB_CSV_PATH}");
                return;
            }

            // 1. 선택된 필터 조건들을 리스트로 만듭니다.
            var selectedGenerations = new List<int>();
            for (int i = 0; i < _generationToggles.Length; i++)
            {
                if (_generationToggles[i]) selectedGenerations.Add(i + 1);
            }

            var selectedRarities = new List<RarityCategory>();
            var rarityValues = (RarityCategory[])Enum.GetValues(typeof(RarityCategory));
            for (int i = 0; i < _rarityToggles.Length; i++)
            {
                if (_rarityToggles[i]) selectedRarities.Add(rarityValues[i]);
            }

            var selectedForms = new List<string>();
            for (int i = 0; i < _formToggles.Length; i++)
            {
                if (_formToggles[i]) selectedForms.Add(_formFilterNames[i]);
            }

            if (selectedGenerations.Count == 0 && selectedRarities.Count == 0 && selectedForms.Count == 0)
            {
                Debug.LogWarning("No filter conditions were selected. Pool will be empty.");
            }

            // 2. PokemonDB.csv 전체를 읽고 필터링합니다.
            var poolEntries = new List<PoolEntry>();
            var lines = File.ReadAllLines(POKEMON_DB_CSV_PATH).Skip(1); // 헤더 제외

            foreach (var line in lines)
            {
                var values = line.Split(',');

                int speciesId = int.Parse(values[0]);
                string formKey = string.IsNullOrWhiteSpace(values[5]) ? "Default" : values[5].Trim();
                int generation = int.Parse(values[8].Trim());
                Enum.TryParse<RarityCategory>(values[17].Trim(), true, out var rarity);

                bool genMatch = selectedGenerations.Count == 0 || selectedGenerations.Contains(generation);
                bool rarityMatch = selectedRarities.Count == 0 || selectedRarities.Contains(rarity);
                bool formMatch = selectedForms.Count == 0 || IsFormMatch(formKey, selectedForms);


                // OR 조건: 선택된 세대이거나, 또는 선택된 희귀도인가?
                if (genMatch && rarityMatch && formMatch)
                {
                    poolEntries.Add(new PoolEntry
                    {
                        speciesId = speciesId,
                        formKey = formKey 
                    });
                }
            }

            // 3. 필터링된 결과를 PoolSO 애셋에 저장합니다.
            if (!Directory.Exists(POOLS_FOLDER_PATH))
            {
                Directory.CreateDirectory(POOLS_FOLDER_PATH);
            }

            string assetPath = Path.Combine(POOLS_FOLDER_PATH, $"{_outputFileName}.asset");
            var poolSo = AssetDatabase.LoadAssetAtPath<PokemonPoolSO>(assetPath);
            if (poolSo == null)
            {
                poolSo = ScriptableObject.CreateInstance<PokemonPoolSO>();
                AssetDatabase.CreateAsset(poolSo, assetPath);
            }

            poolSo.entries = poolEntries;
            EditorUtility.SetDirty(poolSo);

            // 4. PokemonPoolDB에 자동으로 등록/업데이트합니다.
            var poolDb = AssetDatabase.LoadAssetAtPath<PokemonPoolDB>(POOL_DB_PATH);
            if (poolDb != null)
            {
                var mappingIndex = poolDb.pools.FindIndex(p => p.poolName == _outputFileName);
                if (mappingIndex >= 0)
                {
                    var existingMapping = poolDb.pools[mappingIndex];
                    existingMapping.pool = poolSo;
                    poolDb.pools[mappingIndex] = existingMapping;
                }
                else
                {
                    poolDb.pools.Add(new PokemonPoolDB.PoolMapping { poolName = _outputFileName, pool = poolSo });
                }
                EditorUtility.SetDirty(poolDb);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Pool '{_outputFileName}' generated successfully with {poolEntries.Count} entries.");
        }
        private bool IsFormMatch(string pokemonFormKey, List<string> selectedForms)
        {
            foreach (string selectedForm in selectedForms)
            {
                if (selectedForm == "Paldean")
                {
                    // Paldean은 포함 여부로 검사
                    if (pokemonFormKey.IndexOf("Paldean", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                }
                else
                {
                    // 나머지는 정확히 일치하는지 검사
                    if (string.Equals(pokemonFormKey, selectedForm, StringComparison.OrdinalIgnoreCase)) return true;
                }
            }
            return false;
        }
    }
}