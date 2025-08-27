using System.Collections.Generic;
using UnityEngine;

namespace PokeClicker
{
    // �ϳ��� "��ȭ ��Ģ"�� �����Ѵ�.
    // ��) from: ų����(Default) -> to: �����(Default), ����: ���� >= 30
    // ��) from: ų����(Default) -> to: �����̵�(Default), ����: ���� && �������� == �����ǵ�
    [CreateAssetMenu(menuName = "PokeClicker/DB/EvolutionRule")]
    public class EvolutionRuleSO : ScriptableObject
    {
        [Header("��ȭ ������")]
        public SpeciesSO fromSpecies;         // ���� ��
        public string fromFormKey = "Default"; // ���� ��

        [Header("��ȭ ���")]
        public SpeciesSO toSpecies;           // ��ȭ �� ��
        public string toFormKey = "Default";  // ��ȭ �� ��

        [Header("�켱���� �� ����")]
        public int priority = 0;                     // �ټ� ��Ģ�� ���ÿ� ������ �� ū �� �켱
        public List<EvoConditionSO> conditions = new List<EvoConditionSO>(); // �ʿ� ���� ��� �����ؾ� ��ȭ

        // ���� ��ü�� �� ��Ģ�� "���"���� Ȯ�� (��/�� ��ġ ����)
        public bool IsApplicable(PokemonSaveData p)
        {
            if (fromSpecies == null) return false;
            if (p == null) return false;
            if (p.speciesId != fromSpecies.speciesId) return false;

            // formKey�� ��Ȯ�� ��ġ�ؾ� ���� ������ �����Ѵ�
            // �� ���� ��� "Default"�� ó��
            string key = string.IsNullOrWhiteSpace(fromFormKey) ? "Default" : fromFormKey;
            return p.formKey == key;
        }

        // ��� ������ �˻��Ͽ� ��ȭ �������� Ȯ��
        public bool CheckAll(PokemonSaveData p, IGameTime time, IInventory inv)
        {
            if (conditions == null || conditions.Count == 0) return true; // ������ ������ �׻� ����
            for (int i = 0; i < conditions.Count; i++)
            {
                var c = conditions[i];
                if (c == null) continue; // null ������ ����
                if (!c.IsSatisfied(p, time, inv)) return false;
            }
            return true;
        }

        // �ν����Ϳ��� ���� �ٲ� �� ������ ����
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(fromFormKey)) fromFormKey = "Default";
            if (string.IsNullOrWhiteSpace(toFormKey)) toFormKey = "Default";
        }

        // ��Ģ ��ü�� ��ȿ�� ����(�ʿ� �� ȣ��)
        public bool IsValid(out string reason)
        {
            if (fromSpecies == null)
            {
                reason = "fromSpecies�� ��� �ֽ��ϴ�.";
                return false;
            }
            if (toSpecies == null)
            {
                reason = "toSpecies�� ��� �ֽ��ϴ�.";
                return false;
            }
            reason = null;
            return true;
        }
    }
}
