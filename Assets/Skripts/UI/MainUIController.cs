// 파일: Scripts/UI/MainUIController.cs
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PokeClicker
{
    /// <summary>
    /// 메인 UI를 관리하는 컨트롤러.
    /// 파티의 1번 포켓몬 정보와 애니메이션을 표시합니다.
    /// </summary>
    public class MainUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image pokemonFrontImage; // 포켓몬 전면 애니메이션
        [SerializeField] private TextMeshProUGUI nameText; // 포켓몬 이름 (별명)
        [SerializeField] private TextMeshProUGUI levelText; // 포켓몬 레벨
        [SerializeField] private TextMeshProUGUI currentExpText; // 현재 레벨 내 누적 경험치
        [SerializeField] private Image expBar; // 경험치 바
        [SerializeField] private Button menuButton; // 확장 UI를 여는 버튼

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

            // UI가 활성화되면 파티 1번 포켓몬 정보 업데이트
            UpdateMainUI();
        }

        private void OnDisable()
        {
            if (menuButton != null)
            {
                menuButton.onClick.RemoveListener(OnMenuButtonClick);
            }

            // 애니메이션 코루틴 정지
            if (_playAnimationCoroutine != null)
            {
                StopCoroutine(_playAnimationCoroutine);
                _playAnimationCoroutine = null;
            }
        }

        /// <summary>
        /// 메뉴 버튼 클릭 시 호출됩니다. 확장 UI를 활성화합니다.
        /// </summary>
        private void OnMenuButtonClick()
        {
            UIManager.Instance.SetExpandedPanelActive(true);
        }

        /// <summary>
        /// 파티의 첫 번째 포켓몬을 기반으로 UI를 갱신합니다.
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
                // TODO: SpeciesSO 참조 필요
                // var needExp = ExperienceCurveService.GetNeedExpForNextLevel(species.curveType, _currentPokemon.level);
                // expBar.fillAmount = needExp > 0 ? (float)_currentPokemon.currentExp / needExp : 0;
            }
            else
            {
                expBar.fillAmount = 1; // 만렙
            }

            // TODO: 포켓몬 애니메이션 표시 (VisualSO 참조 필요)
            // StartAnimation(visual.frontFrames, visual.frontAnimationFps
        }

        /// <summary>
        /// UI를 초기화하고 모든 정보를 지웁니다.
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