// 파일: Scripts/UI/PokeSlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // IPointerClickHandler 사용을 위해 추가
using TMPro;

namespace PokeClicker
{
    public class PokeSlotUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI Components")]
        [SerializeField] private Image selectBox;
        [SerializeField] private Image iconImage;
        // 이 슬롯의 데이터
        public int? Puid { get; private set; }
        public int BoxIndex { get; private set; }
        public int SlotIndex { get; private set; }

        // 클릭 이벤트를 컨트롤러에 전달하기 위한 Action
        public System.Action<PokeSlotUI, PointerEventData.InputButton> OnSlotClicked;

        public void Init(int boxIndex, int slotIndex)
        {
            BoxIndex = boxIndex;
            SlotIndex = slotIndex;
        }

        public void SetData(PokemonSaveData p, FormSO form)
        {
            Puid = p.P_uid;
            iconImage.sprite = p.isShiny ? form.visual.shinyIcon : form.visual.icon;
            iconImage.gameObject.SetActive(true);
        }

        public void Clear()
        {
            Puid = null;
            iconImage.gameObject.SetActive(false);
        }

        public void SetSelectBoxActive(bool isActive)
        {
            if (selectBox != null)
            {
                selectBox.gameObject.SetActive(isActive);
            }
        }

        // 슬롯이 클릭되었을 때 호출될 메서드
        public void OnPointerClick(PointerEventData eventData)
        {
            OnSlotClicked?.Invoke(this, eventData.button);
        }
    }
}