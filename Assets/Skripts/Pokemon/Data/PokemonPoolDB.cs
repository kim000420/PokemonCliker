// ����: Scripts/Data/PokemonPoolDB.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PokemonPoolDB", menuName = "PokeClicker/Pokemon Pool DB")]
public class PokemonPoolDB : ScriptableObject
{
    public List<PoolMapping> pools = new List<PoolMapping>();

    [System.Serializable]
    public struct PoolMapping
    {
        public string poolName;
        public PokemonPoolSO pool;
    }

    // �̸����� Pool�� ���� ã�� ���� ��ųʸ� (��Ÿ�ӿ�)
    private Dictionary<string, PokemonPoolSO> _poolDictionary;

    public void Initialize()
    {
        _poolDictionary = new Dictionary<string, PokemonPoolSO>();
        foreach (var mapping in pools)
        {
            _poolDictionary[mapping.poolName] = mapping.pool;
        }
    }

    public PokemonPoolSO GetPool(string poolName)
    {
        _poolDictionary.TryGetValue(poolName, out var pool);
        return pool;
    }
}