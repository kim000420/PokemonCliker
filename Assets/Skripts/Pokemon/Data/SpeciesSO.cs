using System.Collections.Generic;
using UnityEngine;

namespace PokeClicker
{
    // ���ϸ� "��" ������ �Һ� ������
    // ��ü(���̺� ������)���� �� ID�� �����ϰ�, ǥ��/��� �� �� SO�� �����Ѵ�.
    [CreateAssetMenu(menuName = "PokeClicker/DB/Species")]
    public class SpeciesSO : ScriptableObject
    {
        [Header("�⺻ �ĺ� ����")]
        public int speciesId = 0;         // ������ȣ(������)
        public string nameKey = "";       // ����ȭ Ű. ������ ���� �� ǥ�� �̸����� ���

        [Header("����/���� ����")]
        public ExperienceCurve expCurveId = ExperienceCurve.MediumFast; // ����ġ � �ĺ���
        [Range(1, 100)] public int maxLevel = 100;                      // �ִ� ����(�Ϲ������� 100)

        [Header("����/���� ��ǥ(����)")]
        public GenderPolicy genderPolicy;  // ���� ��å(�����̸� hasGender=false)
        public StatBlock baseStats;        // ������ ��� ������ġ/���Ŀ����� Ȱ�� ����

        [Header("�� ���� ������ �� ���(����)")]
        public List<FormSO> forms = new List<FormSO>(); // ������ ���ǿ�(���� �巡���ؼ� ����)

        // �ν����Ϳ��� ���� �ٲ� �� �ڵ� ����
        private void OnValidate()
        {
            // speciesId�� 1 �̻��� ����
            if (speciesId < 1) speciesId = 1;

            // maxLevel�� 1~100 ������ ����
            if (maxLevel < 1) maxLevel = 1;
            if (maxLevel > 100) maxLevel = 100;

            // �� ����Ʈ���� null �׸� ���� �� �ߺ� ����
            if (forms != null)
            {
                // null ����
                forms.RemoveAll(f => f == null);

                // ���� FormSO�� �ߺ����� ���� �ʵ��� ����
                var seen = new HashSet<FormSO>();
                for (int i = forms.Count - 1; i >= 0; i--)
                {
                    var f = forms[i];
                    if (seen.Contains(f))
                        forms.RemoveAt(i);
                    else
                        seen.Add(f);
                }

                // ���� ����Ǿ� �ִٸ� species ������ ������ �� ���� ����Ű���� �����(���� ����)
                for (int i = 0; i < forms.Count; i++)
                {
                    if (forms[i] != null && forms[i].species != this)
                    {
                        forms[i].species = this;
                    }
                }
            }
        }

        // ��ȿ�� �˻縦 �ڵ忡�� ��������� ȣ���ϰ� ���� �� ���
        public bool IsValid(out string reason)
        {
            if (speciesId < 1)
            {
                reason = "speciesId�� 1 �̻��̾�� �մϴ�.";
                return false;
            }
            if (string.IsNullOrEmpty(nameKey))
            {
                reason = "nameKey�� ��� �ֽ��ϴ�. ����ȭ Ű�� �����ϼ���.";
                return false;
            }
            reason = null;
            return true;
        }
    }
}
