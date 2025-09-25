using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// �Է� �̺�Ʈ�� �޾Ƽ� EXP/ģ�е� �й��ڿ� �����ϴ� ���� ���ɽ�Ʈ������.
    /// </summary>
    public class GameProgressController : MonoBehaviour
    {
        [Header("Wiring")]
        public LoginManager loginManager;               // LoginManager�� ���� ����
        public InputCapture inputCapture;               // ���� InputCapture �Ҵ�
        public ClickRewardPolicy rewardPolicy;          // ��å SO
        public OwnedPokemonManager owned;               // Ʈ���̳� �ε� �� ����
        public PokemonLevelupManager levelupManager;    // �̺�Ʈ �����
        public ClickProgressTracker tracker;            // Ʈ���̳� ������ �ε�/���̺�

        // ��/� ��ȸ(������ Service Locator ��Ÿ�Ϸ� �Լ� ����)
        public SpeciesDB speciesDB;               // ����: speciesId -> SpeciesSO

        private PartyExpDistributor _exp;
        private PartyFriendshipDistributor _friend;

        void Awake()
        {
            if (loginManager != null)
            {
                loginManager.OnLoginSuccess += StartGameSystem;
            }

            //// TODO: ������ üũ �ؾ���
            //_exp = new PartyExpDistributor(
            //    owned,
            //    levelupManager,
            //    speciesId => speciesDB.GetSpecies(speciesId));

            //_friend = new PartyFriendshipDistributor(
            //    owned,
            //    tracker,
            //    rewardPolicy);
        }

        //void OnEnable()
        //{
        //    if (inputCapture != null && owned != null && tracker != null)
        //        inputCapture.OnGameInput += HandleGameInput;
        //}

        void OnDisable()
        {
            if (inputCapture != null)
                inputCapture.OnGameInput -= HandleGameInput;
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
            if (inputCapture != null)
            {
                inputCapture.OnGameInput += HandleGameInput;
            }

            Debug.Log("���� �ý��� ����!");
        }

        private void HandleGameInput()
        {
            // EXP �й�
            _exp.GiveExpToParty(rewardPolicy.GetExpPerInput());

            // ģ�е� �й�(�ֱ� ���� �ÿ��� ���ο��� ����)
            _friend.OnInput();

            // ���̺� �̺�Ʈ
        }
    }
}
