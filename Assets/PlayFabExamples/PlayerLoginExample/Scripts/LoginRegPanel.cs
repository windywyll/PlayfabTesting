using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Security;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using PlayFab;
using PlayFab.ClientModels;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;

public class LoginRegPanel : MonoBehaviour
{
    public InputField LoginUsernameField;
    public InputField LoginPasswordField;
    public Text LoginErrorText;
    public Button LoginButton;
    public InputField RegUsernameField;
    public InputField RegEmailField;
    public InputField RegPasswordField;
    public Text RegErrorText;
    public Button RegButton;

    void OnEnable()
    {
        LoginButton.onClick.AddListener(Login);
        RegButton.onClick.AddListener(Register);
    }

    void OnDisable()
    {
        LoginButton.onClick.RemoveAllListeners();
        RegButton.onClick.RemoveAllListeners();
    }


    public void Login()
    {

         var loginRequest = new LoginWithPlayFabRequest()
         {
             TitleId = PlayFabSettings.TitleId,
             Username = LoginUsernameField.text, 
             Password = LoginPasswordField.text
         };

        PlayFabClientAPI.LoginWithPlayFab(loginRequest, (result) =>
        {
            LoginSuccess(result.PlayFabId);
        }, (error) =>
        {
            LoginErrorText.text = error.ErrorMessage;
            LoginErrorText.gameObject.transform.parent.gameObject.SetActive(true);
            PlayFabErrorHandler.HandlePlayFabError(error);
        });

    }

    public void Register()
    {
        /*
        var request = new RegisterPlayFabUserRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            Username = RegUsernameField.text,
            Password = RegPasswordField.text,
            Email = RegEmailField.text
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, (result) =>
        {
            RegisterSuccess(result.PlayFabId);
        }, (error) =>
        {
            RegErrorText.text = error.ErrorMessage;
            RegErrorText.gameObject.transform.parent.gameObject.SetActive(true);
            PlayFabErrorHandler.HandlePlayFabError(error);
        });*/
    }

    private void sendMail(String hash)
    {
        
        MailMessage msg = new MailMessage();

        msg.To.Add(RegEmailField.text);
        msg.Subject = "E-mail Verification";
        msg.Body = "Click on the following link to Verify your address:\n" +
                    "http://www.ttg.com/verify?v=" + hash + 
                    "If the link does not work, copy the link in your address bar";
        msg.From = new MailAddress("mauricejrd@gmail.com", "TTG");

        SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
        smtpServer.Port = 587;
        smtpServer.Credentials = new NetworkCredential("mauricejrd@gmail.com", "ktwuxazbmjmqwabr") as ICredentialsByHost;
        smtpServer.EnableSsl = true;
        ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            { return true; };
        smtpServer.Send(msg);
    }

    internal static string GetStringSha256Hash(string text)
    {
        if (String.IsNullOrEmpty(text))
            return String.Empty;

        using (var sha = new SHA256Managed())
        {
            byte[] textData = Encoding.UTF8.GetBytes(text);
            byte[] hash = sha.ComputeHash(textData);
            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }
    }

    private bool isAccountVerified(string playFabID)
    {
        UserDataRecord data = null;

        GetUserDataRequest req = new GetUserDataRequest()
        {
            PlayFabId = playFabID,
            Keys = null
        };
        
        PlayFabClientAPI.GetUserReadOnlyData(req, (result) => {
            Debug.Log("Got user data:");

            if ((result.Data == null) || (result.Data.Count == 0))
            {
                Debug.Log("No user data available");
            }
            else
            {
                result.Data.TryGetValue("Verified", out data);
                Debug.Log(data.Value);
            }
        }, (error) => {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.ErrorMessage);
        });

        if (data != null)
        {
            if (data.Value == "true")
                return true;
        }

        return false;
    }

    private void LoginSuccess(string playFabId)
    {
        PlayFab.PlayFabAuthManager.PlayFabId = playFabId;
        //deactivate panel
        //LoginErrorText.gameObject.transform.parent.gameObject.SetActive(false);
        //RegErrorText.gameObject.transform.parent.gameObject.SetActive(false);

        //isAccountVerified(playFabId);

        StartCoroutine(ShowGameMenu());

        Debug.Log("Login Successfully Go to your game..");
    }

    private void Logout()
    {
        //Wipe SessionTicket
    }

    private void RegisterSuccess(string playFabId)
    {
        PlayFab.PlayFabAuthManager.PlayFabId = playFabId;
        LoginErrorText.gameObject.transform.parent.gameObject.SetActive(false);
        RegErrorText.gameObject.transform.parent.gameObject.SetActive(false);

        String toHash = GetStringSha256Hash(RegEmailField.text);
        toHash += GetStringSha256Hash(RegUsernameField.text);
        toHash += GetStringSha256Hash(DateTime.UtcNow.ToString());

        sendMail(toHash);

        CloudScripts.AddVerificationsData(toHash);

        PlayFab.PlayFabAuthManager.PlayFabId = null;

        Debug.Log("Register Successfully");
    }

    IEnumerator ShowGameMenu()
    {
        yield return new WaitForSeconds(1f);
        PlayFabDialogManager.SendEvent("GameMenu");
    }
}
