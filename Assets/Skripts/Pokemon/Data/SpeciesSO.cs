using System.Collections.Generic;
using UnityEngine;

namespace PokeClicker
{
    // 포켓몬 "종" 단위의 불변 데이터
    [CreateAssetMenu(menuName = "PokeClicker/DB/Species")]
    public class SpeciesSO : ScriptableObject
    {
        [Header("기본 식별 정보")]
        public int speciesId = 0;          // 도감번호(고유값)
        public string nameKey = "";        // 현지화 키
        [Range(1, 9)] public int generation = 1; // 포켓몬 세대 (1~9)

        [Header("성장/레벨 설정")]
        public ExperienceCurve expCurveId = ExperienceCurve.MediumFast;
        [Range(1, 100)] public int maxLevel = 100;

        [Header("성별/능력치")]
        public GenderPolicy genderPolicy;  // 성별 정책
        public StatBlock baseStats;        // H,A,B,C,D,S
        [SerializeField, HideInInspector] private int totalStats; // 합계 (자동 계산됨)

        [Header("이 종이 보유한 폼 목록(선택)")]
        public List<FormSO> forms = new List<FormSO>();

        // totalStats 값을 읽기 전용으로 제공
        public int TotalStats => totalStats;

        private void OnValidate()
        {
            // speciesId 보정
            if (speciesId < 1) speciesId = 1;

            // maxLevel 보정
            if (maxLevel < 1) maxLevel = 1;
            if (maxLevel > 100) maxLevel = 100;

            // 총합 스탯 계산
            totalStats = baseStats.hp + baseStats.atk + baseStats.def +
                         baseStats.spa + baseStats.spd + baseStats.spe;

            // 폼 리스트 정리
            if (forms != null)
            {
                var seen = new HashSet<FormSO>();
                for (int i = 0; i < forms.Count; i++)
                {
                    var f = forms[i];
                    if (f == null) continue;
                    if (seen.Contains(f))
                        forms[i] = null; // 중복은 null로 바꿔서 슬롯 유지
                    else
                        seen.Add(f);
                }

                // 역참조 보정: null이 아닌 항목만 species 역링크 정리
                for (int i = 0; i < forms.Count; i++)
                {
                    var f = forms[i];
                    if (f != null && f.species != this)
                        f.species = this;
                }
            }
        }

        // 외부에서 유효성 검사 호출 가능
        public bool IsValid(out string reason)
        {
            if (speciesId < 1)
            {
                reason = "speciesId는 1 이상이어야 합니다.";
                return false;
            }
            if (string.IsNullOrEmpty(nameKey))
            {
                reason = "nameKey가 비어 있습니다.";
                return false;
            }
            reason = null;
            return true;
        }
    }
}
