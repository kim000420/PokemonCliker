using System;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 포획/지급 시 사용되는 난수 유틸리티.
    /// - 이로치 판정
    /// - 성별 결정
    /// - IVs 굴림
    /// 기본은 UnityEngine.Random 을 사용하고, 테스트 재현을 위해 내부 시드 RNG 로 전환 가능하다.
    /// </summary>
    public static class RandomService
    {
        // 내부 시드 RNG. null 이면 UnityEngine.Random 사용
        private static System.Random _rng;

        /// <summary>
        /// 테스트 재현을 위한 시드 고정
        /// </summary>
        public static void UseSeed(int seed)
        {
            _rng = new System.Random(seed);
        }

        /// <summary>
        /// 기본 RNG 로 되돌린다.
        /// </summary>
        public static void UseUnityRandom()
        {
            _rng = null;
        }

        /// <summary>
        /// 이로치 판정. 기본 1/4096
        /// </summary>
        public static bool RollShiny()
        {
            int r = NextInt(4096); // 0~4095 
            return r == 0;
        }

        /// <summary>
        /// 성별 결정. hasGender = false 이면 Genderless 반환.
        /// hasGender = true 이면 maleRate0to100 확률로 Male, 아니면 Female.
        /// </summary>
        public static Gender RollGender(GenderPolicy policy)
        {
            if (!policy.hasGender) return Gender.Genderless;

            int r = NextInt(100); // 0..99
            return (r < Mathf.Clamp(policy.maleRate0to100, 0, 100)) ? Gender.Male : Gender.Female;
        }

        /// <summary>
        /// 단일 스탯의 IV 굴림. 0..31
        /// </summary>
        public static int RollIV()
        {
            return NextInt(32); // 0..31
        }


        // 무작위 성격 하나 뽑기 
        public static NatureId RollNature()
        {
            int idx = NextInt(25);
            return (NatureId)idx;
        }

        /// <summary>
        /// 6개 스탯 IVs 전부 굴림.
        /// </summary>
        public static IVs RollIVsAll()
        {
            return new IVs
            {
                hp = RollIV(),
                atk = RollIV(),
                def = RollIV(),
                spa = RollIV(),
                spd = RollIV(),
                spe = RollIV()
            };
        }

        /// <summary>
        /// 내부 공통 난수 추출: 0..maxExclusive-1
        /// </summary>
        public static int NextInt(int maxExclusive)
        {
            if (_rng != null)
                return _rng.Next(0, maxExclusive);
            else
                return UnityEngine.Random.Range(0, maxExclusive);
        }
    }
}
