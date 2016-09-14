using UnityEngine;
using System.Collections;
using PlayFab;

public class BackgroundPanel : MonoBehaviour {

    void Awake()
    {
        PlayFab.PlayFabAuthManager.OnPlatformCheckComplete += OnPlatFormCheck;
        PlayFab.PlayFabAuthManager.OnReturnPlayerCheckComplete += OnReturnPlayerCheck;
    }

    void OnDisable()
    {
        PlayFab.PlayFabAuthManager.OnPlatformCheckComplete -= OnPlatFormCheck;
        PlayFab.PlayFabAuthManager.OnReturnPlayerCheckComplete -= OnReturnPlayerCheck;
    }

    private void OnReturnPlayerCheck(PlayFabAuthManager.LinkTypes linkType)
    {
        HandlePlayFabLoginTypes(linkType);
    }

    private void OnPlatFormCheck(PlayFabAuthManager.LinkTypes linkType)
    {
        HandlePlayFabLoginTypes(linkType);
    }

    private void HandlePlayFabLoginTypes(PlayFabAuthManager.LinkTypes linkType)
    {
        Debug.Log(linkType);
        var _linkType = linkType;
        
        if (linkType == PlayFabAuthManager.LinkTypes.Custom)
        {
            _linkType = PlayFabAuthManager.LinkTypes.PlayFab;
        }

        if (_linkType == PlayFabAuthManager.LinkTypes.PlayFab)
        {
            Debug.Log("Send to Login Screen.");
            StartCoroutine(ShowLoginScreen());
        }
        else
        {
            PlayFabAuthManager.LoginByLinkType(linkType);
        }

    }

    IEnumerator ShowLoginScreen()
    {
        yield return new WaitForSeconds(1f);
        PlayFabDialogManager.SendEvent("Login");
    }

}
