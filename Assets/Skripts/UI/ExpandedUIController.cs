// 파일: Scripts/UI/ExpandedUIController.cs
using UnityEngine;
using UnityEngine.UI;

namespace PokeClicker
{
    /// <summary>
    /// 메인 UI에서 접근하는 확장 UI 패널의 최상위 컨트롤러.
    /// 하위 메뉴(메뉴, PokePC, Party 등)의 활성화/비활성화를 관리합니다.
    /// </summary>
    public class ExpandedUIController : MonoBehaviour
    {
        [Header("Sub-Panels")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject pokePcPanel;
        [SerializeField] private GameObject partyPanel;
        [SerializeField] private GameObject summaryPanel;

        [Header("UI Elements")]
        [SerializeField] private Button closeButton; // 확장 UI를 닫는 버튼

        private void OnEnable()
        {
            // 확장 UI가 활성화되면 기본적으로 메뉴 패널을 표시
            ShowMenuPanel();
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClick);
            }
        }

        private void OnDisable()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClick);
            }
        }

        /// <summary>
        /// 닫기 버튼 클릭 시 호출됩니다.
        /// </summary>
        private void OnCloseButtonClick()
        {
            UIManager.Instance.SetExpandedPanelActive(false);
        }

        /// <summary>
        /// 모든 서브 패널을 비활성화합니다.
        /// </summary>
        private void DeactivateAllSubPanels()
        {
            menuPanel.SetActive(false);
            pokePcPanel.SetActive(false);
            partyPanel.SetActive(false);
            summaryPanel.SetActive(false);
        }

        /// <summary>
        /// 메뉴 패널을 활성화하고 다른 패널은 비활성화합니다.
        /// </summary>
        public void ShowMenuPanel()
        {
            DeactivateAllSubPanels();
            menuPanel.SetActive(true);
        }

        /// <summary>
        /// PokePC 패널을 활성화하고 다른 패널은 비활성화합니다.
        /// </summary>
        public void ShowPokePcPanel()
        {
            DeactivateAllSubPanels();
            pokePcPanel.SetActive(true);
        }

        /// <summary>
        /// 파티 패널을 활성화하고 다른 패널은 비활성화합니다.
        /// </summary>
        public void ShowPartyPanel()
        {
            DeactivateAllSubPanels();
            partyPanel.SetActive(true);
        }

        /// <summary>
        /// 요약 패널을 활성화하고 다른 패널은 비활성화합니다.
        /// </summary>
        public void ShowSummaryPanel()
        {
            DeactivateAllSubPanels();
            summaryPanel.SetActive(true);
        }
    }
}