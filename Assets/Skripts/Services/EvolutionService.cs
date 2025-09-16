using System;
using System.Collections.Generic;

namespace PokeClicker
{
    /// <summary>
    /// ��ȭ ������ ������ ����Ѵ�.
    /// - ��/�� ��ȯ�� �����Ѵ�.
    /// - ����, ����, IVs, �̷�ġ ���� �����Ѵ�.
    /// </summary>
    public static class EvolutionService
    {
        /// <summary>
        /// ������ ��Ģ �� �켱������ ���� ���� �ϳ��� �����Ѵ�.
        /// ��ȯ��: ����� ��Ģ. ������ null
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

            // �ĺ� ����
            EvolutionRuleSO top = null;
            int topPriority = int.MinValue;

            foreach (var rule in allRules)
            {
                if (rule == null) continue;
                if (!rule.IsApplicable(p)) continue;               // ��/�� ��ġ (from) Ȯ��
                if (!rule.CheckAll(p, time, inv)) continue;        // ��� ���� ����

                if (rule.priority > topPriority)
                {
                    top = rule;
                    topPriority = rule.priority;
                }
            }

            if (top == null) return null;

            // ����: ��, ���� ����
            var fromSpeciesId = p.speciesId;
            var fromFormKey = p.formKey;

            p.speciesId = top.toSpecies != null ? top.toSpecies.speciesId : p.speciesId;
            p.formKey = string.IsNullOrWhiteSpace(top.toFormKey) ? "Default" : top.toFormKey;

            // �����丮 ���� �ʵ�� ��å�� ���� ����
            // p.metFormKey �� ���� ���� ���� �ǹ��ϹǷ� �״�� ����

            onEvolved?.Invoke(p, top);
            return top;
        }
    }
}
