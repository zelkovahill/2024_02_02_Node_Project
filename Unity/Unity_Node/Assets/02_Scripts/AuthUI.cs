using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuthUI : MonoBehaviour
{
    public InputField usernameInput;
    public InputField passwordInput;

    public Button registerButton;
    public Button loginButton;
    public Button logoutButton;
    public Button getDataButton;


    public Text statusText;

    private AuthManager authManager;

    private void Start()
    {
        authManager = GetComponent<AuthManager>();
        registerButton.onClick.AddListener(OnResisterClick);
        loginButton.onClick.AddListener(OnLoginClick);
        logoutButton.onClick.AddListener(OnLogoutClick);
        getDataButton.onClick.AddListener(GetDataCoroutine);

    }

    private void OnResisterClick()
    {
        StartCoroutine(RegisterCorutine());
    }
    private void OnLoginClick()
    {
        StartCoroutine(LoginCorutine());
    }

    private void OnLogoutClick()
    {
        StartCoroutine(LogoutCorutine());
    }

    private void GetDataCoroutine()
    {
        StartCoroutine(GetDataCoroutone());
    }


    private IEnumerator RegisterCorutine()
    {
        statusText.text = "회원가입 중...";
        yield return StartCoroutine(authManager.Register(usernameInput.text, passwordInput.text));
        statusText.text = "회원가입 성공. 로그인 해주세요";
    }

    private IEnumerator LoginCorutine()
    {
        statusText.text = "로그인 중...";
        yield return StartCoroutine(authManager.Login(usernameInput.text, passwordInput.text));
        statusText.text = "로그인 성공";
    }

    private IEnumerator LogoutCorutine()
    {
        statusText.text = "로그아웃 중...";
        yield return StartCoroutine(authManager.Logout());
        statusText.text = "로그아웃 성공";
    }

    private IEnumerator GetDataCoroutone()
    {
        statusText.text = "데이터 요청 중...";
        yield return StartCoroutine(authManager.GetProtectedData());
        statusText.text = "데이터 요청 완료";
    }


}
