// ����: Scripts/UI/MainUIController.cs
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PokeClicker
{
    /// <summary>
    /// ���� UI�� �����ϴ� ��Ʈ�ѷ�.
    /// ��Ƽ�� 1�� ���ϸ� ������ �ִϸ��̼��� ǥ���մϴ�.
    /// </summary>
    public class MainUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image pokemonFrontImage; // ���ϸ� ���� �ִϸ��̼�
        [SerializeField] private TextMeshProUGUI nameText; // ���ϸ� �̸� (����)
        [SerializeField] private TextMeshProUGUI levelText; // ���ϸ� ����
        [SerializeField] private TextMeshProUGUI currentExpText; // ���� ���� �� ���� ����ġ
        [SerializeField] private Image expBar; // ����ġ ��
        [SerializeField] private Button menuButton; // Ȯ�� UI�� ���� ��ư

        [Header("Dependencies")]
        [SerializeField] private OwnedPokemonManager ownedPokemonManager;
        [SerializeField] private SpeciesDB speciesDB;
        [SerializeField] private PokemonLevelupManager levelupManager;

        private PokemonSaveData _currentPokemon;
        private Coroutine _playAnimationCoroutine;

        private void OnEnable()
        {
            if (menuButton != null)
            {
                menuButton.onClick.AddListener(OnMenuButtonClick);
            }

            if (ownedPokemonManager != null)
            {
                ownedPokemonManager.OnPartyUpdated += HandlePartyUpdate;
            }

            if (levelupManager != null)
            {
                levelupManager.OnExpGained += HandleExpGained;
                levelupManager.OnLevelUp += HandleLevelUp;
            }
        }

        private void OnDisable()
        {
            if (menuButton != null)
            {
                menuButton.onClick.RemoveListener(OnMenuButtonClick);
            }

            if (ownedPokemonManager != null)
            {
                ownedPokemonManager.OnPartyUpdated -= HandlePartyUpdate;
            }

            if (levelupManager != null)
            {
                levelupManager.OnExpGained -= HandleExpGained;
                levelupManager.OnLevelUp -= HandleLevelUp;
            }
        }

        /// <summary>
        /// �޴� ��ư Ŭ�� �� ȣ��˴ϴ�. Ȯ�� UI�� Ȱ��ȭ�մϴ�.
        /// </summary>
        private void OnMenuButtonClick()
        {
            UIManager.Instance.SetExpandedPanelActive(true);
        }

        /// <summary>
        /// ��Ƽ�� ù ��° ���ϸ��� ������� UI�� �����մϴ�.
        /// </summary>
        public void UpdateMainUI()
        {
            if (ownedPokemonManager == null || speciesDB == null)
            {
                ClearUI();
                return;
            }

            var party = ownedPokemonManager.GetParty();
            if (party.Length == 0)
            {
                ClearUI();
                return;
            }

            int firstPokemonUid = ownedPokemonManager.GetParty()[0];
            _currentPokemon = ownedPokemonManager.GetByPuid(firstPokemonUid);

            if (_currentPokemon == null)
            {
                ClearUI();
                return;
            }

            var species = speciesDB.GetSpecies(_currentPokemon.speciesId);
            if (species == null)
            {
                ClearUI();
                return;
            }

            var form = species.GetForm(_currentPokemon.formKey);
            if (form == null || form.visual == null)
            {
                ClearUI();
                return;
            }

            // ���ϸ� ���� ǥ��
            nameText.text = _currentPokemon.GetDisplayName(species);
            levelText.text = $"{_currentPokemon.level}";

            // ����ġ �� ������Ʈ
            if (_currentPokemon.level < species.maxLevel)
            {
                int needExp = ExperienceCurveService.GetNeedExpForNextLevel(species.curveType, _currentPokemon.level);
                currentExpText.text = $"{_currentPokemon.currentExp} / {needExp}";
                expBar.fillAmount = needExp > 0 ? (float)_currentPokemon.currentExp / needExp : 0;
            }
            else
            {
                currentExpText.text = "MAX";
                expBar.fillAmount = 1;
            }

            // ���ϸ� �ִϸ��̼� ǥ��
            var frames = _currentPokemon.isShiny ? form.visual.shinyFrontFrames : form.visual.frontFrames;
            var fps = _currentPokemon.isShiny ? form.visual.shinyFrontAnimationFps : form.visual.frontAnimationFps;

            StartAnimation(frames, fps);
        }

        /// <summary>
        /// ����ġ ���� UI�� ������Ʈ �մϴ�.
        /// �������� �ִ� ������Ʈ�� ��� ���� �Է½� �ִϰ� ù���������� ��� ������Ʈ��
        /// </summary>
        private void UpdateExpUI()
        {
            if (_currentPokemon == null || speciesDB == null) return;

            var species = speciesDB.GetSpecies(_currentPokemon.speciesId);
            if (species == null) return;

            // UpdateMainUI���� ����ġ ���� ������ �����ɴϴ�.
            if (_currentPokemon.level < species.maxLevel)
            {
                int needExp = ExperienceCurveService.GetNeedExpForNextLevel(species.curveType, _currentPokemon.level);
                currentExpText.text = $"{_currentPokemon.currentExp}/{needExp}";
                expBar.fillAmount = needExp > 0 ? (float)_currentPokemon.currentExp / needExp : 0;
            }
            else
            {
                currentExpText.text = "MAX";
                expBar.fillAmount = 1;
            }
        }
        /// <summary>
        /// UI�� �ʱ�ȭ�ϰ� ��� ������ ����ϴ�.
        /// </summary>
        private void ClearUI()
        {
            if (nameText != null) nameText.text = string.Empty;
            if (levelText != null) levelText.text = string.Empty;
            if (currentExpText != null) currentExpText.text = string.Empty;
            if (expBar != null) expBar.fillAmount = 0;
            if (pokemonFrontImage != null) pokemonFrontImage.sprite = null;

            if (_playAnimationCoroutine != null)
            {
                StopCoroutine(_playAnimationCoroutine);
                _playAnimationCoroutine = null;
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
                pokemonFrontImage.enabled = false;
                return;
            }

            pokemonFrontImage.enabled = true;
            _playAnimationCoroutine = StartCoroutine(PlayAnimation(frames, fps));
        }

        private IEnumerator PlayAnimation(Sprite[] frames, float fps)
        {
            float delay = 1f / Mathf.Max(1f, fps);
            int i = 0;
            while (true)
            {
                pokemonFrontImage.sprite = frames[i];
                i = (i + 1) % frames.Length;
                yield return new WaitForSeconds(delay);
            }
        }

        /// <summary>
        /// ����ġ ȹ�� �̺�Ʈ�� �߻����� �� ȣ��˴ϴ�.
        /// </summary>
        private void HandleExpGained(int puid)
        {
            // ���� UI�� �׻� ��Ƽ�� ù ��° ���ϸ��� ǥ���ϹǷ�,
            // � ���ϸ��� ����ġ�� ����� ������� UI ��ü�� �����ϸ� �˴ϴ�.
            UpdateExpUI();
        }

        /// <summary>
        /// ���ϸ��� ���������� �� ȣ��˴ϴ�.
        /// </summary>
        private void HandleLevelUp(int puid, int newLevel)
        {
            // �� UI�� ǥ�õ� ���ϸ��� �������ߴ��� Ȯ���մϴ�.
            if (_currentPokemon != null && puid == _currentPokemon.P_uid)
            {
                // ����, ����ġ �� �� ��� ������ �ѹ��� �����ϱ� ����
                // UpdateMainUI()�� ȣ���մϴ�.
                UpdateMainUI();
            }
        }

        /// <summary>
        /// ��Ƽ ������ ����Ǿ��� �� ȣ��˴ϴ�.
        /// </summary>
        private void HandlePartyUpdate()
        {
            if (ownedPokemonManager == null) return;

            var party = ownedPokemonManager.GetParty();

            // ��Ƽ�� ����ų� 1�� ������ ����� ���
            if (party.Length == 0 || party[0] == 0)
            {
                if (_currentPokemon != null) ClearUI(); // ������ ���ϸ��� �־��ٸ� UI�� ���
                return;
            }

            int newFirstPokemonPuid = party[0];

            // 1�� ���ϸ��� �ٲ������, Ȥ�� ���� ǥ�õ� ���ϸ��� ������ Ȯ��
            if (_currentPokemon == null || _currentPokemon.P_uid != newFirstPokemonPuid)
            {
                // ��ǥ ���ϸ��� �ٲ�����Ƿ� ��ü UI�� ���� (�ִϸ��̼� ����)
                UpdateMainUI();
            }
            else
            {
                // ��ǥ ���ϸ��� �״������ ������ �� �ٸ� ������ �ٲ���� �� �����Ƿ� ����ġ UI�� ����
                UpdateExpUI();
            }
        }
    }
}