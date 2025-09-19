// ����: Scripts/UI/ExpandedUIController.cs
using UnityEngine;
using UnityEngine.UI;

namespace PokeClicker
{
    /// <summary>
    /// ���� UI���� �����ϴ� Ȯ�� UI �г��� �ֻ��� ��Ʈ�ѷ�.
    /// ���� �޴�(�޴�, PokePC, Party ��)�� Ȱ��ȭ/��Ȱ��ȭ�� �����մϴ�.
    /// </summary>
    public class ExpandedUIController : MonoBehaviour
    {
        [Header("Sub-Panels")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject pokePcPanel;
        [SerializeField] private GameObject partyPanel;
        [SerializeField] private GameObject summaryPanel;

        [Header("UI Elements")]
        [SerializeField] private Button closeButton; // Ȯ�� UI�� �ݴ� ��ư

        private void OnEnable()
        {
            // Ȯ�� UI�� Ȱ��ȭ�Ǹ� �⺻������ �޴� �г��� ǥ��
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
        /// �ݱ� ��ư Ŭ�� �� ȣ��˴ϴ�.
        /// </summary>
        private void OnCloseButtonClick()
        {
            UIManager.Instance.SetExpandedPanelActive(false);
        }

        /// <summary>
        /// ��� ���� �г��� ��Ȱ��ȭ�մϴ�.
        /// </summary>
        private void DeactivateAllSubPanels()
        {
            menuPanel.SetActive(false);
            pokePcPanel.SetActive(false);
            partyPanel.SetActive(false);
            summaryPanel.SetActive(false);
        }

        /// <summary>
        /// �޴� �г��� Ȱ��ȭ�ϰ� �ٸ� �г��� ��Ȱ��ȭ�մϴ�.
        /// </summary>
        public void ShowMenuPanel()
        {
            DeactivateAllSubPanels();
            menuPanel.SetActive(true);
        }

        /// <summary>
        /// PokePC �г��� Ȱ��ȭ�ϰ� �ٸ� �г��� ��Ȱ��ȭ�մϴ�.
        /// </summary>
        public void ShowPokePcPanel()
        {
            DeactivateAllSubPanels();
            pokePcPanel.SetActive(true);
        }

        /// <summary>
        /// ��Ƽ �г��� Ȱ��ȭ�ϰ� �ٸ� �г��� ��Ȱ��ȭ�մϴ�.
        /// </summary>
        public void ShowPartyPanel()
        {
            DeactivateAllSubPanels();
            partyPanel.SetActive(true);
        }

        /// <summary>
        /// ��� �г��� Ȱ��ȭ�ϰ� �ٸ� �г��� ��Ȱ��ȭ�մϴ�.
        /// </summary>
        public void ShowSummaryPanel()
        {
            DeactivateAllSubPanels();
            summaryPanel.SetActive(true);
        }
    }
}