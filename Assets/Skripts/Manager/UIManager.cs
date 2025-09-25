// 파일: Scripts/UI/UIManager.cs
using System;
using UnityEngine;

namespace PokeClicker
{
    /// <summary>
    /// 게임 내 모든 UI 패널의 활성화 상태를 중앙에서 관리하는 매니저.
    /// 싱글턴 패턴으로 구현되어 다른 스크립트에서 쉽게 접근할 수 있습니다.
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
            // 싱글턴 인스턴스 설정 메서드
            // UIManager의 유일한 인스턴스를 보장합니다.
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
        /// 특정 UI 패널의 활성화 상태를 설정합니다.
        /// </summary>
        /// <param name="panel">활성화/비활성화할 패널</param>
        /// <param name="isActive">활성화 여부</param>
        public void SetPanelActive(GameObject panel, bool isActive)
        {
            if (panel != null)
            {
                panel.SetActive(isActive);
            }
        }

        /// <summary>
        /// 로그인 UI를 활성화/비활성화합니다.
        /// </summary>
        public void SetLoginPanelActive(bool isActive)
        {
            SetPanelActive(loginPanel, isActive);
        }

        /// <summary>
        /// 메인 UI를 활성화/비활성화합니다.
        /// </summary>
        public void SetMainPanelActive(bool isActive)
        {
            SetPanelActive(mainPanel, isActive);
        }

        /// <summary>
        /// 확장 UI를 활성화/비활성화합니다.
        /// </summary>
        public void SetExpandedPanelActive(bool isActive)
        {
            SetPanelActive(expandedPanel, isActive);
        }

        /// <summary>
        /// 로그인 성공 시 UI를 로그인 → 메인으로 전환합니다.
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