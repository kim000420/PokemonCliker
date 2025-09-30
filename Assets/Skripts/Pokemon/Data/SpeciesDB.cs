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
    }
}