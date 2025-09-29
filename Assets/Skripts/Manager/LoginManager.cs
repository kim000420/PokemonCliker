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
        [Header("Managers in Scene")]
        public PokemonTrainerManager trainerManager;    // �� ��ü
        public OwnedPokemonManager ownedManager;        // �� ��ü
        public SpeciesDB speciesDB;                     // �� ��ü(���� �迭)
        public PokemonLevelupManager levelupManager;    // �� ��ü
        public GameProgressController progressController;// �� ��ü
        public ClickRewardPolicy rewardPolicy;          // ����
        public PuidSequencer puidSequencer;
        public UIManager uiManager;

        private IAccountRepository _accountRepo;
        private ITrainerRepository _trainerRepo;
        private AccountService _accountService;

        public event Action OnLoginSuccess;

        void Awake()
        {
            _accountRepo = new JsonAccountRepository();
            _trainerRepo = new JsonTrainerRepository();

            // AccountService �غ�
            _accountService = new AccountService(_accountRepo);

            if (speciesDB != null)
            {
                speciesDB.Initialize();
            }

            // �Ŵ��� �ʱ�ȭ
            trainerManager.Init(_trainerRepo, ownedManager);
            ownedManager.Init(6, puidSequencer);

            Debug.Log("LoginManager �ʱ�ȭ �Ϸ�. UI���� �α����� �õ����ּ���.");
        }

        // ������������������������������������������������������������������������������������������������������������������������������������������
        // UI���� ȣ���� �޼���
        // ������������������������������������������������������������������������������������������������������������������������������������������

        /// <summary>
        /// ȸ������ �õ�. ����/���� �� �ݹ��� ȣ���մϴ�.
        /// </summary>
        public void TryLogin(string id, string pw, Action<bool, string> onComplete)
        {
            trainerManager.ClearData();

            try
            {
                // �α��� �õ�
                int tuid = _accountService.Login(id, pw);
                // �α��� ���� �� ������ �ε� �� ���� ����
                LoadGameForTrainer(tuid);
                onComplete?.Invoke(true, $"�α��� ����! T_uid={tuid}");

                OnLoginSuccess?.Invoke();
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
            trainerManager.ClearData();

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

                // 1. ȸ������ ���� �� ���� ���� �����͸� �ε��Ͽ� �ʱ�ȭ�մϴ�.
                LoadGameForTrainer(tUid);

                // 2. �ű� ȸ������ ��Ÿ�� ���ϸ� ����
                var factory = GetComponentInChildren<PokemonFactory>();
                if (factory != null)
                {
                    factory.GiveRandomTest(5);
                }
                onComplete?.Invoke(true, $"ȸ������ ����! T_uid={tUid}");
                OnLoginSuccess?.Invoke();
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
            trainerManager.LoadForTrainer(T_uid);

            // ���̺� �ε�
            puidSequencer.InitializeFrom(ownedManager);

            // ���൵(Ŭ�� ����) �ε� - ����ü�� �°� ������ ����(��: trainerRepo.LoadTrainerProgress)
            progressController.tracker = trainerManager.Progress;

            // GameProgressController �輱
            progressController.rewardPolicy = rewardPolicy;
            progressController.owned = ownedManager;
            progressController.levelupManager = levelupManager;
            progressController.speciesDB = speciesDB;

            Debug.Log($"Login flow completed. T_uid={T_uid}, PartyCount={ownedManager.GetParty().Length}");

            trainerManager.Save();
        }

        private void OnApplicationQuit()
        {
            // LoginManager�� �����ϴ� Repository�� ���� �����͸� �����մϴ�.
            if (_accountRepo is JsonAccountRepository jsonRepo)
            {
                Debug.Log("���� �� ���� ������ ���� ����...");
                jsonRepo.ForceSave();
            }
        }
    }
}