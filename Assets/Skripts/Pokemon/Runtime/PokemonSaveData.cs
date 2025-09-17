using System;
using UnityEngine;

namespace PokeClicker
{
    // �÷��̾ �����ϴ� "���ϸ� ��ü"�� ���� ���� ������
    // - ����(���� ����), ���(����ġ), ��ȭ ������ �ܺ� ���񽺿��� ����
    // - �� Ŭ������ ���¸� ������ �Ѵ�
    [Serializable]
    public class PokemonSaveData
    {
        // �ĺ�
        public int P_uid;                  // �ܺ�(�������丮 ��)���� �ο��ϴ� ���� ID
        public int speciesId;              // �� ID (SpeciesSO.speciesId)
        public string formKey = "Default"; // ���� �� Ű ("Default","Alola"...)
        public bool isShiny;               // �̷�ġ ����

        // ����
        public int level = 1;              // ���� ����
        public int currentExp = 0;         // ���� ���� �� ����ġ(����)

        // ����/����
        public string nickname;            // ����(��� ������ �� �̸� ǥ��)
        public Gender gender = Gender.Male;
        public int friendship = 0;         // 0~255 ����
        public string heldItemId;          // ���� ������ ID (����)

        // ���(�ɼ�) �ý���: ����/��ü��
        public NatureId nature = NatureId.Hardy; // ����(25��), �߸� �⺻
        public IVs ivs;                           // ��ü��(0~31), ������� ������ 0 ����

        // ��Ÿ������(ǥ��/����/��ó ������ ����)
        public DateTime obtainedAt;        // ȹ�� �ð�(���� �ð�)
        public int metLevel = 1;           // ó�� ���� ����
        public string metFormKey = "Default"; // ó�� ���� ��

        // ǥ�ÿ� �̸�(���� �켱, ������ �� �̸� Ű)
        public string GetDisplayName(SpeciesSO species)
        {
            if (!string.IsNullOrWhiteSpace(nickname))
                return nickname;
            return species != null ? species.nameKeyEng : "Unknown";
        }

        // �ε� �� �� ���� ����
        public void EnsureValidAfterLoad(SpeciesSO species)
        {
            if (species != null)
            {
                int maxLv = Mathf.Clamp(species.maxLevel, 1, 100);
                if (level < 1) level = 1;
                if (level > maxLv) level = maxLv;

                int need = ExperienceCurveService.GetNeedExpForNextLevel(species.curveType, level);
                if (need == int.MaxValue) currentExp = 0;            // ����
                else currentExp = Mathf.Clamp(currentExp, 0, Math.Max(0, need - 1));
                
            }
            else
            {
                // species ������ �� �ּ� ����
                level = Mathf.Max(1, level);
                currentExp = Mathf.Max(0, currentExp);
            }

            // ģ�е�/IV ����
            if (friendship < 0) friendship = 0;
            ivs.Clamp();
            if (string.IsNullOrWhiteSpace(formKey)) formKey = "Default";
            if (string.IsNullOrWhiteSpace(metFormKey)) metFormKey = formKey;

            // obtainedAt �⺻�� ����
            if (obtainedAt == default) obtainedAt = DateTime.Now;
        }
    }
}
