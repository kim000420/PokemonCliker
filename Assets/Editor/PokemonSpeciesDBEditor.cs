// 파일: Assets/Editor/SpeciesDBEditor.cs
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// SpeciesDB.cs에 대한 커스텀 에디터
    /// 인스펙터에 유용한 기능을 추가합니다.
    /// </summary>
    [CustomEditor(typeof(SpeciesDB))]
    public class PokemonSpeciesDBEditor : Editor
    {
        // SpeciesDB 인스턴스에 대한 참조
        private SpeciesDB _targetDb;

        private void OnEnable()
        {
            // 에디터가 활성화될 때 대상(SpeciesDB)을 캐싱합니다.
            _targetDb = (SpeciesDB)target;
        }

        public override void OnInspectorGUI()
        {
            // 기존 인스펙터 UI를 그립니다. (allSpecies 배열 필드 등)
            base.OnInspectorGUI();

            EditorGUILayout.Space(20); // 시각적 구분선 추가

            EditorGUILayout.LabelField("SpeciesDB Utilities", EditorStyles.boldLabel); // 유틸리티 섹션 제목

            // '모든 SpeciesSO 에셋 불러오기' 버튼
            if (GUILayout.Button("Load All SpeciesSO Assets from Folder"))
            {
                LoadAllSpeciesAssets();
            }

            // '모든 SpeciesSO 에셋을 speciesId 기준으로 정렬' 버튼
            if (GUILayout.Button("Sort SpeciesSO by SpeciesId"))
            {
                SortSpeciesAssets();
            }

            // 'allSpecies' 배열에서 null 항목 제거
            if (GUILayout.Button("Remove Null Entries"))
            {
                RemoveNullEntries();
            }
        }

        /// <summary>
        /// 지정된 폴더에서 모든 SpeciesSO 에셋을 찾아 allSpecies 배열에 등록합니다.
        /// </summary>
        private void LoadAllSpeciesAssets()
        {
            // 에셋 저장 폴더 경로 (PokemonCsvImporter.cs와 동일)
            string folderPath = "Assets/PokemonData";

            // 폴더 내의 모든 SpeciesSO 에셋을 찾습니다.
            string[] guids = AssetDatabase.FindAssets("t:SpeciesSO", new[] { folderPath });

            // 찾은 에셋들을 리스트에 담습니다.
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

            // 중복 및 null을 제거하고 배열을 갱신합니다.
            _targetDb.allSpecies = loadedAssets
                .Where(s => s != null)
                .Distinct()
                .ToArray();

            EditorUtility.SetDirty(_targetDb); // 변경 사항을 저장
            Debug.Log($"[SpeciesDB Editor] {loadedAssets.Count}개의 SpeciesSO 에셋을 불러왔습니다.");
        }

        /// <summary>
        /// allSpecies 배열을 speciesId 기준으로 오름차순 정렬합니다.
        /// </summary>
        private void SortSpeciesAssets()
        {
            if (_targetDb.allSpecies != null)
            {
                _targetDb.allSpecies = _targetDb.allSpecies
                    .Where(s => s != null) // null 항목 제외
                    .OrderBy(s => s.speciesId) // speciesId 기준 정렬
                    .ToArray();

                EditorUtility.SetDirty(_targetDb); // 변경 사항을 저장
                Debug.Log("[SpeciesDB Editor] SpeciesSO 에셋을 정렬했습니다.");
            }
        }

        /// <summary>
        /// allSpecies 배열에서 null 항목을 제거합니다.
        /// </summary>
        private void RemoveNullEntries()
        {
            if (_targetDb.allSpecies != null)
            {
                _targetDb.allSpecies = _targetDb.allSpecies
                    .Where(s => s != null) // null 항목만 제외하고 필터링
                    .ToArray();

                EditorUtility.SetDirty(_targetDb); // 변경 사항을 저장
                Debug.Log("[SpeciesDB Editor] allSpecies 배열에서 null 항목을 제거했습니다.");
            }
        }
    }
}