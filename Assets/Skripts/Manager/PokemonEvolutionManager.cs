// 파일: Scripts/Managers/PokemonEvolutionManager.cs
using System;
using System.Collections.Generic;

namespace PokeClicker
{
    /// <summary>
    /// 진화 판정/적용 매니저.
    /// - 규칙 후보 수집 → 조건 검사 → 우선순위 최고 규칙 적용
    /// - 적용 후 종/폼만 갱신(성별/성격/IVs/이로치 등은 유지)
    /// - 계산/표시는 소비자가 StatService를 호출
    /// </summary>
    public class PokemonEvolutionManager
    {
        public event Action<int /*uid*/, int /*fromSpecies*/, string /*fromForm*/,
                            int /*toSpecies*/, string /*toForm*/, EvolutionRuleSO /*rule*/> OnEvolved;

        /// <summary>
        /// 한 번의 진화 시도. 성공 시 종/폼을 변경하고 이벤트 방출.
        /// 반환: 적용된 규칙 (없으면 null)
        /// </summary>
        public EvolutionRuleSO TryEvolveOnce(
            PokemonSaveData p,
            SpeciesSO currentSpecies,
            IEnumerable<EvolutionRuleSO> allRules,
            IGameTime time,
            IInventory inv)
        {
            var applied = EvolutionService.TryEvolveOnce(
                p, currentSpecies, allRules, time, inv,
                (pp, rule) =>
                {
                    OnEvolved?.Invoke(
                        pp.P_uid,
                        currentSpecies != null ? currentSpecies.speciesId : pp.speciesId,
                        rule != null ? rule.fromFormKey : pp.formKey,
                        rule != null && rule.toSpecies != null ? rule.toSpecies.speciesId : pp.speciesId,
                        rule != null ? rule.toFormKey : pp.formKey,
                        rule
                    );
                });

            return applied;
        }
    }
}
