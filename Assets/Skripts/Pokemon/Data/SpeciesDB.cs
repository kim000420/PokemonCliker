using System;
using System.Collections.Generic;
using UnityEngine;

namespace PokeClicker
{
    public class SpeciesDB : MonoBehaviour
    {
        public SpeciesSO[] allSpecies;

        private Dictionary<int, SpeciesSO> _dict;

        void Awake()
        {
            _dict = new Dictionary<int, SpeciesSO>();
            foreach (var s in allSpecies)
            {
                if (s != null && !_dict.ContainsKey(s.speciesId))
                    _dict[s.speciesId] = s;
            }
        }

        public SpeciesSO GetSpecies(int speciesId)
        {
            _dict.TryGetValue(speciesId, out var s);
            return s;
        }       
        public SpeciesSO Get(int speciesId)
        {
            _dict.TryGetValue(speciesId, out var s);
            return s;
        }
    }

}