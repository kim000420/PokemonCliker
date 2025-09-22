// ����: Scripts/UI/SummaryUIController.cs
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PokeClicker
{
    /// <summary>
    /// ���ϸ��� �� ����(Summary)�� ǥ���ϴ� UI ��Ʈ�ѷ�.
    /// Info Main�� Info Stat �� ���� ���� �����ϸ�, �ִϸ��̼� ����� ���� ó���մϴ�.
    /// </summary>
    public class SummaryUIController : MonoBehaviour
    {
        [Header("UI Elements (Info Overlay)")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image genderIcon;
        [SerializeField] private Image pokemonFrontImage; // ���ϸ� �ִϸ��̼� �̹���
        [SerializeField] private Image heldItemIcon; // ���� ���� ������

        [Header("Info Main Panel")]
        [SerializeField] private GameObject infoMainPanel;
        [SerializeField] private TextMeshProUGUI speciesIdText;
        [SerializeField] private TextMeshProUGUI speciesNameText;
        [SerializeField] private Image typeIconA;
        [SerializeField] private Image typeIconB;
        [SerializeField] private TextMeshProUGUI trainerNameText;
        [SerializeField] private TextMeshProUGUI friendshipText;
        [SerializeField] private TextMeshProUGUI currentExpText;
        [SerializeField] private TextMeshProUGUI nextLevelExpText;
        [SerializeField] private Image expBar;

        [Header("Info Stat Panel")]
        [SerializeField] private GameObject infoStatPanel;
        [SerializeField] private TextMeshProUGUI hpStatText;
        [SerializeField] private TextMeshProUGUI atkStatText;
        [SerializeField] private TextMeshProUGUI defStatText;
        [SerializeField] private TextMeshProUGUI spaStatText;
        [SerializeField] private TextMeshProUGUI spdStatText;
        [SerializeField] private TextMeshProUGUI speStatText;

        [Header("IV Icons")]
        [SerializeField] private List<Image> ivRankIcons;
        [SerializeField] private List<Image> ivStarIcons;

        [Header("Dependencies")]
        [SerializeField] private SpeciesDB speciesDB;
        [SerializeField] private GameIconDB gameIconDB;

        private PokemonSaveData _currentPokemon;
        private SpeciesSO _currentSpecies;
        private Coroutine _playAnimationCoroutine;

        private void OnEnable()
        {
            ShowMainPanel();
        }

        /// <summary>
        /// SummaryUI�� ǥ���� ���ϸ� �����͸� �����ϰ� UI�� �����մϴ�.
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
        /// Info Overlay �г��� UI�� �����մϴ�.
        /// </summary>
        private void UpdateInfoOverlay()
        {
            nameText.text = _currentPokemon.GetDisplayName(_currentSpecies);
            levelText.text = $"Lv.{_currentPokemon.level}";

            // ���� ������ ������Ʈ
            if (genderIcon != null && gameIconDB.miscIcons != null)
            {
                genderIcon.sprite = _currentPokemon.gender switch
                {
                    Gender.Male => gameIconDB.miscIcons.male,
                    Gender.Female => gameIconDB.miscIcons.female,
                    _ => gameIconDB.miscIcons.genderless
                };
            }

            // ���ϸ� �ִϸ��̼� ǥ��
            var form = _currentSpecies.GetForm(_currentPokemon.formKey);
            if (form != null && form.visual != null)
            {
                var frames = _currentPokemon.isShiny ? form.visual.shinyFrontFrames : form.visual.frontFrames;
                var fps = _currentPokemon.isShiny ? form.visual.shinyFrontAnimationFps : form.visual.frontAnimationFps;
                StartAnimation(frames, fps);
            }
        }

        /// <summary>
        /// Info Main �г��� UI�� �����մϴ�.
        /// </summary>
        private void UpdateInfoMainPanel()
        {
            speciesIdText.text = _currentSpecies.speciesId.ToString();
            speciesNameText.text = _currentSpecies.nameKeyKor;

            // Ÿ�� ������ ������Ʈ
            var form = _currentSpecies.GetForm(_currentPokemon.formKey);
            //if (form != null && gameIconDB != null)
            {
                // TODO: TypeIconDB�� GameIconDB�� ���յ� ����
                // typeIconA.sprite = gameIconDB.typeIcons.GetIcon(form.typePair.primary);
                // typeIconB.sprite = form.typePair.hasDualType ? gameIconDB.typeIcons.GetIcon(form.typePair.secondary) : null;
                // typeIconB.gameObject.SetActive(form.typePair.hasDualType);
            }

            // TODO: Ʈ���̳� �̸� ��������
            trainerNameText.text = "TODO";
            friendshipText.text = _currentPokemon.friendship.ToString();

            // ����ġ ���� ������Ʈ
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
        /// Info Stat �г��� UI�� �����մϴ�.
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

            hpStatText.text = derivedStats.hp.ToString();
            atkStatText.text = derivedStats.atk.ToString();
            defStatText.text = derivedStats.def.ToString();
            spaStatText.text = derivedStats.spa.ToString();
            spdStatText.text = derivedStats.spd.ToString();
            speStatText.text = derivedStats.spe.ToString();

            UpdateIvIconsForStat(0, _currentPokemon.ivs.hp);
            UpdateIvIconsForStat(1, _currentPokemon.ivs.atk);
            UpdateIvIconsForStat(2, _currentPokemon.ivs.def);
            UpdateIvIconsForStat(3, _currentPokemon.ivs.spa);
            UpdateIvIconsForStat(4, _currentPokemon.ivs.spd);
            UpdateIvIconsForStat(5, _currentPokemon.ivs.spe);
        }

        /// <summary>
        /// Ư�� ���� ���� IV �����ܵ��� �����մϴ�.
        /// </summary>
        private void UpdateIvIconsForStat(int statIndex, int ivValue)
        {
            if (gameIconDB.miscIcons == null || statIndex >= ivRankIcons.Count || statIndex >= ivStarIcons.Count) return;

            var rankIcon = ivRankIcons[statIndex];
            var starIcon = ivStarIcons[statIndex];

            var rankSprite = gameIconDB.miscIcons.GetIvRankIcon(ivValue);
            rankIcon.sprite = rankSprite;
            rankIcon.enabled = rankSprite != null;

            var starSprite = gameIconDB.miscIcons.GetIvStarIcon(ivValue);
            starIcon.sprite = starSprite;
            starIcon.enabled = starSprite != null;
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
        /// UI�� �ʱ�ȭ�մϴ�.
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
            if (typeIconA != null) typeIconA.sprite = null;
            if (typeIconB != null) typeIconB.sprite = null;
            if (trainerNameText != null) trainerNameText.text = string.Empty;
            if (friendshipText != null) friendshipText.text = string.Empty;
            if (currentExpText != null) currentExpText.text = string.Empty;
            if (nextLevelExpText != null) nextLevelExpText.text = string.Empty;
            if (expBar != null) expBar.fillAmount = 0;

            // Info Stat
            if (hpStatText != null) hpStatText.text = string.Empty;
            if (atkStatText != null) atkStatText.text = string.Empty;
            if (defStatText != null) defStatText.text = string.Empty;
            if (spaStatText != null) spaStatText.text = string.Empty;
            if (spdStatText != null) spdStatText.text = string.Empty;
            if (speStatText != null) speStatText.text = string.Empty;

            // IV ������ �ʱ�ȭ
            foreach (var icon in ivRankIcons) icon.enabled = false;
            foreach (var icon in ivStarIcons) icon.enabled = false;
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
}