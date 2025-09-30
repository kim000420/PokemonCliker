// ����: Scripts/UI/PokeShopUIController.cs
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
            // ���� â�� ���� ������ UI�� �����մϴ�.
            UpdateUI();

            // �� ��ư�� Ŭ�� �̺�Ʈ�� �����մϴ�.
            pullEventButton.onClick.AddListener(OnPullEventClick);
            pullAllButton.onClick.AddListener(OnPullAllClick);
            pullShinyButton.onClick.AddListener(OnPullShinyClick);
            pullLegendaryButton.onClick.AddListener(OnPullLegendaryClick);
            closeButton.onClick.AddListener(OnCloseButtonClick);
        }

        private void OnDisable()
        {
            // ���� â�� ���� �� �̺�Ʈ ������ �����մϴ�.
            pullEventButton.onClick.RemoveAllListeners();
            pullAllButton.onClick.RemoveAllListeners();
            pullShinyButton.onClick.RemoveAllListeners();
            pullLegendaryButton.onClick.RemoveAllListeners();
            closeButton.onClick.RemoveListener(OnCloseButtonClick);
        }

        /// <summary>
        /// ���ͺ� ������ UI�� ǥ���ϰ�, ������ ���� ��ư Ȱ��ȭ ���θ� �����մϴ�.
        /// </summary>
        public void UpdateUI()
        {
            var inventory = trainerManager.Profile.BallInventory;
            if (inventory == null) return;

            int premierCount = inventory.GetBallCount(BallId.PremierBall);
            pullEventCostText.text = "�� " + premierCount;
            pullEventButton.interactable = premierCount > 0;

            int pokeCount = inventory.GetBallCount(BallId.PokeBall);
            pullAllCostText.text = "�� " + pokeCount;
            pullAllButton.interactable = pokeCount > 0;

            int hyperCount = inventory.GetBallCount(BallId.HyperBall);
            pullShinyCostText.text = "�� " + hyperCount;
            pullShinyButton.interactable = hyperCount > 0;

            int masterCount = inventory.GetBallCount(BallId.MasterBall);
            pullLegendaryCostText.text = "�� " + masterCount;
            pullLegendaryButton.interactable = masterCount > 0;
        }

        // --- ��ư Ŭ�� �ڵ鷯 ---

        private void OnPullEventClick()
        {
            var inventory = trainerManager.Profile.BallInventory;
            if (inventory.GetBallCount(BallId.PremierBall) > 0)
            {
                inventory.AddBallCount(BallId.PremierBall, -1);
                var newPokemon = pokemonFactory.PullFromEventPool(); // ���丮 ȣ��
                Debug.Log($"{newPokemon.GetDisplayName(pokemonFactory.speciesDB.GetSpecies(newPokemon.speciesId))}��(��) �̾ҽ��ϴ�!");
                UpdateUI(); // UI ����
            }
        }

        private void OnPullAllClick()
        {
            var inventory = trainerManager.Profile.BallInventory;
            if (inventory.GetBallCount(BallId.PokeBall) > 0)
            {
                inventory.AddBallCount(BallId.PokeBall, -1);
                var newPokemon = pokemonFactory.PullFromAllPool();
                Debug.Log($"{newPokemon.GetDisplayName(pokemonFactory.speciesDB.GetSpecies(newPokemon.speciesId))}��(��) �̾ҽ��ϴ�!");
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
                Debug.Log($"[�̷�ġ!] {newPokemon.GetDisplayName(pokemonFactory.speciesDB.GetSpecies(newPokemon.speciesId))}��(��) �̾ҽ��ϴ�!");
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
                Debug.Log($"[����!] {newPokemon.GetDisplayName(pokemonFactory.speciesDB.GetSpecies(newPokemon.speciesId))}��(��) �̾ҽ��ϴ�!");
                UpdateUI();
            }
        }

        private void OnCloseButtonClick()
        {
            // ���� ��Ʈ�ѷ����� �޴� �г��� �����޶�� ��û
            expandedUIController.ShowMenuPanel();
        }
    }
}