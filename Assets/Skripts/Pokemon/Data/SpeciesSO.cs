using System.Collections.Generic;
using UnityEngine;

namespace PokeClicker
{
    // ���ϸ� "��" ������ �Һ� ������
    [CreateAssetMenu(menuName = "PokeClicker/DB/Species")]
    public class SpeciesSO : ScriptableObject
    {
        [Header("�⺻ �ĺ� ����")]
        public int speciesId = 0;          // ������ȣ(������)
        public string nameKey = "";        // ����ȭ Ű
        [Range(1, 9)] public int generation = 1; // ���ϸ� ���� (1~9)

        [Header("����/���� ����")]
        public ExperienceCurve expCurveId = ExperienceCurve.MediumFast;
        [Range(1, 100)] public int maxLevel = 100;

        [Header("����/�ɷ�ġ")]
        public GenderPolicy genderPolicy;  // ���� ��å
        public StatBlock baseStats;        // H,A,B,C,D,S
        [SerializeField, HideInInspector] private int totalStats; // �հ� (�ڵ� ����)

        [Header("�� ���� ������ �� ���(����)")]
        public List<FormSO> forms = new List<FormSO>();

        // totalStats ���� �б� �������� ����
        public int TotalStats => totalStats;

        private void OnValidate()
        {
            // speciesId ����
            if (speciesId < 1) speciesId = 1;

            // maxLevel ����
            if (maxLevel < 1) maxLevel = 1;
            if (maxLevel > 100) maxLevel = 100;

            // ���� ���� ���
            totalStats = baseStats.hp + baseStats.atk + baseStats.def +
                         baseStats.spa + baseStats.spd + baseStats.spe;

            // �� ����Ʈ ����
            if (forms != null)
            {
                var seen = new HashSet<FormSO>();
                for (int i = 0; i < forms.Count; i++)
                {
                    var f = forms[i];
                    if (f == null) continue;
                    if (seen.Contains(f))
                        forms[i] = null; // �ߺ��� null�� �ٲ㼭 ���� ����
                    else
                        seen.Add(f);
                }

                // ������ ����: null�� �ƴ� �׸� species ����ũ ����
                for (int i = 0; i < forms.Count; i++)
                {
                    var f = forms[i];
                    if (f != null && f.species != this)
                        f.species = this;
                }
            }
        }

        // �ܺο��� ��ȿ�� �˻� ȣ�� ����
        public bool IsValid(out string reason)
        {
            if (speciesId < 1)
            {
                reason = "speciesId�� 1 �̻��̾�� �մϴ�.";
                return false;
            }
            if (string.IsNullOrEmpty(nameKey))
            {
                reason = "nameKey�� ��� �ֽ��ϴ�.";
                return false;
            }
            reason = null;
            return true;
        }
    }
}
