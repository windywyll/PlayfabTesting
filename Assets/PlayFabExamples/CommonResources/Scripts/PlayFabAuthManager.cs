using System;
using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;

namespace PlayFab
{

    public class PlayFabAuthManager : MonoBehaviour
    {
        public enum LinkTypes
        {
            PlayFab = 0,
            Facebook = 1,
            Google = 2,
            Steam = 3,
            Kongregate = 4,
            Custom = 5,
            Ios,
            Android,
            None = -1
        }

        public string TitleId;
        public WebRequestType PlayFabRequestType = WebRequestType.HttpWebRequest;
        public bool TestMode = true; //when test mode is true, it will spoof the custom id everytime it starts up.
        public bool ShowDebug = true;

        public static UserAccountInfo AccountInfo;
        public static string PlayFabId = string.Empty;
        private static string _CustomGuid = string.Empty;
        private static LinkTypes _linkType = LinkTypes.None;
        private static bool _isRegistered;
        public static PlayFabAuthManager Instance { get; private set; }

        #region Events

        public delegate void PlatFormCheckCompleteHandler(LinkTypes linkType);

        public static event PlatFormCheckCompleteHandler OnPlatformCheckComplete;

        public delegate void PlayFabReturnPlayerCheckCompleteHandler(LinkTypes linkType);

        public static event PlayFabReturnPlayerCheckCompleteHandler OnReturnPlayerCheckComplete;

        public delegate void PlayFabAuthenticationCompleteHandler(LinkTypes linkType, LoginResult result);

        public static event PlayFabAuthenticationCompleteHandler OnPlayFabAuthComplete;

        public delegate void PlayFabAuthenticationErrorHandler(LinkTypes linkType, PlayFabError error);

        public static event PlayFabAuthenticationErrorHandler OnPlayFabAuthError;

        #endregion

        private void Awake()
        {
            if (TitleId == null || TitleId.Equals(string.Empty))
            {
                Debug.LogError("To use playfab, you must populate your TitleId on the PlayFabAuthManager GameObject.");
                return;
            }

            PlayFabSettings.TitleId = TitleId;

            //Singleton behaviour
            Instance = this;
            //If test mode, then create a mini guid that will append to all player prefs.
            _CustomGuid = TestMode ? Guid.NewGuid().ToString().Substring(0, 7) : string.Empty;
        }

        private void Start()
        {
            //Check to see if the player has been registered before.
            _isRegistered = PlayerPrefs.HasKey(string.Format("{0}_PlayFabIsRegistered", _CustomGuid));

            if (!_isRegistered)
            {
                CheckPlatform();
            }
            else
            {
                //Okay, check for a stored login type.
                _linkType = !PlayerPrefs.HasKey(string.Format("{0}_PlayFabLinkType", _CustomGuid))
                    ? LinkTypes.None
                    : (LinkTypes) PlayerPrefs.GetInt(string.Format("{0}_PlayFabLinkType", _CustomGuid));
                if (OnReturnPlayerCheckComplete != null)
                {
                    OnReturnPlayerCheckComplete(_linkType);
                }
            }
        }

        public static void LoginByLinkType()
        {
            //Check if we are previously registered. (note: we assigned this variable in awake)
            if (_isRegistered)
            {
                //Okay, check for a stored login type.
                _linkType = !PlayerPrefs.HasKey(string.Format("{0}_PlayFabLinkType", _CustomGuid))
                    ? LinkTypes.None
                    : (LinkTypes) PlayerPrefs.GetInt(string.Format("{0}_PlayFabLinkType", _CustomGuid));

                Instance.LoginToPlayFab(_linkType);
            }
        }

        public static void LoginByLinkType(LinkTypes linkTypes)
        {
            Instance.LoginToPlayFab(_linkType);
        }

        public static string GetCustomGuid()
        {
            return _CustomGuid;
        }

