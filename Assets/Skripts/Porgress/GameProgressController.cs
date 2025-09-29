using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// �Է� �̺�Ʈ�� �޾Ƽ� EXP/ģ�е� �й��ڿ� �����ϴ� ���� ���ɽ�Ʈ������.
    /// </summary>
    public class GameProgressController : MonoBehaviour
    {
        [Header("Encounter Settings")]
        [SerializeField, Range(0f, 0.1f)] private float encounterChance = 0.001f; // 0.1% Ȯ��

        [Header("Wiring")]
        public LoginManager loginManager;               // LoginManager�� ���� ����
        public ClickRewardPolicy rewardPolicy;          // ��å SO
        public OwnedPokemonManager owned;               // Ʈ���̳� �ε� �� ����
        public PokemonLevelupManager levelupManager;    // �̺�Ʈ �����
        public ClickProgressTracker tracker;            // Ʈ���̳� ������ �ε�/���̺�
        public SpeciesDB speciesDB;                     // ��/� ã��
        public PokemonFactory pokemonFactory;           // ���� ���ϸ� ������ ���� �߰�
        public EncounterUIController encounterUI;       // �߻� ���ϸ� ���� UI

        private PartyExpDistributor _exp;
        private PartyFriendshipDistributor _friend;

        void Awake()
        {
            if (loginManager != null)
            {
                loginManager.OnLoginSuccess += StartGameSystem;
            }
        }
        void OnDisable()
        {
            InputHookManager.OnGlobalInput -= HandleGameInput;
        }

        private void StartGameSystem()
        {
            // ������ üũ �� �й��� �ʱ�ȭ
            if (owned == null || levelupManager == null || speciesDB == null || tracker == null || rewardPolicy == null)
            {
                Debug.LogError("���� �ý����� �����ϴ� �� �ʿ��� ���Ӽ��� �����մϴ�.");
                return;
            }

            _exp = new PartyExpDistributor(owned, levelupManager, speciesId => speciesDB.GetSpecies(speciesId));
            _friend = new PartyFriendshipDistributor(owned, tracker, rewardPolicy);

            // �Է� �̺�Ʈ ����
            InputHookManager.OnGlobalInput += HandleGameInput;

            Debug.Log("���� �ý��� ����!");
        }

        private void HandleGameInput()
        {
            // EXP �й�
            _exp.GiveExpToParty(rewardPolicy.GetExpPerInput());

            // ģ�е� �й�(�ֱ� ���� �ÿ��� ���ο��� ����)
            _friend.OnInput();

            CheckForWildEncounter();
        }
        private void CheckForWildEncounter()
        {
            // �̹� �ٸ� ���ϸ��� ���������� �ߺ� ���� ����
            if (encounterUI != null && encounterUI.gameObject.activeInHierarchy) return;

            if (Random.value < encounterChance)
            {
                TriggerWildEncounter();
            }
        }

        private void TriggerWildEncounter()
        {
            var wildPokemon = pokemonFactory.CreateRandomWildPokemon(5); // �ӽ� ���� 5

            if (wildPokemon != null && encounterUI != null)
            {
                Debug.Log($"�߻��� {speciesDB.GetSpecies(wildPokemon.speciesId).nameKeyKor}��(��) ��Ÿ����!");
                encounterUI.ShowEncounter(wildPokemon);
            }
        }
    }
}
