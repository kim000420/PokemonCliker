using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// "��" ������ �Һ� ����(������ȣ, �̸�, ����, ������å, ����, �ִ� ����).
    /// ������ �޶����� ������ ���� FormSO(sub-asset)���� ������.
    /// </summary>
    [CreateAssetMenu(menuName = "PokeClicker/Data/Species")]
    public class SpeciesSO : ScriptableObject
    {
        [Header("Identity (�Һ�)")]
        [Tooltip("������ȣ(����)")] public int speciesId;          // ���ϸ� �������� ��ȣ
        [Tooltip("ǥ�ÿ� �̸�/���ö����� Ű, ����")] public string nameKeyEng;
        [Tooltip("ǥ�ÿ� �̸�/���ö����� Ű, �ѱ�")] public string nameKeyKor;
        [Tooltip("��͵� ī�װ�")] public RarityCategory rarityCategory = RarityCategory.Ordinary; // ��͵� ī�װ�
        [Tooltip("��ȹ��")] public int catchRate = 0; // ��ȹ��

        // [ �˱׷�,���� ] ���� �߰�����
        // Egg_GroupA 
        // Egg_GroupB 
        // Egg_Step

        [Header("��å/����")]
        public GenderPolicy genderPolicy; // ���� ��å
        public ExperienceCurve curveType; // ����ġ �׷�
        [Range(1, 100)] public int maxLevel = 100;

        [Header("Forms (����: sub-asset)")]
        [SerializeField] private List<FormSO> forms = new List<FormSO>(); // �� ����Ʈ(�ް���ȭ,���̸߽�,��������...)

        /// <summary>�� ��� �б� ����</summary>
        public IReadOnlyList<FormSO> Forms => forms;

        /// <summary>(speciesId, formKey) �� FormSO�� ã�´�. formKey�� ��� "Default".</summary>
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

        // ������������������������������������������������������������������������������������������������������������������������������������������
        // ������ ����
        // ������������������������������������������������������������������������������������������������������������������������������������������
#if UNITY_EDITOR
        private void OnValidate()
        {
            // �� ����Ʈ null ����
            if (forms == null) forms = new List<FormSO>();

            for (int i = 0; i < forms.Count; i++)
            {
                var f = forms[i];
                if (f == null) continue;

                f.formKey = NormalizeFormKey(f.formKey);

                // ���� ���ϰ� sub-asset �̸��� formKey�� ����
                if (f.name != f.formKey)
                    f.name = f.formKey;
            }

            // ��/�ߺ� formKey ���� + Ÿ�� ����ȭ
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < forms.Count; i++)
            {
                var f = forms[i];
                if (f == null) continue;

                // �⺻��/Ʈ����
                f.formKey = NormalizeFormKey(f.formKey);

                // �ߺ� formKey ���
                if (!seen.Add(f.formKey))
                    Debug.LogWarning($"{name}: duplicated formKey '{f.formKey}' at index {i}", this);
            }

            // �ּ� �ϳ��� Default �� ����
            if (GetForm("Default") == null)
                Debug.LogWarning($"{name}: Default form is missing.", this);
        }

        /// <summary>�����Ϳ��� ���� FormSO�� sub-asset���� �����ϰ� �߰�(�޴�/��ƿ��)</summary>
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
            // formKey ����ȭ + ���꿡�� �̸� ����ȭ
            form.formKey = NormalizeFormKey(form.formKey);
            if (form.name != form.formKey) form.name = form.formKey;

            // ����Ʈ�� ������ ���
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

            // ����Ʈ�� ���꿡�� ��ü�� ����ȭ
            forms.Clear();
            foreach (var f in subs)
            {
                // formKey/�̸� ����ȭ
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
            // "Default" ����, �� ���� ���ĺ�
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

                if (GUILayout.Button("���꿡��(��) ����Ʈ�� �߰�"))
                {
                    species.Editor_SyncFormsFromSubassets();
                }
                if (GUILayout.Button("Default �� ����Ʈ �� ������"))
                {
                    species.Editor_SortFormsDefaultFirst();
                }
            }
        }
#endif
    }
}
