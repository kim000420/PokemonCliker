// ����: Scripts/UI/PokePCUIController.cs
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
    /// PokePC UI�� �����ϴ� ���� ��Ʈ�ѷ�.
    /// InfoZone�� BoxZone�� �и��Ͽ� �����մϴ�.  x50 y-45
    /// </summary>
    public class PokePCUIController : MonoBehaviour
    {
        // PC�� ����
        private enum ePCState
        {
            Normal, // �⺻ ����
            Move    // 'Move' ��ư�� ���� �̵�/��ü�� ��ٸ��� ����
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

        // ���� ���� ����
        private ePCState _currentState = ePCState.Normal;
        private int? _infoPuid = null;     // ����â�� ǥ���� ���ϸ��� puid
        private int? _puidToMove = null;   // �̵�/��ü�� ���ϸ��� puid
        private PokeSlotUI _popupSlot = null; // �˾� �޴��� ���� ����

        private int _currentBoxIndex = 0;
        private Coroutine _playAnimationCoroutine;

        private void Awake()
        {
            // �˾� �޴� ��ư �̺�Ʈ ����
            summaryButton?.onClick.AddListener(OnSummaryButtonClick);
            moveButton?.onClick.AddListener(OnMoveButtonClick);
            releaseButton?.onClick.AddListener(OnReleaseButtonClick);
            closeButton?.onClick.AddListener(OnCloseButtonClick);

            // �� ���� �ʱ�ȭ �� Ŭ�� �̺�Ʈ ����
            for (int i = 0; i < boxSlots.Count; i++)
            {
                boxSlots[i].Init(_currentBoxIndex, i);
                boxSlots[i].OnSlotClicked += OnSlotClicked;
            }
            for (int i = 0; i < partySlots.Count; i++)
            {
                partySlots[i].Init(-1, i); // boxIndex -1�� ��Ƽ�� �ǹ�
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
        /// �ݱ� ��ư Ŭ�� �� ȣ��˴ϴ�.
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
        /// ���ϸ� ���� ��Ŭ�� �� ȣ��˴ϴ�.
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
                    // �Ϲ� ����: ����â ������Ʈ
                    if (clickedSlot.Puid.HasValue)
                    {
                        _infoPuid = clickedSlot.Puid;
                        UpdateInfoZone(_infoPuid.Value);
                    }
                    break;

                case ePCState.Move:
                    // �̵� ����: �̵� �Ǵ� ��ü ����
                    if (clickedSlot.Puid.HasValue) // �ٸ� ���ϸ�� ��ü
                    {
                        ownedPokemonManager.Swap(_puidToMove.Value, clickedSlot.Puid.Value);
                    }
                    else // �� �������� �̵�
                    {
                        if (clickedSlot.BoxIndex == -1) // ��Ƽ�� �̵�
                            ownedPokemonManager.MoveToParty(_puidToMove.Value, clickedSlot.SlotIndex);
                        else // �ڽ��� �̵�
                            ownedPokemonManager.MoveToBox(_puidToMove.Value, clickedSlot.BoxIndex, clickedSlot.SlotIndex);
                    }

                    // ���� �ʱ�ȭ
                    _puidToMove = null;
                    _currentState = ePCState.Normal;
                    UpdateAllSlotsData(); // UI ��ü ���� (OnPartyUpdated �̺�Ʈ�� �������� ��� �ݿ��� ���� ȣ��)
                    break;
            }
        }


        /// <summary>
        /// ���ϸ� ���� ��Ŭ�� �� ȣ��˴ϴ�.
        /// </summary>
        private void HandleRightClick(PokeSlotUI clickedSlot)
        {
            switch (_currentState)
            {
                case ePCState.Normal:
                    // �Ϲ� ����: �˾� �޴� ǥ��
                    if (clickedSlot.Puid.HasValue)
                    {
                        _popupSlot = clickedSlot;
                        pokemonPopupUI.transform.position = Input.mousePosition;
                        pokemonPopupUI.SetActive(true);
                    }
                    break;

                case ePCState.Move:
                    // �̵� ����: �̵� ���
                    _puidToMove = null;
                    _currentState = ePCState.Normal;
                    UpdateAllSlotsSelection(); // ���� �ڽ��� ����
                    break;
            }
        }

        // --- �˾� �޴� ��ư �ڵ鷯 ---
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
            UpdateAllSlotsSelection(); // ���� UI ����
        }

        private void OnReleaseButtonClick()
        {
            if (_popupSlot == null || !_popupSlot.Puid.HasValue) return;

            ownedPokemonManager.Release(_popupSlot.Puid.Value);
            pokemonPopupUI.SetActive(false);

            // ���� ����â�� ǥ�õǴ� ���ϸ��� ����Ǹ� ����â �ʱ�ȭ
            if (_infoPuid.HasValue && _infoPuid.Value == _popupSlot.Puid.Value)
            {
                _infoPuid = null;
                ClearInfoZone();
            }

            // UI�� OnPartyUpdated �̺�Ʈ�� �߻��Ͽ� �ڵ����� ���ŵ˴ϴ�.
        }
        /// <summary>
        /// ��� ������ ���ϸ� �����͸� �ٽ� �����մϴ�.
        /// </summary>
        private void UpdateAllSlotsData()
        {
            // �ڽ� ���� ����
            var boxContents = ownedPokemonManager.GetBoxes().ElementAtOrDefault(_currentBoxIndex);
            for (int i = 0; i < boxSlots.Count; i++)
            {
                int? puid = (boxContents != null && i < boxContents.Length && boxContents[i] != 0) ? boxContents[i] : null;
                UpdateSlot(boxSlots[i], puid);
            }

            // ��Ƽ ���� ����
            var party = ownedPokemonManager.GetParty();
            for (int i = 0; i < partySlots.Count; i++)
            {
                int? puid = (i < party.Length && party[i] != 0) ? party[i] : null;
                UpdateSlot(partySlots[i], puid);
            }
            UpdateAllSlotsSelection(); // ������ ���� �� ���� ���µ� ����
        }

        // ���� �ϳ��� ������Ʈ�ϴ� ���� �޼���
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
        /// ��� ������ '���õ�' UI(SelectBox) ���¸� �����մϴ�.
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
        /// InfoZone�� UI�� �����մϴ�.
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

            // �̸�, ���� ������ ������Ʈ
            infoZoneUI.nameText.text = p.GetDisplayName(species);
            infoZoneUI.genderIcon.sprite = p.gender switch
            {
                Gender.Male => gameIconDB.miscIcons.male,
                Gender.Female => gameIconDB.miscIcons.female,
                _ => gameIconDB.miscIcons.genderless
            };

            // ���� �ִϸ��̼� ������Ʈ
            var frames = p.isShiny ? form.visual.shinyFrontFrames : form.visual.frontFrames;
            var fps = p.isShiny ? form.visual.shinyFrontAnimationFps : form.visual.frontAnimationFps;
            StartAnimation(frames, fps);

            // ����, ������, ����ġ, Ÿ�� ������ ������Ʈ
            infoZoneUI.levelText.text = $"{p.level}";
            infoZoneUI.heldItemText.text = string.IsNullOrWhiteSpace(p.heldItemId) ? "����" : p.heldItemId; // TODO: ������ �̸��� DB���� ���������� ����
            infoZoneUI.expText.text = $"{p.currentExp} / {ExperienceCurveService.GetNeedExpForNextLevel(species.curveType, p.level)}";

            // Ÿ�� ������ ������Ʈ (����/��� Ÿ�Կ� ���� �ٸ��� ǥ��)
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
        /// InfoZone�� UI�� �ʱ�ȭ�մϴ�.
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
        /// ���ϸ� �ִϸ��̼��� ����մϴ�.
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
        /// �ִϸ��̼� �ڷ�ƾ�� �����մϴ�.
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
    /// PokePC UI�� InfoZone�� �ʿ��� UI ������Ʈ���� ��Ƴ��� Ŭ����.
    /// </summary>
    [System.Serializable]
    public class PokeInfoZoneUI
    {
        public TextMeshProUGUI nameText;
        public Image genderIcon;
        public Image pokemonFrontImage; // ���� �ִϸ��̼� �̹���
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI heldItemText;
        public TextMeshProUGUI expText;
        public Image singlePrimaryTypeIcon; // ���� Ÿ�� ������
        public Image dualPrimaryTypeIcon;   // ��� Ÿ�� (����) ������
        public Image dualSecondaryTypeIcon; // ��� Ÿ�� (����) ������
    }
}