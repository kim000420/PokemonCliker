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
    [Serializable]
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
    [Serializable]
    public struct DerivedStats
    {
        public int hp, atk, def, spa, spd, spe;
    }

    // ���ϸ� Ÿ�� ������
    // secondary �� ���� ��쿡�� None ���� ǥ���Ѵ�
    public enum TypeEnum
    {
        None = 0,
        Normal, Fire, Water, Grass, Electric, Ice,
        Fighting, Poison, Ground, Flying, Psychic,
        Bug, Rock, Ghost, Dragon, Dark, Steel, Fairy
    }

    // ���� ������
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
        public int maleRate0to100;
    }

    // ��� Ÿ�� ������ ���� Ÿ�� �� ����ü
    // primary �� �ݵ�� None �� �ƴ� ���� �����Ѵ�
    // secondary �� primary �� ���� ��� �ڵ����� None ���� ����ȭ�Ѵ�
    [System.Serializable]
    public struct TypePair
    {
        public TypeEnum primary;
        public TypeEnum secondary;

        // ���� �� ��Ģ�� ���� �����Ѵ�
        public static TypePair Create(TypeEnum primary, TypeEnum secondary = TypeEnum.None)
        {
            var t = new TypePair { primary = primary, secondary = secondary };
            t.Normalize();
            return t;
        }

        // ����ȭ ��Ģ
        // 1 primary �� None �̸� secondary �� None ���� �����
        // 2 secondary �� primary �� ������ secondary �� None ���� �ٲ۴�
        public void Normalize()
        {
            if (primary == TypeEnum.None)
            {
                secondary = TypeEnum.None;
                return;
            }
            if (secondary == primary)
            {
                secondary = TypeEnum.None;
            }
        }

        // Ÿ�� ���� ��ȯ 1 �Ǵ� 2
        public int Count()
        {
            return (primary != TypeEnum.None) && (secondary != TypeEnum.None) ? 2 : (primary != TypeEnum.None ? 1 : 0);
        }

        // Ư�� Ÿ���� �����ϴ��� �˻�
        public bool Has(TypeEnum t)
        {
            if (t == TypeEnum.None) return false;
            return primary == t || secondary == t;
        }

        // �迭�� ��ȯ �׻� ���� 1 �Ǵ� 2 �Ǵ� 0
        public TypeEnum[] ToArray()
        {
            if (primary == TypeEnum.None) return System.Array.Empty<TypeEnum>();
            if (secondary == TypeEnum.None) return new[] { primary };
            return new[] { primary, secondary };
        }

        // secondary �� ������ None �� �����ش�
        public void Get(out TypeEnum t1, out TypeEnum t2)
        {
            t1 = primary;
            t2 = (secondary == TypeEnum.None) ? TypeEnum.None : secondary;
        }
    }

    // ����ġ � �ĺ���
    // ���� ��� ������ ExperienceCurveSO ���� �����Ѵ�
    public enum ExperienceCurve
    {
        Erratic, Fast, MediumFast, MediumSlow, Slow, Fluctuating
    }
}
