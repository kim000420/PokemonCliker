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

        private PokemonSaveData _currentPokemon;
        private Coroutine _playAnimationCoroutine;

        private void OnEnable()
        {
            if (menuButton != null)
            {
                menuButton.onClick.AddListener(OnMenuButtonClick);
            }

            // UI�� Ȱ��ȭ�Ǹ� ��Ƽ 1�� ���ϸ� ���� ������Ʈ
            UpdateMainUI();
        }

        private void OnDisable()
        {
            if (menuButton != null)
            {
                menuButton.onClick.RemoveListener(OnMenuButtonClick);
            }

            // �ִϸ��̼� �ڷ�ƾ ����
            if (_playAnimationCoroutine != null)
            {
                StopCoroutine(_playAnimationCoroutine);
                _playAnimationCoroutine = null;
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
            if (ownedPokemonManager == null || ownedPokemonManager.Party.Count == 0)
            {
                ClearUI();
                return;
            }

            int firstPokemonUid = ownedPokemonManager.Party[0];
            _currentPokemon = ownedPokemonManager.GetByPuid(firstPokemonUid);

            if (_currentPokemon == null)
            {
                ClearUI();
                return;
            }

            nameText.text = _currentPokemon.GetDisplayName();
            levelText.text = $"Lv.{_currentPokemon.level}";
            if (_currentPokemon.level < 100)
            {
                // TODO: SpeciesSO ���� �ʿ�
                // var needExp = ExperienceCurveService.GetNeedExpForNextLevel(species.curveType, _currentPokemon.level);
                // expBar.fillAmount = needExp > 0 ? (float)_currentPokemon.currentExp / needExp : 0;
            }
            else
            {
                expBar.fillAmount = 1; // ����
            }

            // TODO: ���ϸ� �ִϸ��̼� ǥ�� (VisualSO ���� �ʿ�)
            // StartAnimation(visual.frontFrames, visual.frontAnimationFps
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
    }
}