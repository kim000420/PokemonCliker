using System;
using UnityEngine;

namespace PokeClicker
{
    // 플레이어가 보유하는 "포켓몬 개체"의 순수 데이터
    // 저장/로드 대상이며, 표시나 계산 시엔 SpeciesSO, FormSO 등을 참조한다.
    [Serializable]
    public class PokemonSaveData
    {
        // 식별
        public int uid;                 // 개체 고유 ID (세이브 내부에서 유일)
        public int speciesId;           // 종 ID (SpeciesSO.speciesId)
        public string formKey = "Default"; // 폼 키 ("Default", "Alola" 등)
        public bool isShiny;            // 이로치 여부

        // 이름/성장
        public string nickname;         // 별명 (비어 있으면 종 이름을 사용)
        public int level = 1;           // 현재 레벨
        public int currentExp = 0;      // 현재 레벨 내에서의 경험치(증분)

        // 상태
        public Gender gender = Gender.Male;
        public int friendship = 0;      // 친밀도 (0~255 권장)
        public string heldItemId;       // 소지 아이템 ID (선택)

        // 표시용 이름 반환 (별명이 있으면 별명, 없으면 종 이름 키)
        public string GetDisplayName(SpeciesSO species)
        {
            if (!string.IsNullOrWhiteSpace(nickname))
                return nickname;
            return species != null ? species.nameKey : "Unknown";
        }

        // 성별 정책에 따라 성별을 굴린다
        public static Gender RollGender(GenderPolicy policy)
        {
            if (!policy.hasGender) return Gender.Genderless;

            int r = UnityEngine.Random.Range(0, 100); // 0~99
            return (r < policy.maleRate0to100) ? Gender.Male : Gender.Female;
        }

        // 로드 직후나 수동 수정 후에 값 범위를 정리한다
        // - 레벨: 1~종의 최대 레벨
        // - 경험치: 현재 레벨에서 필요한 요구치 미만으로 클램프
        // - 친밀도: 0 이상
        public void EnsureValidAfterLoad(SpeciesSO species, ExperienceCurveSO curve)
        {
            if (species == null) return;

            // 레벨 범위 보정
            int maxLv = Mathf.Clamp(species.maxLevel, 1, 100);
            if (level < 1) level = 1;
            if (level > maxLv) level = maxLv;

            // 경험치 보정
            if (curve != null)
            {
                int need = curve.GetNeedExpForNextLevel(level);
                if (need == int.MaxValue) currentExp = 0; // 만렙
                else currentExp = Mathf.Clamp(currentExp, 0, Mathf.Max(0, need - 1));
            }
            else
            {
                // 곡선이 없다면 음수만 방지
                currentExp = Mathf.Max(0, currentExp);
            }

            // 친밀도 보정
            if (friendship < 0) friendship = 0;
        }

        // 새 개체 생성 헬퍼
        // - 종/폼/성별/이로치 여부를 받아 초기 상태를 만든다
        public static PokemonSaveData CreateNew(int uid, SpeciesSO species, string formKey, bool isShiny, Gender? fixedGender = null)
        {
            var p = new PokemonSaveData();
            p.uid = uid;
            p.speciesId = (species != null) ? species.speciesId : 0;
            p.formKey = string.IsNullOrWhiteSpace(formKey) ? "Default" : formKey;
            p.isShiny = isShiny;

            p.level = 1;
            p.currentExp = 0;
            p.friendship = 0;

            // 성별 결정
            if (fixedGender.HasValue)
                p.gender = fixedGender.Value;
            else
                p.gender = (species != null) ? RollGender(species.genderPolicy) : Gender.Genderless;

            return p;
        }
    }
}
