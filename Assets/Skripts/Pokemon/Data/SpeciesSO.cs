using System.Collections.Generic;
using UnityEngine;

namespace PokeClicker
{
    // 포켓몬 "종" 단위의 불변 데이터
    // 개체(세이브 데이터)에는 종 ID만 저장하고, 표시/계산 시 이 SO를 참조한다.
    [CreateAssetMenu(menuName = "PokeClicker/DB/Species")]
    public class SpeciesSO : ScriptableObject
    {
        [Header("기본 식별 정보")]
        public int speciesId = 0;         // 도감번호(고유값)
        public string nameKey = "";       // 현지화 키. 별명이 없을 때 표시 이름으로 사용

        [Header("성장/레벨 설정")]
        public ExperienceCurve expCurveId = ExperienceCurve.MediumFast; // 경험치 곡선 식별자
        [Range(1, 100)] public int maxLevel = 100;                      // 최대 레벨(일반적으로 100)

        [Header("성별/정렬 지표(선택)")]
        public GenderPolicy genderPolicy;  // 성별 정책(무성이면 hasGender=false)
        public StatBlock baseStats;        // 전투가 없어도 수집가치/정렬용으로 활용 가능

        [Header("이 종이 보유한 폼 목록(선택)")]
        public List<FormSO> forms = new List<FormSO>(); // 에디터 편의용(직접 드래그해서 연결)

        // 인스펙터에서 값이 바뀔 때 자동 정리
        private void OnValidate()
        {
            // speciesId는 1 이상을 권장
            if (speciesId < 1) speciesId = 1;

            // maxLevel은 1~100 범위로 고정
            if (maxLevel < 1) maxLevel = 1;
            if (maxLevel > 100) maxLevel = 100;

            // 폼 리스트에서 null 항목 제거 및 중복 제거
            if (forms != null)
            {
                // null 제거
                forms.RemoveAll(f => f == null);

                // 같은 FormSO가 중복으로 들어가지 않도록 정리
                var seen = new HashSet<FormSO>();
                for (int i = forms.Count - 1; i >= 0; i--)
                {
                    var f = forms[i];
                    if (seen.Contains(f))
                        forms.RemoveAt(i);
                    else
                        seen.Add(f);
                }

                // 폼이 연결되어 있다면 species 참조가 역으로 이 종을 가리키도록 맞춘다(편의 보정)
                for (int i = 0; i < forms.Count; i++)
                {
                    if (forms[i] != null && forms[i].species != this)
                    {
                        forms[i].species = this;
                    }
                }
            }
        }

        // 유효성 검사를 코드에서 명시적으로 호출하고 싶을 때 사용
        public bool IsValid(out string reason)
        {
            if (speciesId < 1)
            {
                reason = "speciesId는 1 이상이어야 합니다.";
                return false;
            }
            if (string.IsNullOrEmpty(nameKey))
            {
                reason = "nameKey가 비어 있습니다. 현지화 키를 지정하세요.";
                return false;
            }
            reason = null;
            return true;
        }
    }
}
