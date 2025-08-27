using System.Collections.Generic;
using UnityEngine;

namespace PokeClicker
{
    // 하나의 "진화 규칙"을 정의한다.
    // 예) from: 킬리아(Default) -> to: 가디안(Default), 조건: 레벨 >= 30
    // 예) from: 킬리아(Default) -> to: 엘레이드(Default), 조건: 남성 && 가진도구 == 각성의돌
    [CreateAssetMenu(menuName = "PokeClicker/DB/EvolutionRule")]
    public class EvolutionRuleSO : ScriptableObject
    {
        [Header("진화 시작점")]
        public SpeciesSO fromSpecies;         // 현재 종
        public string fromFormKey = "Default"; // 현재 폼

        [Header("진화 결과")]
        public SpeciesSO toSpecies;           // 진화 후 종
        public string toFormKey = "Default";  // 진화 후 폼

        [Header("우선순위 및 조건")]
        public int priority = 0;                     // 다수 규칙이 동시에 만족될 때 큰 값 우선
        public List<EvoConditionSO> conditions = new List<EvoConditionSO>(); // 필요 조건 모두 충족해야 진화

        // 현재 개체가 이 규칙의 "대상"인지 확인 (종/폼 일치 여부)
        public bool IsApplicable(PokemonSaveData p)
        {
            if (fromSpecies == null) return false;
            if (p == null) return false;
            if (p.speciesId != fromSpecies.speciesId) return false;

            // formKey가 정확히 일치해야 같은 폼으로 간주한다
            // 빈 값일 경우 "Default"로 처리
            string key = string.IsNullOrWhiteSpace(fromFormKey) ? "Default" : fromFormKey;
            return p.formKey == key;
        }

        // 모든 조건을 검사하여 진화 가능한지 확인
        public bool CheckAll(PokemonSaveData p, IGameTime time, IInventory inv)
        {
            if (conditions == null || conditions.Count == 0) return true; // 조건이 없으면 항상 가능
            for (int i = 0; i < conditions.Count; i++)
            {
                var c = conditions[i];
                if (c == null) continue; // null 조건은 무시
                if (!c.IsSatisfied(p, time, inv)) return false;
            }
            return true;
        }

        // 인스펙터에서 값이 바뀔 때 간단한 정리
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(fromFormKey)) fromFormKey = "Default";
            if (string.IsNullOrWhiteSpace(toFormKey)) toFormKey = "Default";
        }

        // 규칙 자체의 유효성 점검(필요 시 호출)
        public bool IsValid(out string reason)
        {
            if (fromSpecies == null)
            {
                reason = "fromSpecies가 비어 있습니다.";
                return false;
            }
            if (toSpecies == null)
            {
                reason = "toSpecies가 비어 있습니다.";
                return false;
            }
            reason = null;
            return true;
        }
    }
}
