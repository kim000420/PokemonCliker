// ����: Scripts/Managers/RewardManager.cs
using UnityEngine;
using System.Collections;
using PokeClicker;

public class RewardManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float rewardIntervalMinutes = 30.0f; // ���� ���� (�� ����)
    [SerializeField] private int requiredClicks = 1000;

    [Header("Dependencies")]
    [SerializeField] private PokemonTrainerManager trainerManager;
    [SerializeField] private GameObject rewardPopup; // 30�и��� ��� �˾� UI

    private void Start()
    {
        // ������ ���۵Ǹ� Ÿ�̸� �ڷ�ƾ�� �����մϴ�.
        StartCoroutine(RewardTimerRoutine());
    }

    private IEnumerator RewardTimerRoutine()
    {
        while (true)
        {
            // ������ �ð�(��)��ŭ ��ٸ��ϴ�.
            yield return new WaitForSeconds(rewardIntervalMinutes * 60);

            // �ð��� �Ǹ� �˾� UI�� Ȱ��ȭ�մϴ�.
            if (rewardPopup != null)
            {
                Debug.Log("���ͺ� ȹ�� ��ȸ! �˾��� Ȱ��ȭ�մϴ�.");
                rewardPopup.SetActive(true);
            }
        }
    }

    /// <summary>
    /// �˾� UI�� ��ư�� ȣ���� �޼���. Ŭ�� Ƚ���� Ȯ���ϰ� ������ �����մϴ�.
    /// </summary>
    public void ClaimReward()
    {
        // 1. Ŭ�� Ƚ���� ������� Ȯ��
        if (trainerManager.Progress.totalInputs < requiredClicks)
        {
            Debug.Log("Ŭ�� Ƚ���� �����Ͽ� ������ ���� �� �����ϴ�.");
            // ���⿡ "Ŭ���� �����մϴ�" ���� UI �ǵ���� �߰��� �� �ֽ��ϴ�.
            return;
        }

        // 2. Ŭ�� Ƚ�� ���� (totalInputs�� ���� ���̴� ���, ������ ������ ����� ���� �ֽ��ϴ�)
        // ���⼭�� �����ϰ� totalInputs�� ���� �����ϰڽ��ϴ�.
        trainerManager.Progress.totalInputs -= requiredClicks;
        Debug.Log($"{requiredClicks} Ŭ���� �Ҹ��߽��ϴ�. ���� Ŭ��: {trainerManager.Progress.totalInputs}");

        // 3. Ȯ���� ���� ���� ���ͺ� ����
        GrantRandomPokeball();

        // 4. �˾� UI ��Ȱ��ȭ
        rewardPopup.SetActive(false);
    }

    private void GrantRandomPokeball()
    {
        var inventory = trainerManager.Profile.BallInventory;
        if (inventory == null) return;

        // �⺻ �� ��÷ (���ͺ�, �����ۺ�, �����ͺ�)
        float roll = Random.value; // 0.0 ~ 1.0
        if (roll < 0.01f) // 1%
        {
            inventory.AddBallCount(BallId.MasterBall, 1);
            Debug.Log("�����ͺ� ȹ��!");
        }
        else if (roll < 0.06f) // 5% (1% + 5%)
        {
            inventory.AddBallCount(BallId.HyperBall, 1);
            Debug.Log("�����ۺ� ȹ��!");
        }
        else // 94%
        {
            inventory.AddBallCount(BallId.PokeBall, 1);
            Debug.Log("���ͺ� ȹ��!");
        }

        // �����̾�� ������ 20% Ȯ���� �߰� ����
        if (Random.value < 0.20f)
        {
            inventory.AddBallCount(BallId.PremierBall, 1);
            Debug.Log("�߰��� �����̾ ȹ��!");
        }

        // ����� �κ��丮 ������ �����ϱ� ���� Save ȣ��
        trainerManager.Save();
    }
}