using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class GameMenuPanel : MonoBehaviour {

    public Button GameButton, LeaderboardButton;
    private bool statsObtained;
    private int myRankings;
    private List<string> wantedStats;

    void OnEnable()
    {
        LeaderboardButton.onClick.AddListener(clickOnLeaderboard);
        GameButton.onClick.AddListener(startGame);
    }

    void OnDisable()
    {
        LeaderboardButton.onClick.RemoveAllListeners();
        GameButton.onClick.RemoveAllListeners();
    }

    #region updateRankings

    public void updateLeaderboard(int newRkg)
    {
        List<StatisticUpdate> valToUpdate = new List<StatisticUpdate>();

        StatisticUpdate rkgToUpdate = new StatisticUpdate();
        rkgToUpdate.StatisticName = "GTBL_Rankings";
        rkgToUpdate.Value = newRkg;

        Debug.Log(newRkg);

        valToUpdate.Add(rkgToUpdate);

        UpdatePlayerStatisticsRequest request = new UpdatePlayerStatisticsRequest()
        {
            Statistics = valToUpdate
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, OnUpdPlayerStatsSuccess, OnUpdPlayerStatsError);
    }

    private void OnUpdPlayerStatsSuccess(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Successfully updated ranking.");
    }

    private void OnUpdPlayerStatsError(PlayFabError error)
    {
        Debug.Log(error.ErrorMessage);
    }

    #endregion

    #region ButtonLeaderboard
    public void clickOnLeaderboard()
    {
        StartCoroutine(ShowLeaderboardScreen());
    }

    IEnumerator ShowLeaderboardScreen()
    {
        yield return new WaitForSeconds(1f);
        PlayFabDialogManager.SendEvent("Leaderboard");
    }
    #endregion

    #region SimulateGame
    IEnumerator simulateGame()
    {
        do
        {
            yield return new WaitForSeconds(1f);
        } while (statsObtained == false);

        int simulateOpponent, matchResult, newRankings;
        float matchValue = 0.0f;

        System.Random r = new System.Random();
        simulateOpponent = r.Next(Mathf.Max(0, myRankings - 50), myRankings + 51);
        matchResult = r.Next(1, 21);

        if (matchResult > 8)
        {
            matchValue = 0.5f;
            if (matchResult > 11)
            {
                matchValue = 1.0f;
            }
        }

        newRankings = ELORanking.CalculateELORanks(myRankings, simulateOpponent, matchValue);

        updateLeaderboard(newRankings);
    }

    public void startGame()
    {
        statsObtained = false;

        getRankings();

        StartCoroutine(simulateGame());
    }

    private void getRankings()
    {
        wantedStats = new List<string>();
        wantedStats.Add("GTBL_Rankings");

        GetPlayerStatisticsRequest request = new GetPlayerStatisticsRequest()
        {
            StatisticNames = wantedStats
        };

        PlayFabClientAPI.GetPlayerStatistics(request, OnReqStatsSuccess, OnReqStatsError);
    }

    private void OnReqStatsSuccess(GetPlayerStatisticsResult result)
    {
        foreach(StatisticValue val in result.Statistics)
        {
            if (val.StatisticName == "GTBL_Rankings")
                myRankings = val.Value;
        }

        statsObtained = true;
    }

    private void OnReqStatsError(PlayFabError error)
    {
        Debug.Log(error.ErrorMessage);
    }

    #endregion
}
