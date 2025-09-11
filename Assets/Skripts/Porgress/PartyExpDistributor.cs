using System;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// �Է� 1ȸ�� ��Ƽ �������� EXP�� �й��ϴ� ���ɽ�Ʈ������.
    /// ����� ExpService�� �����ϰ�, ������ �̺�Ʈ�� PokemonLevelupManager�� ���� �����Ѵ�.
    /// </summary>
    public class PartyExpDistributor
    {
        private readonly OwnedPokemonManager _owned;
        private readonly PokemonLevelupManager _levelup;
        private readonly Func<int, SpeciesSO> _getSpeciesById;          // speciesId -> SpeciesSO
        private readonly Func<SpeciesSO, ExperienceCurveSO> _getCurve;  // �� -> ����ġ � SO

        public PartyExpDistributor(
            OwnedPokemonManager owned,
            PokemonLevelupManager levelup,
            Func<int, SpeciesSO> getSpeciesById,
            Func<SpeciesSO, ExperienceCurveSO> getCurve)
        {
            _owned = owned;
            _levelup = levelup;
            _getSpeciesById = getSpeciesById;
            _getCurve = _getCurve;
        }

        /// <summary>
        /// ��Ƽ �������� expGain��ŭ �ο�. (���� ������ ����)
        /// </summary>
        public void GiveExpToParty(int expGainPerInput)
        {
            if (expGainPerInput <= 0) return;

            var party = _owned.Party;
            for (int i = 0; i < party.Count; i++)
            {
                var p = _owned.GetByPuid(party[i]);
                if (p == null) continue;

                var species = _getSpeciesById(p.speciesId);
                if (species == null) continue;

                var curve = _getCurve(species);
                if (curve == null) continue;

                _levelup.AddExpAndHandleLevelUps(p, species, curve, expGainPerInput);
            }
        }
    }
}
