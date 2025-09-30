// 파일: Scripts/Data/PokeballInventory.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PokeballInventory : ISerializationCallbackReceiver
{
    // 실제 데이터는 Dictionary에 저장하여 사용하기 편리하게 만듭니다.
    private Dictionary<string, int> _ballCounts = new Dictionary<string, int>();

    // JsonUtility는 Dictionary를 직접 저장하지 못하므로, List 두 개로 변환하여 저장합니다.
    [SerializeField] private List<string> keys = new List<string>();
    [SerializeField] private List<int> values = new List<int>();

    // 볼 개수를 가져오는 메서드
    public int GetBallCount(string ballId)
    {
        _ballCounts.TryGetValue(ballId, out int count);
        return count;
    }

    // 볼 개수를 변경하는 메서드 (더하거나 뺄 때 사용)
    public void AddBallCount(string ballId, int amount)
    {
        if (!_ballCounts.TryAdd(ballId, amount))
        {
            _ballCounts[ballId] += amount;
        }
    }

    // 저장 직전에 호출됨: Dictionary -> List 변환
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (var pair in _ballCounts)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    // 불러온 직후에 호출됨: List -> Dictionary 변환
    public void OnAfterDeserialize()
    {
        _ballCounts = new Dictionary<string, int>();
        for (int i = 0; i < keys.Count; i++)
        {
            _ballCounts[keys[i]] = values[i];
        }
    }
}

// 몬스터볼 ID를 상수로 관리하여 오타를 방지합니다.
public static class BallId
{
    public const string PokeBall = "poke_ball";
    public const string PremierBall = "premier_ball";
    public const string HyperBall = "hyper_ball";
    public const string MasterBall = "master_ball";
}