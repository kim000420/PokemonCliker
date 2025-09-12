// 파일: Scripts/Services/ExperienceCurveService.cs
using System;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 경험치 곡선 6종(Erratic/Fast/MediumFast/MediumSlow/Slow/Fluctuating)의
    /// "총 누적 EXP(레벨 n에 도달하기 위한 합계)"와 "다음 레벨까지 필요 EXP" 계산기.
    /// - 레벨 범위: 1..100 (100 이상은 만렙으로 취급)
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
                    // 주: Gen1~2의 L1 음수 문제는 Gen3+에서 테이블로 해결되지만 여기선 공식 그대로 사용
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

        // 다음 레벨까지 남은 경험치 반환 메서드
        public static int GetNeedExpForNextLevel(ExperienceCurve curve, int level)
        {
            if (level >= MaxLevel) return int.MaxValue;
            int a = GetTotalExp(curve, level);
            int b = GetTotalExp(curve, level + 1);
            return Math.Max(0, b - a);
        }

        // 누적 EXP로 역추적하여 현재 레벨 반환 메서드
        public static int GetLevelForTotalExp(ExperienceCurve curve, int totalExp)
        {
            totalExp = Math.Max(0, totalExp);
            // 이진 탐색 (1..100)
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

        // ─────────────────────────────────────────────────────────────────────
        // Erratic, Fluctuating
        // ─────────────────────────────────────────────────────────────────────

        private static int ErraticTotal(int level)
        {
            // n^3 * (100 - n)/50,              for n < 50
            // n^3 * (150 - n)/100,             for 50 ≤ n < 68
            // n^3 * floor((1911 - 10n)/3) /500,for 68 ≤ n < 98
            // n^3 * (160 - n)/100,             for 98 ≤ n
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
            // n^3 * ( n + 14 ) / 50,              for 15 ≤ n < 36
            // n^3 * ( floor(n/2) + 32 ) / 50,     for 36 ≤ n < 100
            // (일부 레벨에서 floor 위치 때문에 (n-1)/2 형태가 보정되는 논의가 있으나,
            // 총 누적 EXP 테이블과 일치하도록 floor를 포함해 구현) :contentReference[oaicite:2]{index=2}
            double n = level;
            if (level < 15)
                return (int)Math.Floor(Math.Pow(n, 3) * (Math.Floor((n + 1.0) / 3.0) + 24.0) / 50.0);
            if (level < 36)
                return (int)Math.Floor(Math.Pow(n, 3) * (n + 14.0) / 50.0);
            return (int)Math.Floor(Math.Pow(n, 3) * (Math.Floor(n / 2.0) + 32.0) / 50.0);
        }
    }
}
