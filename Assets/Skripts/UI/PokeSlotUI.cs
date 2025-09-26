// ����: Scripts/UI/PokeSlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // IPointerClickHandler ����� ���� �߰�
using TMPro;

namespace PokeClicker
{
    public class PokeSlotUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI Components")]
        [SerializeField] private Image selectBox;
        [SerializeField] private Image iconImage;
        // �� ������ ������
        public int? Puid { get; private set; }
        public int BoxIndex { get; private set; }
        public int SlotIndex { get; private set; }

        // Ŭ�� �̺�Ʈ�� ��Ʈ�ѷ��� �����ϱ� ���� Action
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

        // ������ Ŭ���Ǿ��� �� ȣ��� �޼���
        public void OnPointerClick(PointerEventData eventData)
        {
            OnSlotClicked?.Invoke(this, eventData.button);
        }
    }
}