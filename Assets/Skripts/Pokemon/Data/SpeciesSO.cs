using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// "종" 단위의 불변 정보(도감번호, 이름, 세대, 성별정책, 성장곡선, 최대 레벨).
    /// 폼별로 달라지는 정보는 하위 FormSO(sub-asset)들이 가진다.
    /// </summary>
    [CreateAssetMenu(menuName = "PokeClicker/Data/Species")]
    public class SpeciesSO : ScriptableObject
    {
        [Header("Identity (불변)")]
        [Tooltip("도감번호(유일)")] public int speciesId;          // 포켓몬 전국도감 번호
        [Tooltip("표시용 이름/로컬라이즈 키, 영어")] public string nameKeyEng;
        [Tooltip("표시용 이름/로컬라이즈 키, 한글")] public string nameKeyKor;
        [Tooltip("희귀도 카테고리")] public RarityCategory rarityCategory = RarityCategory.Ordinary; // 희귀도 카테고리
        [Tooltip("포획률")] public int catchRate = 0; // 포획률

        // [ 알그룹,스탭 ] 추후 추가예정
        // Egg_GroupA 
        // Egg_GroupB 
        // Egg_Step

        [Header("정책/성장")]
        public GenderPolicy genderPolicy; // 성별 정책
        public ExperienceCurve curveType; // 경험치 그룹
        [Range(1, 100)] public int maxLevel = 100;

        [Header("Forms (가변: sub-asset)")]
        [SerializeField] private List<FormSO> forms = new List<FormSO>(); // 폼 리스트(메가진화,다이멕스,리전폼등...)

        /// <summary>폼 목록 읽기 전용</summary>
        public IReadOnlyList<FormSO> Forms => forms;

        /// <summary>(speciesId, formKey) 로 FormSO를 찾는다. formKey가 비면 "Default".</summary>
        public FormSO GetForm(string formKey)
        {
            var key = NormalizeFormKey(formKey);
            for (int i = 0; i < forms.Count; i++)
            {
                var f = forms[i];
                if (f != null && string.Equals(NormalizeFormKey(f.formKey), key, StringComparison.Ordinal))
                    return f;
            }
            return null;
        }

        public static string NormalizeFormKey(string key)
        {
            return string.IsNullOrWhiteSpace(key) ? "Default" : key.Trim();
        }

        // ─────────────────────────────────────────────────────────────────────
        // 에디터 검증
        // ─────────────────────────────────────────────────────────────────────
#if UNITY_EDITOR
        private void OnValidate()
        {
            // 폼 리스트 null 방지
            if (forms == null) forms = new List<FormSO>();

            for (int i = 0; i < forms.Count; i++)
            {
                var f = forms[i];
                if (f == null) continue;

                f.formKey = NormalizeFormKey(f.formKey);

                // 보기 편하게 sub-asset 이름을 formKey로 맞춤
                if (f.name != f.formKey)
                    f.name = f.formKey;
            }

            // 빈/중복 formKey 정리 + 타입 정규화
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < forms.Count; i++)
            {
                var f = forms[i];
                if (f == null) continue;

                // 기본값/트리밍
                f.formKey = NormalizeFormKey(f.formKey);

                // 중복 formKey 경고
                if (!seen.Add(f.formKey))
                    Debug.LogWarning($"{name}: duplicated formKey '{f.formKey}' at index {i}", this);
            }

            // 최소 하나의 Default 폼 권장
            if (GetForm("Default") == null)
                Debug.LogWarning($"{name}: Default form is missing.", this);
        }

        /// <summary>에디터에서 하위 FormSO를 sub-asset으로 안전하게 추가(메뉴/유틸용)</summary>
        public FormSO Editor_AddFormIfMissing(string formKey = "Default")
        {
            formKey = NormalizeFormKey(formKey);
            var found = GetForm(formKey);
            if (found != null) return found;

            var f = ScriptableObject.CreateInstance<FormSO>();
            f.name = $"{name}_Form_{formKey}";
            f.formKey = formKey;

            UnityEditor.AssetDatabase.AddObjectToAsset(f, this);
            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(f));
            forms.Add(f);
            UnityEditor.EditorUtility.SetDirty(this);
            return f;
        }

        public void Editor_AddOrEnsureFormRef(FormSO form)
        {
            if (form == null) return;
            // formKey 정규화 + 서브에셋 이름 동기화
            form.formKey = NormalizeFormKey(form.formKey);
            if (form.name != form.formKey) form.name = form.formKey;

            // 리스트에 없으면 등록
            if (!forms.Contains(form))
            {
                forms.Add(form);
                EditorUtility.SetDirty(this);
            }
        }

        public void Editor_SyncFormsFromSubassets()
        {
            string path = AssetDatabase.GetAssetPath(this);
            var subs = AssetDatabase.LoadAllAssetsAtPath(path).OfType<FormSO>().ToList();

            // 리스트를 서브에셋 실체와 동기화
            forms.Clear();
            foreach (var f in subs)
            {
                // formKey/이름 정규화
                f.formKey = NormalizeFormKey(f.formKey);
                if (f.name != f.formKey) f.name = f.formKey;
                forms.Add(f);
                EditorUtility.SetDirty(f);
            }
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public void Editor_SortFormsDefaultFirst()
        {
            // "Default" 먼저, 그 다음 알파벳
            forms = forms.Where(f => f != null)
                         .OrderBy(f => f.formKey == "Default" ? 0 : 1)
                         .ThenBy(f => f.formKey, System.StringComparer.Ordinal)
                         .ToList();
            EditorUtility.SetDirty(this);
        }

        [CustomEditor(typeof(PokeClicker.SpeciesSO))]
        public class SpeciesSOEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                var species = (PokeClicker.SpeciesSO)target;
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Forms Utilities", EditorStyles.boldLabel);

                if (GUILayout.Button("서브에셋(폼) 리스트로 추가"))
                {
                    species.Editor_SyncFormsFromSubassets();
                }
                if (GUILayout.Button("Default 가 리스트 폼 맨위로"))
                {
                    species.Editor_SortFormsDefaultFirst();
                }
            }
        }
#endif
    }
}
