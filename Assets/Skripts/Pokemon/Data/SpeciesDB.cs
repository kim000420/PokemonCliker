using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PokeClicker
{
    public class SpeciesDB : MonoBehaviour
    {
        public SpeciesSO[] allSpecies;

        /// <summary>
        /// 종 정보를 저장하는 딕셔너리
        /// </summary>
        private Dictionary<int, SpeciesSO> _dict;

        // Start() 대신 외부에서 명시적으로 초기화를 호출하도록 public 메서드 생성
        public void Initialize()
        {
            _dict = new Dictionary<int, SpeciesSO>();
            foreach (var s in allSpecies)
            {
                if (s != null && !_dict.ContainsKey(s.speciesId))
                    _dict[s.speciesId] = s;
            }
        }

        void Awake()
        {
            // 의존성 주입을 위해 MonoBehaviour가 아니더라도 초기화되도록 Awake()에서도 호출
            Initialize();
        }

        public SpeciesSO GetSpecies(int speciesId)
        {
            _dict.TryGetValue(speciesId, out var s);
            return s;
        }
        public SpeciesSO GetRandom()
        {
            var list = allSpecies?.Where(s => s != null).ToList();
            if (list == null || list.Count == 0) return null;
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        /// <summary>스타터 풀을 희귀도 포함 범위로 필터링해서 랜덤 선택</summary>
        public SpeciesSO GetRandomByStarterTier(StarterTier tier)
        {
            if (allSpecies == null || allSpecies.Length == 0) return null;

            bool Include(RarityCategory r)
            {
                switch (tier)
                {
                    case StarterTier.NormalOnly: return r == RarityCategory.Ordinary;
                    case StarterTier.Include_SemiLegendary: return r == RarityCategory.Ordinary || r == RarityCategory.SemiLegendary;
                    case StarterTier.Include_Legendary: return r == RarityCategory.Ordinary || r == RarityCategory.SemiLegendary || r == RarityCategory.Legendary;
                    case StarterTier.Include_Mythical: return true;
                    default: return r == RarityCategory.Ordinary;
                }
            }

            var pool = new List<SpeciesSO>();
            foreach (var s in allSpecies)
            {
                if (s == null) continue;
                if (Include(s.rarityCategory)) pool.Add(s);
            }
            if (pool.Count == 0) return null;
            return pool[UnityEngine.Random.Range(0, pool.Count)];
        }
    }
}