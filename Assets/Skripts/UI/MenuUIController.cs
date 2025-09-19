// 파일: Scripts/UI/MenuUIController.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace PokeClicker
{
    /// <summary>
    /// 확장 UI 내의 기본 메뉴 패널을 관리하는 컨트롤러.
    /// 트레이너 이름과 파티 포켓몬 아이콘, 하위 메뉴 버튼들을 제어합니다.
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
            // PokeDexButton, InventoryButton은 추후 추가 예정

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
        /// PokePC 버튼 클릭 시 호출됩니다.
        /// </summary>
        private void OnPokePcButtonClick()
        {
            expandedUIController.ShowPokePcPanel();
        }

        /// <summary>
        /// 파티 버튼 클릭 시 호출됩니다.
        /// </summary>
        private void OnPartyButtonClick()
        {
            expandedUIController.ShowPartyPanel();
        }

        /// <summary>
        /// 메뉴 UI의 트레이너 이름과 파티 포켓몬 아이콘을 갱신합니다.
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