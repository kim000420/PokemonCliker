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

        // ���콺 ��ư Down (L/R/M)
        if (Input.GetMouseButtonDown(0)) { mouseDown = true; }
        if (Input.GetMouseButtonDown(1)) { mouseDown = true; }
        if (Input.GetMouseButtonDown(2)) { mouseDown = true; }

        // �ջ� ��Ģ: ���콺 Down �̸� 1 ����
        if (mouseDown) total++;

        // Ű���� Down: anyKeyDown�� ���콺�� �����ϹǷ�, ������ ó���� ���� ����
        if (Input.anyKeyDown && !mouseDown) total++;

        // UI �ݿ�
        if (txtTotal) txtTotal.text = $"CLICKS: {total}";

        // ����(�ɼ�)
        if (Input.GetKeyDown(KeyCode.R)) total = 0;
    }

}
