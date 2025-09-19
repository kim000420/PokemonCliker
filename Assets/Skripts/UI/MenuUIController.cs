// ����: Scripts/UI/MenuUIController.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

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
        [SerializeField] private List<Image> partyIcons = new List<Image>();
        [SerializeField] private Button pokePcButton;
        [SerializeField] private Button partyButton;
        [SerializeField] private Button pokeDexButton;
        [SerializeField] private Button inventoryButton;

        [Header("Dependencies")]
        [SerializeField] private PokemonTrainerManager trainerManager;
        [SerializeField] private OwnedPokemonManager ownedPokemonManager;
        [SerializeField] private SpeciesDB speciesDB;
        [SerializeField] private ExpandedUIController expandedUIController;

        private void OnEnable()
        {
            if (pokePcButton != null)
            {
                pokePcButton.onClick.AddListener(OnPokePcButtonClick);
            }
            if (partyButton != null)
            {
                partyButton.onClick.AddListener(OnPartyButtonClick);
            }
            // PokeDexButton, InventoryButton�� ���� �߰� ����

            UpdateMenuUI();
        }

        private void OnDisable()
        {
            if (pokePcButton != null)
            {
                pokePcButton.onClick.RemoveListener(OnPokePcButtonClick);
            }
            if (partyButton != null)
            {
                partyButton.onClick.RemoveListener(OnPartyButtonClick);
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
        /// �޴� UI�� Ʈ���̳� �̸��� ��Ƽ ���ϸ� �������� �����մϴ�.
        /// </summary>
        public void UpdateMenuUI()
        {
            if (trainerManager != null && trainerManager.Profile != null)
            {
                trainerNameText.text = trainerManager.Profile.TrainerName;
            }

            if (ownedPokemonManager == null || speciesDB == null) return;

            var party = ownedPokemonManager.Party;
            for (int i = 0; i < partyIcons.Count; i++)
            {
                var iconImage = partyIcons[i];
                if (i < party.Count)
                {
                    var p = ownedPokemonManager.GetByPuid(party[i]);
                    if (p != null)
                    {
                        var species = speciesDB.GetSpecies(p.speciesId);
                        var form = species.GetForm(p.formKey);
                        if (form != null && form.visual != null)
                        {
                            iconImage.sprite = p.isShiny ? form.visual.shinyIcon : form.visual.icon;
                            iconImage.enabled = true;
                        }
                    }
                }
                else
                {
                    iconImage.enabled = false;
                }
            }
        }
    }
}