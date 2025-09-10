using System;
using System.Collections.Generic;

namespace PokeClicker
{
    /// <summary>
    /// 진화 판정과 적용을 담당한다.
    /// - 종/폼 전환만 수행한다.
    /// - 성별, 성격, IVs, 이로치 등은 유지한다.
    /// </summary>
    public static class EvolutionService
    {
        /// <summary>
        /// 가능한 규칙 중 우선순위가 가장 높은 하나를 적용한다.
        /// 반환값: 적용된 규칙. 없으면 null
        /// </summary>
        public static EvolutionRuleSO TryEvolveOnce(
            PokemonSaveData p,
            SpeciesSO currentSpecies,
            IEnumerable<EvolutionRuleSO> allRules,
            IGameTime time,
            IInventory inv,
            Action<PokemonSaveData, EvolutionRuleSO> onEvolved = null
        )
        {
            if (p == null || currentSpecies == null || allRules == null) return null;

            // 후보 수집
            EvolutionRuleSO top = null;
            int topPriority = int.MinValue;

            foreach (var rule in allRules)
            {
                if (rule == null) continue;
                if (!rule.IsApplicable(p)) continue;               // 종/폼 일치 (from) 확인
                if (!rule.CheckAll(p, time, inv)) continue;        // 모든 조건 충족

                if (rule.priority > topPriority)
                {
                    top = rule;
                    topPriority = rule.priority;
                }
            }

            if (top == null) return null;

            // 적용: 종, 폼만 변경
            var fromSpeciesId = p.speciesId;
            var fromFormKey = p.formKey;

            p.speciesId = top.toSpecies != null ? top.toSpecies.speciesId : p.speciesId;
            p.formKey = string.IsNullOrWhiteSpace(top.toFormKey) ? "Default" : top.toFormKey;

            // 히스토리 관련 필드는 정책에 따라 유지
            // p.metFormKey 는 최초 만남 폼을 의미하므로 그대로 유지

            onEvolved?.Invoke(p, top);
            return top;
        }
    }
}
