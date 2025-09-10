// ����: Scripts/Managers/PokemonEvolutionManager.cs
using System;
using System.Collections.Generic;

namespace PokeClicker
{
    /// <summary>
    /// ��ȭ ����/���� �Ŵ���.
    /// - ��Ģ �ĺ� ���� �� ���� �˻� �� �켱���� �ְ� ��Ģ ����
    /// - ���� �� ��/���� ����(����/����/IVs/�̷�ġ ���� ����)
    /// - ���/ǥ�ô� �Һ��ڰ� StatService�� ȣ��
    /// </summary>
    public class PokemonEvolutionManager
    {
        public event Action<int /*uid*/, int /*fromSpecies*/, string /*fromForm*/,
                            int /*toSpecies*/, string /*toForm*/, EvolutionRuleSO /*rule*/> OnEvolved;

        /// <summary>
        /// �� ���� ��ȭ �õ�. ���� �� ��/���� �����ϰ� �̺�Ʈ ����.
        /// ��ȯ: ����� ��Ģ (������ null)
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
