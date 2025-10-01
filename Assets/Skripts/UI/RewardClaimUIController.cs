// ����: Scripts/UI/RewardClaimUIController.cs
using PokeClicker;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardClaimUIController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private RewardManager rewardManager;

    [Header("UI Elements")]
    [SerializeField] private Button claimButton; // ��Ʈ ���� ������ ��ư

    private void Awake()
    {
        claimButton.onClick.AddListener(OnClaimButtonClick);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        claimButton.interactable = true; // ��ư �ٽ� Ȱ��ȭ
    }

    private void OnClaimButtonClick()
    {
        rewardManager.ClaimReward();
        gameObject.SetActive(false);
    }

    public void ShowFeedback(string message)
    {
        // TODO: "Ŭ�� ����" ���� �޽����� ��� �����ִ� ���
    }
}