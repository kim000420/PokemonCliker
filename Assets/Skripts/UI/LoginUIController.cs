// ����: Scripts/UI/LoginUIController.cs
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PokeClicker
{
    /// <summary>
    /// �α��� �� ȸ������ �׽�Ʈ�� ���� UI ��Ʈ�ѷ�.
    /// �Է� �ʵ��� ���� ��ư �̺�Ʈ�� LoginManager�� �����մϴ�.
    /// </summary>
    public class LoginUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TMP_InputField idInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private TMP_InputField displayNameInput;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button registerButton;
        [SerializeField] private TextMeshProUGUI feedbackText;

        [Header("Dependencies")]
        [SerializeField] private LoginManager loginManager;

        private void OnEnable()
        {
            // UI ��ư Ŭ�� �̺�Ʈ ����
            // LoginManager�� �Ҵ�Ǿ� �ִ��� Ȯ��
            if (loginManager == null)
            {
                Debug.LogError("Login Manager�� �Ҵ���� �ʾҽ��ϴ�. �ν����Ϳ��� �Ҵ����ּ���.", this);
                return;
            }

            if (loginButton != null)
            {
                loginButton.onClick.AddListener(OnLoginButtonClick);
            }
            if (registerButton != null)
            {
                registerButton.onClick.AddListener(OnRegisterButtonClick);
            }
        }

        private void OnDisable()
        {
            // UI ��ư �̺�Ʈ ���� ����
            if (loginButton != null)
            {
                loginButton.onClick.RemoveListener(OnLoginButtonClick);
            }
            if (registerButton != null)
            {
                registerButton.onClick.RemoveListener(OnRegisterButtonClick);
            }
        }

        /// <summary>
        /// �α��� ��ư Ŭ�� �� ȣ��˴ϴ�.
        /// </summary>
        private void OnLoginButtonClick()
        {
            // �Է� ��ȿ�� �˻�
            if (string.IsNullOrWhiteSpace(idInput.text) || string.IsNullOrWhiteSpace(passwordInput.text))
            {
                ShowFeedback("ID�� ��й�ȣ�� �Է����ּ���.", false);
                return;
            }

            // LoginManager�� �α��� ��û
            loginManager.TryLogin(idInput.text, passwordInput.text, OnLoginComplete);
        }

        /// <summary>
        /// ȸ������ ��ư Ŭ�� �� ȣ��˴ϴ�.
        /// </summary>
        private void OnRegisterButtonClick()
        {
            // �Է� ��ȿ�� �˻�
            if (string.IsNullOrWhiteSpace(idInput.text) || string.IsNullOrWhiteSpace(passwordInput.text) || string.IsNullOrWhiteSpace(displayNameInput.text))
            {
                ShowFeedback("ID, ��й�ȣ, ǥ�� �̸��� ��� �Է����ּ���.", false);
                return;
            }

            // LoginManager�� ȸ������ ��û
            loginManager.TryRegister(idInput.text, passwordInput.text, displayNameInput.text, OnRegisterComplete);
        }

        /// <summary>
        /// �α��� �Ϸ� �� �ǵ���� ǥ���մϴ�.
        /// </summary>
        private void OnLoginComplete(bool success, string message)
        {
            ShowFeedback(message, success);
            if (success)
            {
                UIManager.Instance.SwitchToMainUI();
            }
        }

        /// <summary>
        /// ȸ������ �Ϸ� �� �ǵ���� ǥ���մϴ�.
        /// </summary>
        private void OnRegisterComplete(bool success, string message)
        {
            ShowFeedback(message, success);
            if (success)
            {
                UIManager.Instance.SwitchToMainUI();

                Debug.Log($"[UIManager.Instance.SwitchToMainUI]");
            }
        }

        private void ShowFeedback(string message, bool isSuccess)
        {
            if (feedbackText == null) return;
            feedbackText.text = message;
            feedbackText.color = isSuccess ? Color.green : Color.red;
        }
    }
}