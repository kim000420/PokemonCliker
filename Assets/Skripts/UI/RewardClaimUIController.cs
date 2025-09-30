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
    [SerializeField] private TextMeshProUGUI costText; // "-1000" 텍스트
    [SerializeField] private List<BallIconMapping> ballIcons; // 볼 아이콘 목록

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 1.0f;
    [SerializeField] private float shakeAmount = 5f;
    [SerializeField] private float shakeSpeed = 30f;
    [SerializeField] private float iconPopHeight = 100f;
    [SerializeField] private float costTextMoveHeight = 50f;

    [System.Serializable]
    public struct BallIconMapping
    {
        public string ballId;
        public GameObject iconPrefab; // 튀어오를 아이콘 프리팹
    }

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
    }

    public void StartClaimAnimation(List<string> rewardedBalls)
    {
        StartCoroutine(ClaimRoutine(rewardedBalls));
    }

    private IEnumerator ClaimRoutine(List<string> rewardedBalls)
    {
        claimButton.interactable = false; // 연출 중 버튼 비활성화

        // 3. -1000 텍스트 표시
        costText.gameObject.SetActive(true);
        // (연출: 텍스트가 잠시 나타났다가 사라지는 로직 추가 가능)

        // 5. 버튼 흔들기 & 1. 2. 볼 튀어오르기 (1초 동안)
        float duration = 1.0f;
        float elapsed = 0f;
        Vector3 originalButtonPos = claimButton.transform.localPosition;

        // 볼 아이콘 생성
        foreach (string ballId in rewardedBalls)
        {
            var mapping = ballIcons.Find(b => b.ballId == ballId);
            if (mapping.iconPrefab != null)
            {
                // 팝업의 자식으로 아이콘 생성
                var icon = Instantiate(mapping.iconPrefab, transform);
                StartCoroutine(AnimateBallIcon(icon));
            }
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // 좌우로 흔들기 (Sin 함수 이용)
            float offsetX = Mathf.Sin(elapsed * 30f) * 5f;
            claimButton.transform.localPosition = new Vector3(originalButtonPos.x + offsetX, originalButtonPos.y, originalButtonPos.z);
            yield return null;
        }

        claimButton.transform.localPosition = originalButtonPos; // 위치 원상복구
        costText.gameObject.SetActive(false); // 코스트 텍스트 숨기기
        gameObject.SetActive(false); // 전체 팝업 비활성화
    }

    public void ShowFeedback(string message)
    {
        // TODO: "클릭 부족" 등의 메시지를 잠시 보여주는 기능
    }

    private IEnumerator AnimateBallIcon(GameObject icon)
    {
        var rectTransform = icon.GetComponent<RectTransform>();
        var canvasGroup = icon.GetComponent<CanvasGroup>();
        rectTransform.localScale = Vector3.zero;
        canvasGroup.alpha = 1f;

        float popDuration = animationDuration * 0.5f;
        float fadeDuration = animationDuration * 0.5f;

        float elapsed = 0f;
        Vector2 startPos = rectTransform.anchoredPosition;

        // 튀어오르는 연출
        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / popDuration;

            float yOffset = Mathf.Sin(progress * Mathf.PI) * iconPopHeight;

            // ▼▼▼ localPosition -> anchoredPosition으로 변경 ▼▼▼
            rectTransform.anchoredPosition = startPos + new Vector2(0, yOffset);

            rectTransform.localScale = Vector3.one * Mathf.Sin(progress * Mathf.PI) * 1.2f;

            yield return null;
        }

        elapsed = 0f;
        // 사라지는 연출
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeDuration;
            canvasGroup.alpha = 1f - progress;
            yield return null;
        }

        Destroy(icon);
    }
}