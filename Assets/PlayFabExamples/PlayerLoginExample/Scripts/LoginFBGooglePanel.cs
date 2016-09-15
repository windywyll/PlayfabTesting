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
    protected string lastResponse = "";

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

    void Awake()
    {
        FBInit();
    }

    #region Facebook

    #region initFB
    private void FBInit()
    {
        FB.Init(OnInitComplete, OnHideUnity);
    }

    private void OnInitComplete()
    {
        if (FB.IsLoggedIn)
        {
            Debug.Log("LoggedIn");
        }
        else
        {
            CallFBLogin();
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        Debug.Log("Is game showing? " + isGameShown);
    }
    #endregion

    #region LoginFB
    public void LoginFB()
    {

        CallFBLogin();
        
    }

    private void CallFBLogin()
    {
        FB.Login("public_profile,email,user_friends", LoginCallback);
        
    }

    void LoginCallback(FBResult result)
    {
        if (result.Error != null)
            lastResponse = "Error Response:\n" + result.Error;
        else if (!FB.IsLoggedIn)
        {
            lastResponse = "Login cancelled by Player";
        }
        else
        {
            lastResponse = "Login was successful!";

            var loginRequest = new LoginWithFacebookRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                AccessToken = FB.AccessToken,
                CreateAccount = true
            };

            PlayFabClientAPI.LoginWithFacebook(loginRequest, (PFresult) =>
            {
                LoginRegisterSuccess(PFresult.PlayFabId);
            }, (error) =>
            {
                LoginFBErrorText.text = error.ErrorMessage;
                LoginFBErrorText.gameObject.transform.parent.gameObject.SetActive(true);
                PlayFabErrorHandler.HandlePlayFabError(error);
            });

            StartCoroutine("ShowGameMenu");
        }
    }

    #endregion

    #endregion

    #region GoogleLoginNeverFinished

    //The google Login Should probably have it's own panel and own script but as is, it doesn't ork
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
    #endregion

    IEnumerator ShowGameMenu()
    {
        yield return new WaitForSeconds(1f);
        PlayFabDialogManager.SendEvent("GameMenu");
    }

}
