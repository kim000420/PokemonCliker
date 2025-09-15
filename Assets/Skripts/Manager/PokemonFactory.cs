// 파일: Scripts/Managers/PokemonFactory.cs
using System;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 포획/지급 전용 팩토리.
    /// - 종/폼/레벨과 옵션(성격/이로치/성별/IVs)을 기준으로 PokemonSaveData를 생성한다.
    /// - 계산/표시는 여기서 하지 않는다(StatService를 사용하는 쪽에서 호출).
    /// </summary>
    public static class PokemonFactory
    {
        [Serializable]
        public struct Options
        {
            public bool rollNature;              // 성격 랜덤 적용 여부 (기본: true)
            public bool rollShiny;               // 이로치 판정 적용 여부 (기본: true)
            public int shinyOneOver;             // 이로치 확률 분모(1/x). 0 또는 음수면 4096으로 간주
            public bool rollGender;              // 성별 정책 적용 여부 (기본: true)
            public bool rollIVs;                 // IVs 랜덤 굴림 여부 (기본: false ? 전투 없으면 OFF)
            public DateTime obtainedAt;          // 획득 시각 (default이면 Now)
        }

        public static PokemonSaveData Create(
            SpeciesSO species,
            string formKey,
            int level,
            Options? optNullable = null
        )
        {
            if (species == null) throw new ArgumentNullException(nameof(species));
            var opt = optNullable ?? new Options
            {
                rollNature = true,
                rollShiny = true,
                shinyOneOver = 4096,
                rollGender = true,
                rollIVs = false,
                obtainedAt = default
            };

            // 기본값 보정
            if (string.IsNullOrWhiteSpace(formKey)) formKey = "Default";
            level = Mathf.Clamp(level, 1, Mathf.Clamp(species.maxLevel, 1, 100));

            // 필수 필드 채우기
            var p = new PokemonSaveData
            {
                P_uid = 0, // 소유 매니저가 부여
                speciesId = species.speciesId,
                formKey = formKey,
                isShiny = opt.rollShiny ? RandomService.RollShiny(opt.shinyOneOver > 0 ? opt.shinyOneOver : 4096) : false,
                level = level,
                currentExp = 0,
                nickname = null,
                gender = opt.rollGender ? RandomService.RollGender(species.genderPolicy) : Gender.Genderless,
                friendship = 0,
                heldItemId = null,
                nature = opt.rollNature ? NatureTable.PickRandom() : NatureId.Hardy,
                ivs = opt.rollIVs ? RandomService.RollIVsAll() : default,
                obtainedAt = opt.obtainedAt == default ? DateTime.Now : opt.obtainedAt,
                metLevel = level,
                metFormKey = formKey
            };

            // 저장 안전 보정(상한/하한)
            p.EnsureValidAfterLoad(species, species.curveType);

            return p;
        }
    }
}
