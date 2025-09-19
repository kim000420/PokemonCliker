// ����: Scripts/UI/PartyUIController.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

namespace PokeClicker
{
    /// <summary>
    /// ��Ƽ UI�� �����ϴ� ��Ʈ�ѷ�.
    /// ��Ƽ�� �ִ� ���ϸ� 6������ �����ܰ� ������ ǥ���ϰ�,
    /// ���� ��ü ����� ����մϴ�.
    /// </summary>
    public class PartyUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private List<PartyPokemonSlot> slots = new List<PartyPokemonSlot>();
        [SerializeField] private Button closeButton;

        [Header("Dependencies")]
        [SerializeField] private OwnedPokemonManager ownedPokemonManager;
        [SerializeField] private SpeciesDB speciesDB;

        private int? _selectedSlotIndex = null;

        private void OnEnable()
        {
            if (ownedPokemonManager != null)
            {
                UpdatePartyUI();
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClick);
            }
        }

        private void OnDisable()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClick);
            }
        }

        /// <summary>
        /// ��Ƽ UI�� �ݱ� ��ư Ŭ�� �� ȣ��˴ϴ�.
        /// </summary>
        private void OnCloseButtonClick()
        {
            // �� ��ư�� ExpandedUIController�� ����� ���̹Ƿ�,
            // ���⼭�� ExpandedUIController�� ShowMenuPanel()�� ȣ���մϴ�.
            var expandedController = GetComponentInParent<ExpandedUIController>();
            if (expandedController != null)
            {
                expandedController.ShowMenuPanel();
            }
        }

        /// <summary>
        /// ��Ƽ UI�� ��� ������ �����մϴ�.
        /// </summary>
        public void UpdatePartyUI()
        {
            if (ownedPokemonManager == null || speciesDB == null) return;

            var party = ownedPokemonManager.Party;
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (i < party.Count)
                {
                    var p = ownedPokemonManager.GetByPuid(party[i]);
                    if (p != null)
                    {
                        var species = speciesDB.GetSpecies(p.speciesId);
                        if (species != null)
                        {
                            var form = species.GetForm(p.formKey);
                            if (form != null && form.visual != null)
                            {
                                // �����ܰ� ���� ǥ��
                                slot.SetData(p, form);
                                slot.iconButton.onClick.RemoveAllListeners();
                                int slotIndex = i; // Ŭ���� �̽� ����
                                slot.iconButton.onClick.AddListener(() => OnPokemonSlotClick(slotIndex));
                            }
                        }
                    }
                }
                else
                {
                    // �� ���� ó��
                    slot.Clear();
                }
            }
        }

        /// <summary>
        /// ���ϸ� ���� Ŭ�� �� ȣ��˴ϴ�.
        /// </summary>
        private void OnPokemonSlotClick(int slotIndex)
        {
            // ���õ� ������ �̹� ������ ��ü
            if (_selectedSlotIndex.HasValue)
            {
                int oldIndex = _selectedSlotIndex.Value;
                if (oldIndex != slotIndex)
                {
                    // ��Ƽ ���ϸ� ���� ��ü
                    SwapPartyPokemon(oldIndex, slotIndex);
                    // ���� ���� �ʱ�ȭ
                    _selectedSlotIndex = null;
                }
                else
                {
                    // ���� ������ �ٽ� Ŭ���ϸ� ���� ����
                    _selectedSlotIndex = null;
                }
            }
            else
            {
                // ù ��° ���� ����
                _selectedSlotIndex = slotIndex;
            }
        }

        /// <summary>
        /// �� ���ϸ��� ������ ��ü�մϴ�.
        /// </summary>
        private void SwapPartyPokemon(int indexA, int indexB)
        {
            if (ownedPokemonManager == null) return;

            var party = ownedPokemonManager.Party.ToList();
            if (indexA < 0 || indexA >= party.Count || indexB < 0 || indexB >= party.Count)
            {
                return;
            }

            // ���� ��ü ����
            var puidA = party[indexA];
            var puidB = party[indexB];
            ownedPokemonManager.Swap(puidA, puidB);

            UpdatePartyUI(); // UI ����
        }
    }

    // UI ������ ���� ������ Ȧ�� Ŭ����
    [System.Serializable]
    public class PartyPokemonSlot
    {
        public Button iconButton;
        public Image iconImage;
        public TextMeshProUGUI levelText;

        public void SetData(PokemonSaveData p, FormSO form)
        {
            iconButton.gameObject.SetActive(true);
            iconImage.sprite = p.isShiny ? form.visual.shinyIcon : form.visual.icon;
            levelText.text = $"Lv.{p.level}";
        }

        public void Clear()
        {
            iconButton.gameObject.SetActive(false);
        }
    }
}