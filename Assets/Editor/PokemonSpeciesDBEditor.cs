// ����: Assets/Editor/SpeciesDBEditor.cs
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// SpeciesDB.cs�� ���� Ŀ���� ������
    /// �ν����Ϳ� ������ ����� �߰��մϴ�.
    /// </summary>
    [CustomEditor(typeof(SpeciesDB))]
    public class PokemonSpeciesDBEditor : Editor
    {
        // SpeciesDB �ν��Ͻ��� ���� ����
        private SpeciesDB _targetDb;

        private void OnEnable()
        {
            // �����Ͱ� Ȱ��ȭ�� �� ���(SpeciesDB)�� ĳ���մϴ�.
            _targetDb = (SpeciesDB)target;
        }

        public override void OnInspectorGUI()
        {
            // ���� �ν����� UI�� �׸��ϴ�. (allSpecies �迭 �ʵ� ��)
            base.OnInspectorGUI();

            EditorGUILayout.Space(20); // �ð��� ���м� �߰�

            EditorGUILayout.LabelField("SpeciesDB Utilities", EditorStyles.boldLabel); // ��ƿ��Ƽ ���� ����

            // '��� SpeciesSO ���� �ҷ�����' ��ư
            if (GUILayout.Button("Load All SpeciesSO Assets from Folder"))
            {
                LoadAllSpeciesAssets();
            }

            // '��� SpeciesSO ������ speciesId �������� ����' ��ư
            if (GUILayout.Button("Sort SpeciesSO by SpeciesId"))
            {
                SortSpeciesAssets();
            }

            // 'allSpecies' �迭���� null �׸� ����
            if (GUILayout.Button("Remove Null Entries"))
            {
                RemoveNullEntries();
            }
        }

        /// <summary>
        /// ������ �������� ��� SpeciesSO ������ ã�� allSpecies �迭�� ����մϴ�.
        /// </summary>
        private void LoadAllSpeciesAssets()
        {
            // ���� ���� ���� ��� (PokemonCsvImporter.cs�� ����)
            string folderPath = "Assets/PokemonData";

            // ���� ���� ��� SpeciesSO ������ ã���ϴ�.
            string[] guids = AssetDatabase.FindAssets("t:SpeciesSO", new[] { folderPath });

            // ã�� ���µ��� ����Ʈ�� ����ϴ�.
            var loadedAssets = new List<SpeciesSO>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<SpeciesSO>(path);
                if (asset != null)
                {
                    loadedAssets.Add(asset);
                }
            }

            // �ߺ� �� null�� �����ϰ� �迭�� �����մϴ�.
            _targetDb.allSpecies = loadedAssets
                .Where(s => s != null)
                .Distinct()
                .ToArray();

            EditorUtility.SetDirty(_targetDb); // ���� ������ ����
            Debug.Log($"[SpeciesDB Editor] {loadedAssets.Count}���� SpeciesSO ������ �ҷ��Խ��ϴ�.");
        }

        /// <summary>
        /// allSpecies �迭�� speciesId �������� �������� �����մϴ�.
        /// </summary>
        private void SortSpeciesAssets()
        {
            if (_targetDb.allSpecies != null)
            {
                _targetDb.allSpecies = _targetDb.allSpecies
                    .Where(s => s != null) // null �׸� ����
                    .OrderBy(s => s.speciesId) // speciesId ���� ����
                    .ToArray();

                EditorUtility.SetDirty(_targetDb); // ���� ������ ����
                Debug.Log("[SpeciesDB Editor] SpeciesSO ������ �����߽��ϴ�.");
            }
        }

        /// <summary>
        /// allSpecies �迭���� null �׸��� �����մϴ�.
        /// </summary>
        private void RemoveNullEntries()
        {
            if (_targetDb.allSpecies != null)
            {
                _targetDb.allSpecies = _targetDb.allSpecies
                    .Where(s => s != null) // null �׸� �����ϰ� ���͸�
                    .ToArray();

                EditorUtility.SetDirty(_targetDb); // ���� ������ ����
                Debug.Log("[SpeciesDB Editor] allSpecies �迭���� null �׸��� �����߽��ϴ�.");
            }
        }
    }
}