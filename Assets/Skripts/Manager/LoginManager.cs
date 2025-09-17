// 파일: Scripts/Boot/LoginManager.cs
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
        public string loginId = "test@user";
        public string loginPw = "1234";
        public string displayName = "Trainer";

        private IAccountRepository _accountRepo;
        private ITrainerRepository _trainerRepo;

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

            // AccountService 준비
            var accountService = new AccountService(_accountRepo); // SO/enum 없이 순수 클래스 사용 :contentReference[oaicite:3]{index=3}

            // 로그인 시도 → 없으면 등록
            int T_uid;
            try
            {
                T_uid = accountService.Login(loginId, loginPw);   // 존재하면 로그인 :contentReference[oaicite:4]{index=4}
            }
            catch
            {
                T_uid = accountService.Register(loginId, loginPw, displayName); // 없으면 등록 :contentReference[oaicite:5]{index=5}
            }

            // 트레이너 매니저 초기화
            trainerManager.Init(_trainerRepo);                     // 생성자 대신 Init 사용 (위에서 수정) :contentReference[oaicite:6]{index=6}
            // OwnedPokemonManager도 Init(선택) - 아이디 프로바이더 없으면 생략 가능
            ownedManager.Init(6, puidSequencer);                            // 파티 제한 6 기본 (위에서 추가) :contentReference[oaicite:7]{index=7}

            // 세이브 로드
            trainerManager.LoadForTrainer(T_uid);                  // 프로필 + 보유 데이터 로드 :contentReference[oaicite:8]{index=8}
            puidSequencer.InitializeFrom(ownedManager);
            ownedManager.LoadFromRepository(_trainerRepo, T_uid);
            Debug.Log($"[LOGIN] T_uid={T_uid}, Party={ownedManager.Party.Count}, Table={ownedManager.Table.Count}, NextPuid(estimate) ready.");

            // 진행도(클릭 누적) 로드 - 구현체에 맞게 가져와 주입(예: trainerRepo.LoadTrainerProgress)
            // 여기선 데모로 새로 만든다.
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
