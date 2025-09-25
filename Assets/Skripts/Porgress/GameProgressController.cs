using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 입력 이벤트를 받아서 EXP/친밀도 분배자에 위임하는 상위 오케스트레이터.
    /// </summary>
    public class GameProgressController : MonoBehaviour
    {
        [Header("Wiring")]
        public LoginManager loginManager;               // LoginManager에 대한 참조
        public InputCapture inputCapture;               // 씬의 InputCapture 할당
        public ClickRewardPolicy rewardPolicy;          // 정책 SO
        public OwnedPokemonManager owned;               // 트레이너 로딩 후 주입
        public PokemonLevelupManager levelupManager;    // 이벤트 방출용
        public ClickProgressTracker tracker;            // 트레이너 단위로 로드/세이브

        // 종/곡선 조회(간단히 Service Locator 스타일로 함수 주입)
        public SpeciesDB speciesDB;               // 예시: speciesId -> SpeciesSO

        private PartyExpDistributor _exp;
        private PartyFriendshipDistributor _friend;

        void Awake()
        {
            if (loginManager != null)
            {
                loginManager.OnLoginSuccess += StartGameSystem;
            }

            //// TODO: 의존성 체크 해야함
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
            // 의존성 체크 후 분배자 초기화
            if (owned == null || levelupManager == null || speciesDB == null || tracker == null || rewardPolicy == null)
            {
                Debug.LogError("게임 시스템을 시작하는 데 필요한 종속성이 부족합니다.");
                return;
            }

            _exp = new PartyExpDistributor(owned, levelupManager, speciesId => speciesDB.GetSpecies(speciesId));
            _friend = new PartyFriendshipDistributor(owned, tracker, rewardPolicy);

            // 입력 이벤트 구독
            if (inputCapture != null)
            {
                inputCapture.OnGameInput += HandleGameInput;
            }

            Debug.Log("게임 시스템 시작!");
        }

        private void HandleGameInput()
        {
            // EXP 분배
            _exp.GiveExpToParty(rewardPolicy.GetExpPerInput());

            // 친밀도 분배(주기 도달 시에만 내부에서 지급)
            _friend.OnInput();

            // 세이브 이벤트
        }
    }
}
