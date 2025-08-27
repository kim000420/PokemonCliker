using System;
using UnityEngine;

namespace PokeClicker
{
    // �÷��̾ �����ϴ� "���ϸ� ��ü"�� ���� ������
    // ����/�ε� ����̸�, ǥ�ó� ��� �ÿ� SpeciesSO, FormSO ���� �����Ѵ�.
    [Serializable]
    public class PokemonSaveData
    {
        // �ĺ�
        public int uid;                 // ��ü ���� ID (���̺� ���ο��� ����)
        public int speciesId;           // �� ID (SpeciesSO.speciesId)
        public string formKey = "Default"; // �� Ű ("Default", "Alola" ��)
        public bool isShiny;            // �̷�ġ ����

        // �̸�/����
        public string nickname;         // ���� (��� ������ �� �̸��� ���)
        public int level = 1;           // ���� ����
        public int currentExp = 0;      // ���� ���� �������� ����ġ(����)

        // ����
        public Gender gender = Gender.Male;
        public int friendship = 0;      // ģ�е� (0~255 ����)
        public string heldItemId;       // ���� ������ ID (����)

        // ǥ�ÿ� �̸� ��ȯ (������ ������ ����, ������ �� �̸� Ű)
        public string GetDisplayName(SpeciesSO species)
        {
            if (!string.IsNullOrWhiteSpace(nickname))
                return nickname;
            return species != null ? species.nameKey : "Unknown";
        }

        // ���� ��å�� ���� ������ ������
        public static Gender RollGender(GenderPolicy policy)
        {
            if (!policy.hasGender) return Gender.Genderless;

            int r = UnityEngine.Random.Range(0, 100); // 0~99
            return (r < policy.maleRate0to100) ? Gender.Male : Gender.Female;
        }

        // �ε� ���ĳ� ���� ���� �Ŀ� �� ������ �����Ѵ�
        // - ����: 1~���� �ִ� ����
        // - ����ġ: ���� �������� �ʿ��� �䱸ġ �̸����� Ŭ����
        // - ģ�е�: 0 �̻�
        public void EnsureValidAfterLoad(SpeciesSO species, ExperienceCurveSO curve)
        {
            if (species == null) return;

            // ���� ���� ����
            int maxLv = Mathf.Clamp(species.maxLevel, 1, 100);
            if (level < 1) level = 1;
            if (level > maxLv) level = maxLv;

            // ����ġ ����
            if (curve != null)
            {
                int need = curve.GetNeedExpForNextLevel(level);
                if (need == int.MaxValue) currentExp = 0; // ����
                else currentExp = Mathf.Clamp(currentExp, 0, Mathf.Max(0, need - 1));
            }
            else
            {
                // ��� ���ٸ� ������ ����
                currentExp = Mathf.Max(0, currentExp);
            }

            // ģ�е� ����
            if (friendship < 0) friendship = 0;
        }

        // �� ��ü ���� ����
        // - ��/��/����/�̷�ġ ���θ� �޾� �ʱ� ���¸� �����
        public static PokemonSaveData CreateNew(int uid, SpeciesSO species, string formKey, bool isShiny, Gender? fixedGender = null)
        {
            var p = new PokemonSaveData();
            p.uid = uid;
            p.speciesId = (species != null) ? species.speciesId : 0;
            p.formKey = string.IsNullOrWhiteSpace(formKey) ? "Default" : formKey;
            p.isShiny = isShiny;

            p.level = 1;
            p.currentExp = 0;
            p.friendship = 0;

            // ���� ����
            if (fixedGender.HasValue)
                p.gender = fixedGender.Value;
            else
                p.gender = (species != null) ? RollGender(species.genderPolicy) : Gender.Genderless;

            return p;
        }
    }
}
