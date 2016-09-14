using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;

public class LoginFBGooglePanel : MonoBehaviour
{
    public InputField LoginFBUsernameField;
    public InputField LoginFBPasswordField;
    public Text LoginFBErrorText;
    public Button LoginFBButton;
    public InputField LoginGoogleUsernameField;
    public InputField LoginGooglePasswordField;
    public Text LoginGoogleErrorText;
    public Button LoginGoogleButton;

    void OnEnable()
    {
        LoginFBButton.onClick.AddListener(LoginFB);
        LoginGoogleButton.onClick.AddListener(LoginGoogle);
    }

    void OnDisable()
    {
        LoginFBButton.onClick.RemoveAllListeners();
        LoginGoogleButton.onClick.RemoveAllListeners();
    }


    public void LoginFB()
    {

        var loginRequest = new LoginWithFacebookRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            AccessToken = "jfidsoph",
            CreateAccount = true
            //Username = LoginFBUsernameField.text,
            //Password = LoginFBPasswordField.text
        };

        PlayFabClientAPI.LoginWithFacebook(loginRequest, (result) =>
        {
            LoginRegisterSuccess(result.PlayFabId);
        }, (error) =>
        {
            LoginFBErrorText.text = error.ErrorMessage;
            LoginFBErrorText.gameObject.transform.parent.gameObject.SetActive(true);
            PlayFabErrorHandler.HandlePlayFabError(error);
        });

    }

    public void LoginGoogle()
    {
        var request = new LoginWithGoogleAccountRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            AccessToken = "klmnkm",
            CreateAccount = true
            //Username = LoginGoogleUsernameField.text,
            ///Password = LoginGooglePasswordField.text
        };
        PlayFabClientAPI.LoginWithGoogleAccount(request, (result) =>
        {
            LoginRegisterSuccess(result.PlayFabId);
        }, (error) =>
        {
            LoginGoogleErrorText.text = error.ErrorMessage;
            LoginGoogleErrorText.gameObject.transform.parent.gameObject.SetActive(true);
            PlayFabErrorHandler.HandlePlayFabError(error);
        });
    }

    private void LoginRegisterSuccess(string playFabId)
    {
        PlayFab.PlayFabAuthManager.PlayFabId = playFabId;
        LoginFBErrorText.gameObject.transform.parent.gameObject.SetActive(false);
        LoginGoogleErrorText.gameObject.transform.parent.gameObject.SetActive(false);

        Debug.Log("Register Successfully");

        Debug.Log("Login Successfully Go to your game..");
    }

    IEnumerator ShowGameMenu()
    {
        yield return new WaitForSeconds(1f);
        PlayFabDialogManager.SendEvent("GameMenu");
    }

}
