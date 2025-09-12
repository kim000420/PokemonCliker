using System;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 경험치 추가와 레벨업 루프를 담당한다.
    /// - PokemonSaveData.level 과 currentExp 를 갱신한다.
    /// - SpeciesSO.maxLevel 을 넘지 않도록 보정한다.
    /// - 레벨업마다 onLevelUp 콜백을 호출한다.
    /// </summary>
    public static class ExpService
    {
        /// <summary>
        /// 경험치를 추가하고 필요하면 연속 레벨업을 처리한다.
        /// 반환값: 최종적으로 누적된 레벨업 횟수
        /// </summary>
        public static int AddExpAndHandleLevelUps(
            PokemonSaveData p,
            SpeciesSO species,
            ExperienceCurve curve,
            int gainedExp,
            Action<int> onLevelUp = null // newLevel 전달
        )
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            if (species == null) throw new ArgumentNullException(nameof(species));

            // 안전 보정
            int maxLv = Mathf.Clamp(species.maxLevel, 1, 100); // 1~100 (SpeciesSO 규칙)
            if (p.level < 1) p.level = 1;
            if (p.level > maxLv) p.level = maxLv;
            if (gainedExp < 0) gainedExp = 0;

            // 만렙이면 경험치는 의미 없음
            if (p.level >= maxLv)
            {
                p.currentExp = 0; // 관례적으로 0 고정
                return 0;
            }

            // 경험치 추가
            p.currentExp += gainedExp;

            int levelUps = 0;

            // while 루프: 현재 레벨에서 필요 EXP를 채우면 레벨업
            while (p.level < maxLv)
            {
                int need = ExperienceCurveService.GetNeedExpForNextLevel(species.ExpCurve, p.level); // 다음 레벨까지 필요 EXP
                if (need == int.MaxValue) // 안전 처리
                {
                    p.currentExp = 0;
                    break;
                }

                if (p.currentExp >= need)
                {
                    p.currentExp -= need;
                    p.level += 1;
                    levelUps += 1;

                    onLevelUp?.Invoke(p.level);

                    // 만렙 도달 시 잔여 경험치는 소거
                    if (p.level >= maxLv)
                    {
                        p.level = maxLv;
                        p.currentExp = 0;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            // 현재 레벨에서의 currentExp 상한 보정
            if (p.level >= maxLv) p.currentExp = 0;
            else
            {
                int need = ExperienceCurveService.GetNeedExpForNextLevel(species.ExpCurve, p.level);
                if (need != int.MaxValue)
                    p.currentExp = Mathf.Clamp(p.currentExp, 0, Math.Max(0, need - 1));
            }

            return levelUps;
        }
    }
}
