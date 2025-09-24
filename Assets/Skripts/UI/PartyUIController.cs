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
        [SerializeField] private Button exitButton;

        [Header("Dependencies")]
        [SerializeField] private OwnedPokemonManager ownedPokemonManager;
        [SerializeField] private SpeciesDB speciesDB;
        [SerializeField] private GameIconDB gameIconDB;

        private int? _selectedSlotIndex = null;

        private void OnEnable()
        {
            if (ownedPokemonManager != null)
            {
                UpdatePartyUI();
            }

            if (exitButton != null)
            {
                exitButton.onClick.AddListener(OnCloseButtonClick);
            }
        }

        private void OnDisable()
        {
            if (exitButton != null)
            {
                exitButton.onClick.RemoveListener(OnCloseButtonClick);
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
                                slot.SetData(p, form, species, gameIconDB);
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
        public Image pokemonIconImage;
        public Image genderIconImage;
        public Image expBar;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI expText;

        /// <summary>
        /// ���ϸ� �����͸� ���Կ� �����մϴ�.
        /// </summary>
        public void SetData(PokemonSaveData p, FormSO form, SpeciesSO species, GameIconDB gameIconDB)
        {
            // ���� Ȱ��ȭ �� ������ ǥ��
            iconButton.gameObject.SetActive(true);

            // ���ϸ� �̹��� ����
            pokemonIconImage.sprite = p.isShiny ? form.visual.shinyIcon : form.visual.icon;

            // ���� ������ ����
            genderIconImage.sprite = p.gender switch
            {
                Gender.Male => gameIconDB.miscIcons.male,
                Gender.Female => gameIconDB.miscIcons.female,
                _ => gameIconDB.miscIcons.genderless
            };

            // ���� �� �̸� ����
            levelText.text = $"Lv.{p.level}";
            nameText.text = p.GetDisplayName(species);

            // ����ġ ���� ����
            if (p.level < species.maxLevel)
            {
                int needExp = ExperienceCurveService.GetNeedExpForNextLevel(species.curveType, p.level);
                expText.text = $"{p.currentExp} / {needExp}";
                expBar.fillAmount = needExp > 0 ? (float)p.currentExp / needExp : 0;
            }
            else
            {
                expText.text = "MAX";
                expBar.fillAmount = 1;
            }
        }

        /// <summary>
        /// ������ ���ϴ�.
        /// </summary>
        public void Clear()
        {
            iconButton.gameObject.SetActive(false);
            nameText.text = string.Empty;
            levelText.text = string.Empty;
            expText.text = string.Empty;
            expBar.fillAmount = 0;
            genderIconImage.sprite = null;
        }
    }
}