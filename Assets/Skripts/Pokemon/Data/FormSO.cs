using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 한 종의 "폼"에 따라 달라지는 가변 정보.
    /// - 타입(듀얼), 종족값(BaseStats) 등
    /// - 아이콘/애니메이션/치수/특성 등도 폼에 귀속된다면 여기에 추가
    /// </summary>
    public class FormSO : ScriptableObject
    {

        [Tooltip("세대")] public int generation = 1; // 폼의 세대

        [Header("Key")]
        public string formKey = "Default";   // SpeciesSO.NormalizeFormKey 로 정규화

        [Header("Typing")]
        public TypePair typePair;            // 듀얼 타입(정규화 내장)

        [Header("Base Stats (폼별)")]
        public StatBlock baseStats;          // 종족값
    }
}
