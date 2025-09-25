// ����: Scripts/UI/MenuUIController.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PokeClicker
{
    /// <summary>
    /// Ȯ�� UI ���� �⺻ �޴� �г��� �����ϴ� ��Ʈ�ѷ�.
    /// Ʈ���̳� �̸��� ��Ƽ ���ϸ� ������, ���� �޴� ��ư���� �����մϴ�.
    /// </summary>
    public class MenuUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI trainerNameText;
        [SerializeField] private List<MenuPartySlot> slots = new List<MenuPartySlot>(); 
        [SerializeField] private Button pokePcButton;
        [SerializeField] private Button pokeDexButton;
        [SerializeField] private Button partyButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button exitButton; 

        [Header("Dependencies")]
        [SerializeField] private PokemonTrainerManager trainerManager;
        [SerializeField] private OwnedPokemonManager ownedPokemonManager;
        [SerializeField] private SpeciesDB speciesDB;
        [SerializeField] private ExpandedUIController expandedUIController;
        [SerializeField] private SummaryUIController summaryUIController;

        private void OnEnable()
        {
            if (ownedPokemonManager != null)
            {
                ownedPokemonManager.OnPartyUpdated += UpdateMenuUI;
            }
            if (pokePcButton != null)
            {
                pokePcButton.onClick.AddListener(OnPokePcButtonClick);
            }
            if (partyButton != null)
            {
                partyButton.onClick.AddListener(OnPartyButtonClick);
            }
            if (exitButton != null)
            {
                exitButton.onClick.AddListener(OnExitButtonClick);
            }

            // Ʈ���̳� �̸� ������Ʈ
            if (trainerManager != null && trainerNameText != null)
            {
                trainerNameText.text = trainerManager.TrainerName;
            }
        }

        private void OnDisable()
        {
            if (ownedPokemonManager != null)
            {
                ownedPokemonManager.OnPartyUpdated -= UpdateMenuUI;
            }
            if (pokePcButton != null)
            {
                pokePcButton.onClick.RemoveListener(OnPokePcButtonClick);
            }
            if (partyButton != null)
            {
                partyButton.onClick.RemoveListener(OnPartyButtonClick);
            }
            if (exitButton != null)
            {
                exitButton.onClick.RemoveListener(OnExitButtonClick);
            }
        }

        /// <summary>
        /// PokePC ��ư Ŭ�� �� ȣ��˴ϴ�.
        /// </summary>
        private void OnPokePcButtonClick()
        {
            expandedUIController.ShowPokePcPanel();
        }

        /// <summary>
        /// ��Ƽ ��ư Ŭ�� �� ȣ��˴ϴ�.
        /// </summary>
        private void OnPartyButtonClick()
        {
            expandedUIController.ShowPartyPanel();
        }
        /// <summary>
        /// Exit ��ư Ŭ�� �� ȣ��˴ϴ�.
        /// </summary>
        private void OnExitButtonClick()
        {
            // Ȯ�� UI�� ��Ȱ��ȭ�Ͽ� ���� ȭ������ ���ư��ϴ�.
            UIManager.Instance.SetExpandedPanelActive(false);
        }
        /// <summary>
        /// �޴� UI�� Ʈ���̳� �̸��� ��Ƽ ���ϸ� �������� �����մϴ�.
        /// </summary>
        public void UpdateMenuUI()
        {
            if (ownedPokemonManager == null || speciesDB == null)
            {
                return;
            }

            var party = ownedPokemonManager.GetParty();
            if (party == null)
            {
                return;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (i < party.Length)
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
                                slot.SetData(p, form, species, summaryUIController);
                                int slotIndex = i; // Ŭ���� �̽� ����
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
    }

    /// <summary>
    /// �޴��� ��Ƽ�� ���Կ� �ʿ��� Ŭ����
    /// </summary>
    [System.Serializable]
    public class MenuPartySlot
    {
        public Button summaryUIOpen;
        public Image pokemonIconImage;
        public Image expBar;

        public void SetData(PokemonSaveData p, FormSO form, SpeciesSO species, SummaryUIController summaryUIController)
        {
            summaryUIOpen.gameObject.SetActive(true);
            summaryUIOpen.onClick.RemoveAllListeners();
            summaryUIOpen.onClick.AddListener(() => OnSummaryUIOpenButtonClick(p, summaryUIController));

            // ���ϸ� ������ ����
            pokemonIconImage.gameObject.SetActive(true);
            pokemonIconImage.sprite = p.isShiny ? form.visual.shinyIcon : form.visual.icon;


            // ����ġ ���� ����
            if (p.level < species.maxLevel)
            {
                int needExp = ExperienceCurveService.GetNeedExpForNextLevel(species.curveType, p.level);
                expBar.fillAmount = needExp > 0 ? (float)p.currentExp / needExp : 0;
            }
            else
            {
                expBar.fillAmount = 1;
            }
        }

        private void OnSummaryUIOpenButtonClick(PokemonSaveData p, SummaryUIController summaryUIController)
        {
            if (summaryUIController != null)
            {
                summaryUIController.gameObject.SetActive(true);
                summaryUIController.SetPokemon(p);
            }
        }

        public void Clear()
        {
            summaryUIOpen.gameObject.SetActive(false);
            pokemonIconImage.sprite = null;
            expBar.fillAmount = 0;
            pokemonIconImage.gameObject.SetActive(false);
            summaryUIOpen.onClick.RemoveAllListeners();
        }
    }
}