        private void LoginToPlayFab(LinkTypes linkType)
        {
            switch (linkType)
            {
                case LinkTypes.PlayFab:
                    if (PlayerPrefs.HasKey(string.Format("{0}_PlayFabUsername", _CustomGuid)) &&
                        PlayerPrefs.HasKey(string.Format("{0}_PlayFabPassword", _CustomGuid)))
                    {
                        PlayFabClientAPI.LoginWithPlayFab(new LoginWithPlayFabRequest()
                        {
                            TitleId = PlayFabSettings.TitleId,
                            Username = PlayerPrefs.GetString(string.Format("{0}_PlayFabUsername", _CustomGuid)),
                            Password = PlayerPrefs.GetString(string.Format("{0}_PlayFabPassword", _CustomGuid))
                        }, (result) =>
                        {
                            HandleLoginResult(result, linkType);
                        }, HandleLoginError);

                    }
                    else
                    {
                        if (ShowDebug)
                        {
                            Debug.Log("Stored username or password not found.");
                        }
                    }
                    break;
                case LinkTypes.Android:
#if UNITY_ANDROID && !UNITY_EDITOR
                    AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
                    AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
                    AndroidJavaClass secure = new AndroidJavaClass("android.provider.Settings$Secure");
                    var deviceId = secure.CallStatic<string>("getString", contentResolver, "android_id");
                    PlayFabClientAPI.LoginWithAndroidDeviceID(new PlayFab.ClientModels.LoginWithAndroidDeviceIDRequest()
                    {
                        AndroidDeviceId = deviceId,
                        AndroidDevice = SystemInfo.deviceModel,
                        OS = SystemInfo.operatingSystem,
                        TitleId = PlayFabSettings.TitleId,
                        CreateAccount=true
                    }, (result)=>{
                        HandleLoginResult(result, linkType);
                    }, HandleLoginError);
#endif
                    break;
                case LinkTypes.Ios:
#if UNITY_IOS && !UNITY_EDITOR
    //TODO: get device id from ios (research how to)
                    var deviceId = SystemInfo.deviceUniqueIdentifier
                    PlayFabClientAPI.LoginWithIOSDeviceID(new PlayFab.ClientModels.LoginWithIOSDeviceIDRequest() { 
                        DeviceId = deviceId,
                        DeviceModel = SystemInfo.deviceModel,
                        OS = SystemInfo.operatingSystem,
                        TitleId = PlayFabSettings.TitleId,
                        CreateAccount=true
                    },  (result)=>{
                        HandleLoginResult(result, linkType);
              
                    }, HandleLoginError);
#endif
                    break;
                case LinkTypes.Facebook:
                    if (!PlayerPrefs.HasKey(string.Format("{0}_PlayFabFacebookAccessToken", _CustomGuid)))
                    {
                        if (ShowDebug)
                        {
                            Debug.LogError("Missing FacebookAccess Token in prefs.");
                        }
                        break;
                    }
                    PlayFabClientAPI.LoginWithFacebook(new LoginWithFacebookRequest()
                    {
                        TitleId = PlayFabSettings.TitleId,
                        AccessToken =
                            PlayerPrefs.GetString(string.Format("{0}_PlayFabFacebookAccessToken", _CustomGuid)),
                        CreateAccount = true
                    }, (result) =>
                    {
                        HandleLoginResult(result, linkType);
                    }, HandleLoginError);
                    break;
                case LinkTypes.Google:
                    if (!PlayerPrefs.HasKey(string.Format("{0}_PlayFabGooglePublisherId", _CustomGuid)) ||
                        !PlayerPrefs.HasKey(string.Format("{0}_PlayFabGoogleAccessToken", _CustomGuid)))
                    {
                        if (ShowDebug)
                        {
                            Debug.LogError("Missing PublisherId or AccessToken in prefs.");
                        }
                        break;
                    }
                    PlayFabClientAPI.LoginWithGoogleAccount(new LoginWithGoogleAccountRequest()
                    {
                        TitleId = PlayFabSettings.TitleId,
                        PublisherId = PlayerPrefs.GetString(string.Format("{0}_PlayFabGooglePublisherId", _CustomGuid)),
                        AccessToken = PlayerPrefs.GetString(string.Format("{0}_PlayFabGoogleAccessToken", _CustomGuid)),
                        CreateAccount = true
                    }, (result) =>
                    {
                        HandleLoginResult(result, linkType);
                    }, HandleLoginError);
                    break;
                case LinkTypes.Steam:
                    if (!PlayerPrefs.HasKey(string.Format("{0}_PlayFabSteamTicket", _CustomGuid)))
                    {
                        if (ShowDebug)
                        {
                            Debug.LogError("Missing Steam Ticket in prefs.");
                        }
                        break;
                    }
                    PlayFabClientAPI.LoginWithSteam(new LoginWithSteamRequest()
                    {
                        TitleId = PlayFabSettings.TitleId,
                        SteamTicket = PlayerPrefs.GetString(string.Format("{0}_PlayFabSteamTicket", _CustomGuid)),
                        CreateAccount = true
                    }, (result) =>
                    {
                        HandleLoginResult(result, linkType);
                    }, HandleLoginError);
                    break;
                case LinkTypes.Kongregate:
                    if (!PlayerPrefs.HasKey(string.Format("{0}_PlayFabKongregateId", _CustomGuid)) ||
                        !PlayerPrefs.HasKey(string.Format("{0}_PlayFabKongregateAuthTicket", _CustomGuid)))
                    {
                        if (ShowDebug)
                        {
                            Debug.LogError("Missing KongregateId or Auth Ticket in prefs.");
                        }
                        break;
                    }
                    PlayFabClientAPI.LoginWithKongregate(new LoginWithKongregateRequest()
                    {
                        TitleId = PlayFabSettings.TitleId,
                        KongregateId = PlayerPrefs.GetString(string.Format("{0}_PlayFabKongregateId", _CustomGuid)),
                        AuthTicket =
                            PlayerPrefs.GetString(string.Format("{0}_PlayFabKongregateAuthTicket", _CustomGuid)),
                        CreateAccount = true
                    }, (result) =>
                    {
                        HandleLoginResult(result, linkType);
                    }, HandleLoginError);
                    break;
                case LinkTypes.Custom:

                    var customId = SystemInfo.deviceUniqueIdentifier;
                    if (TestMode)
                    {
                        customId = string.Format("{0}{1}", customId, _CustomGuid);
                    }

                    PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
                    {
                        TitleId = PlayFabSettings.TitleId,
                        CustomId = customId,
                        CreateAccount = true
                    }, (result) =>
                    {
                        HandleLoginResult(result, linkType);
                    }, HandleLoginError);
                    break;
            }
        }

