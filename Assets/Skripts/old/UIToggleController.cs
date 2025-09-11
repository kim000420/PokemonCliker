using UnityEngine;
using UnityEngine.UI;

public class UIToggleController : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private Button toggleBtn;          // MiniBar ������ ��ư
    [SerializeField] private GameObject expandedPanel;  // Ȯ�� UI ��Ʈ �г�
    [SerializeField] private RectTransform miniBar;     // ���� ǥ�� ��(��� ���� UI)

    [Header("Behavior")]
    [SerializeField] private bool startExpanded = false;
    [SerializeField] private bool closeOnEsc = true;

    private bool isExpanded;

    void Awake()
    {
        // �ʱ� ����
        SetExpanded(startExpanded);

        // ��ư ����
        if (toggleBtn != null)
            toggleBtn.onClick.AddListener(Toggle);
    }

    void Update()
    {
        if (closeOnEsc && isExpanded && Input.GetKeyDown(KeyCode.Escape))
            SetExpanded(false);
    }

    public void Toggle() => SetExpanded(!isExpanded);

    public void SetExpanded(bool expand)
    {
        isExpanded = expand;

        // Ȯ�� �г� ��/����
        if (expandedPanel != null)
            expandedPanel.SetActive(isExpanded);

        // �̴Ϲٴ� �׻� ���̰�(���ϸ� Ȯ�� �� �̴Ϲ� �������� �ٲ㵵 ��)
        if (miniBar != null)
            miniBar.gameObject.SetActive(true);
    }
}
