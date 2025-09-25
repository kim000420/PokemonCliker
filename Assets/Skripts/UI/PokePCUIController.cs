// 파일: Scripts/UI/PokePCUIController.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PokeClicker
{
    /// <summary>
    /// PokePC UI를 관리하는 메인 컨트롤러.
    /// InfoZone과 BoxZone을 분리하여 관리합니다.  x50 y-45
    /// </summary>
    public class PokePCUIController : MonoBehaviour
    {
        [Header("UI Zones")]
        [SerializeField] private PokeInfoZoneUI infoZoneUI;
        [SerializeField] private PokeBoxZoneUI boxZoneUI;
        [SerializeField] private Button closeButton;

        [Header("Popup Menu UI")]
        [SerializeField] private GameObject pokemonPopupUI;
        [SerializeField] private Button summaryButton;
        [SerializeField] private Button moveButton;
        [SerializeField] private Button releaseButton;

        [Header("Dependencies")]
        [SerializeField] private OwnedPokemonManager ownedPokemonManager;
        [SerializeField] private SpeciesDB speciesDB;
        [SerializeField] private GameIconDB gameIconDB; // 타입 아이콘 및 기타 아이콘 참조
        [SerializeField] private PokemonTrainerManager trainerManager;
        [SerializeField] private SummaryUIController summaryUIController; 

        private int? _selectedPuid = null; //
        private int? _popupPuid;
        private int _currentBoxIndex = 0;
        private const int PokemonPerBox = 30;

        private Coroutine _playAnimationCoroutine;

        private void Awake()
        {
            if (summaryButton != null)
            {
                summaryButton.onClick.AddListener(OnSummaryButtonClick);
            }
            if (moveButton != null)
            {
                moveButton.onClick.AddListener(OnMoveButtonClick);
            }
            if (releaseButton != null)
            {
                releaseButton.onClick.AddListener(OnReleaseButtonClick);
            }
        }

        private void OnEnable()
        {
            if (ownedPokemonManager != null)
            {
                // 파티 업데이트 시 박스 존 갱신
                ownedPokemonManager.OnPartyUpdated += UpdateBoxZone; 

                // UI가 활성화되면 첫 포켓몬 정보를 InfoZone에 표시
                var firstPokemon = ownedPokemonManager.EnumerateAll().FirstOrDefault();
                if (firstPokemon != null)
                {
                    UpdateInfoZone(firstPokemon.P_uid);
                }

                if (closeButton != null)
                {
                    closeButton.onClick.AddListener(OnCloseButtonClick);
                }

                if (pokemonPopupUI != null)
                {
                    pokemonPopupUI.SetActive(false); // UI 활성화 시 팝업 비활성화
                }
                UpdateBoxZone();
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClick);
            }
        }

        private void OnDisable()
        {
            if (ownedPokemonManager != null)
            {
                ownedPokemonManager.OnPartyUpdated -= UpdateBoxZone;
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClick);
            }
            StopAnimation();

            if (pokemonPopupUI != null)
            {
                pokemonPopupUI.SetActive(false); // UI 비활성화 시 팝업 비활성화
            }
        }

        /// <summary>
        /// 닫기 버튼 클릭 시 호출됩니다.
        /// </summary>
        private void OnCloseButtonClick()
        {
            var expandedController = GetComponentInParent<ExpandedUIController>();
            if (expandedController != null)
            {
                expandedController.ShowMenuPanel();
            }
        }

        /// <summary>
        /// 포켓몬 슬롯 좌클릭 시 호출됩니다.
        /// </summary>
        public void OnPokemonSlotLeftClick(int? puid, int boxIndex, int slotIndex)
        {
            if (pokemonPopupUI != null) pokemonPopupUI.SetActive(false);

            if (_selectedPuid.HasValue) // 이동 모드 활성화 상태
            {
                if (puid.HasValue) // 포켓몬이 있는 슬롯을 클릭
                {
                    ownedPokemonManager.Swap(_selectedPuid.Value, puid.Value);
                    _selectedPuid = null;
                }
                else // 빈 슬롯을 클릭 (이동)
                {
                    if (boxIndex == -1)
                    {
                        ownedPokemonManager.MoveToParty(_selectedPuid.Value, slotIndex);
                    }
                    else
                    {
                        ownedPokemonManager.MoveToBox(_selectedPuid.Value, boxIndex, slotIndex);
                    }
                    _selectedPuid = null;
                }
            }
            else // 정보 갱신 모드
            {
                if (puid.HasValue) // 포켓몬이 있는 슬롯 클릭
                {
                    _selectedPuid = puid.Value;
                    UpdateInfoZone(puid.Value);
                }
                // 빈 슬롯 클릭시 아무것도 안 함
            }

            UpdateBoxZone(); // UI 갱신
        }

        /// <summary>
        /// 포켓몬 슬롯 우클릭 시 호출됩니다.
        /// </summary>
        public void OnPokemonSlotRightClick(int? puid)
        {
            if (puid.HasValue)
            {
                _selectedPuid = null;
                UpdateBoxZone();

                ShowPokemonPopup(puid.Value);
            }
            else
            {
                if (pokemonPopupUI != null) pokemonPopupUI.SetActive(false);
            }
        }

        private void UpdateUIBasedOnSelection()
        {
            if (_selectedPuid.HasValue)
            {
                UpdateInfoZone(_selectedPuid.Value);
            }
            else
            {
                ClearInfoZone();
            }
            UpdateBoxZone();
        }

        private void ShowPokemonPopup(int puid)
        {
            if (pokemonPopupUI == null) return;

            _popupPuid = puid; // 팝업이 참조할 PUID 설정

            pokemonPopupUI.SetActive(true);
            pokemonPopupUI.transform.position = Input.mousePosition;
        }

        private void OnSummaryButtonClick()
        {
            if (!_popupPuid.HasValue) return;

            summaryUIController.gameObject.SetActive(true);
            StartCoroutine(DelayedSetPokemon(_popupPuid.Value));
            if (pokemonPopupUI != null) pokemonPopupUI.SetActive(false);
        }

        private IEnumerator DelayedSetPokemon(int puid)
        {
            yield return null; // 한 프레임 대기
            summaryUIController.SetPokemon(ownedPokemonManager.GetByPuid(puid));
        }

        private void OnMoveButtonClick()
        {
            if (!_popupPuid.HasValue) return;

            _selectedPuid = _popupPuid.Value;
            UpdateUIBasedOnSelection();
            if (pokemonPopupUI != null) pokemonPopupUI.SetActive(false);
        }

        private void OnReleaseButtonClick()
        {
            if (!_popupPuid.HasValue) return;

            ownedPokemonManager.Release(_popupPuid.Value);
            _selectedPuid = null;
            UpdateUIBasedOnSelection();
            if (pokemonPopupUI != null) pokemonPopupUI.SetActive(false);
        }

        /// <summary>
        /// InfoZone의 UI를 갱신합니다.
        /// </summary>
        private void UpdateInfoZone(int puid)
        {
            var p = ownedPokemonManager.GetByPuid(puid);
            if (p == null || speciesDB == null || gameIconDB == null)
            {
                ClearInfoZone();
                return;
            }

            var species = speciesDB.GetSpecies(p.speciesId);
            if (species == null)
            {
                ClearInfoZone();
                return;
            }

            var form = species.GetForm(p.formKey);
            if (form == null)
            {
                ClearInfoZone();
                return;
            }

            // 이름, 성별 아이콘 업데이트
            infoZoneUI.nameText.text = p.GetDisplayName(species);
            infoZoneUI.genderIcon.sprite = p.gender switch
            {
                Gender.Male => gameIconDB.miscIcons.male,
                Gender.Female => gameIconDB.miscIcons.female,
                _ => gameIconDB.miscIcons.genderless
            };

            // 전면 애니메이션 업데이트
            var frames = p.isShiny ? form.visual.shinyFrontFrames : form.visual.frontFrames;
            var fps = p.isShiny ? form.visual.shinyFrontAnimationFps : form.visual.frontAnimationFps;
            StartAnimation(frames, fps);

            // 레벨, 아이템, 경험치, 타입 아이콘 업데이트
            infoZoneUI.levelText.text = $"{p.level}";
            infoZoneUI.heldItemText.text = string.IsNullOrWhiteSpace(p.heldItemId) ? "없음" : p.heldItemId; // TODO: 아이템 이름을 DB에서 가져오도록 수정
            infoZoneUI.expText.text = $"{p.currentExp} / {ExperienceCurveService.GetNeedExpForNextLevel(species.curveType, p.level)}";

            // 타입 아이콘 업데이트 (단일/듀얼 타입에 따라 다르게 표시)
            if (form.typePair.hasDualType)
            {
                infoZoneUI.singlePrimaryTypeIcon.gameObject.SetActive(false);
                infoZoneUI.dualPrimaryTypeIcon.gameObject.SetActive(true);
                infoZoneUI.dualSecondaryTypeIcon.gameObject.SetActive(true);
                infoZoneUI.dualPrimaryTypeIcon.sprite = gameIconDB.typeIcons.GetIcon(form.typePair.primary);
                infoZoneUI.dualSecondaryTypeIcon.sprite = gameIconDB.typeIcons.GetIcon(form.typePair.secondary);
            }
            else
            {
                infoZoneUI.singlePrimaryTypeIcon.gameObject.SetActive(true);
                infoZoneUI.dualPrimaryTypeIcon.gameObject.SetActive(false);
                infoZoneUI.dualSecondaryTypeIcon.gameObject.SetActive(false);
                infoZoneUI.singlePrimaryTypeIcon.sprite = gameIconDB.typeIcons.GetIcon(form.typePair.primary);
            }
        }

        /// <summary>
        /// InfoZone의 UI를 초기화합니다.
        /// </summary>
        private void ClearInfoZone()
        {
            infoZoneUI.nameText.text = string.Empty;
            infoZoneUI.genderIcon.sprite = null;
            infoZoneUI.pokemonFrontImage.sprite = null;
            infoZoneUI.levelText.text = string.Empty;
            infoZoneUI.heldItemText.text = string.Empty;
            infoZoneUI.expText.text = string.Empty;
            infoZoneUI.singlePrimaryTypeIcon.sprite = null;
            infoZoneUI.dualPrimaryTypeIcon.sprite = null;
            infoZoneUI.dualSecondaryTypeIcon.sprite = null;
            StopAnimation();
        }

        /// <summary>
        /// BoxZone의 UI를 갱신합니다.
        /// </summary>
        private void UpdateBoxZone()
        {
            if (ownedPokemonManager == null || speciesDB == null) return;

            // 박스 이름 업데이트
            boxZoneUI.boxNameText.text = $"박스 {_currentBoxIndex + 1}";

            // 박스 슬롯 갱신
            var boxContents = ownedPokemonManager.GetBoxes().ElementAtOrDefault(_currentBoxIndex);
            for (int i = 0; i < boxZoneUI.boxSlots.Count; i++)
            {
                var slot = boxZoneUI.boxSlots[i];
                int? puid = null;
                if (boxContents != null && i < boxContents.Length && boxContents[i] != 0)
                {
                    puid = boxContents[i];
                    var p = ownedPokemonManager.GetByPuid(puid.Value);
                    if (p != null)
                    {
                        var species = speciesDB.GetSpecies(p.speciesId);
                        var form = species.GetForm(p.formKey);
                        if (form?.visual != null)
                        {
                            slot.SetData(p, form);
                        }
                    }
                }
                else slot.Clear();

                slot.SetSelectBoxActive(_selectedPuid.HasValue && _selectedPuid.Value == puid);

                slot.iconButton.onClick.RemoveAllListeners();
                int? capturedPuid = puid;
                int capturedBoxIndex = _currentBoxIndex;
                int capturedSlotIndex = i;
                slot.iconButton.onClick.AddListener(() => OnPokemonSlotLeftClick(capturedPuid, capturedBoxIndex, capturedSlotIndex));
            }


            // 파티 슬롯 갱신
            var party = ownedPokemonManager.GetParty();
            for (int i = 0; i < boxZoneUI.partySlots.Count; i++)
            {
                var slot = boxZoneUI.partySlots[i];
                int? puid = null;
                if (i < party.Length && party[i] != 0)
                {
                    puid = party[i];
                    var p = ownedPokemonManager.GetByPuid(puid.Value);
                    if (p != null)
                    {
                        var species = speciesDB.GetSpecies(p.speciesId);
                        var form = species.GetForm(p.formKey);
                        if (form?.visual != null)
                        {
                            slot.SetData(p, form);
                        }
                    }
                }
                else slot.Clear();

                slot.SetSelectBoxActive(_selectedPuid.HasValue && _selectedPuid.Value == puid);

                slot.iconButton.onClick.RemoveAllListeners(); // 기존 리스너 모두 제거
                int? capturedPuid = puid;
                int capturedSlotIndex = i;
                slot.iconButton.onClick.AddListener(() => OnPokemonSlotLeftClick(capturedPuid, -1, capturedSlotIndex));
            }
        }

        /// <summary>
        /// 포켓몬 애니메이션을 재생합니다.
        /// </summary>
        private void StartAnimation(Sprite[] frames, float fps)
        {
            if (_playAnimationCoroutine != null)
            {
                StopCoroutine(_playAnimationCoroutine);
            }

            if (frames == null || frames.Length == 0)
            {
                infoZoneUI.pokemonFrontImage.enabled = false;
                return;
            }

            infoZoneUI.pokemonFrontImage.enabled = true;
            _playAnimationCoroutine = StartCoroutine(PlayAnimation(frames, fps));
        }

        /// <summary>
        /// 애니메이션 코루틴을 정지합니다.
        /// </summary>
        private void StopAnimation()
        {
            if (_playAnimationCoroutine != null)
            {
                StopCoroutine(_playAnimationCoroutine);
                _playAnimationCoroutine = null;
            }
        }

        private IEnumerator PlayAnimation(Sprite[] frames, float fps)
        {
            float delay = 1f / Mathf.Max(1f, fps);
            int i = 0;
            while (true)
            {
                infoZoneUI.pokemonFrontImage.sprite = frames[i];
                i = (i + 1) % frames.Length;
                yield return new WaitForSeconds(delay);
            }
        }
    }

    /// <summary>
    /// PokePC UI의 InfoZone에 필요한 UI 컴포넌트들을 모아놓은 클래스.
    /// </summary>
    [System.Serializable]
    public class PokeInfoZoneUI
    {
        public TextMeshProUGUI nameText;
        public Image genderIcon;
        public Image pokemonFrontImage; // 전면 애니메이션 이미지
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI heldItemText;
        public TextMeshProUGUI expText;
        public Image singlePrimaryTypeIcon; // 단일 타입 아이콘
        public Image dualPrimaryTypeIcon;   // 듀얼 타입 (좌측) 아이콘
        public Image dualSecondaryTypeIcon; // 듀얼 타입 (우측) 아이콘
    }

    /// <summary>
    /// PokePC UI의 BoxZone에 필요한 UI 컴포넌트들을 모아놓은 클래스.
    /// </summary>
    [System.Serializable]
    public class PokeBoxZoneUI
    {
        public Button prevBoxButton;
        public Button nextBoxButton;
        public TextMeshProUGUI boxNameText;
        public List<PokePokemonSlot> boxSlots = new List<PokePokemonSlot>();
        public List<PokePokemonSlot> partySlots = new List<PokePokemonSlot>();
    }

    /// <summary>
    /// PokePC UI의 포켓몬 아이콘 슬롯을 위한 클래스.
    /// </summary>
    [System.Serializable]
    public class PokePokemonSlot
    {
        public Button iconButton;
        public Image iconImage;
        public Image selectBox;

        public void SetData(PokemonSaveData p, FormSO form)
        {
            iconButton.gameObject.SetActive(true);
            iconImage.sprite = p.isShiny ? form.visual.shinyIcon : form.visual.icon;
            iconImage.gameObject.SetActive(true);
        }

        public void Clear()
        {
            iconButton.gameObject.SetActive(true); // 빈 슬롯도 클릭 가능하게 활성화
            iconImage.gameObject.SetActive(false); // 아이콘 이미지만 비활성화
        }

        public void SetSelectBoxActive(bool isActive)
        {
            if (selectBox != null)
            {
                selectBox.gameObject.SetActive(isActive);
            }
        }
    }
}