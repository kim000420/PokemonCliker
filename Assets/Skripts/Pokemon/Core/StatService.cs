using UnityEngine;

namespace PokeClicker
{
    // ��ü��(IV) ����ü: ���� �ø��� ���� 0~31
    [System.Serializable]
    public struct IVs
    {
        [Range(0, 31)] public int hp;
        [Range(0, 31)] public int atk;
        [Range(0, 31)] public int def;
        [Range(0, 31)] public int spa;
        [Range(0, 31)] public int spd;
        [Range(0, 31)] public int spe;

        public static IVs Zero => new IVs { hp = 0, atk = 0, def = 0, spa = 0, spd = 0, spe = 0 };

        // ���� ����(�ε� �� �Ǵ� �ܺ� �Է� �� ���)
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

    // ��� ���(����ġ) ����
    public struct DerivedStats
    {
        public int hp;
        public int atk;
        public int def;
        public int spa;
        public int spd;
        public int spe;

        public int Total() => hp + atk + def + spa + spd + spe;
    }

    // ����ġ ��� ���� ����
    // - ������ ������ ����/����/ǥ�ø� ���� ����ġ�� ����� �� �ִ�
    // - ����(HP, ��Ÿ)�� ���� �ø��� ����(���ġ EV �̻�� ����)
    public static class StatService
    {
        // �ܺο��� ���� ���� ����� ������(�� SO + ���̺굥���� ���)
        // - species.baseStats �� p.level �� ����Ͽ� ����ġ�� ����Ѵ�
        // - ivs/nature �� �ܺο��� ����(������Ʈ ��å�� ���� ���̺꿡 �����ϰų� �ɼ����� ��Ȱ��ȭ)
        public static DerivedStats ComputeFor(PokemonSaveData p, SpeciesSO species, IVs ivs, NatureId nature,
                                              bool useIV = true, bool useNature = true)
        {
            int level = (p != null) ? Mathf.Clamp(p.level, 1, (species != null ? species.maxLevel : 100)) : 1;
            var baseStats = (species != null) ? species.baseStats : default;

            return Compute(baseStats, level, ivs, nature, useIV, useNature);
        }

        // �ٽ� ��� �Լ�
        // - baseStats: ������(H,A,B,C,D,S)
        // - level: 1~100
        // - ivs: 0~31 (useIV=false �̸� 0���� ���)
        // - nature: ���� (useNature=false �̸� ���� 1.0)
        public static DerivedStats Compute(StatBlock baseStats, int level, IVs ivs, NatureId nature,
                                           bool useIV = true, bool useNature = true)
        {
            level = Mathf.Clamp(level, 1, 100);

            // IV ��� �ɼ� ó��
            if (!useIV) ivs = IVs.Zero;
            else ivs.Clamp();

            // ���� ���� ��ȸ(HP ����)
            NatureMultiplier mult = useNature ? NatureTable.Get(nature) : new NatureMultiplier(1f, 1f, 1f, 1f, 1f);

            DerivedStats r;

            // HP ����:
            // HP = ((2*Base + IV) * Level / 100) + Level + 10
            r.hp = CalcHP(baseStats.hp, ivs.hp, level);

            // ��Ÿ ���� ����:
            // Stat = ( ((2*Base + IV) * Level / 100) + 5 ) �� ���ݹ���
            r.atk = CalcOther(baseStats.atk, ivs.atk, level, mult.atk);
            r.def = CalcOther(baseStats.def, ivs.def, level, mult.def);
            r.spa = CalcOther(baseStats.spa, ivs.spa, level, mult.spa);
            r.spd = CalcOther(baseStats.spd, ivs.spd, level, mult.spd);
            r.spe = CalcOther(baseStats.spe, ivs.spe, level, mult.spe);

            return r;
        }

        // HP ���(���� ����)
        private static int CalcHP(int baseStat, int iv, int level)
        {
            // (2*Base + IV) * Level / 100 �� �߰� ����� �Ǽ� ���� �� ������ ����
            float core = ((2f * baseStat + iv) * level) / 100f;
            int value = Mathf.FloorToInt(core) + level + 10;
            return Mathf.Max(1, value); // ���� ����
        }

        // ��Ÿ ���� ���(���� ����)
        private static int CalcOther(int baseStat, int iv, int level, float natureMul)
        {
            float core = ((2f * baseStat + iv) * level) / 100f;
            int pre = Mathf.FloorToInt(core) + 5;
            float withNature = pre * natureMul;

            // ���� �ø���� �Ҽ��� ����
            int value = Mathf.FloorToInt(withNature);
            return Mathf.Max(1, value); // ���� ����
        }
    }
}
