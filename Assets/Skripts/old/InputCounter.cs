using UnityEngine;
using TMPro;

public class InputCounter : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI txtTotal;

    [Header("Counts (read-only)")]
    [SerializeField] private long total;

    void Update()
    {
        bool mouseDown = false;

        // 마우스 버튼 Down (L/R/M)
        if (Input.GetMouseButtonDown(0)) { mouseDown = true; }
        if (Input.GetMouseButtonDown(1)) { mouseDown = true; }
        if (Input.GetMouseButtonDown(2)) { mouseDown = true; }

        // 합산 규칙: 마우스 Down 이면 1 증가
        if (mouseDown) total++;

        // 키보드 Down: anyKeyDown은 마우스도 포함하므로, 위에서 처리한 경우는 제외
        if (Input.anyKeyDown && !mouseDown) total++;

        // UI 반영
        if (txtTotal) txtTotal.text = $"CLICKS: {total}";

        // 리셋(옵션)
        if (Input.GetKeyDown(KeyCode.R)) total = 0;
    }

}
