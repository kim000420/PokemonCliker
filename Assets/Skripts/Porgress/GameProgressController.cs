using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 입력 이벤트를 받아서 EXP/친밀도 분배자에 위임하는 상위 오케스트레이터.
    /// </summary>
    public class GameProgressController : MonoBehaviour
    {
        [Header("Wiring")]
        public InputCapture inputCapture;               // 씬의 InputCapture 할당
        public ClickRewardPolicy rewardPolicy;          // 정책 SO
        public OwnedPokemonManager owned;               // 트레이너 로딩 후 주입
        public PokemonLevelupManager levelupManager;    // 이벤트 방출용
        public ClickProgressTracker tracker;            // 트레이너 단위로 로드/세이브

        // 종/곡선 조회(간단히 Service Locator 스타일로 함수 주입)
        public SpeciesDatabase speciesDB;               // 예시: speciesId -> SpeciesSO
        public ExperienceCurveDatabase curveDB;         // 예시: speciesSO -> ExperienceCurveSO

        private PartyExpDistributor _exp;
        private PartyFriendshipDistributor _friend;

        void Awake()
        {
            // 의존성 체크는 생략했지만 실제 프로젝트에선 null 검사 권장
            _exp = new PartyExpDistributor(
                owned,
                levelupManager,
                speciesId => speciesDB.GetSpecies(speciesId),
                species => curveDB.GetCurveForSpecies(species));

            _friend = new PartyFriendshipDistributor(
                owned,
                tracker,
                rewardPolicy);
        }

        void OnEnable()
        {
            if (inputCapture != null)
                inputCapture.OnGameInput += HandleGameInput;
        }

        void OnDisable()
        {
            if (inputCapture != null)
                inputCapture.OnGameInput -= HandleGameInput;
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

    // 아래 두 클래스는 예시용 스텁입니다. 프로젝트의 실제 DB/인덱스에 맞춰 바꾸세요.
    public class SpeciesDatabase : MonoBehaviour
    {
        public SpeciesSO[] all;
        public SpeciesSO GetSpecies(int id)
        {
            foreach (var s in all) if (s != null && s.speciesId == id) return s;
            return null;
        }
    }
    public class ExperienceCurveDatabase : MonoBehaviour
    {
        public ExperienceCurveSO defaultCurve;
        public ExperienceCurveSO GetCurveForSpecies(SpeciesSO s)
        {
            // 종에 따라 다른 곡선을 사용한다면 여기에서 매핑
            return defaultCurve;
        }
    }
}
