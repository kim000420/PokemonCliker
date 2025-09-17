// ����: Scripts/Boot/LoginManager.cs
using System;
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
        // ���� demo �ʵ�� UI���� ���� �����Ƿ� ����
        // public string loginId = "test@user";
        // public string loginPw = "1234";
        // public string displayName = "Trainer";

        private IAccountRepository _accountRepo;
        private ITrainerRepository _trainerRepo;
        private AccountService _accountService;

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

            // ������ �ذ�: AccountService ���� �� JsonAccountRepository �ʱ�ȭ
            var jsonAccountRepo = _accountRepo as JsonAccountRepository;
            if (jsonAccountRepo != null)
            {
                jsonAccountRepo.Initialize();
            }

            // AccountService �غ�
            _accountService = new AccountService(_accountRepo);

            // SpeciesDB�� �ٸ� ������Ʈ�鿡 ���Ǳ� ���� ���� �ʱ�ȭ�ǵ��� ����
            if (speciesDB != null)
            {
                speciesDB.Initialize();
            }

            Debug.Log("LoginManager �ʱ�ȭ �Ϸ�. UI���� �α����� �õ����ּ���.");
        }

        // ������������������������������������������������������������������������������������������������������������������������������������������
        // UI���� ȣ���� �޼���
        // ������������������������������������������������������������������������������������������������������������������������������������������

        /// <summary>
        /// �α��� �õ�. ����/���� �� �ݹ��� ȣ���մϴ�.
        /// </summary>
        public void TryLogin(string id, string pw, Action<bool, string> onComplete)
        {
            try
            {
                // �α��� �õ�
                int tUid = _accountService.Login(id, pw);
                // �α��� ���� �� ������ �ε� �� ���� ����
                LoadGameForTrainer(tUid);
                onComplete?.Invoke(true, $"�α��� ����! T_uid={tUid}");
            }
            catch (InvalidOperationException e)
            {
                // �α��� ����
                onComplete?.Invoke(false, e.Message);
            }
        }

        /// <summary>
        /// ȸ������ �õ�. ����/���� �� �ݹ��� ȣ���մϴ�.
        /// </summary>
        public void TryRegister(string id, string pw, string displayName, Action<bool, string> onComplete)
        {
            try
            {
                // ȸ������ �õ�
                int tUid = _accountService.Register(id, pw, displayName);

                // �ű� Ʈ���̳� ������ ���� �� ����
                var newProfile = new TrainerProfile
                {
                    T_uid = tUid,
                    TrainerName = string.IsNullOrWhiteSpace(displayName) ? $"Trainer{tUid}" : displayName,
                    CreatedAt = System.DateTime.Now
                };
                _trainerRepo.SaveTrainerProfile(newProfile); // ITrainerRepository�� ���� ����
                Debug.Log($"[LOGIN] �ű� Ʈ���̳� ������ ���� �Ϸ�.");

                // �ű� ȸ������ ��Ÿ�� ���ϸ� ����
                var factory = GetComponentInChildren<PokemonFactory>();
                if (factory != null)
                {
                    // ����: 1�� ���ϸ�(�̻��ؾ�)�� ���� 5�� ����
                    factory.GiveStarterForSignup(1, "Default", 5);
                }

                // ȸ������ ���� �� ������ �ε� �� ���� ����
                LoadGameForTrainer(tUid);
                onComplete?.Invoke(true, $"ȸ������ ����! T_uid={tUid}");
            }
            catch (InvalidOperationException e)
            {
                // ȸ������ ���� (ID �ߺ� ��)
                onComplete?.Invoke(false, e.Message);
            }
        }

        /// <summary>
        /// �α���/ȸ������ ���� �� ���� �����͸� �ε��մϴ�.
        /// </summary>
        private void LoadGameForTrainer(int T_uid)
        {
            // Ʈ���̳� �Ŵ��� �ʱ�ȭ
            trainerManager.Init(_trainerRepo);
            ownedManager.Init(6, puidSequencer);

            // ���̺� �ε�
            trainerManager.LoadForTrainer(T_uid);
            puidSequencer.InitializeFrom(ownedManager);
            ownedManager.LoadFromRepository(_trainerRepo, T_uid);
            Debug.Log($"[LOGIN] T_uid={T_uid}, Party={ownedManager.Party.Count}, Table={ownedManager.Table.Count}, NextPuid(estimate) ready.");

            // ���൵(Ŭ�� ����) �ε� - ����ü�� �°� ������ ����(��: trainerRepo.LoadTrainerProgress)
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






//// ����: Scripts/Boot/LoginManager.cs
//using UnityEngine;

//namespace PokeClicker
//{
//    /// <summary>
//    /// ���� ���� �⺻ �帧:
//    /// 1) ���� �α���/���  T_uid ȹ��
//    /// 2) Ʈ���̳� �Ŵ��� Init + ���̺� �ε�
//    /// 3) Ŭ�� ����/�й� ��Ʈ�ѷ��� ������ ����
//    /// </summary>
//    public class LoginManager : MonoBehaviour
//    {
//        [Header("Repositories")]
//        public MonoBehaviour accountRepositoryProvider; // IAccountRepository ����(MonoBehaviour or ScriptableObject)
//        public MonoBehaviour trainerRepositoryProvider; // ITrainerRepository ����(MonoBehaviour or ScriptableObject)

//        [Header("Managers in Scene")]
//        public PokemonTrainerManager trainerManager;    // �� ��ü
//        public OwnedPokemonManager ownedManager;        // �� ��ü
//        public SpeciesDB speciesDB;                     // �� ��ü(���� �迭)
//        public PokemonLevelupManager levelupManager;    // �� ��ü
//        public GameProgressController progressController;// �� ��ü
//        public InputCapture inputCapture;               // �� ��ü
//        public ClickRewardPolicy rewardPolicy;          // ����
//        public ClickProgressTracker tracker;            // Ʈ���̳� ���� ������(�ε� �� ����)
//        public PuidSequencer puidSequencer;

//        [Header("Login inputs (demo)")]
//        public string loginId = "test@user";
//        public string loginPw = "1234";
//        public string displayName = "Trainer";

//        private IAccountRepository _accountRepo;
//        private ITrainerRepository _trainerRepo;

//        void Awake()
//        {
//            // Repo ĳ����
//            _accountRepo = accountRepositoryProvider as IAccountRepository;
//            _trainerRepo = trainerRepositoryProvider as ITrainerRepository;

//            if (_accountRepo == null || _trainerRepo == null)
//            {
//                Debug.LogError("Repository ����ü�� ������� �ʾҽ��ϴ�.");
//                enabled = false;
//                return;
//            }

//            // _accountRepo�� MonoBehaviour�̰� Initialize() �޼��尡 �ִٸ� ȣ��
//            var jsonAccountRepo = _accountRepo as JsonAccountRepository; // JsonAccountRepository Ÿ������ Ȯ��
//            if (jsonAccountRepo != null)
//            {
//                jsonAccountRepo.Initialize(); // �ʱ�ȭ �޼��� ��������� ȣ��
//            }
//                        if (speciesDB != null) // Ȥ�� �� �Ҵ� ������ ���� �� üũ
//            {
//                speciesDB.Initialize();
//            }
//            // AccountService �غ�
//            var accountService = new AccountService(_accountRepo);

//            if (speciesDB != null) // Ȥ�� �� �Ҵ� ������ ���� �� üũ
//            {
//                speciesDB.Initialize();
//            }

//            // �α��� �õ� �� ������ ���
//            int T_uid;
//            try
//            {
//                T_uid = accountService.Login(loginId, loginPw);   // �����ϸ� �α��� :contentReference[oaicite:4]{index=4}
//            }
//            catch
//            {
//                T_uid = accountService.Register(loginId, loginPw, displayName); // ������ ��� :contentReference[oaicite:5]{index=5}

//                // �ű� Ʈ���̳� ������ ���� �� ����
//                var newProfile = new TrainerProfile
//                {
//                    T_uid = T_uid,
//                    TrainerName = string.IsNullOrWhiteSpace(displayName) ? $"Trainer{T_uid}" : displayName,
//                    CreatedAt = System.DateTime.Now
//                };
//                _trainerRepo.SaveTrainerProfile(newProfile); // ITrainerRepository�� ���� ����
//                Debug.Log($"[LOGIN] �ű� Ʈ���̳� ������ ���� �Ϸ�.");

//                // �ű� Ʈ���̳� ��Ÿ�� ���ϸ� ���� (�׽�Ʈ��)
//                var factory = GetComponentInChildren<PokemonFactory>();
//                if (factory != null)
//                {
//                    // ����: 1�� ���ϸ�(�̻��ؾ�)�� ���� 5�� ����
//                    factory.GiveStarterForSignup(1, "Default", 5);
//                }
//            }

//            // Ʈ���̳� �Ŵ��� �ʱ�ȭ
//            trainerManager.Init(_trainerRepo);                     // ������ ��� Init ��� (������ ����) :contentReference[oaicite:6]{index=6}
//            // OwnedPokemonManager�� Init(����) - ���̵� ���ι��̴� ������ ���� ����
//            ownedManager.Init(6, puidSequencer);                            // ��Ƽ ���� 6 �⺻ (������ �߰�) :contentReference[oaicite:7]{index=7}

//            // ���̺� �ε�
//            trainerManager.LoadForTrainer(T_uid);                  // ������ + ���� ������ �ε� :contentReference[oaicite:8]{index=8}
//            ownedManager.LoadFromRepository(_trainerRepo, T_uid);
//            puidSequencer.InitializeFrom(ownedManager);
//            Debug.Log($"[LOGIN] T_uid={T_uid}, Party={ownedManager.Party.Count}, Table={ownedManager.Table.Count}, NextPuid(estimate) ready.");

//            // ���൵(Ŭ�� ����) �ε� - ����ü�� �°� ������ ����(��: trainerRepo.LoadTrainerProgress)
//            // ���⼱ ����� ���� �����.
//            if (tracker == null) tracker = new ClickProgressTracker();

//            // GameProgressController �輱
//            progressController.inputCapture = inputCapture;
//            progressController.rewardPolicy = rewardPolicy;
//            progressController.owned = ownedManager;
//            progressController.levelupManager = levelupManager;
//            progressController.tracker = tracker;
//            progressController.speciesDB = speciesDB;
//            if (speciesDB == null)
//            {
//                Debug.LogError("SpeciesDB�� LoginManager�� �Ҵ���� �ʾҽ��ϴ�. ������ �Ҵ����ּ���.", this);
//                return; // ������ ���� ���� �Լ� ����
//            }
//            speciesDB.Initialize();

//            Debug.Log($"Login flow completed. T_uid={T_uid}, PartyCount={ownedManager.Party.Count}");
//        }
//    }
//}
