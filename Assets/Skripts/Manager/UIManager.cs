// ����: Scripts/UI/UIManager.cs
using System;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// ���� �� ��� UI �г��� Ȱ��ȭ ���¸� �߾ӿ��� �����ϴ� �Ŵ���.
    /// �̱��� �������� �����Ǿ� �ٸ� ��ũ��Ʈ���� ���� ������ �� �ֽ��ϴ�.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject expandedPanel;
        [SerializeField] private MainUIController mainUIController;

        private void Awake()
        {
            // �̱��� �ν��Ͻ� ���� �޼���
            // UIManager�� ������ �ν��Ͻ��� �����մϴ�.
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Ư�� UI �г��� Ȱ��ȭ ���¸� �����մϴ�.
        /// </summary>
        /// <param name="panel">Ȱ��ȭ/��Ȱ��ȭ�� �г�</param>
        /// <param name="isActive">Ȱ��ȭ ����</param>
        public void SetPanelActive(GameObject panel, bool isActive)
        {
            if (panel != null)
            {
                panel.SetActive(isActive);
            }
        }

        /// <summary>
        /// �α��� UI�� Ȱ��ȭ/��Ȱ��ȭ�մϴ�.
        /// </summary>
        public void SetLoginPanelActive(bool isActive)
        {
            SetPanelActive(loginPanel, isActive);
        }

        /// <summary>
        /// ���� UI�� Ȱ��ȭ/��Ȱ��ȭ�մϴ�.
        /// </summary>
        public void SetMainPanelActive(bool isActive)
        {
            SetPanelActive(mainPanel, isActive);
        }

        /// <summary>
        /// Ȯ�� UI�� Ȱ��ȭ/��Ȱ��ȭ�մϴ�.
        /// </summary>
        public void SetExpandedPanelActive(bool isActive)
        {
            SetPanelActive(expandedPanel, isActive);
        }

        /// <summary>
        /// �α��� ���� �� UI�� �α��� �� �������� ��ȯ�մϴ�.
        /// </summary>
        public void SwitchToMainUI()
        {
            SetLoginPanelActive(false);
            SetMainPanelActive(true);

            if (mainUIController != null)
            {
                mainUIController.UpdateMainUI();
            }
        }

        public MainUIController GetMainUIController()
        {
            return mainUIController;
        }
    }
}