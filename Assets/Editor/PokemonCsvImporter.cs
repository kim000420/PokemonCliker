// ����: Editor/PokemonCsvImporter.cs
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PokeClicker.EditorTools
{
    /// <summary>
    /// CSV ������� SpeciesSO + FormSO�� ����/�����ϴ� ������ ��.
    /// - ���ϸ� ��Ģ: 0001_Bulbasaur.asset
    /// - FormSO�� �ݵ�� �ش� SpeciesSO�� sub-asset���� ����
    /// - �ߺ� ���� ������ ���� ��� �ε� �� ����
    /// </summary>
    public class PokemonCsvImporter : EditorWindow
    {
        [Header("CSV Import Settings")]
        public TextAsset csvFile;
        public int startLine = 2; // 1-based, ���� 1���� ���
        public int endLine = 0;   // 0�̸� ������
        public string saveFolder = "Assets/PokemonData";

        [MenuItem("Tools/Pokemon CSV Importer")]
        public static void OpenWindow()
        {
            GetWindow<PokemonCsvImporter>("Pokemon CSV Importer");
        }

        private Vector2 _scroll;

        void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);

            EditorGUILayout.Space();

            startLine = EditorGUILayout.IntField("Start Line (1-based)", startLine);
            endLine = EditorGUILayout.IntField("End Line (0 = all)", endLine);

            EditorGUILayout.Space();

            saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);

            EditorGUILayout.Space();

            if (GUILayout.Button("Import CSV"))
            {
                if (csvFile == null)
                {
                    Debug.LogError("CSV ������ �����ϼ���.");
                }
                else
                {
                    Import();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void Import()
        {
            string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int lineCount = lines.Length;

            int start = Mathf.Max(1, startLine); // 1-based
            int end = (endLine <= 0 || endLine > lineCount) ? lineCount : endLine;

            Debug.Log($"[PokemonCsvImporter] Importing lines {start}..{end} from {csvFile.name}");

            for (int i = start; i <= end; i++)
            {
                string line = lines[i - 1];
                try
                {
                    ProcessLine(line, i);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[PokemonCsvImporter] Line {i} error: {ex.Message}\n{line}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[PokemonCsvImporter] Import finished.");
        }

        private void ProcessLine(string line, int lineNumber)
        {
            /// <summary> 
            /// PokemonDB.CSV �÷� �ε���
            /// 0.species_ID	
            /// 1.species_eng_name	
            /// 2.species_kor_name	
            /// 3.form_ID	
            /// 4.form_eng_name	
            /// 5.form_key	
            /// 6.type_a	
            /// 7.type_b	
            /// 8.generation	
            /// 9.gender_unknown	
            /// 10.gender_male	
            /// 11.gender_female	
            /// 12.egg_steps	
            /// 13.egg_group1	
            /// 14.egg_group2	
            /// 15.catch_rate	
            /// 16.experience_group	
            /// 17.rarity_category
            /// 18.HP	
            /// 19.A	
            /// 20.B	
            /// 21.C	
            /// 22.D	
            /// 23.S	
            /// 24.Total
            /// </summary>

            string[] cols = line.Split(',');
            if (cols.Length < 24)
                throw new Exception("�÷� ���� �����մϴ� (�ּ� 24)");

            int dex = int.Parse(cols[0].Trim()); // ������ȣ
            string engName = cols[1].Trim(); // ������
            string korName = cols[2].Trim(); // �ѱ۸�
            int catchRate = int.Parse(cols[15].Trim()); // ��ȹ��
            //int eggSteps = int.Parse(cols[12].Trim()); // �˽���
            //int eggGroup1 = int.Parse(cols[13].Trim()); // �˱׷�1
            //int eggGroup2 = int.Parse(cols[14].Trim()); // �˱׷�2
            string formKey = string.IsNullOrWhiteSpace(cols[5]) ? "Default" : cols[5].Trim(); // �� Ű

            // �⺻ ���/���ϸ�
            string fileName = $"{dex:0000}_{Sanitize(engName)}.asset";
            string path = Path.Combine(saveFolder, fileName);

            // SpeciesSO �ε�/����
            SpeciesSO species = AssetDatabase.LoadAssetAtPath<SpeciesSO>(path);
            if (species == null)
            {
                species = ScriptableObject.CreateInstance<SpeciesSO>();
                species.speciesId = dex;
                species.nameKeyEng = engName;
                species.nameKeyKor = korName;
                species.catchRate = catchRate;
                species.rarityCategory = ParseRarityCategory(cols[17]);
                species.curveType = ParseGrowthCurve(cols[16]);
                species.genderPolicy = ParseGenderPolicy(cols[9], cols[10]);

                if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);
                AssetDatabase.CreateAsset(species, path);
                Debug.Log($"[Importer] Created SpeciesSO: {fileName}");
            }
            else
            {
                // ����
                species.speciesId = dex;
                species.nameKeyEng = engName;
                species.nameKeyKor = korName;
                species.catchRate = catchRate;
                species.rarityCategory = ParseRarityCategory(cols[17]);
                species.curveType = ParseGrowthCurve(cols[16]);
                species.genderPolicy = ParseGenderPolicy(cols[9], cols[10]);
                Debug.Log($"[Importer] Updated SpeciesSO: {fileName}");
            }

            // FormSO ã��/����
            var form = species.GetForm(formKey);
            if (form == null)
            {
                form = ScriptableObject.CreateInstance<FormSO>();
                form.formKey = formKey;
                // ���� ���ϰ� sub-asset �̸��� formKey��
                form.name = SpeciesSO.NormalizeFormKey(formKey);

                AssetDatabase.AddObjectToAsset(form, species);

                // ����Ʈ �ڵ� ��� (�ߺ� üũ/�̸� ����ȭ ����)
                species.Editor_AddOrEnsureFormRef(form);

                Debug.Log($"[Importer] Created FormSO: {species.speciesId} {formKey}");
            }
            else
            {
                // ���� ���� �̸�/Ű ����ȭ & ����Ʈ ����
                form.formKey = SpeciesSO.NormalizeFormKey(form.formKey);
                if (form.name != form.formKey) form.name = form.formKey;
                species.Editor_AddOrEnsureFormRef(form);
            }

            // FormSO ������ ����

            form.formId = int.Parse(cols[3].Trim());
            form.generation = SafeInt(cols[8], 1); // ����

            form.typePair = new TypePair(); // Ÿ��
            form.typePair = ParseType(cols[6], cols[7]);

            form.baseStats = new StatBlock // ����
            {
                hp = SafeInt(cols[18]),
                atk = SafeInt(cols[19]),
                def = SafeInt(cols[20]),
                spa = SafeInt(cols[21]),
                spd = SafeInt(cols[22]),
                spe = SafeInt(cols[23])
            };

            EditorUtility.SetDirty(form);
            EditorUtility.SetDirty(species);
        }

        // ����������������������������������������������������������������������������������������������������������������������������
        // �ļ� ��ƿ
        // ����������������������������������������������������������������������������������������������������������������������������

        // string -> int
        private int SafeInt(string s, int def = 0)
        {
            return int.TryParse(s.Trim(), out int v) ? v : def;
        }

        // ?
        private string Sanitize(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Replace(" ", "_");
        }

        // ��� enumŸ������ ��ȯ
        private RarityCategory ParseRarityCategory(string Sraritycategory)
        {
            if (Enum.TryParse<RarityCategory>(Sraritycategory.Trim(), true, out var rC))
                return rC;
            return RarityCategory.Ordinary;
        }

        // ����ġĿ��׷� enumŸ������ ��ȯ
        private ExperienceCurve ParseGrowthCurve(string SgrowthCurve)
        {
            if (Enum.TryParse<ExperienceCurve>(SgrowthCurve.Trim(), true, out var gC))
                return gC;
            return ExperienceCurve.MediumFast;
        }

        // ���� ��ȯ
        private GenderPolicy ParseGenderPolicy(string genderUnknown, string maleRateStr)
        {
            GenderPolicy gp = new GenderPolicy();
            bool isUnknown = string.Equals(genderUnknown?.Trim(), "TRUE", StringComparison.OrdinalIgnoreCase);

            if (isUnknown)
            {
                gp.hasGender = false;
                gp.maleRate0to100 = 0;
            }
            else
            {
                gp.hasGender = true;
                if (float.TryParse(maleRateStr?.Trim(), out var maleF))
                    gp.maleRate0to100 = Mathf.Clamp(Mathf.RoundToInt(maleF), 0, 100);
                else
                    gp.maleRate0to100 = 50; // ���� �� �⺻��
            }
            return gp;
        }

        // Ÿ�� ��ȯ
        private TypePair ParseType(string typeA, string typeB)
        {
            TypePair result = new TypePair();

            result.primary = TypeEnum.None;
            result.secondary = TypeEnum.None;
            result.hasDualType = false;

            if (!string.IsNullOrWhiteSpace(typeA) &&
                Enum.TryParse<TypeEnum>(typeA.Trim(), true, out var tA))
            {
                result.primary = tA;
            }

            if (!string.IsNullOrWhiteSpace(typeB) &&
                Enum.TryParse<TypeEnum>(typeB.Trim(), true, out var tB) &&
                tB != TypeEnum.None)
            {
                result.secondary = tB;
                result.hasDualType = true;
            }

            return result;
        }

    }
}
