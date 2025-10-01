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

            if (trainerManager.Progress.totalInputs < requiredClicks)
            {
                Debug.Log("클릭 횟수가 부족하여 보상을 받을 수 없습니다.");
                rewardClaimUI.ShowFeedback("클릭 횟수가 부족합니다!");
                return;
            }

            trainerManager.Progress.totalInputs -= requiredClicks;
            Debug.Log($"{requiredClicks} 클릭을 소모했습니다. 남은 클릭: {trainerManager.Progress.totalInputs}");

            GrantRandomPokeball();
        }

        private void GrantRandomPokeball()
        {
            var inventory = trainerManager.Profile.BallInventory;
            if (inventory == null) return;

            // 보상 지급 로직
            float roll = Random.value;
            if (roll < 0.01f) { inventory.AddBallCount(BallId.MasterBall, 1); }
            else if (roll < 0.06f) { inventory.AddBallCount(BallId.HyperBall, 1); }
            else { inventory.AddBallCount(BallId.PokeBall, 1); }

            if (Random.value < 0.20f) { inventory.AddBallCount(BallId.PremierBall, 1); }

            // 보상 지급 후 즉시 저장
            trainerManager.Save();
        }
    }
}