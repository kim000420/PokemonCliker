using System;
using UnityEngine;

namespace PokeClicker
{
    // 포켓몬 종족값
    [System.Serializable]
    public struct StatBlock
    {
        public int hp;
        public int atk;
        public int def;
        public int spa;
        public int spd;
        public int spe;
    }

    // 포켓몬 개체값
    [System.Serializable]
    public struct IVs
    {
        public int hp, atk, def, spa, spd, spe;
        public void Clamp()
        {
            hp = Mathf.Clamp(hp, 0, 31);
            atk = Mathf.Clamp(atk, 0, 31);
            def = Mathf.Clamp(def, 0, 31);
            spa = Mathf.Clamp(spa, 0, 31);
            spd = Mathf.Clamp(spd, 0, 31);
            spe = Mathf.Clamp(spe, 0, 31);
        }
    }

    // 포켓몬 스텟 실측값
    [System.Serializable]
    public struct DerivedStats
    {
        public int hp, atk, def, spa, spd, spe;
    }

    // 포켓몬 타입 열거형
    // secondary 가 없는 경우에는 None 으로 표기한다
    [System.Serializable]
    public enum TypeEnum
    {
        None = 0,
        Normal, Fire, Water, Grass, Electric, Ice,
        Fighting, Poison, Ground, Flying, Psychic,
        Bug, Rock, Ghost, Dragon, Dark, Steel, Fairy
    }

    // 성격 열거형 25종
    public enum NatureId
    {
        Hardy, Lonely, Brave, Adamant, Naughty,
        Bold, Docile, Relaxed, Impish, Lax,
        Timid, Hasty, Serious, Jolly, Naive,
        Modest, Mild, Quiet, Bashful, Rash,
        Calm, Gentle, Sassy, Careful, Quirky
    }

    // 성별 열거형
    [System.Serializable]
    public enum Gender
    {
        Male,
        Female,
        Genderless
    }

    // 종 단위 성별 정책
    // hasGender 가 false 면 성별이 없는 종으로 간주한다
    // hasGender 가 true 면 maleRate0to100 의 확률로 남성이 생성된다
    [System.Serializable]
    public struct GenderPolicy
    {
        public bool hasGender;
        [Range(0, 100)]
        public float maleRate0to100;
    }

    // 듀얼 타입 지원을 위한 타입 쌍 구조체
    // primary 는 반드시 None 이 아닐 것을 권장한다
    // secondary 가 primary 와 같을 경우 자동으로 None 으로 정규화한다
    [System.Serializable]
    public struct TypePair
    {
        public bool hasDualType;
        public TypeEnum primary;
        public TypeEnum secondary;
    }

    // 경험치 곡선 식별자
    // 실제 계산 로직은 ExperienceCurveSO 에서 제공한다
    [System.Serializable]
    public enum ExperienceCurve
    {
        Erratic, Fast, MediumFast, MediumSlow, Slow, Fluctuating
    }
    
    // 희귀도 카테고리 (일반, 전설, 준전설, 환상종 등..)
    [System.Serializable]
    public enum RarityCategory
    {
        Ordinary, Legendary, SemiLegendary, Mythical
    }

    /// <summary>스타터 풀 범위</summary>
    public enum StarterTier
    {
        NormalOnly,
        Include_SemiLegendary,
        Include_Legendary,
        Include_Mythical
    }

    // 알그룹
    //
}
