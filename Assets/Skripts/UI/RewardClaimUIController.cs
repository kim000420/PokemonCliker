// 파일: Scripts/UI/RewardClaimUIController.cs
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
    [SerializeField] private Button claimButton; // 하트 편지 아이콘 버튼

    private void Awake()
    {
        claimButton.onClick.AddListener(OnClaimButtonClick);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        claimButton.interactable = true; // 버튼 다시 활성화
    }

    private void OnClaimButtonClick()
    {
        rewardManager.ClaimReward();
        gameObject.SetActive(false);
    }

    public void ShowFeedback(string message)
    {
        // TODO: "클릭 부족" 등의 메시지를 잠시 보여주는 기능
    }
}