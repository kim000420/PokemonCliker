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
        /// �� ������ �����ϴ� ��ųʸ�
        /// </summary>
        private Dictionary<int, SpeciesSO> _dict;

        // Start() ��� �ܺο��� ��������� �ʱ�ȭ�� ȣ���ϵ��� public �޼��� ����
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
            // ������ ������ ���� MonoBehaviour�� �ƴϴ��� �ʱ�ȭ�ǵ��� Awake()������ ȣ��
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

        /// <summary>��Ÿ�� Ǯ�� ��͵� ���� ������ ���͸��ؼ� ���� ����</summary>
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