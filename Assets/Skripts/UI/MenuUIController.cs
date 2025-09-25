// 파일: Scripts/UI/MenuUIController.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

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

            // 트레이너 이름 업데이트
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
        /// Exit 버튼 클릭 시 호출됩니다.
        /// </summary>
        private void OnExitButtonClick()
        {
            // 확장 UI를 비활성화하여 메인 화면으로 돌아갑니다.
            UIManager.Instance.SetExpandedPanelActive(false);
        }
        /// <summary>
        /// 메뉴 UI의 트레이너 이름과 파티 포켓몬 아이콘을 갱신합니다.
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
                                // 아이콘과 레벨 표시
                                slot.SetData(p, form, species, summaryUIController);
                                int slotIndex = i; // 클로저 이슈 방지
                            }
                        }
                    }
                }
                else
                {
                    // 빈 슬롯 처리
                    slot.Clear();
                }
            }
        }
    }

    /// <summary>
    /// 메뉴의 파티원 슬롯에 필요한 클래스
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

            // 포켓몬 아이콘 설정
            pokemonIconImage.gameObject.SetActive(true);
            pokemonIconImage.sprite = p.isShiny ? form.visual.shinyIcon : form.visual.icon;


            // 경험치 정보 설정
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