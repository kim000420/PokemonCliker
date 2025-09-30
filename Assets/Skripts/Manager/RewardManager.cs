// 파일: Scripts/Managers/RewardManager.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PokeClicker
{
    public class RewardManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float rewardIntervalMinutes = 30.0f; // 보상 간격 (분 단위)
        [SerializeField] private int requiredClicks = 1000;

        [Header("Dependencies")]
        [SerializeField] private PokemonTrainerManager trainerManager;
        [SerializeField] private GameObject rewardPopup; // 30분마다 띄울 팝업 UI
        [SerializeField] private RewardClaimUIController rewardClaimUI;

        public float TimeRemaining { get; private set; }

        private void Start()
        {
            // 게임이 시작되면 타이머 코루틴을 실행합니다.
            StartCoroutine(RewardTimerRoutine());
        }

        private IEnumerator RewardTimerRoutine()
        {
            while (true)
            {
                while (TimeRemaining > 0)
                {
                    TimeRemaining -= Time.deltaTime;
                    yield return null;
                }

                // 시간이 0이 되면 팝업 UI를 활성화
                if (rewardClaimUI != null && !rewardClaimUI.gameObject.activeInHierarchy)
                {
                    Debug.Log("몬스터볼 획득 기회! 팝업을 활성화합니다.");
                    rewardClaimUI.Show();
                }
                yield return new WaitForSeconds(1f); // 팝업이 떠있는 동안 매초 타이머를 다시 체크하지 않도록 잠시 대기
            }
        }

        /// <summary>
        /// 팝업 UI의 버튼이 호출할 메서드. 클릭 횟수를 확인하고 보상을 지급합니다.
        /// </summary>
        public void ClaimReward()
        {
            // 1. 클릭 횟수가 충분한지 확인
            if (trainerManager.Progress.totalInputs < requiredClicks)
            {
                Debug.Log("클릭 횟수가 부족하여 보상을 받을 수 없습니다.");
                // 여기에 "클릭이 부족합니다" 같은 UI 피드백을 추가할 수 있습니다.
                return;
            }

            // 2. 클릭 횟수 차감 (totalInputs을 직접 줄이는 대신, 별도의 변수를 사용할 수도 있습니다)
            // 여기서는 간단하게 totalInputs을 직접 차감하겠습니다.
            trainerManager.Progress.totalInputs -= requiredClicks;
            var rewardedBalls = GrantRandomPokeball(); // 지급된 볼 목록을 받음

            rewardClaimUI.StartClaimAnimation(rewardedBalls);
            Debug.Log($"{requiredClicks} 클릭을 소모했습니다. 남은 클릭: {trainerManager.Progress.totalInputs}");

            // UI 컨트롤러에 연출 시작을 요청
            rewardClaimUI.StartClaimAnimation(rewardedBalls);

            // 타이머 초기화
            TimeRemaining = rewardIntervalMinutes * 60;
        }

        private List<string> GrantRandomPokeball()
        {
            var inventory = trainerManager.Profile.BallInventory;
            if (inventory == null) return new List<string>();

            var rewardedBalls = new List<string>();

            float roll = Random.value;
            if (roll < 0.01f)
            {
                inventory.AddBallCount(BallId.MasterBall, 1);
                rewardedBalls.Add(BallId.MasterBall);
            }
            else if (roll < 0.06f)
            {
                inventory.AddBallCount(BallId.HyperBall, 1);
                rewardedBalls.Add(BallId.HyperBall);
            }
            else
            {
                inventory.AddBallCount(BallId.PokeBall, 1);
                rewardedBalls.Add(BallId.PokeBall);
            }

            if (Random.value < 0.20f)
            {
                inventory.AddBallCount(BallId.PremierBall, 1);
                rewardedBalls.Add(BallId.PremierBall);
            }

            trainerManager.Save();
            return rewardedBalls;
        }
    }
}