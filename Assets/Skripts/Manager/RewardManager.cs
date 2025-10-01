// ����: Scripts/Managers/RewardManager.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PokeClicker
{
    public class RewardManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float rewardIntervalMinutes = 30.0f; // ���� ���� (�� ����)
        [SerializeField] private int requiredClicks = 1000;

        [Header("Dependencies")]
        [SerializeField] private PokemonTrainerManager trainerManager;
        [SerializeField] private GameObject rewardPopup; // 30�и��� ��� �˾� UI
        [SerializeField] private RewardClaimUIController rewardClaimUI;

        public float TimeRemaining { get; private set; }

        private void Start()
        {
            // ������ ���۵Ǹ� Ÿ�̸� �ڷ�ƾ�� �����մϴ�.
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

                // �ð��� 0�� �Ǹ� �˾� UI�� Ȱ��ȭ
                if (rewardClaimUI != null && !rewardClaimUI.gameObject.activeInHierarchy)
                {
                    Debug.Log("���ͺ� ȹ�� ��ȸ! �˾��� Ȱ��ȭ�մϴ�.");
                    rewardClaimUI.Show();
                }
                yield return new WaitForSeconds(1f); // �˾��� ���ִ� ���� ���� Ÿ�̸Ӹ� �ٽ� üũ���� �ʵ��� ��� ���
            }
        }

        /// <summary>
        /// �˾� UI�� ��ư�� ȣ���� �޼���. Ŭ�� Ƚ���� Ȯ���ϰ� ������ �����մϴ�.
        /// </summary>
        public void ClaimReward()
        {

            if (trainerManager.Progress.totalInputs < requiredClicks)
            {
                Debug.Log("Ŭ�� Ƚ���� �����Ͽ� ������ ���� �� �����ϴ�.");
                rewardClaimUI.ShowFeedback("Ŭ�� Ƚ���� �����մϴ�!");
                return;
            }

            trainerManager.Progress.totalInputs -= requiredClicks;
            Debug.Log($"{requiredClicks} Ŭ���� �Ҹ��߽��ϴ�. ���� Ŭ��: {trainerManager.Progress.totalInputs}");

            GrantRandomPokeball();
        }

        private void GrantRandomPokeball()
        {
            var inventory = trainerManager.Profile.BallInventory;
            if (inventory == null) return;

            // ���� ���� ����
            float roll = Random.value;
            if (roll < 0.01f) { inventory.AddBallCount(BallId.MasterBall, 1); }
            else if (roll < 0.06f) { inventory.AddBallCount(BallId.HyperBall, 1); }
            else { inventory.AddBallCount(BallId.PokeBall, 1); }

            if (Random.value < 0.20f) { inventory.AddBallCount(BallId.PremierBall, 1); }

            // ���� ���� �� ��� ����
            trainerManager.Save();
        }
    }
}