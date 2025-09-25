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

        public PartyExpDistributor(
            OwnedPokemonManager owned,
            PokemonLevelupManager levelup,
            Func<int, SpeciesSO> getSpeciesById)
        {
            _owned = owned;
            _levelup = levelup;
            _getSpeciesById = getSpeciesById;
        }

        /// <summary>
        /// ��Ƽ �������� expGain��ŭ �ο�. (���� ������ ����)
        /// </summary>
        public void GiveExpToParty(int expGainPerInput)
        {
            if (expGainPerInput <= 0) return;

            var party = _owned.GetParty();
            for (int i = 0; i < party.Length; i++)
            {
                if (party[i] == 0) continue;

                var p = _owned.GetByPuid(party[i]);
                if (p == null) continue;

                var species = _getSpeciesById(p.speciesId);
                if (species == null) continue;

                _levelup.AddExpAndHandleLevelUps(p, species, species.curveType, expGainPerInput);
            }
        }
    }
}
