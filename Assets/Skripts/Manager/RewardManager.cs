// 파일: Scripts/Managers/RewardManager.cs
using UnityEngine;
using System.Collections;
using PokeClicker;

public class RewardManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float rewardIntervalMinutes = 30.0f; // 보상 간격 (분 단위)
    [SerializeField] private int requiredClicks = 1000;

    [Header("Dependencies")]
    [SerializeField] private PokemonTrainerManager trainerManager;
    [SerializeField] private GameObject rewardPopup; // 30분마다 띄울 팝업 UI

    private void Start()
    {
        // 게임이 시작되면 타이머 코루틴을 실행합니다.
        StartCoroutine(RewardTimerRoutine());
    }

    private IEnumerator RewardTimerRoutine()
    {
        while (true)
        {
            // 지정된 시간(분)만큼 기다립니다.
            yield return new WaitForSeconds(rewardIntervalMinutes * 60);

            // 시간이 되면 팝업 UI를 활성화합니다.
            if (rewardPopup != null)
            {
                Debug.Log("몬스터볼 획득 기회! 팝업을 활성화합니다.");
                rewardPopup.SetActive(true);
            }
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
        Debug.Log($"{requiredClicks} 클릭을 소모했습니다. 남은 클릭: {trainerManager.Progress.totalInputs}");

        // 3. 확률에 따라 랜덤 몬스터볼 지급
        GrantRandomPokeball();

        // 4. 팝업 UI 비활성화
        rewardPopup.SetActive(false);
    }

    private void GrantRandomPokeball()
    {
        var inventory = trainerManager.Profile.BallInventory;
        if (inventory == null) return;

        // 기본 볼 추첨 (몬스터볼, 하이퍼볼, 마스터볼)
        float roll = Random.value; // 0.0 ~ 1.0
        if (roll < 0.01f) // 1%
        {
            inventory.AddBallCount(BallId.MasterBall, 1);
            Debug.Log("마스터볼 획득!");
        }
        else if (roll < 0.06f) // 5% (1% + 5%)
        {
            inventory.AddBallCount(BallId.HyperBall, 1);
            Debug.Log("하이퍼볼 획득!");
        }
        else // 94%
        {
            inventory.AddBallCount(BallId.PokeBall, 1);
            Debug.Log("몬스터볼 획득!");
        }

        // 프리미어볼은 별도로 20% 확률로 추가 지급
        if (Random.value < 0.20f)
        {
            inventory.AddBallCount(BallId.PremierBall, 1);
            Debug.Log("추가로 프리미어볼 획득!");
        }

        // 변경된 인벤토리 정보를 저장하기 위해 Save 호출
        trainerManager.Save();
    }
}