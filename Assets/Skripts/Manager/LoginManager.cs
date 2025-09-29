// 파일: Scripts/Boot/LoginManager.cs
using System;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 게임 시작 기본 흐름:
    /// 1) 계정 로그인/등록  T_uid 획득
    /// 2) 트레이너 매니저 Init + 세이브 로드
    /// 3) 클릭 진행/분배 컨트롤러에 의존성 주입
    /// </summary>
    public class LoginManager : MonoBehaviour
    {
        [Header("Managers in Scene")]
        public PokemonTrainerManager trainerManager;    // 씬 객체
        public OwnedPokemonManager ownedManager;        // 씬 객체
        public SpeciesDB speciesDB;                     // 씬 객체(에셋 배열)
        public PokemonLevelupManager levelupManager;    // 씬 객체
        public GameProgressController progressController;// 씬 객체
        public ClickRewardPolicy rewardPolicy;          // 에셋
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

            // AccountService 준비
            _accountService = new AccountService(_accountRepo);

            if (speciesDB != null)
            {
                speciesDB.Initialize();
            }

            // 매니저 초기화
            trainerManager.Init(_trainerRepo, ownedManager);
            ownedManager.Init(6, puidSequencer);

            Debug.Log("LoginManager 초기화 완료. UI에서 로그인을 시도해주세요.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // UI에서 호출할 메서드
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 회원가입 시도. 성공/실패 시 콜백을 호출합니다.
        /// </summary>
        public void TryLogin(string id, string pw, Action<bool, string> onComplete)
        {
            trainerManager.ClearData();

            try
            {
                // 로그인 시도
                int tuid = _accountService.Login(id, pw);
                // 로그인 성공 후 데이터 로드 및 게임 시작
                LoadGameForTrainer(tuid);
                onComplete?.Invoke(true, $"로그인 성공! T_uid={tuid}");

                OnLoginSuccess?.Invoke();
            }
            catch (InvalidOperationException e)
            {
                // 로그인 실패
                onComplete?.Invoke(false, e.Message);
            }
        }

        /// <summary>
        /// 회원가입 시도. 성공/실패 시 콜백을 호출합니다.
        /// </summary>
        public void TryRegister(string id, string pw, string displayName, Action<bool, string> onComplete)
        {
            trainerManager.ClearData();

            try
            {
                // 회원가입 시도
                int tUid = _accountService.Register(id, pw, displayName);

                // 신규 트레이너 프로필 생성 및 저장
                var newProfile = new TrainerProfile
                {
                    T_uid = tUid,
                    TrainerName = string.IsNullOrWhiteSpace(displayName) ? $"Trainer{tUid}" : displayName,
                    CreatedAt = System.DateTime.Now
                };
                _trainerRepo.SaveTrainerProfile(newProfile); // ITrainerRepository를 통해 저장
                Debug.Log($"[LOGIN] 신규 트레이너 프로필 저장 완료.");

                // 1. 회원가입 성공 후 먼저 게임 데이터를 로드하여 초기화합니다.
                LoadGameForTrainer(tUid);

                // 2. 신규 회원에게 스타터 포켓몬 지급
                var factory = GetComponentInChildren<PokemonFactory>();
                if (factory != null)
                {
                    factory.GiveRandomTest(5);
                }
                onComplete?.Invoke(true, $"회원가입 성공! T_uid={tUid}");
                OnLoginSuccess?.Invoke();
            }
            catch (InvalidOperationException e)
            {
                // 회원가입 실패 (ID 중복 등)
                onComplete?.Invoke(false, e.Message);
            }
        }

        /// <summary>
        /// 로그인/회원가입 성공 후 게임 데이터를 로드합니다.
        /// </summary>
        private void LoadGameForTrainer(int T_uid)
        {
            // 트레이너 매니저 초기화
            trainerManager.LoadForTrainer(T_uid);

            // 세이브 로드
            puidSequencer.InitializeFrom(ownedManager);

            // 진행도(클릭 누적) 로드 - 구현체에 맞게 가져와 주입(예: trainerRepo.LoadTrainerProgress)
            progressController.tracker = trainerManager.Progress;

            // GameProgressController 배선
            progressController.rewardPolicy = rewardPolicy;
            progressController.owned = ownedManager;
            progressController.levelupManager = levelupManager;
            progressController.speciesDB = speciesDB;

            Debug.Log($"Login flow completed. T_uid={T_uid}, PartyCount={ownedManager.GetParty().Length}");

            trainerManager.Save();
        }

        private void OnApplicationQuit()
        {
            // LoginManager가 관리하는 Repository의 최종 데이터를 저장합니다.
            if (_accountRepo is JsonAccountRepository jsonRepo)
            {
                Debug.Log("종료 전 계정 데이터 최종 저장...");
                jsonRepo.ForceSave();
            }
        }
    }
}