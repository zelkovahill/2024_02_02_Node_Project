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
        statusText.text = "ȸ������ ��...";
        yield return StartCoroutine(authManager.Register(usernameInput.text, passwordInput.text));
        statusText.text = "ȸ������ ����. �α��� ���ּ���";
    }

    private IEnumerator LoginCorutine()
    {
        statusText.text = "�α��� ��...";
        yield return StartCoroutine(authManager.Login(usernameInput.text, passwordInput.text));
        statusText.text = "�α��� ����";
    }

    private IEnumerator LogoutCorutine()
    {
        statusText.text = "�α׾ƿ� ��...";
        yield return StartCoroutine(authManager.Logout());
        statusText.text = "�α׾ƿ� ����";
    }

    private IEnumerator GetDataCoroutone()
    {
        statusText.text = "������ ��û ��...";
        yield return StartCoroutine(authManager.GetProtectedData());
        statusText.text = "������ ��û �Ϸ�";
    }


}
