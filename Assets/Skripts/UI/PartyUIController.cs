// 파일: Scripts/UI/PartyUIController.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

namespace PokeClicker
{
    /// <summary>
    /// 파티 UI를 관리하는 컨트롤러.
    /// 파티에 있는 포켓몬 6마리의 아이콘과 정보를 표시하고,
    /// 순서 교체 기능을 담당합니다.
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
        /// 파티 UI의 닫기 버튼 클릭 시 호출됩니다.
        /// </summary>
        private void OnCloseButtonClick()
        {
            // 이 버튼은 ExpandedUIController에 연결될 것이므로,
            // 여기서는 ExpandedUIController의 ShowMenuPanel()을 호출합니다.
            var expandedController = GetComponentInParent<ExpandedUIController>();
            if (expandedController != null)
            {
                expandedController.ShowMenuPanel();
            }
        }

        /// <summary>
        /// 파티 UI의 모든 슬롯을 갱신합니다.
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
                                // 아이콘과 레벨 표시
                                slot.SetData(p, form, species, gameIconDB);
                                slot.iconButton.onClick.RemoveAllListeners();
                                int slotIndex = i; // 클로저 이슈 방지
                                slot.iconButton.onClick.AddListener(() => OnPokemonSlotClick(slotIndex));
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

        /// <summary>
        /// 포켓몬 슬롯 클릭 시 호출됩니다.
        /// </summary>
        private void OnPokemonSlotClick(int slotIndex)
        {
            // 선택된 슬롯이 이미 있으면 교체
            if (_selectedSlotIndex.HasValue)
            {
                int oldIndex = _selectedSlotIndex.Value;
                if (oldIndex != slotIndex)
                {
                    // 파티 포켓몬 순서 교체
                    SwapPartyPokemon(oldIndex, slotIndex);
                    // 선택 상태 초기화
                    _selectedSlotIndex = null;
                }
                else
                {
                    // 같은 슬롯을 다시 클릭하면 선택 해제
                    _selectedSlotIndex = null;
                }
            }
            else
            {
                // 첫 번째 슬롯 선택
                _selectedSlotIndex = slotIndex;
            }
        }

        /// <summary>
        /// 두 포켓몬의 순서를 교체합니다.
        /// </summary>
        private void SwapPartyPokemon(int indexA, int indexB)
        {
            if (ownedPokemonManager == null) return;

            var party = ownedPokemonManager.Party.ToList();
            if (indexA < 0 || indexA >= party.Count || indexB < 0 || indexB >= party.Count)
            {
                return;
            }

            // 순서 교체 로직
            var puidA = party[indexA];
            var puidB = party[indexB];
            ownedPokemonManager.Swap(puidA, puidB);

            UpdatePartyUI(); // UI 갱신
        }
    }

    // UI 슬롯을 위한 데이터 홀더 클래스
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
        /// 포켓몬 데이터를 슬롯에 설정합니다.
        /// </summary>
        public void SetData(PokemonSaveData p, FormSO form, SpeciesSO species, GameIconDB gameIconDB)
        {
            // 슬롯 활성화 및 데이터 표시
            iconButton.gameObject.SetActive(true);

            // 포켓몬 이미지 설정
            pokemonIconImage.sprite = p.isShiny ? form.visual.shinyIcon : form.visual.icon;

            // 성별 아이콘 설정
            genderIconImage.sprite = p.gender switch
            {
                Gender.Male => gameIconDB.miscIcons.male,
                Gender.Female => gameIconDB.miscIcons.female,
                _ => gameIconDB.miscIcons.genderless
            };

            // 레벨 및 이름 설정
            levelText.text = $"Lv.{p.level}";
            nameText.text = p.GetDisplayName(species);

            // 경험치 정보 설정
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
        /// 슬롯을 비웁니다.
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