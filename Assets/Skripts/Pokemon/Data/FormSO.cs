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
        [Header("Form ID")]
        public int formId;

        [Header("From Key")]
        public string formKey = "Default";   // SpeciesSO.NormalizeFormKey 로 정규화

        [Tooltip("세대")] public int generation = 1; // 폼의 세대

        [Header("Typing")]
        public TypePair typePair;            // 듀얼 타입(정규화 내장)

        [Header("Base Stats (폼별)")]
        public StatBlock baseStats;          // 종족값
        
        [Header("Visuals (SO)")]
        public PokemonVisualSO visual;       // 아이콘과 애니메이션 에셋을 참조
    }
}
