// 파일: Scripts/UI/PokeShopUIController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PokeClicker
{
    public class PokeShopUIController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PokemonTrainerManager trainerManager;
        [SerializeField] private PokemonFactory pokemonFactory;
        [SerializeField] private ExpandedUIController expandedUIController;

        [Header("UI Elements")]
        [SerializeField] private Button closeButton;

        [Header("Shop item")]
        [SerializeField] private Button pullEventButton;
        [SerializeField] private TextMeshProUGUI pullEventCostText;
        [SerializeField] private Button pullAllButton;
        [SerializeField] private TextMeshProUGUI pullAllCostText;
        [SerializeField] private Button pullShinyButton;
        [SerializeField] private TextMeshProUGUI pullShinyCostText;
        [SerializeField] private Button pullLegendaryButton;
        [SerializeField] private TextMeshProUGUI pullLegendaryCostText;

        private void OnEnable()
        {
            // 상점 창이 켜질 때마다 UI를 갱신합니다.
            UpdateUI();

            // 각 버튼에 클릭 이벤트를 연결합니다.
            pullEventButton.onClick.AddListener(OnPullEventClick);
            pullAllButton.onClick.AddListener(OnPullAllClick);
            pullShinyButton.onClick.AddListener(OnPullShinyClick);
            pullLegendaryButton.onClick.AddListener(OnPullLegendaryClick);
            closeButton.onClick.AddListener(OnCloseButtonClick);
        }

        private void OnDisable()
        {
            // 상점 창이 꺼질 때 이벤트 연결을 해제합니다.
            pullEventButton.onClick.RemoveAllListeners();
            pullAllButton.onClick.RemoveAllListeners();
            pullShinyButton.onClick.RemoveAllListeners();
            pullLegendaryButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveListener(OnCloseButtonClick);
        }

        /// <summary>
        /// 몬스터볼 개수를 UI에 표시하고, 개수에 따라 버튼 활성화 여부를 결정합니다.
        /// </summary>
        public void UpdateUI()
        {
            var inventory = trainerManager.Profile.BallInventory;
            if (inventory == null) return;

            int premierCount = inventory.GetBallCount(BallId.PremierBall);
            pullEventCostText.text = "× " + premierCount;
            pullEventButton.interactable = premierCount > 0;

            int pokeCount = inventory.GetBallCount(BallId.PokeBall);
            pullAllCostText.text = "× " + pokeCount;
            pullAllButton.interactable = pokeCount > 0;

            int hyperCount = inventory.GetBallCount(BallId.HyperBall);
            pullShinyCostText.text = "× " + hyperCount;
            pullShinyButton.interactable = hyperCount > 0;

            int masterCount = inventory.GetBallCount(BallId.MasterBall);
            pullLegendaryCostText.text = "× " + masterCount;
            pullLegendaryButton.interactable = masterCount > 0;
        }

        // --- 버튼 클릭 핸들러 ---

        private void OnPullEventClick()
        {
            var inventory = trainerManager.Profile.BallInventory;
            if (inventory.GetBallCount(BallId.PremierBall) > 0)
            {
                inventory.AddBallCount(BallId.PremierBall, -1);
                var newPokemon = pokemonFactory.PullFromEventPool(); // 팩토리 호출
                Debug.Log($"{newPokemon.GetDisplayName(pokemonFactory.speciesDB.GetSpecies(newPokemon.speciesId))}을(를) 뽑았습니다!");
                UpdateUI(); // UI 갱신
            }
        }

        private void OnPullAllClick()
        {
            var inventory = trainerManager.Profile.BallInventory;
            if (inventory.GetBallCount(BallId.PokeBall) > 0)
            {
                inventory.AddBallCount(BallId.PokeBall, -1);
                var newPokemon = pokemonFactory.PullFromAllPool();
                Debug.Log($"{newPokemon.GetDisplayName(pokemonFactory.speciesDB.GetSpecies(newPokemon.speciesId))}을(를) 뽑았습니다!");
                UpdateUI();
            }
        }

        private void OnPullShinyClick()
        {
            var inventory = trainerManager.Profile.BallInventory;
            if (inventory.GetBallCount(BallId.HyperBall) > 0)
            {
                inventory.AddBallCount(BallId.HyperBall, -1);
                var newPokemon = pokemonFactory.PullFromShinyPool();
                Debug.Log($"[이로치!] {newPokemon.GetDisplayName(pokemonFactory.speciesDB.GetSpecies(newPokemon.speciesId))}을(를) 뽑았습니다!");
                UpdateUI();
            }
        }

        private void OnPullLegendaryClick()
        {
            var inventory = trainerManager.Profile.BallInventory;
            if (inventory.GetBallCount(BallId.MasterBall) > 0)
            {
                inventory.AddBallCount(BallId.MasterBall, -1);
                var newPokemon = pokemonFactory.PullFromLegendaryPool();
                Debug.Log($"[전설!] {newPokemon.GetDisplayName(pokemonFactory.speciesDB.GetSpecies(newPokemon.speciesId))}을(를) 뽑았습니다!");
                UpdateUI();
            }
        }

        private void OnCloseButtonClick()
        {
            // 상위 컨트롤러에게 메뉴 패널을 보여달라고 요청
            expandedUIController.ShowMenuPanel();
        }
    }
}