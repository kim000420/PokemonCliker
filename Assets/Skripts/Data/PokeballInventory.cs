// ����: Scripts/Data/PokeballInventory.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PokeballInventory : ISerializationCallbackReceiver
{
    // ���� �����ʹ� Dictionary�� �����Ͽ� ����ϱ� ���ϰ� ����ϴ�.
    private Dictionary<string, int> _ballCounts = new Dictionary<string, int>();

    // JsonUtility�� Dictionary�� ���� �������� ���ϹǷ�, List �� ���� ��ȯ�Ͽ� �����մϴ�.
    [SerializeField] private List<string> keys = new List<string>();
    [SerializeField] private List<int> values = new List<int>();

    // �� ������ �������� �޼���
    public int GetBallCount(string ballId)
    {
        _ballCounts.TryGetValue(ballId, out int count);
        return count;
    }

    // �� ������ �����ϴ� �޼��� (���ϰų� �� �� ���)
    public void AddBallCount(string ballId, int amount)
    {
        if (!_ballCounts.TryAdd(ballId, amount))
        {
            _ballCounts[ballId] += amount;
        }
    }

    // ���� ������ ȣ���: Dictionary -> List ��ȯ
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

    // �ҷ��� ���Ŀ� ȣ���: List -> Dictionary ��ȯ
    public void OnAfterDeserialize()
    {
        _ballCounts = new Dictionary<string, int>();
        for (int i = 0; i < keys.Count; i++)
        {
            _ballCounts[keys[i]] = values[i];
        }
    }
}

// ���ͺ� ID�� ����� �����Ͽ� ��Ÿ�� �����մϴ�.
public static class BallId
{
    public const string PokeBall = "poke_ball";
    public const string PremierBall = "premier_ball";
    public const string HyperBall = "hyper_ball";
    public const string MasterBall = "master_ball";
}