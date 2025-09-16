using System;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// ��ȹ/���� �� ���Ǵ� ���� ��ƿ��Ƽ.
    /// - �̷�ġ ����
    /// - ���� ����
    /// - IVs ����
    /// �⺻�� UnityEngine.Random �� ����ϰ�, �׽�Ʈ ������ ���� ���� �õ� RNG �� ��ȯ �����ϴ�.
    /// </summary>
    public static class RandomService
    {
        // ���� �õ� RNG. null �̸� UnityEngine.Random ���
        private static System.Random _rng;

        /// <summary>
        /// �׽�Ʈ ������ ���� �õ� ����
        /// </summary>
        public static void UseSeed(int seed)
        {
            _rng = new System.Random(seed);
        }

        /// <summary>
        /// �⺻ RNG �� �ǵ�����.
        /// </summary>
        public static void UseUnityRandom()
        {
            _rng = null;
        }

        /// <summary>
        /// �̷�ġ ����. �⺻ 1/4096
        /// overrideOneOver ������ Ȯ���� ��� �� �ִ�. (�� 512 �̸� 1/512)
        /// </summary>
        public static bool RollShiny(int overrideOneOver = 4096)
        {
            int denom = Mathf.Max(1, overrideOneOver);
            int r = NextInt(denom); // 0..denom-1
            return r == 0;
        }

        /// <summary>
        /// ���� ����. hasGender = false �̸� Genderless ��ȯ.
        /// hasGender = true �̸� maleRate0to100 Ȯ���� Male, �ƴϸ� Female.
        /// </summary>
        public static Gender RollGender(GenderPolicy policy)
        {
            if (!policy.hasGender) return Gender.Genderless;

            int r = NextInt(100); // 0..99
            return (r < Mathf.Clamp(policy.maleRate0to100, 0, 100)) ? Gender.Male : Gender.Female;
        }

        /// <summary>
        /// ���� ������ IV ����. 0..31
        /// </summary>
        public static int RollIV()
        {
            return NextInt(32); // 0..31
        }


        // ������ ���� �ϳ� �̱� 
        public static NatureId RollNature()
        {
            int idx = NextInt(24);
            return (NatureId)idx;
        }

        /// <summary>
        /// 6�� ���� IVs ���� ����.
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
        /// ���� ���� ���� ����: 0..maxExclusive-1
        /// </summary>
        private static int NextInt(int maxExclusive)
        {
            if (_rng != null)
                return _rng.Next(0, maxExclusive);
            else
                return UnityEngine.Random.Range(0, maxExclusive);
        }
    }
}
