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
    [SerializeField] private TextMeshProUGUI costText; // "-1000" �ؽ�Ʈ
    [SerializeField] private List<BallIconMapping> ballIcons; // �� ������ ���

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
        public GameObject iconPrefab; // Ƣ����� ������ ������
    }

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
    }

    public void StartClaimAnimation(List<string> rewardedBalls)
    {
        StartCoroutine(ClaimRoutine(rewardedBalls));
    }

    private IEnumerator ClaimRoutine(List<string> rewardedBalls)
    {
        claimButton.interactable = false; // ���� �� ��ư ��Ȱ��ȭ

        // 3. -1000 �ؽ�Ʈ ǥ��
        costText.gameObject.SetActive(true);
        // (����: �ؽ�Ʈ�� ��� ��Ÿ���ٰ� ������� ���� �߰� ����)

        // 5. ��ư ���� & 1. 2. �� Ƣ������� (1�� ����)
        float duration = 1.0f;
        float elapsed = 0f;
        Vector3 originalButtonPos = claimButton.transform.localPosition;

        // �� ������ ����
        foreach (string ballId in rewardedBalls)
        {
            var mapping = ballIcons.Find(b => b.ballId == ballId);
            if (mapping.iconPrefab != null)
            {
                // �˾��� �ڽ����� ������ ����
                var icon = Instantiate(mapping.iconPrefab, transform);
                StartCoroutine(AnimateBallIcon(icon));
            }
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // �¿�� ���� (Sin �Լ� �̿�)
            float offsetX = Mathf.Sin(elapsed * 30f) * 5f;
            claimButton.transform.localPosition = new Vector3(originalButtonPos.x + offsetX, originalButtonPos.y, originalButtonPos.z);
            yield return null;
        }

        claimButton.transform.localPosition = originalButtonPos; // ��ġ ���󺹱�
        costText.gameObject.SetActive(false); // �ڽ�Ʈ �ؽ�Ʈ �����
        gameObject.SetActive(false); // ��ü �˾� ��Ȱ��ȭ
    }

    public void ShowFeedback(string message)
    {
        // TODO: "Ŭ�� ����" ���� �޽����� ��� �����ִ� ���
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

        // Ƣ������� ����
        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / popDuration;

            float yOffset = Mathf.Sin(progress * Mathf.PI) * iconPopHeight;

            // ���� localPosition -> anchoredPosition���� ���� ����
            rectTransform.anchoredPosition = startPos + new Vector2(0, yOffset);

            rectTransform.localScale = Vector3.one * Mathf.Sin(progress * Mathf.PI) * 1.2f;

            yield return null;
        }

        elapsed = 0f;
        // ������� ����
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