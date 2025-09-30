// ÆÄÀÏ: Scripts/Data/PokemonPoolSO.cs
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PoolEntry
{
    public int speciesId;
    public string formKey;
}

[CreateAssetMenu(fileName = "NewPokemonPool", menuName = "PokeClicker/Pokemon Pool")]
public class PokemonPoolSO : ScriptableObject
{
    public List<PoolEntry> entries = new List<PoolEntry>();
}