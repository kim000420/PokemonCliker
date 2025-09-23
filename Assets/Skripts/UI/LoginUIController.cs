// 파일: Scripts/UI/LoginUIController.cs
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PokeClicker
{
    /// <summary>
    /// 로그인 및 회원가입 테스트를 위한 UI 컨트롤러.
    /// 입력 필드의 값과 버튼 이벤트를 LoginManager에 전달합니다.
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
            // UI 버튼 클릭 이벤트 연결
            // LoginManager가 할당되어 있는지 확인
            if (loginManager == null)
            {
                Debug.LogError("Login Manager가 할당되지 않았습니다. 인스펙터에서 할당해주세요.", this);
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
            // UI 버튼 이벤트 연결 해제
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
        /// 로그인 버튼 클릭 시 호출됩니다.
        /// </summary>
        private void OnLoginButtonClick()
        {
            // 입력 유효성 검사
            if (string.IsNullOrWhiteSpace(idInput.text) || string.IsNullOrWhiteSpace(passwordInput.text))
            {
                ShowFeedback("ID와 비밀번호를 입력해주세요.", false);
                return;
            }

            // LoginManager에 로그인 요청
            loginManager.TryLogin(idInput.text, passwordInput.text, OnLoginComplete);
        }

        /// <summary>
        /// 회원가입 버튼 클릭 시 호출됩니다.
        /// </summary>
        private void OnRegisterButtonClick()
        {
            // 입력 유효성 검사
            if (string.IsNullOrWhiteSpace(idInput.text) || string.IsNullOrWhiteSpace(passwordInput.text) || string.IsNullOrWhiteSpace(displayNameInput.text))
            {
                ShowFeedback("ID, 비밀번호, 표시 이름을 모두 입력해주세요.", false);
                return;
            }

            // LoginManager에 회원가입 요청
            loginManager.TryRegister(idInput.text, passwordInput.text, displayNameInput.text, OnRegisterComplete);
        }

        /// <summary>
        /// 로그인 완료 시 피드백을 표시합니다.
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
        /// 회원가입 완료 시 피드백을 표시합니다.
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