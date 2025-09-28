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

            // 포켓몬 정보 표시
            nameText.text = _currentPokemon.GetDisplayName(species);
            levelText.text = $"{_currentPokemon.level}";

            // 경험치 바 업데이트
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

            // 포켓몬 애니메이션 표시
            var frames = _currentPokemon.isShiny ? form.visual.shinyFrontFrames : form.visual.frontFrames;
            var fps = _currentPokemon.isShiny ? form.visual.shinyFrontAnimationFps : form.visual.frontAnimationFps;

            StartAnimation(frames, fps);
        }

        /// <summary>
        /// 경험치 관련 UI만 업데이트 합니다.
        /// 지속적인 애니 업데이트의 경우 연속 입력시 애니가 첫프레임으로 계속 업데이트함
        /// </summary>
        private void UpdateExpUI()
        {
            if (_currentPokemon == null || speciesDB == null) return;

            var species = speciesDB.GetSpecies(_currentPokemon.speciesId);
            if (species == null) return;

            // UpdateMainUI에서 경험치 관련 로직만 가져옵니다.
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

        /// <summary>
        /// 경험치 획득 이벤트가 발생했을 때 호출됩니다.
        /// </summary>
        private void HandleExpGained(int puid)
        {
            // 메인 UI는 항상 파티의 첫 번째 포켓몬을 표시하므로,
            // 어떤 포켓몬이 경험치를 얻었든 상관없이 UI 전체를 갱신하면 됩니다.
            UpdateExpUI();
        }

        /// <summary>
        /// 포켓몬이 레벨업했을 때 호출됩니다.
        /// </summary>
        private void HandleLevelUp(int puid, int newLevel)
        {
            // 이 UI에 표시된 포켓몬이 레벨업했는지 확인합니다.
            if (_currentPokemon != null && puid == _currentPokemon.P_uid)
            {
                // 레벨, 경험치 바 등 모든 정보를 한번에 갱신하기 위해
                // UpdateMainUI()를 호출합니다.
                UpdateMainUI();
            }
        }

        /// <summary>
        /// 파티 정보가 변경되었을 때 호출됩니다.
        /// </summary>
        private void HandlePartyUpdate()
        {
            if (ownedPokemonManager == null) return;

            var party = ownedPokemonManager.GetParty();

            // 파티가 비었거나 1번 슬롯이 비었을 경우
            if (party.Length == 0 || party[0] == 0)
            {
                if (_currentPokemon != null) ClearUI(); // 이전에 포켓몬이 있었다면 UI를 비움
                return;
            }

            int newFirstPokemonPuid = party[0];

            // 1번 포켓몬이 바뀌었는지, 혹은 아직 표시된 포켓몬이 없는지 확인
            if (_currentPokemon == null || _currentPokemon.P_uid != newFirstPokemonPuid)
            {
                // 대표 포켓몬이 바뀌었으므로 전체 UI를 갱신 (애니메이션 포함)
                UpdateMainUI();
            }
            else
            {
                // 대표 포켓몬은 그대로지만 레벨업 등 다른 정보가 바뀌었을 수 있으므로 경험치 UI만 갱신
                UpdateExpUI();
            }
        }
    }
}