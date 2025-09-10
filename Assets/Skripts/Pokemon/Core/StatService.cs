using UnityEngine;

namespace PokeClicker
{
    // 개체값(IV) 구조체: 메인 시리즈 기준 0~31
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

        // 범위 보정(로드 후 또는 외부 입력 시 사용)
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

    // 계산 결과(실측치) 묶음
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

    // 실측치 계산 전용 서비스
    // - 전투가 없더라도 수집/정렬/표시를 위해 실측치를 사용할 수 있다
    // - 공식(HP, 기타)은 메인 시리즈 기준(노력치 EV 미사용 버전)
    public static class StatService
    {
        // 외부에서 가장 많이 사용할 진입점(종 SO + 세이브데이터 기반)
        // - species.baseStats 와 p.level 을 사용하여 실측치를 계산한다
        // - ivs/nature 는 외부에서 전달(프로젝트 정책에 따라 세이브에 저장하거나 옵션으로 비활성화)
        public static DerivedStats ComputeFor(PokemonSaveData p, SpeciesSO species, IVs ivs, NatureId nature,
                                              bool useIV = true, bool useNature = true)
        {
            int level = (p != null) ? Mathf.Clamp(p.level, 1, (species != null ? species.maxLevel : 100)) : 1;
            var baseStats = (species != null) ? species.baseStats : default;

            return Compute(baseStats, level, ivs, nature, useIV, useNature);
        }

        // 핵심 계산 함수
        // - baseStats: 종족값(H,A,B,C,D,S)
        // - level: 1~100
        // - ivs: 0~31 (useIV=false 이면 0으로 취급)
        // - nature: 성격 (useNature=false 이면 배율 1.0)
        public static DerivedStats Compute(StatBlock baseStats, int level, IVs ivs, NatureId nature,
                                           bool useIV = true, bool useNature = true)
        {
            level = Mathf.Clamp(level, 1, 100);

            // IV 사용 옵션 처리
            if (!useIV) ivs = IVs.Zero;
            else ivs.Clamp();

            // 성격 배율 조회(HP 제외)
            NatureMultiplier mult = useNature ? NatureTable.Get(nature) : new NatureMultiplier(1f, 1f, 1f, 1f, 1f);

            DerivedStats r;

            // HP 공식:
            // HP = ((2*Base + IV) * Level / 100) + Level + 10
            r.hp = CalcHP(baseStats.hp, ivs.hp, level);

            // 기타 스탯 공식:
            // Stat = ( ((2*Base + IV) * Level / 100) + 5 ) × 성격배율
            r.atk = CalcOther(baseStats.atk, ivs.atk, level, mult.atk);
            r.def = CalcOther(baseStats.def, ivs.def, level, mult.def);
            r.spa = CalcOther(baseStats.spa, ivs.spa, level, mult.spa);
            r.spd = CalcOther(baseStats.spd, ivs.spd, level, mult.spd);
            r.spe = CalcOther(baseStats.spe, ivs.spe, level, mult.spe);

            return r;
        }

        // HP 계산(내부 보조)
        private static int CalcHP(int baseStat, int iv, int level)
        {
            // (2*Base + IV) * Level / 100 의 중간 결과는 실수 연산 후 버림을 가정
            float core = ((2f * baseStat + iv) * level) / 100f;
            int value = Mathf.FloorToInt(core) + level + 10;
            return Mathf.Max(1, value); // 안전 보정
        }

        // 기타 스탯 계산(내부 보조)
        private static int CalcOther(int baseStat, int iv, int level, float natureMul)
        {
            float core = ((2f * baseStat + iv) * level) / 100f;
            int pre = Mathf.FloorToInt(core) + 5;
            float withNature = pre * natureMul;

            // 메인 시리즈는 소수점 버림
            int value = Mathf.FloorToInt(withNature);
            return Mathf.Max(1, value); // 안전 보정
        }
    }
}
