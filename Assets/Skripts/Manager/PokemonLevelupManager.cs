// 파일: Scripts/Managers/PokemonLevelupManager.cs
using System;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 레벨업 루프 매니저.
    /// - ExpService를 호출하여 경험치 추가 및 연쇄 레벨업을 처리
    /// - 각 단계에서 OnLevelUp 이벤트 방출(소비자가 StatService 재계산)
    /// </summary>
    public class PokemonLevelupManager : MonoBehaviour
    {
        public event Action<int /*uid*/, int /*newLevel*/> OnLevelUp;
        public event Action<int /*puid*/> OnExpGained;

        /// <summary>
        /// 경험치 추가 → 연쇄 레벨업 처리.
        /// 반환: 총 레벨업 횟수
        /// </summary>
        public int AddExpAndHandleLevelUps(
            PokemonSaveData p,
            SpeciesSO species,
            ExperienceCurve curveType,
            int gainedExp)
        {
            OnExpGained?.Invoke(p.P_uid);

            int cnt = ExpService.AddExpAndHandleLevelUps(
                p, species, curveType, gainedExp,
                newLv => OnLevelUp?.Invoke(p.P_uid, newLv));

            return cnt;
        }

    }
}
