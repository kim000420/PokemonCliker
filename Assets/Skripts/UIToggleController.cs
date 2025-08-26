using UnityEngine;
using UnityEngine.UI;

public class UIToggleController : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private Button toggleBtn;          // MiniBar 오른쪽 버튼
    [SerializeField] private GameObject expandedPanel;  // 확장 UI 루트 패널
    [SerializeField] private RectTransform miniBar;     // 숫자 표시 바(축소 상태 UI)

    [Header("Behavior")]
    [SerializeField] private bool startExpanded = false;
    [SerializeField] private bool closeOnEsc = true;

    private bool isExpanded;

    void Awake()
    {
        // 초기 상태
        SetExpanded(startExpanded);

        // 버튼 연결
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

        // 확장 패널 온/오프
        if (expandedPanel != null)
            expandedPanel.SetActive(isExpanded);

        // 미니바는 항상 보이게(원하면 확장 시 미니바 숨김으로 바꿔도 됨)
        if (miniBar != null)
            miniBar.gameObject.SetActive(true);
    }
}
