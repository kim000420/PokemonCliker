// 파일: Scripts/UI/SummaryUIController.cs
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PokeClicker
{
    /// <summary>
    /// 포켓몬의 상세 정보(Summary)를 표시하는 UI 컨트롤러.
    /// Info Main과 Info Stat 두 개의 탭을 관리하며, 애니메이션 재생을 직접 처리합니다.
    /// </summary>
    public class SummaryUIController : MonoBehaviour
    {
        [Header("UI Elements (Info Overlay)")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image genderIcon;
        [SerializeField] private Image pokemonFrontImage; // 포켓몬 애니메이션 이미지
        [SerializeField] private Image heldItemIcon; // 가진 도구 아이콘

        [Header("Info Main Panel")]
        [SerializeField] private GameObject infoMainPanel;
        [SerializeField] private TextMeshProUGUI speciesIdText;
        [SerializeField] private TextMeshProUGUI speciesNameText;

        [SerializeField] public Image singlePrimaryTypeIcon; // 단일 타입 아이콘
        [SerializeField] public Image dualPrimaryTypeIcon;   // 듀얼 타입 (좌측) 아이콘
        [SerializeField] public Image dualSecondaryTypeIcon; // 듀얼 타입 (우측) 아이콘

        [SerializeField] private TextMeshProUGUI trainerNameText;
        [SerializeField] private TextMeshProUGUI friendshipText;
        [SerializeField] private TextMeshProUGUI currentExpText;
        [SerializeField] private TextMeshProUGUI nextLevelExpText;
        [SerializeField] private Image expBar;

        [Header("Info Stat Panel")]
        [SerializeField] private GameObject infoStatPanel;
        [SerializeField] private List<InfoStatSlot> slots = new List<InfoStatSlot>();

        [Header("Dependencies")]
        [SerializeField] private SpeciesDB speciesDB;
        [SerializeField] private GameIconDB gameIconDB;
        [SerializeField] private PokemonTrainerManager trainerManager;

        private PokemonSaveData _currentPokemon;
        private SpeciesSO _currentSpecies;
        private Coroutine _playAnimationCoroutine;

        private void OnEnable()
        {
            ShowMainPanel();
        }

        /// <summary>
        /// SummaryUI에 표시할 포켓몬 데이터를 설정하고 UI를 갱신합니다.
        /// </summary>
        public void SetPokemon(PokemonSaveData pokemon)
        {
            _currentPokemon = pokemon;
            if (_currentPokemon == null)
            {
                ClearUI();
                return;
            }

            _currentSpecies = speciesDB.GetSpecies(pokemon.speciesId);
            if (_currentSpecies == null)
            {
                ClearUI();
                return;
            }

            UpdateInfoOverlay();
            UpdateInfoMainPanel();
            UpdateInfoStatPanel();
        }

        /// <summary>
        /// Info Overlay 패널의 UI를 갱신합니다.
        /// </summary>
        private void UpdateInfoOverlay()
        {
            nameText.text = _currentPokemon.GetDisplayName(_currentSpecies);
            levelText.text = $"Lv.{_currentPokemon.level}";

            // 성별 아이콘 업데이트
            if (genderIcon != null && gameIconDB.miscIcons != null)
            {
                genderIcon.sprite = _currentPokemon.gender switch
                {
                    Gender.Male => gameIconDB.miscIcons.male,
                    Gender.Female => gameIconDB.miscIcons.female,
                    _ => gameIconDB.miscIcons.genderless
                };
            }

            // 포켓몬 애니메이션 표시
            var form = _currentSpecies.GetForm(_currentPokemon.formKey);
            if (form != null && form.visual != null)
            {
                var frames = _currentPokemon.isShiny ? form.visual.shinyFrontFrames : form.visual.frontFrames;
                var fps = _currentPokemon.isShiny ? form.visual.shinyFrontAnimationFps : form.visual.frontAnimationFps;
                StartAnimation(frames, fps);
            }
        }

        /// <summary>
        /// Info Main 패널의 UI를 갱신합니다.
        /// </summary>
        private void UpdateInfoMainPanel()
        {
            speciesIdText.text = _currentSpecies.speciesId.ToString();
            speciesNameText.text = _currentSpecies.nameKeyKor;

            // 타입 아이콘 업데이트
            var form = _currentSpecies.GetForm(_currentPokemon.formKey);
            if (form != null && gameIconDB != null && gameIconDB.typeIcons != null)
            {
                bool hasDualType = form.typePair.hasDualType;
                singlePrimaryTypeIcon.gameObject.SetActive(!hasDualType);
                dualPrimaryTypeIcon.gameObject.SetActive(hasDualType);
                dualSecondaryTypeIcon.gameObject.SetActive(hasDualType);

                if (!hasDualType)
                {
                    singlePrimaryTypeIcon.sprite = gameIconDB.typeIcons.GetIcon(form.typePair.primary);
                }
                else
                {
                    dualPrimaryTypeIcon.sprite = gameIconDB.typeIcons.GetIcon(form.typePair.primary);
                    dualSecondaryTypeIcon.sprite = gameIconDB.typeIcons.GetIcon(form.typePair.secondary);
                }
            }

            // TODO: 트레이너 이름 가져오기
            trainerNameText.text = trainerManager.TrainerName;
            friendshipText.text = _currentPokemon.friendship.ToString();

            // 경험치 정보 업데이트
            if (_currentPokemon.level < _currentSpecies.maxLevel)
            {
                int needExp = ExperienceCurveService.GetNeedExpForNextLevel(_currentSpecies.curveType, _currentPokemon.level);
                currentExpText.text = _currentPokemon.currentExp.ToString();
                nextLevelExpText.text = needExp.ToString();
                expBar.fillAmount = (float)_currentPokemon.currentExp / needExp;
            }
            else
            {
                currentExpText.text = "MAX";
                nextLevelExpText.text = "MAX";
                expBar.fillAmount = 1;
            }
        }

        /// <summary>
        /// Info Stat 패널의 UI를 갱신합니다.
        /// </summary>
        private void UpdateInfoStatPanel()
        {
            var form = _currentSpecies.GetForm(_currentPokemon.formKey);
            if (form == null) return;

            var derivedStats = StatService.ComputeFor(
                _currentPokemon,
                _currentSpecies,
                form.formKey,
                _currentPokemon.ivs,
                _currentPokemon.nature,
                true,
                true);

            if (slots.Count >= 6)
            {
                var stats = new List<int> {
                    derivedStats.hp, derivedStats.atk, derivedStats.def,
                    derivedStats.spa, derivedStats.spd, derivedStats.spe
                };
                var ivs = new List<int> {
                    _currentPokemon.ivs.hp, _currentPokemon.ivs.atk, _currentPokemon.ivs.def,
                    _currentPokemon.ivs.spa, _currentPokemon.ivs.spd, _currentPokemon.ivs.spe
                };

                for (int i = 0; i < 6; i++)
                {
                    slots[i].SetData(stats[i], ivs[i], gameIconDB.miscIcons);
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
        /// UI를 초기화합니다.
        /// </summary>
        private void ClearUI()
        {
            if (nameText != null) nameText.text = string.Empty;
            if (levelText != null) levelText.text = string.Empty;
            if (genderIcon != null) genderIcon.sprite = null;
            if (heldItemIcon != null) heldItemIcon.sprite = null;
            if (pokemonFrontImage != null) pokemonFrontImage.sprite = null;

            if (_playAnimationCoroutine != null)
            {
                StopCoroutine(_playAnimationCoroutine);
                _playAnimationCoroutine = null;
            }

            // Info Main
            if (speciesIdText != null) speciesIdText.text = string.Empty;
            if (speciesNameText != null) speciesNameText.text = string.Empty;
            if (singlePrimaryTypeIcon != null) singlePrimaryTypeIcon.sprite = null;
            if (dualPrimaryTypeIcon != null) dualPrimaryTypeIcon.sprite = null;
            if (dualSecondaryTypeIcon != null) dualSecondaryTypeIcon.sprite = null;
            if (trainerNameText != null) trainerNameText.text = string.Empty;
            if (friendshipText != null) friendshipText.text = string.Empty;
            if (currentExpText != null) currentExpText.text = string.Empty;
            if (nextLevelExpText != null) nextLevelExpText.text = string.Empty;
            if (expBar != null) expBar.fillAmount = 0;

            // Stat Slot
            foreach (var slot in slots)
            {
                slot.Clear();
            }
        }

        public void ShowMainPanel()
        {
            infoMainPanel.SetActive(true);
            infoStatPanel.SetActive(false);
        }

        public void ShowStatPanel()
        {
            infoMainPanel.SetActive(false);
            infoStatPanel.SetActive(true);
        }
    }

    [System.Serializable]
    public class InfoStatSlot
    {
        public TextMeshProUGUI statText;

        public Image IVsStarIcon;
        public Image IVsRankIcon; 


        /// <summary>
        /// 슬롯에 능력치 데이터 설정
        /// </summary>
        public void SetData(int statValue, int ivValue, MiscIconSO miscIcons)
        {
            statText.text = statValue.ToString();
            IVsRankIcon.sprite = miscIcons.GetIvRankIcon(ivValue);
            IVsStarIcon.sprite = miscIcons.GetIvStarIcon(ivValue);
        }

        /// <summary>
        /// 슬롯 초기화
        /// </summary>
        public void Clear()
        {
            statText.text = string.Empty;
            IVsRankIcon.sprite = null;
            IVsStarIcon.sprite = null;
        }
    }
}