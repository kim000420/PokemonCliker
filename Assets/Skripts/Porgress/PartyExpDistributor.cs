using System;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 입력 1회당 파티 전원에게 EXP를 분배하는 오케스트레이터.
    /// 계산은 ExpService가 수행하고, 레벨업 이벤트는 PokemonLevelupManager를 통해 방출한다.
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
        /// 파티 전원에게 expGain만큼 부여. (없는 슬롯은 무시)
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