        private void CheckPlatform()
        {

            #region platform detection(s)

            #region Standalone (PC/Mac/Linux)

#if UNITY_STANDALONE || UNITY_WEBGL || UNITY_EDITOR
            //Using a little bit of reflection to see if we are on Steam or Kongregate.
            Type steamType = System.Reflection.Assembly.GetExecutingAssembly().GetType("SteamManager", false);
            Type kongregateType = System.Reflection.Assembly.GetExecutingAssembly().GetType("KongregateManager", false);

            if (steamType != null)
            {
                _linkType = LinkTypes.Steam;
            }
            else if (kongregateType != null)
            {
                _linkType = LinkTypes.Kongregate;
            }
            else
            {
                _linkType = LinkTypes.Custom;
            }
#endif

            #endregion

            #region iOS

#if UNITY_IOS && !UNITY_EDITOR
    //This will link / login via Ios Device Id until another link type has been established
                    _linkType = LinkTypes.Ios;
#endif

            #endregion

            #region Android

#if UNITY_ANDROID && !UNITY_EDITOR
    //This will link / login via Android Device Id until another link type has been established
                    _linkType = LinkTypes.Android;
#endif

            #endregion

            #region Xbox (any)

#if UNITY_XBOX360 || UNITY_XBOXONE && !UNITY_EDITOR
    //TODO: Will need to be modified for XBOX Live login and Integration when available.
    //This will link / login via CustomID Until another Link Type has been established
                    _linkType = LinkTypes.Custom;
#endif

            #endregion

            #region PlayStation (any)

#if UNITY_PS3 || UNITY_PS4 && !UNITY_EDITOR
    //TODO: I think there is a PS network login that we can handle.
    //This will link / login via CustomID Until another Link Type has been established
                    _linkType = LinkTypes.Custom;

#endif

            #endregion

            #region Other (WP8, RT, WSA, Tizen, Blackberry)

#if UNITY_WP8 || UNITY_BLACKBERRY || UNITY_WINRT || UNITY_WSA || UNITY_TIZEN && !UNITY_EDITOR
    //This will link / login via CustomID Until another Link Type has been established
                    _linkType = LinkTypes.Custom;

#endif

            #endregion

            #endregion

            if (OnPlatformCheckComplete != null)
            {
                OnPlatformCheckComplete(_linkType);
            }

        }

        private void HandleLoginResult(LoginResult result, LinkTypes linkType)
        {
            PlayFabId = result.PlayFabId;
            
            //Remember what type of authentication we did last.
            PlayerPrefs.SetInt(string.Format("{0}_PlayFabLinkType", _CustomGuid), (int)_linkType);

            //Get player Account info and store it.
            PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), (accountInfoResult) =>
            {
                Debug.Log("Account Info Received Succesfully!");
                AccountInfo = accountInfoResult.AccountInfo;

                //We make this call here to ensure that Account Info is available after login.
                if (OnPlayFabAuthComplete != null)
                {
                    OnPlayFabAuthComplete(linkType, result);
                }

            }, (accountInfoError) =>
            {
                Debug.Log(accountInfoError.ErrorMessage);
                //Note, this should never really happen. Unless, they lost connection between login & making the GetAccountInfo call.

                //Set some default info
                AccountInfo = new UserAccountInfo();
                AccountInfo.PlayFabId = result.PlayFabId;
                AccountInfo.TitleInfo = new UserTitleInfo();

                //Continue the login, even know we did not get the Account Info.
                if (OnPlayFabAuthComplete != null)
                {
                    OnPlayFabAuthComplete(linkType, result);
                }
            });

        }

        private void HandleLoginError(PlayFabError error)
        {
            if (ShowDebug)
            {
                Debug.Log(string.Format("Login Error: {0}", error.ErrorMessage));
            }
            if (OnPlayFabAuthError != null)
            {
                OnPlayFabAuthError(_linkType, error);
            }
        }

    }
}