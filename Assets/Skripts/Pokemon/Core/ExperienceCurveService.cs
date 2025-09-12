// ����: Scripts/Services/ExperienceCurveService.cs
using System;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// ����ġ � 6��(Erratic/Fast/MediumFast/MediumSlow/Slow/Fluctuating)��
    /// "�� ���� EXP(���� n�� �����ϱ� ���� �հ�)"�� "���� �������� �ʿ� EXP" ����.
    /// - ���� ����: 1..100 (100 �̻��� �������� ���)
    /// </summary>
    public static class ExperienceCurveService
    {
        public const int MaxLevel = 100;

        public static int GetTotalExp(ExperienceCurve curve, int level)
        {
            level = Mathf.Clamp(level, 1, MaxLevel);

            switch (curve)
            {
                case ExperienceCurve.Fast:
                    // 4/5 * n^3
                    return (int)Math.Floor(4f * Math.Pow(level, 3) / 5f);

                case ExperienceCurve.MediumFast:
                    // n^3
                    return (int)Math.Pow(level, 3);

                case ExperienceCurve.MediumSlow:
                    // (6/5)n^3 - 15n^2 + 100n - 140
                    // ��: Gen1~2�� L1 ���� ������ Gen3+���� ���̺�� �ذ������ ���⼱ ���� �״�� ���
                    double n = level;
                    return (int)Math.Floor(1.2 * n * n * n - 15.0 * n * n + 100.0 * n - 140.0);

                case ExperienceCurve.Slow:
                    // 5/4 * n^3
                    return (int)Math.Floor(5f * Math.Pow(level, 3) / 4f);

                case ExperienceCurve.Erratic:
                    return ErraticTotal(level);

                case ExperienceCurve.Fluctuating:
                    return FluctuatingTotal(level);

                default:
                    throw new ArgumentOutOfRangeException(nameof(curve), curve, null);
            }
        }

        // ���� �������� ���� ����ġ ��ȯ �޼���
        public static int GetNeedExpForNextLevel(ExperienceCurve curve, int level)
        {
            if (level >= MaxLevel) return int.MaxValue;
            int a = GetTotalExp(curve, level);
            int b = GetTotalExp(curve, level + 1);
            return Math.Max(0, b - a);
        }

        // ���� EXP�� �������Ͽ� ���� ���� ��ȯ �޼���
        public static int GetLevelForTotalExp(ExperienceCurve curve, int totalExp)
        {
            totalExp = Math.Max(0, totalExp);
            // ���� Ž�� (1..100)
            int lo = 1, hi = MaxLevel, ans = 1;
            while (lo <= hi)
            {
                int mid = (lo + hi) >> 1;
                int t = GetTotalExp(curve, mid);
                if (t <= totalExp)
                {
                    ans = mid;
                    lo = mid + 1;
                }
                else hi = mid - 1;
            }
            return ans;
        }

        // ������������������������������������������������������������������������������������������������������������������������������������������
        // Erratic, Fluctuating
        // ������������������������������������������������������������������������������������������������������������������������������������������

        private static int ErraticTotal(int level)
        {
            // n^3 * (100 - n)/50,              for n < 50
            // n^3 * (150 - n)/100,             for 50 �� n < 68
            // n^3 * floor((1911 - 10n)/3) /500,for 68 �� n < 98
            // n^3 * (160 - n)/100,             for 98 �� n
            double n = level;
            if (level < 50)
                return (int)Math.Floor(Math.Pow(n, 3) * (100.0 - n) / 50.0);
            if (level < 68)
                return (int)Math.Floor(Math.Pow(n, 3) * (150.0 - n) / 100.0);
            if (level < 98)
                return (int)Math.Floor(Math.Pow(n, 3) * Math.Floor((1911.0 - 10.0 * n) / 3.0) / 500.0);
            return (int)Math.Floor(Math.Pow(n, 3) * (160.0 - n) / 100.0);
        }

        private static int FluctuatingTotal(int level)
        {
            // n^3 * ( floor((n+1)/3) + 24 ) / 50, for n < 15
            // n^3 * ( n + 14 ) / 50,              for 15 �� n < 36
            // n^3 * ( floor(n/2) + 32 ) / 50,     for 36 �� n < 100
            // (�Ϻ� �������� floor ��ġ ������ (n-1)/2 ���°� �����Ǵ� ���ǰ� ������,
            // �� ���� EXP ���̺�� ��ġ�ϵ��� floor�� ������ ����) :contentReference[oaicite:2]{index=2}
            double n = level;
            if (level < 15)
                return (int)Math.Floor(Math.Pow(n, 3) * (Math.Floor((n + 1.0) / 3.0) + 24.0) / 50.0);
            if (level < 36)
                return (int)Math.Floor(Math.Pow(n, 3) * (n + 14.0) / 50.0);
            return (int)Math.Floor(Math.Pow(n, 3) * (Math.Floor(n / 2.0) + 32.0) / 50.0);
        }
    }
}
