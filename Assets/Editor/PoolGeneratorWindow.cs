// ����: Assets/Editor/PoolGeneratorWindow.cs
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
        // --- ���� ---
        private const string POKEMON_DB_CSV_PATH = "Assets/Data/PokemonDB_v006.csv"; // PokemonDB.csv ������ ���� ��η� �������ּ���.
        private const string POOL_DB_PATH = "Assets/Data/PokemonPoolDB.asset";
        private const string POOLS_FOLDER_PATH = "Assets/Data/PokemonPools";

        // --- UI�� ���� ---
        private string _outputFileName = "NewPool";
        private bool[] _generationToggles = new bool[9]; // 1������� 9�������
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

            EditorGUILayout.HelpBox("�� ī�װ� �������� OR, ī�װ� ������ AND�� �۵��մϴ�. ī�װ����� �ƹ��͵� �������� ������ �ش� ī�װ��� ���͸����� �ʽ��ϴ�.", MessageType.Info);

            EditorGUILayout.Space();

            // --- ���� UI �κ� (������ ����) ---
            GUILayout.Label("���� ��� ���� ����", EditorStyles.boldLabel);
            for (int i = 0; i < _generationToggles.Length; i++) _generationToggles[i] = EditorGUILayout.Toggle($"Gen {i + 1}", _generationToggles[i]);
            EditorGUILayout.Space();

            GUILayout.Label("��͵� ī�װ� ����", EditorStyles.boldLabel);
            string[] rarityNames = Enum.GetNames(typeof(RarityCategory));
            for (int i = 0; i < _rarityToggles.Length; i++) _rarityToggles[i] = EditorGUILayout.Toggle(rarityNames[i], _rarityToggles[i]);
            EditorGUILayout.Space();

            GUILayout.Label("������ ����", EditorStyles.boldLabel);
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

            // 1. ���õ� ���� ���ǵ��� ����Ʈ�� ����ϴ�.
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

            // 2. PokemonDB.csv ��ü�� �а� ���͸��մϴ�.
            var poolEntries = new List<PoolEntry>();
            var lines = File.ReadAllLines(POKEMON_DB_CSV_PATH).Skip(1); // ��� ����

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


                // OR ����: ���õ� �����̰ų�, �Ǵ� ���õ� ��͵��ΰ�?
                if (genMatch && rarityMatch && formMatch)
                {
                    poolEntries.Add(new PoolEntry
                    {
                        speciesId = speciesId,
                        formKey = formKey 
                    });
                }
            }

            // 3. ���͸��� ����� PoolSO �ּ¿� �����մϴ�.
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

            // 4. PokemonPoolDB�� �ڵ����� ���/������Ʈ�մϴ�.
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
                    // Paldean�� ���� ���η� �˻�
                    if (pokemonFormKey.IndexOf("Paldean", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                }
                else
                {
                    // �������� ��Ȯ�� ��ġ�ϴ��� �˻�
                    if (string.Equals(pokemonFormKey, selectedForm, StringComparison.OrdinalIgnoreCase)) return true;
                }
            }
            return false;
        }
    }
}