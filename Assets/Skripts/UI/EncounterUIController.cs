// ����: Scripts/UI/EncounterUIController.cs
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

        private PokemonSaveData _wildPokemon; // ���� ��Ÿ�� �߻� ���ϸ�

        private void Awake()
        {
            catchButton.onClick.AddListener(OnCatchButtonClick);
        }

        /// <summary>
        /// GameProgressController�� ȣ���� �޼���. �߻� ���ϸ� UI�� ǥ���մϴ�.
        /// </summary>
        public void ShowEncounter(PokemonSaveData wildPokemon)
        {
            _wildPokemon = wildPokemon;
            if (_wildPokemon == null) return;

            var species = speciesDB.GetSpecies(_wildPokemon.speciesId);
            var form = species.GetForm(_wildPokemon.formKey);

            // UI ������Ʈ
            nameText.text = species.nameKeyKor;
            levelText.text = $"{_wildPokemon.level}";
            pokemonImage.sprite = _wildPokemon.isShiny ? form.visual.shinyIcon : form.visual.icon;

            // �г� Ȱ��ȭ
            gameObject.SetActive(true);
        }

        /// <summary>
        /// '���' ��ư�� ������ �� ȣ��˴ϴ�.
        /// </summary>
        private void OnCatchButtonClick()
        {
            if (_wildPokemon != null && ownedPokemonManager != null)
            {
                // ���ϸ��� ���� ��Ͽ� �߰�
                ownedPokemonManager.Add(_wildPokemon);
            }

            // �г� ��Ȱ��ȭ
            gameObject.SetActive(false);
        }
    }
}