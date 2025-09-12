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
    [Serializable]
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
    [Serializable]
    public struct DerivedStats
    {
        public int hp, atk, def, spa, spd, spe;
    }

    // 포켓몬 타입 열거형
    // secondary 가 없는 경우에는 None 으로 표기한다
    public enum TypeEnum
    {
        None = 0,
        Normal, Fire, Water, Grass, Electric, Ice,
        Fighting, Poison, Ground, Flying, Psychic,
        Bug, Rock, Ghost, Dragon, Dark, Steel, Fairy
    }

    // 성별 열거형
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
        public int maleRate0to100;
    }

    // 듀얼 타입 지원을 위한 타입 쌍 구조체
    // primary 는 반드시 None 이 아닐 것을 권장한다
    // secondary 가 primary 와 같을 경우 자동으로 None 으로 정규화한다
    [System.Serializable]
    public struct TypePair
    {
        public TypeEnum primary;
        public TypeEnum secondary;

        // 생성 시 규칙을 강제 적용한다
        public static TypePair Create(TypeEnum primary, TypeEnum secondary = TypeEnum.None)
        {
            var t = new TypePair { primary = primary, secondary = secondary };
            t.Normalize();
            return t;
        }

        // 정규화 규칙
        // 1 primary 가 None 이면 secondary 도 None 으로 만든다
        // 2 secondary 가 primary 와 같으면 secondary 를 None 으로 바꾼다
        public void Normalize()
        {
            if (primary == TypeEnum.None)
            {
                secondary = TypeEnum.None;
                return;
            }
            if (secondary == primary)
            {
                secondary = TypeEnum.None;
            }
        }

        // 타입 개수 반환 1 또는 2
        public int Count()
        {
            return (primary != TypeEnum.None) && (secondary != TypeEnum.None) ? 2 : (primary != TypeEnum.None ? 1 : 0);
        }

        // 특정 타입을 포함하는지 검사
        public bool Has(TypeEnum t)
        {
            if (t == TypeEnum.None) return false;
            return primary == t || secondary == t;
        }

        // 배열로 반환 항상 길이 1 또는 2 또는 0
        public TypeEnum[] ToArray()
        {
            if (primary == TypeEnum.None) return System.Array.Empty<TypeEnum>();
            if (secondary == TypeEnum.None) return new[] { primary };
            return new[] { primary, secondary };
        }

        // secondary 가 없으면 None 을 돌려준다
        public void Get(out TypeEnum t1, out TypeEnum t2)
        {
            t1 = primary;
            t2 = (secondary == TypeEnum.None) ? TypeEnum.None : secondary;
        }
    }

    // 경험치 곡선 식별자
    // 실제 계산 로직은 ExperienceCurveSO 에서 제공한다
    public enum ExperienceCurve
    {
        Erratic, Fast, MediumFast, MediumSlow, Slow, Fluctuating
    }
}
