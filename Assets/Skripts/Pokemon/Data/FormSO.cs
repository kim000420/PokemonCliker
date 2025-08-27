using UnityEngine;

namespace PokeClicker
{
    // 한 종(Species) 안에 존재하는 하나의 폼(기본/리전폼 등)을 정의한다.
    // 타입, 아이콘, 프리뷰 애니메이터 등의 외형 정보를 이 단위에서 관리한다.
    [CreateAssetMenu(menuName = "PokeClicker/DB/Form")]
    public class FormSO : ScriptableObject
    {
        [Header("소속 종 / 폼 식별")]
        public SpeciesSO species;           // 어떤 종에 속하는 폼인지
        public string formKey = "Default";  // "Default", "Alola", "Galar", "Hisui", "Paldea" 등

        [Header("타입(듀얼 지원)")]
        public TypePair types = TypePair.Create(TypeEnum.None, TypeEnum.None);

        [System.Serializable]
        public struct VisualSet
        {
            public Sprite icon;                                // 가방/목록에서 쓰는 아이콘
            public RuntimeAnimatorController frontAnimator;    // 프리뷰용 전면 애니메이터
        }

        [Header("표시 리소스 (일반 / 이로치)")]
        public VisualSet normal;
        public VisualSet shiny;

        // 인스펙터에서 값이 바뀔 때 자동 정리
        private void OnValidate()
        {
            // formKey 공백 방지
            if (string.IsNullOrWhiteSpace(formKey))
                formKey = "Default";

            // 타입 입력 정규화 (primary가 None이면 secondary도 None, 중복 제거 등)
            types.Normalize();

            // 종 역참조 보정: 폼이 종 리스트에 없다면 에디터 단계에서 수동으로 맞추길 권장
            // (자동 삽입은 의도치 않은 의존성 변경을 발생시킬 수 있어 여기서는 수행하지 않는다)
        }

        // 현재 폼이 가진 타입 개수(0,1,2)
        public int GetTypeCount()
        {
            return types.Count();
        }

        // 특정 타입을 포함하는지
        public bool HasType(TypeEnum t)
        {
            return types.Has(t);
        }

        // isShiny 여부에 따라 표시용 아이콘을 반환
        public Sprite GetIcon(bool isShiny)
        {
            return isShiny ? shiny.icon : normal.icon;
        }

        // isShiny 여부에 따라 프리뷰 애니메이터를 반환
        public RuntimeAnimatorController GetFrontAnimator(bool isShiny)
        {
            return isShiny ? shiny.frontAnimator : normal.frontAnimator;
        }
    }
}
