using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class LoginScript : MonoBehaviour {
    [SerializeField] GameObject loginPanel, mainMenuPanel;
    [SerializeField] GameObject FailedToLoginText, PleaseLoginWithFacebookText, ConnectingText;

    public void LoginClicked() {
        FailedToLoginText.SetActive(false);
        PleaseLoginWithFacebookText.SetActive(false);
        ConnectingText.SetActive(true);

        new Thread(delegate () {
            if (NetworkDatabase.NDB.TryLogin())
                sucessConnect = true;
            else
                failedConnect = true;
        }).Start();
    }

    private void Start() {
        if (!NetworkDatabase.NDB.IsLoggedIn()) {
            SwitchToLoginPanel();
        }

    }

    public void LogoutClicked() {
        NetworkDatabase.NDB.Logout();
        SwitchToLoginPanel();
    }

    private void SwitchToLoginPanel() {
        FailedToLoginText.SetActive(false);
        PleaseLoginWithFacebookText.SetActive(false);
        ConnectingText.SetActive(false);
        loginPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        NetworkDatabase.NDB.OnLoggedIn += () => { shouldSwitch = true; return true; };
    }

    public static bool logout = false;
    bool failedConnect = false;
    bool sucessConnect = false;
    bool shouldSwitch = false;
    private void Update() {
        if (shouldSwitch) {
            loginPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
            shouldSwitch = false;
        }
        if (failedConnect) {
            ConnectingText.SetActive(false);
            FailedToLoginText.SetActive(true);
            failedConnect = false;
        }
        if (sucessConnect) {
            ConnectingText.SetActive(false);
            PleaseLoginWithFacebookText.SetActive(true);
            sucessConnect = false;
        }
        if (logout) {
            LogoutClicked();
            logout = false;
        }
    }
}
