using System;
using UnityEngine;

namespace PokeClicker
{
    // 플레이어가 보유하는 "포켓몬 개체"의 순수 저장 데이터
    // - 생성(랜덤 결정), 계산(실측치), 진화 적용은 외부 서비스에서 수행
    // - 이 클래스는 상태를 보관만 한다
    [Serializable]
    public class PokemonSaveData
    {
        // 식별
        public int P_uid;                  // 외부(리포지토리 등)에서 부여하는 고유 ID
        public int speciesId;              // 종 ID (SpeciesSO.speciesId)
        public string formKey = "Default"; // 현재 폼 키 ("Default","Alola"...)
        public bool isShiny;               // 이로치 여부

        // 성장
        public int level = 1;              // 현재 레벨
        public int currentExp = 0;         // 현재 레벨 내 경험치(증분)

        // 관계/상태
        public string nickname;            // 별명(비어 있으면 종 이름 표시)
        public Gender gender = Gender.Male;
        public int friendship = 0;         // 0~255 권장
        public string heldItemId;          // 소지 아이템 ID (선택)

        // 고급(옵션) 시스템: 성격/개체값
        public NatureId nature = NatureId.Hardy; // 성격(25종), 중립 기본
        public IVs ivs;                           // 개체값(0~31), 사용하지 않으면 0 유지

        // 메타데이터(표시/정렬/출처 추적에 유용)
        public DateTime obtainedAt;        // 획득 시각(현지 시간)
        public int metLevel = 1;           // 처음 만난 레벨
        public string metFormKey = "Default"; // 처음 만난 폼

        // 표시용 이름(별명 우선, 없으면 종 이름 키)
        public string GetDisplayName(SpeciesSO species)
        {
            if (!string.IsNullOrWhiteSpace(nickname))
                return nickname;
            return species != null ? species.nameKeyEng : "Unknown";
        }

        // 로드 후 값 범위 보정
        public void EnsureValidAfterLoad(SpeciesSO species)
        {
            if (species != null)
            {
                int maxLv = Mathf.Clamp(species.maxLevel, 1, 100);
                if (level < 1) level = 1;
                if (level > maxLv) level = maxLv;

                int need = ExperienceCurveService.GetNeedExpForNextLevel(species.curveType, level);
                if (need == int.MaxValue) currentExp = 0;            // 만렙
                else currentExp = Mathf.Clamp(currentExp, 0, Math.Max(0, need - 1));
                
            }
            else
            {
                // species 미지정 시 최소 보정
                level = Mathf.Max(1, level);
                currentExp = Mathf.Max(0, currentExp);
            }

            // 친밀도/IV 보정
            if (friendship < 0) friendship = 0;
            ivs.Clamp();
            if (string.IsNullOrWhiteSpace(formKey)) formKey = "Default";
            if (string.IsNullOrWhiteSpace(metFormKey)) metFormKey = formKey;

            // obtainedAt 기본값 보정
            if (obtainedAt == default) obtainedAt = DateTime.Now;
        }
    }
}
