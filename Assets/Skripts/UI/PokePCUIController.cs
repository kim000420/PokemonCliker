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
        // PC의 상태
        private enum ePCState
        {
            Normal, // 기본 상태
            Move    // 'Move' 버튼을 눌러 이동/교체를 기다리는 상태
        }

        [Header("UI Zones")]
        [SerializeField] private PokeInfoZoneUI infoZoneUI;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI boxNameText;
        [SerializeField] private List<PokeSlotUI> boxSlots = new List<PokeSlotUI>();
        [SerializeField] private List<PokeSlotUI> partySlots = new List<PokeSlotUI>();

        [Header("Popup Menu UI")]
        [SerializeField] private GameObject pokemonPopupUI;
        [SerializeField] private Button summaryButton;
        [SerializeField] private Button moveButton;
        [SerializeField] private Button releaseButton;

        [Header("Dependencies")]
        [SerializeField] private OwnedPokemonManager ownedPokemonManager;
        [SerializeField] private SpeciesDB speciesDB;
        [SerializeField] private GameIconDB gameIconDB;
        [SerializeField] private SummaryUIController summaryUIController;

        // 상태 관리 변수
        private ePCState _currentState = ePCState.Normal;
        private int? _infoPuid = null;     // 정보창에 표시할 포켓몬의 puid
        private int? _puidToMove = null;   // 이동/교체할 포켓몬의 puid
        private PokeSlotUI _popupSlot = null; // 팝업 메뉴가 열린 슬롯

        private int _currentBoxIndex = 0;
        private Coroutine _playAnimationCoroutine;

        private void Awake()
        {
            // 팝업 메뉴 버튼 이벤트 연결
            summaryButton?.onClick.AddListener(OnSummaryButtonClick);
            moveButton?.onClick.AddListener(OnMoveButtonClick);
            releaseButton?.onClick.AddListener(OnReleaseButtonClick);
            closeButton?.onClick.AddListener(OnCloseButtonClick);

            // 각 슬롯 초기화 및 클릭 이벤트 구독
            for (int i = 0; i < boxSlots.Count; i++)
            {
                boxSlots[i].Init(_currentBoxIndex, i);
                boxSlots[i].OnSlotClicked += OnSlotClicked;
            }
            for (int i = 0; i < partySlots.Count; i++)
            {
                partySlots[i].Init(-1, i); // boxIndex -1은 파티를 의미
                partySlots[i].OnSlotClicked += OnSlotClicked;
            }
        }

        private void OnEnable()
        {
            ownedPokemonManager.OnPartyUpdated += UpdateAllSlotsData;
            _currentState = ePCState.Normal;
            _puidToMove = null;
            pokemonPopupUI.SetActive(false);

            UpdateAllSlotsData();

            var firstPokemon = ownedPokemonManager.EnumerateAll().FirstOrDefault();
            if (firstPokemon != null)
            {
                _infoPuid = firstPokemon.P_uid;
                UpdateInfoZone(_infoPuid.Value);
            }
        }

        private void OnDisable()
        {
            ownedPokemonManager.OnPartyUpdated -= UpdateAllSlotsData;
            StopAnimation();
        }

        /// <summary>
        /// 닫기 버튼 클릭 시 호출됩니다.
        /// </summary>
        private void OnCloseButtonClick()
        {
            gameObject.SetActive(false);
            var expandedController = GetComponentInParent<ExpandedUIController>();
            if (expandedController != null)
            {
                expandedController.ShowMenuPanel();
            }
        }

        /// <summary>
        /// 포켓몬 슬롯 좌클릭 시 호출됩니다.
        /// </summary>
        private void OnSlotClicked(PokeSlotUI clickedSlot, PointerEventData.InputButton button)
        {
            pokemonPopupUI.SetActive(false);

            if (button == PointerEventData.InputButton.Left)
            {
                HandleLeftClick(clickedSlot);
            }
            else if (button == PointerEventData.InputButton.Right)
            {
                HandleRightClick(clickedSlot);
            }
        }

        private void HandleLeftClick(PokeSlotUI clickedSlot)
        {
            switch (_currentState)
            {
                case ePCState.Normal:
                    // 일반 상태: 정보창 업데이트
                    if (clickedSlot.Puid.HasValue)
                    {
                        _infoPuid = clickedSlot.Puid;
                        UpdateInfoZone(_infoPuid.Value);
                    }
                    break;

                case ePCState.Move:
                    // 이동 상태: 이동 또는 교체 실행
                    if (clickedSlot.Puid.HasValue) // 다른 포켓몬과 교체
                    {
                        ownedPokemonManager.Swap(_puidToMove.Value, clickedSlot.Puid.Value);
                    }
                    else // 빈 슬롯으로 이동
                    {
                        if (clickedSlot.BoxIndex == -1) // 파티로 이동
                            ownedPokemonManager.MoveToParty(_puidToMove.Value, clickedSlot.SlotIndex);
                        else // 박스로 이동
                            ownedPokemonManager.MoveToBox(_puidToMove.Value, clickedSlot.BoxIndex, clickedSlot.SlotIndex);
                    }

                    // 상태 초기화
                    _puidToMove = null;
                    _currentState = ePCState.Normal;
                    UpdateAllSlotsData(); // UI 전체 갱신 (OnPartyUpdated 이벤트가 해주지만 즉시 반영을 위해 호출)
                    break;
            }
        }


        /// <summary>
        /// 포켓몬 슬롯 우클릭 시 호출됩니다.
        /// </summary>
        private void HandleRightClick(PokeSlotUI clickedSlot)
        {
            switch (_currentState)
            {
                case ePCState.Normal:
                    // 일반 상태: 팝업 메뉴 표시
                    if (clickedSlot.Puid.HasValue)
                    {
                        _popupSlot = clickedSlot;
                        pokemonPopupUI.transform.position = Input.mousePosition;
                        pokemonPopupUI.SetActive(true);
                    }
                    break;

                case ePCState.Move:
                    // 이동 상태: 이동 취소
                    _puidToMove = null;
                    _currentState = ePCState.Normal;
                    UpdateAllSlotsSelection(); // 선택 박스만 갱신
                    break;
            }
        }

        // --- 팝업 메뉴 버튼 핸들러 ---
        private void OnSummaryButtonClick()
        {
            if (_popupSlot == null || !_popupSlot.Puid.HasValue) return;

            summaryUIController.gameObject.SetActive(true);
            summaryUIController.SetPokemon(ownedPokemonManager.GetByPuid(_popupSlot.Puid.Value));
            pokemonPopupUI.SetActive(false);
        }

        private void OnMoveButtonClick()
        {
            if (_popupSlot == null || !_popupSlot.Puid.HasValue) return;

            _currentState = ePCState.Move;
            _puidToMove = _popupSlot.Puid;
            pokemonPopupUI.SetActive(false);
            UpdateAllSlotsSelection(); // 선택 UI 갱신
        }

        private void OnReleaseButtonClick()
        {
            if (_popupSlot == null || !_popupSlot.Puid.HasValue) return;

            ownedPokemonManager.Release(_popupSlot.Puid.Value);
            pokemonPopupUI.SetActive(false);

            // 만약 정보창에 표시되던 포켓몬이 방출되면 정보창 초기화
            if (_infoPuid.HasValue && _infoPuid.Value == _popupSlot.Puid.Value)
            {
                _infoPuid = null;
                ClearInfoZone();
            }

            // UI는 OnPartyUpdated 이벤트가 발생하여 자동으로 갱신됩니다.
        }
        /// <summary>
        /// 모든 슬롯의 포켓몬 데이터를 다시 설정합니다.
        /// </summary>
        private void UpdateAllSlotsData()
        {
            // 박스 슬롯 갱신
            var boxContents = ownedPokemonManager.GetBoxes().ElementAtOrDefault(_currentBoxIndex);
            for (int i = 0; i < boxSlots.Count; i++)
            {
                int? puid = (boxContents != null && i < boxContents.Length && boxContents[i] != 0) ? boxContents[i] : null;
                UpdateSlot(boxSlots[i], puid);
            }

            // 파티 슬롯 갱신
            var party = ownedPokemonManager.GetParty();
            for (int i = 0; i < partySlots.Count; i++)
            {
                int? puid = (i < party.Length && party[i] != 0) ? party[i] : null;
                UpdateSlot(partySlots[i], puid);
            }
            UpdateAllSlotsSelection(); // 데이터 갱신 후 선택 상태도 갱신
        }

        // 슬롯 하나를 업데이트하는 헬퍼 메서드
        private void UpdateSlot(PokeSlotUI slot, int? puid)
        {
            if (puid.HasValue)
            {
                var p = ownedPokemonManager.GetByPuid(puid.Value);
                var species = speciesDB.GetSpecies(p.speciesId);
                var form = species.GetForm(p.formKey);
                slot.SetData(p, form);
            }
            else
            {
                slot.Clear();
            }
        }

        /// <summary>
        /// 모든 슬롯의 '선택됨' UI(SelectBox) 상태를 갱신합니다.
        /// </summary>
        private void UpdateAllSlotsSelection()
        {
            foreach (var slot in boxSlots)
            {
                slot.SetSelectBoxActive(_puidToMove.HasValue && slot.Puid.HasValue && slot.Puid.Value == _puidToMove.Value);
            }
            foreach (var slot in partySlots)
            {
                slot.SetSelectBoxActive(_puidToMove.HasValue && slot.Puid.HasValue && slot.Puid.Value == _puidToMove.Value);
            }
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
}