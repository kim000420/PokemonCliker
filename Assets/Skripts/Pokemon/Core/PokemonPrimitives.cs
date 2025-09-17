using System;
using UnityEngine;

namespace PokeClicker
{
    // ���ϸ� ������
    [System.Serializable]
    public struct StatBlock
    {
        public int hp;
        public int atk;
        public int def;
        public int spa;
        public int spd;
        public int spe;
    }

    // ���ϸ� ��ü��
    [System.Serializable]
    public struct IVs
    {
        public int hp, atk, def, spa, spd, spe;
        public void Clamp()
        {
            hp = Mathf.Clamp(hp, 0, 31);
            atk = Mathf.Clamp(atk, 0, 31);
            def = Mathf.Clamp(def, 0, 31);
            spa = Mathf.Clamp(spa, 0, 31);
            spd = Mathf.Clamp(spd, 0, 31);
            spe = Mathf.Clamp(spe, 0, 31);
        }
    }

    // ���ϸ� ���� ������
    [System.Serializable]
    public struct DerivedStats
    {
        public int hp, atk, def, spa, spd, spe;
    }

    // ���ϸ� Ÿ�� ������
    // secondary �� ���� ��쿡�� None ���� ǥ���Ѵ�
    [System.Serializable]
    public enum TypeEnum
    {
        None = 0,
        Normal, Fire, Water, Grass, Electric, Ice,
        Fighting, Poison, Ground, Flying, Psychic,
        Bug, Rock, Ghost, Dragon, Dark, Steel, Fairy
    }

    // ���� ������ 25��
    public enum NatureId
    {
        Hardy, Lonely, Brave, Adamant, Naughty,
        Bold, Docile, Relaxed, Impish, Lax,
        Timid, Hasty, Serious, Jolly, Naive,
        Modest, Mild, Quiet, Bashful, Rash,
        Calm, Gentle, Sassy, Careful, Quirky
    }

    // ���� ������
    [System.Serializable]
    public enum Gender
    {
        Male,
        Female,
        Genderless
    }

    // �� ���� ���� ��å
    // hasGender �� false �� ������ ���� ������ �����Ѵ�
    // hasGender �� true �� maleRate0to100 �� Ȯ���� ������ �����ȴ�
    [System.Serializable]
    public struct GenderPolicy
    {
        public bool hasGender;
        [Range(0, 100)]
        public float maleRate0to100;
    }

    // ��� Ÿ�� ������ ���� Ÿ�� �� ����ü
    // primary �� �ݵ�� None �� �ƴ� ���� �����Ѵ�
    // secondary �� primary �� ���� ��� �ڵ����� None ���� ����ȭ�Ѵ�
    [System.Serializable]
    public struct TypePair
    {
        public bool hasDualType;
        public TypeEnum primary;
        public TypeEnum secondary;
    }

    // ����ġ � �ĺ���
    // ���� ��� ������ ExperienceCurveSO ���� �����Ѵ�
    [System.Serializable]
    public enum ExperienceCurve
    {
        Erratic, Fast, MediumFast, MediumSlow, Slow, Fluctuating
    }
    
    // ��͵� ī�װ� (�Ϲ�, ����, ������, ȯ���� ��..)
    [System.Serializable]
    public enum RarityCategory
    {
        Ordinary, Legendary, SemiLegendary, Mythical
    }

    /// <summary>��Ÿ�� Ǯ ����</summary>
    public enum StarterTier
    {
        NormalOnly,
        Include_SemiLegendary,
        Include_Legendary,
        Include_Mythical
    }

    // �˱׷�
    //
}
