using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;

public class LeaderboardScript : MonoBehaviour {

    private static Dictionary<string, int> userStatistics;
    public Button backButton;

    void OnEnable()
    {
        backButton.onClick.AddListener(backToMenu);
        GetUserStatistics();
    }

    void OnDisable()
    {
        backButton.onClick.RemoveAllListeners();
    }

    public void backToMenu()
    {
        StartCoroutine(ShowGameMenu());
    }

    IEnumerator ShowGameMenu()
    {
        yield return new WaitForSeconds(1f);
        PlayFabDialogManager.SendEvent("GameMenu");
    }

    /***************************************
     * retrieve Score for logged in player *
     ***************************************/

    #region User Statistics
    public static void GetUserStatistics()
    {
        //Server Access
        //GetUserStatisticsRequest request = new GetUserStatisticsRequest();
        //PlayFabClientAPI.GetUserStatistics(request, OnGetUserStatisticsSuccess, OnGetUserStatisticsError);

        //Client API Access
        GetLeaderboardAroundPlayerRequest request = new GetLeaderboardAroundPlayerRequest()
        {
            StatisticName = "GTBL_Rankings"
        };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnGetUserStatisticsSuccess, OnGetUserStatisticsError);
    }

    /*private static void OnGetUserStatisticsSuccess(GetUserStatisticsResult result)
    {
        //Server Access
        PF_Bridge.raiseCallbackSuccess("", PlayFabAPIMethods.GetUserStatistics, MessageDisplayStyle.none);
        foreach (var each in result.UserStatistics)
        {
            userStatistics[each.Key] = each.Value;
            Debug.Log(each.Key + " : " + each.Value);
        }
    }*/

    private static void OnGetUserStatisticsSuccess(GetLeaderboardAroundPlayerResult result)
    { 
        //Client API Access
        foreach(var res in result.Leaderboard)
        {
            Debug.Log((res.Position + 1) + " - " + res.DisplayName + " : " + res.StatValue);
        }
    }

    private static void OnGetUserStatisticsError(PlayFabError error)
    {
        Debug.Log(error.ErrorMessage);
    }
    #endregion
}
