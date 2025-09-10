// 파일: Scripts/Managers/PokemonLevelupManager.cs
using System;

namespace PokeClicker
{
    /// <summary>
    /// 레벨업 루프 매니저.
    /// - ExpService를 호출하여 경험치 추가 및 연쇄 레벨업을 처리
    /// - 각 단계에서 OnLevelUp 이벤트 방출(소비자가 StatService 재계산)
    /// </summary>
    public class PokemonLevelupManager
    {
        public event Action<int /*uid*/, int /*newLevel*/> OnLevelUp;

        /// <summary>
        /// 경험치 추가 → 연쇄 레벨업 처리.
        /// 반환: 총 레벨업 횟수
        /// </summary>
        public int AddExpAndHandleLevelUps(
            PokemonSaveData p,
            SpeciesSO species,
            ExperienceCurveSO curve,
            int gainedExp)
        {
            int cnt = ExpService.AddExpAndHandleLevelUps(
                p, species, curve, gainedExp,
                newLv => OnLevelUp?.Invoke(p.P_uid, newLv));

            return cnt;
        }
    }
}
