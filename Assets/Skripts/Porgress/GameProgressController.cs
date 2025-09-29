using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 입력 이벤트를 받아서 EXP/친밀도 분배자에 위임하는 상위 오케스트레이터.
    /// </summary>
    public class GameProgressController : MonoBehaviour
    {
        [Header("Encounter Settings")]
        [SerializeField, Range(0f, 0.1f)] private float encounterChance = 0.001f; // 0.1% 확률

        [Header("Wiring")]
        public LoginManager loginManager;               // LoginManager에 대한 참조
        public ClickRewardPolicy rewardPolicy;          // 정책 SO
        public OwnedPokemonManager owned;               // 트레이너 로딩 후 주입
        public PokemonLevelupManager levelupManager;    // 이벤트 방출용
        public ClickProgressTracker tracker;            // 트레이너 단위로 로드/세이브
        public SpeciesDB speciesDB;                     // 종/곡선 찾기
        public PokemonFactory pokemonFactory;           // 랜덤 포켓몬 생성을 위해 추가
        public EncounterUIController encounterUI;       // 야생 포켓몬 등장 UI

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
            // 의존성 체크 후 분배자 초기화
            if (owned == null || levelupManager == null || speciesDB == null || tracker == null || rewardPolicy == null)
            {
                Debug.LogError("게임 시스템을 시작하는 데 필요한 종속성이 부족합니다.");
                return;
            }

            _exp = new PartyExpDistributor(owned, levelupManager, speciesId => speciesDB.GetSpecies(speciesId));
            _friend = new PartyFriendshipDistributor(owned, tracker, rewardPolicy);

            // 입력 이벤트 구독
            InputHookManager.OnGlobalInput += HandleGameInput;

            Debug.Log("게임 시스템 시작!");
        }

        private void HandleGameInput()
        {
            // EXP 분배
            _exp.GiveExpToParty(rewardPolicy.GetExpPerInput());

            // 친밀도 분배(주기 도달 시에만 내부에서 지급)
            _friend.OnInput();

            CheckForWildEncounter();
        }
        private void CheckForWildEncounter()
        {
            // 이미 다른 포켓몬이 나와있으면 중복 등장 방지
            if (encounterUI != null && encounterUI.gameObject.activeInHierarchy) return;

            if (Random.value < encounterChance)
            {
                TriggerWildEncounter();
            }
        }

        private void TriggerWildEncounter()
        {
            var wildPokemon = pokemonFactory.CreateRandomWildPokemon(5); // 임시 레벨 5

            if (wildPokemon != null && encounterUI != null)
            {
                Debug.Log($"야생의 {speciesDB.GetSpecies(wildPokemon.speciesId).nameKeyKor}이(가) 나타났다!");
                encounterUI.ShowEncounter(wildPokemon);
            }
        }
    }
}
