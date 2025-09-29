// 파일: Scripts/UI/EncounterUIController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PokeClicker
{
    public class EncounterUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image pokemonImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Button catchButton;

        [Header("Dependencies")]
        [SerializeField] private OwnedPokemonManager ownedPokemonManager;
        [SerializeField] private SpeciesDB speciesDB;

        private PokemonSaveData _wildPokemon; // 현재 나타난 야생 포켓몬

        private void Awake()
        {
            catchButton.onClick.AddListener(OnCatchButtonClick);
        }

        /// <summary>
        /// GameProgressController가 호출할 메서드. 야생 포켓몬 UI를 표시합니다.
        /// </summary>
        public void ShowEncounter(PokemonSaveData wildPokemon)
        {
            _wildPokemon = wildPokemon;
            if (_wildPokemon == null) return;

            var species = speciesDB.GetSpecies(_wildPokemon.speciesId);
            var form = species.GetForm(_wildPokemon.formKey);

            // UI 업데이트
            nameText.text = species.nameKeyKor;
            levelText.text = $"{_wildPokemon.level}";
            pokemonImage.sprite = _wildPokemon.isShiny ? form.visual.shinyIcon : form.visual.icon;

            // 패널 활성화
            gameObject.SetActive(true);
        }

        /// <summary>
        /// '잡기' 버튼을 눌렀을 때 호출됩니다.
        /// </summary>
        private void OnCatchButtonClick()
        {
            if (_wildPokemon != null && ownedPokemonManager != null)
            {
                // 포켓몬을 소유 목록에 추가
                ownedPokemonManager.Add(_wildPokemon);
            }

            // 패널 비활성화
            gameObject.SetActive(false);
        }
    }
}