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

        [Header("Dependencies")]
        [SerializeField] private OwnedPokemonManager ownedPokemonManager;
        [SerializeField] private SpeciesDB speciesDB;
        [SerializeField] private GameIconDB gameIconDB; // 타입 아이콘 및 기타 아이콘 참조
        [SerializeField] private PokemonTrainerManager trainerManager;

        private int? _selectedPuid = null;
        private int _currentBoxIndex = 0;
        private const int PokemonPerBox = 30;

        private Coroutine _playAnimationCoroutine;

        private void OnEnable()
        {
            if (ownedPokemonManager != null)
            {
                // UI가 활성화되면 첫 포켓몬 정보를 InfoZone에 표시
                var firstPokemon = ownedPokemonManager.EnumerateAll().FirstOrDefault();
                if (firstPokemon != null)
                {
                    UpdateInfoZone(firstPokemon.P_uid);
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
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClick);
            }
            StopAnimation();
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
        /// 포켓몬 슬롯 클릭 시 호출됩니다. (좌클릭: 선택/교체, 우클릭: 선택 취소)
        /// </summary>
        private void OnPokemonSlotClick(int puid)
        {
            if (Input.GetMouseButtonDown(0)) // 좌클릭 (선택/교체)
            {
                if (!_selectedPuid.HasValue)
                {
                    // 첫 번째 포켓몬 선택
                    _selectedPuid = puid;
                }
                else if (_selectedPuid.Value == puid)
                {
                    // 같은 포켓몬을 다시 클릭 시 선택 유지 (우클릭으로만 해제)
                }
                else
                {
                    // 다른 포켓몬 클릭 시 교체
                    ownedPokemonManager.Swap(_selectedPuid.Value, puid);
                    _selectedPuid = null;
                }

                UpdateBoxZone(); // UI 갱신
            }
            else if (Input.GetMouseButtonDown(1)) // 우클릭 (선택 취소)
            {
                if (_selectedPuid.HasValue && _selectedPuid.Value == puid)
                {
                    _selectedPuid = null;
                    UpdateBoxZone(); // UI 갱신
                }
            }

            // InfoZone 갱신 (선택된 포켓몬 정보 표시)
            UpdateInfoZone(puid);
        }

        /// <summary>
        /// InfoZone의 UI를 갱신합니다.
        /// </summary>
        private void UpdateInfoZone(int puid)
        {
            var p = ownedPokemonManager.GetByPuid(puid);
            if (p == null || speciesDB == null || gameIconDB == null) return;

            var species = speciesDB.GetSpecies(p.speciesId);
            if (species == null) return;

            var form = species.GetForm(p.formKey);
            if (form == null) return;

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
            infoZoneUI.levelText.text = $"Lv.{p.level}";
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
        /// BoxZone의 UI를 갱신합니다.
        /// </summary>
        private void UpdateBoxZone()
        {
            if (ownedPokemonManager == null || speciesDB == null) return;

            // 박스 이름 업데이트
            boxZoneUI.boxNameText.text = $"BOX {_currentBoxIndex + 1}";

            // 박스 슬롯 갱신
            var boxContents = ownedPokemonManager.Boxes.ElementAtOrDefault(_currentBoxIndex);
            for (int i = 0; i < boxZoneUI.boxSlots.Count; i++)
            {
                var slot = boxZoneUI.boxSlots[i];
                if (boxContents != null && i < boxContents.Count)
                {
                    int puid = boxContents[i];
                    var p = ownedPokemonManager.GetByPuid(puid);
                    if (p != null)
                    {
                        var species = speciesDB.GetSpecies(p.speciesId);
                        var form = species.GetForm(p.formKey);
                        if (form?.visual != null)
                        {
                            slot.SetData(p, form);
                            slot.SetSelectBoxActive(_selectedPuid.HasValue && _selectedPuid.Value == puid);
                            slot.iconButton.onClick.RemoveAllListeners();
                            slot.iconButton.onClick.AddListener(() => OnPokemonSlotClick(puid));
                        }
                    }
                }
                else
                {
                    slot.Clear();
                }
            }

            // 파티 슬롯 갱신
            var party = ownedPokemonManager.Party;
            for (int i = 0; i < boxZoneUI.partySlots.Count; i++)
            {
                var slot = boxZoneUI.partySlots[i];
                if (i < party.Count)
                {
                    int puid = party[i];
                    var p = ownedPokemonManager.GetByPuid(puid);
                    if (p != null)
                    {
                        var species = speciesDB.GetSpecies(p.speciesId);
                        var form = species.GetForm(p.formKey);
                        if (form?.visual != null)
                        {
                            slot.SetData(p, form);
                            slot.SetSelectBoxActive(_selectedPuid.HasValue && _selectedPuid.Value == puid);
                            slot.iconButton.onClick.RemoveAllListeners();
                            slot.iconButton.onClick.AddListener(() => OnPokemonSlotClick(puid));
                        }
                    }
                }
                else
                {
                    slot.Clear();
                }
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
        }

        public void Clear()
        {
            iconButton.gameObject.SetActive(false);
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