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
        [Header("Repositories")]
        public MonoBehaviour accountRepositoryProvider; // IAccountRepository 구현(MonoBehaviour or ScriptableObject)
        public MonoBehaviour trainerRepositoryProvider; // ITrainerRepository 구현(MonoBehaviour or ScriptableObject)

        [Header("Managers in Scene")]
        public PokemonTrainerManager trainerManager;    // 씬 객체
        public OwnedPokemonManager ownedManager;        // 씬 객체
        public SpeciesDB speciesDB;                     // 씬 객체(에셋 배열)
        public PokemonLevelupManager levelupManager;    // 씬 객체
        public GameProgressController progressController;// 씬 객체
        public InputCapture inputCapture;               // 씬 객체
        public ClickRewardPolicy rewardPolicy;          // 에셋
        public ClickProgressTracker tracker;            // 트레이너 진행 데이터(로드 후 주입)
        public PuidSequencer puidSequencer;

        [Header("Login inputs (demo)")]
        // 기존 demo 필드는 UI에서 직접 받으므로 제거
        // public string loginId = "test@user";
        // public string loginPw = "1234";
        // public string displayName = "Trainer";

        private IAccountRepository _accountRepo;
        private ITrainerRepository _trainerRepo;
        private AccountService _accountService;

        void Awake()
        {
            // Repo 캐스팅
            _accountRepo = accountRepositoryProvider as IAccountRepository;
            _trainerRepo = trainerRepositoryProvider as ITrainerRepository;

            if (_accountRepo == null || _trainerRepo == null)
            {
                Debug.LogError("Repository 구현체가 연결되지 않았습니다.");
                enabled = false;
                return;
            }

            // 의존성 해결: AccountService 생성 전 JsonAccountRepository 초기화
            var jsonAccountRepo = _accountRepo as JsonAccountRepository;
            if (jsonAccountRepo != null)
            {
                jsonAccountRepo.Initialize();
            }

            // AccountService 준비
            _accountService = new AccountService(_accountRepo);

            // SpeciesDB가 다른 컴포넌트들에 사용되기 전에 먼저 초기화되도록 수정
            if (speciesDB != null)
            {
                speciesDB.Initialize();
            }

            Debug.Log("LoginManager 초기화 완료. UI에서 로그인을 시도해주세요.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // UI에서 호출할 메서드
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 로그인 시도. 성공/실패 시 콜백을 호출합니다.
        /// </summary>
        public void TryLogin(string id, string pw, Action<bool, string> onComplete)
        {
            try
            {
                // 로그인 시도
                int tUid = _accountService.Login(id, pw);
                // 로그인 성공 후 데이터 로드 및 게임 시작
                LoadGameForTrainer(tUid);
                onComplete?.Invoke(true, $"로그인 성공! T_uid={tUid}");
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

                // 신규 회원에게 스타터 포켓몬 지급
                var factory = GetComponentInChildren<PokemonFactory>();
                if (factory != null)
                {
                    // 예시: 1번 포켓몬(이상해씨)을 레벨 5로 지급
                    factory.GiveStarterForSignup(1, "Default", 5);
                }

                // 회원가입 성공 후 데이터 로드 및 게임 시작
                LoadGameForTrainer(tUid);
                onComplete?.Invoke(true, $"회원가입 성공! T_uid={tUid}");
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
            trainerManager.Init(_trainerRepo);
            ownedManager.Init(6, puidSequencer);

            // 세이브 로드
            trainerManager.LoadForTrainer(T_uid);
            puidSequencer.InitializeFrom(ownedManager);
            ownedManager.LoadFromRepository(_trainerRepo, T_uid);
            Debug.Log($"[LOGIN] T_uid={T_uid}, Party={ownedManager.Party.Count}, Table={ownedManager.Table.Count}, NextPuid(estimate) ready.");

            // 진행도(클릭 누적) 로드 - 구현체에 맞게 가져와 주입(예: trainerRepo.LoadTrainerProgress)
            if (tracker == null) tracker = new ClickProgressTracker();

            // GameProgressController 배선
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






//// 파일: Scripts/Boot/LoginManager.cs
//using UnityEngine;

//namespace PokeClicker
//{
//    /// <summary>
//    /// 게임 시작 기본 흐름:
//    /// 1) 계정 로그인/등록  T_uid 획득
//    /// 2) 트레이너 매니저 Init + 세이브 로드
//    /// 3) 클릭 진행/분배 컨트롤러에 의존성 주입
//    /// </summary>
//    public class LoginManager : MonoBehaviour
//    {
//        [Header("Repositories")]
//        public MonoBehaviour accountRepositoryProvider; // IAccountRepository 구현(MonoBehaviour or ScriptableObject)
//        public MonoBehaviour trainerRepositoryProvider; // ITrainerRepository 구현(MonoBehaviour or ScriptableObject)

//        [Header("Managers in Scene")]
//        public PokemonTrainerManager trainerManager;    // 씬 객체
//        public OwnedPokemonManager ownedManager;        // 씬 객체
//        public SpeciesDB speciesDB;                     // 씬 객체(에셋 배열)
//        public PokemonLevelupManager levelupManager;    // 씬 객체
//        public GameProgressController progressController;// 씬 객체
//        public InputCapture inputCapture;               // 씬 객체
//        public ClickRewardPolicy rewardPolicy;          // 에셋
//        public ClickProgressTracker tracker;            // 트레이너 진행 데이터(로드 후 주입)
//        public PuidSequencer puidSequencer;

//        [Header("Login inputs (demo)")]
//        public string loginId = "test@user";
//        public string loginPw = "1234";
//        public string displayName = "Trainer";

//        private IAccountRepository _accountRepo;
//        private ITrainerRepository _trainerRepo;

//        void Awake()
//        {
//            // Repo 캐스팅
//            _accountRepo = accountRepositoryProvider as IAccountRepository;
//            _trainerRepo = trainerRepositoryProvider as ITrainerRepository;

//            if (_accountRepo == null || _trainerRepo == null)
//            {
//                Debug.LogError("Repository 구현체가 연결되지 않았습니다.");
//                enabled = false;
//                return;
//            }

//            // _accountRepo가 MonoBehaviour이고 Initialize() 메서드가 있다면 호출
//            var jsonAccountRepo = _accountRepo as JsonAccountRepository; // JsonAccountRepository 타입인지 확인
//            if (jsonAccountRepo != null)
//            {
//                jsonAccountRepo.Initialize(); // 초기화 메서드 명시적으로 호출
//            }
//                        if (speciesDB != null) // 혹시 모를 할당 누락을 위해 널 체크
//            {
//                speciesDB.Initialize();
//            }
//            // AccountService 준비
//            var accountService = new AccountService(_accountRepo);

//            if (speciesDB != null) // 혹시 모를 할당 누락을 위해 널 체크
//            {
//                speciesDB.Initialize();
//            }

//            // 로그인 시도 → 없으면 등록
//            int T_uid;
//            try
//            {
//                T_uid = accountService.Login(loginId, loginPw);   // 존재하면 로그인 :contentReference[oaicite:4]{index=4}
//            }
//            catch
//            {
//                T_uid = accountService.Register(loginId, loginPw, displayName); // 없으면 등록 :contentReference[oaicite:5]{index=5}

//                // 신규 트레이너 프로필 생성 및 저장
//                var newProfile = new TrainerProfile
//                {
//                    T_uid = T_uid,
//                    TrainerName = string.IsNullOrWhiteSpace(displayName) ? $"Trainer{T_uid}" : displayName,
//                    CreatedAt = System.DateTime.Now
//                };
//                _trainerRepo.SaveTrainerProfile(newProfile); // ITrainerRepository를 통해 저장
//                Debug.Log($"[LOGIN] 신규 트레이너 프로필 저장 완료.");

//                // 신규 트레이너 스타팅 포켓몬 지급 (테스트용)
//                var factory = GetComponentInChildren<PokemonFactory>();
//                if (factory != null)
//                {
//                    // 예시: 1번 포켓몬(이상해씨)을 레벨 5로 지급
//                    factory.GiveStarterForSignup(1, "Default", 5);
//                }
//            }

//            // 트레이너 매니저 초기화
//            trainerManager.Init(_trainerRepo);                     // 생성자 대신 Init 사용 (위에서 수정) :contentReference[oaicite:6]{index=6}
//            // OwnedPokemonManager도 Init(선택) - 아이디 프로바이더 없으면 생략 가능
//            ownedManager.Init(6, puidSequencer);                            // 파티 제한 6 기본 (위에서 추가) :contentReference[oaicite:7]{index=7}

//            // 세이브 로드
//            trainerManager.LoadForTrainer(T_uid);                  // 프로필 + 보유 데이터 로드 :contentReference[oaicite:8]{index=8}
//            ownedManager.LoadFromRepository(_trainerRepo, T_uid);
//            puidSequencer.InitializeFrom(ownedManager);
//            Debug.Log($"[LOGIN] T_uid={T_uid}, Party={ownedManager.Party.Count}, Table={ownedManager.Table.Count}, NextPuid(estimate) ready.");

//            // 진행도(클릭 누적) 로드 - 구현체에 맞게 가져와 주입(예: trainerRepo.LoadTrainerProgress)
//            // 여기선 데모로 새로 만든다.
//            if (tracker == null) tracker = new ClickProgressTracker();

//            // GameProgressController 배선
//            progressController.inputCapture = inputCapture;
//            progressController.rewardPolicy = rewardPolicy;
//            progressController.owned = ownedManager;
//            progressController.levelupManager = levelupManager;
//            progressController.tracker = tracker;
//            progressController.speciesDB = speciesDB;
//            if (speciesDB == null)
//            {
//                Debug.LogError("SpeciesDB가 LoginManager에 할당되지 않았습니다. 씬에서 할당해주세요.", this);
//                return; // 오류를 막기 위해 함수 종료
//            }
//            speciesDB.Initialize();

//            Debug.Log($"Login flow completed. T_uid={T_uid}, PartyCount={ownedManager.Party.Count}");
//        }
//    }
//}
