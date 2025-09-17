// ����: Scripts/Boot/LoginManager.cs
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// ���� ���� �⺻ �帧:
    /// 1) ���� �α���/���  T_uid ȹ��
    /// 2) Ʈ���̳� �Ŵ��� Init + ���̺� �ε�
    /// 3) Ŭ�� ����/�й� ��Ʈ�ѷ��� ������ ����
    /// </summary>
    public class LoginManager : MonoBehaviour
    {
        [Header("Repositories")]
        public MonoBehaviour accountRepositoryProvider; // IAccountRepository ����(MonoBehaviour or ScriptableObject)
        public MonoBehaviour trainerRepositoryProvider; // ITrainerRepository ����(MonoBehaviour or ScriptableObject)

        [Header("Managers in Scene")]
        public PokemonTrainerManager trainerManager;    // �� ��ü
        public OwnedPokemonManager ownedManager;        // �� ��ü
        public SpeciesDB speciesDB;                     // �� ��ü(���� �迭)
        public PokemonLevelupManager levelupManager;    // �� ��ü
        public GameProgressController progressController;// �� ��ü
        public InputCapture inputCapture;               // �� ��ü
        public ClickRewardPolicy rewardPolicy;          // ����
        public ClickProgressTracker tracker;            // Ʈ���̳� ���� ������(�ε� �� ����)
        public PuidSequencer puidSequencer;

        [Header("Login inputs (demo)")]
        public string loginId = "test@user";
        public string loginPw = "1234";
        public string displayName = "Trainer";

        private IAccountRepository _accountRepo;
        private ITrainerRepository _trainerRepo;

        void Awake()
        {
            // Repo ĳ����
            _accountRepo = accountRepositoryProvider as IAccountRepository;
            _trainerRepo = trainerRepositoryProvider as ITrainerRepository;

            if (_accountRepo == null || _trainerRepo == null)
            {
                Debug.LogError("Repository ����ü�� ������� �ʾҽ��ϴ�.");
                enabled = false;
                return;
            }

            // AccountService �غ�
            var accountService = new AccountService(_accountRepo); // SO/enum ���� ���� Ŭ���� ��� :contentReference[oaicite:3]{index=3}

            // �α��� �õ� �� ������ ���
            int T_uid;
            try
            {
                T_uid = accountService.Login(loginId, loginPw);   // �����ϸ� �α��� :contentReference[oaicite:4]{index=4}
            }
            catch
            {
                T_uid = accountService.Register(loginId, loginPw, displayName); // ������ ��� :contentReference[oaicite:5]{index=5}
            }

            // Ʈ���̳� �Ŵ��� �ʱ�ȭ
            trainerManager.Init(_trainerRepo);                     // ������ ��� Init ��� (������ ����) :contentReference[oaicite:6]{index=6}
            // OwnedPokemonManager�� Init(����) - ���̵� ���ι��̴� ������ ���� ����
            ownedManager.Init(6, puidSequencer);                            // ��Ƽ ���� 6 �⺻ (������ �߰�) :contentReference[oaicite:7]{index=7}

            // ���̺� �ε�
            trainerManager.LoadForTrainer(T_uid);                  // ������ + ���� ������ �ε� :contentReference[oaicite:8]{index=8}
            puidSequencer.InitializeFrom(ownedManager);
            ownedManager.LoadFromRepository(_trainerRepo, T_uid);
            Debug.Log($"[LOGIN] T_uid={T_uid}, Party={ownedManager.Party.Count}, Table={ownedManager.Table.Count}, NextPuid(estimate) ready.");

            // ���൵(Ŭ�� ����) �ε� - ����ü�� �°� ������ ����(��: trainerRepo.LoadTrainerProgress)
            // ���⼱ ����� ���� �����.
            if (tracker == null) tracker = new ClickProgressTracker();

            // GameProgressController �輱
            progressController.inputCapture = inputCapture;
            progressController.rewardPolicy = rewardPolicy;
            progressController.owned = ownedManager;
            progressController.levelupManager = levelupManager;
            progressController.tracker = tracker;
            progressController.speciesDB = speciesDB;

            Debug.Log($"Login flow completed. T_uid={T_uid}, PartyCount={ownedManager.Party.Count}");
        }
    }
}
