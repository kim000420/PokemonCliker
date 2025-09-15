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
            startLine = EditorGUILayout.IntField("Start Line (1-based)", startLine);
            endLine = EditorGUILayout.IntField("End Line (0 = all)", endLine);
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
            // CSV ���� / �����δ� 0������ ����
            // 0.species_id	
            // 1.eng_name_key	
            // 2.kor_name_key	
            // 3.name	
            // 4.form_key	
            // 5.type_a	
            // 6.type_b	
            // 7.generation	
            // 8.Gender_Unknown	
            // 9.Gender_Male	
            // 10.Gender_Female	
            // 11.Egg_Steps	
            // 12.Egg_Group1	
            // 13.Egg_Group2	
            // 14.Get_Rate	
            // 15.Experience_Type	
            // 16.Category	
            // 17.HP	
            // 18.A	
            // 19.B	
            // 20.C	
            // 21.D	
            // 22.S	
            // 23.Total

            string[] cols = line.Split(',');
            if (cols.Length < 24)
                throw new Exception("�÷� ���� �����մϴ� (�ּ� 24)");

            int dex = int.Parse(cols[0].Trim()); // ������ȣ
            string engName = cols[1].Trim(); // ������
            string korName = cols[2].Trim(); // �ѱ۸�
            int catchRate = int.Parse(cols[14].Trim());
            string formKey = string.IsNullOrWhiteSpace(cols[4]) ? "Default" : cols[4].Trim(); // �� Ű

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
                species.rarityCategory = ParseRarityCategory(cols[16]);
                species.curveType = ParseGrowthCurve(cols[15]);
                species.genderPolicy = ParseGenderPolicy(cols[8], cols[9]);

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
                species.curveType = ParseGrowthCurve(cols[15]);
                species.genderPolicy = ParseGenderPolicy(cols[8], cols[9]);
                Debug.Log($"[Importer] Updated SpeciesSO: {fileName}");
            }

            // FormSO ã��/����
            var form = species.GetForm(formKey);
            if (form == null)
            {
                form = ScriptableObject.CreateInstance<FormSO>();
                form.formKey = formKey;
                AssetDatabase.AddObjectToAsset(form, species);
                Debug.Log($"[Importer] Created FormSO: {species.speciesId} {formKey}");
            }

            // FormSO ������ ����

            form.generation = SafeInt(cols[7], 1); // ����

            form.typePair = new TypePair(); // Ÿ��
            form.typePair = ParseType(cols[5], cols[6]);

            form.baseStats = new StatBlock // ����
            {
                hp = SafeInt(cols[17]),
                atk = SafeInt(cols[18]),
                def = SafeInt(cols[19]),
                spa = SafeInt(cols[20]),
                spd = SafeInt(cols[21]),
                spe = SafeInt(cols[22])
            };

            EditorUtility.SetDirty(species);
            EditorUtility.SetDirty(form);
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


        private TypePair ParseType(string typeA, string typeB)
        {
            TypePair result = new TypePair();

            // 1. �⺻��
            result.primary = TypeEnum.None;
            result.secondary = TypeEnum.None;
            result.hasDualType = false;

            // 2. ���ڿ� �� enum �Ľ� (��ҹ��� ����)
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
