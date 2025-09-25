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
        [SerializeField] private GameIconDB gameIconDB; // Ÿ�� ������ �� ��Ÿ ������ ����
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
                // ��Ƽ ������Ʈ �� �ڽ� �� ����
                ownedPokemonManager.OnPartyUpdated += UpdateBoxZone; 

                // UI�� Ȱ��ȭ�Ǹ� ù ���ϸ� ������ InfoZone�� ǥ��
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
                    pokemonPopupUI.SetActive(false); // UI Ȱ��ȭ �� �˾� ��Ȱ��ȭ
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
                pokemonPopupUI.SetActive(false); // UI ��Ȱ��ȭ �� �˾� ��Ȱ��ȭ
            }
        }

        /// <summary>
        /// �ݱ� ��ư Ŭ�� �� ȣ��˴ϴ�.
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
        /// ���ϸ� ���� ��Ŭ�� �� ȣ��˴ϴ�.
        /// </summary>
        public void OnPokemonSlotLeftClick(int? puid, int boxIndex, int slotIndex)
        {
            if (pokemonPopupUI != null) pokemonPopupUI.SetActive(false);

            if (_selectedPuid.HasValue) // �̵� ��� Ȱ��ȭ ����
            {
                if (puid.HasValue) // ���ϸ��� �ִ� ������ Ŭ��
                {
                    ownedPokemonManager.Swap(_selectedPuid.Value, puid.Value);
                    _selectedPuid = null;
                }
                else // �� ������ Ŭ�� (�̵�)
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
            else // ���� ���� ���
            {
                if (puid.HasValue) // ���ϸ��� �ִ� ���� Ŭ��
                {
                    _selectedPuid = puid.Value;
                    UpdateInfoZone(puid.Value);
                }
                // �� ���� Ŭ���� �ƹ��͵� �� ��
            }

            UpdateBoxZone(); // UI ����
        }

        /// <summary>
        /// ���ϸ� ���� ��Ŭ�� �� ȣ��˴ϴ�.
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

            _popupPuid = puid; // �˾��� ������ PUID ����

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
            yield return null; // �� ������ ���
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
        /// BoxZone�� UI�� �����մϴ�.
        /// </summary>
        private void UpdateBoxZone()
        {
            if (ownedPokemonManager == null || speciesDB == null) return;

            // �ڽ� �̸� ������Ʈ
            boxZoneUI.boxNameText.text = $"�ڽ� {_currentBoxIndex + 1}";

            // �ڽ� ���� ����
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


            // ��Ƽ ���� ����
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

                slot.iconButton.onClick.RemoveAllListeners(); // ���� ������ ��� ����
                int? capturedPuid = puid;
                int capturedSlotIndex = i;
                slot.iconButton.onClick.AddListener(() => OnPokemonSlotLeftClick(capturedPuid, -1, capturedSlotIndex));
            }
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

    /// <summary>
    /// PokePC UI�� BoxZone�� �ʿ��� UI ������Ʈ���� ��Ƴ��� Ŭ����.
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
    /// PokePC UI�� ���ϸ� ������ ������ ���� Ŭ����.
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
            iconButton.gameObject.SetActive(true); // �� ���Ե� Ŭ�� �����ϰ� Ȱ��ȭ
            iconImage.gameObject.SetActive(false); // ������ �̹����� ��Ȱ��ȭ
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