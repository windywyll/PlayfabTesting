using UnityEngine;
using System.Collections;

public class ELORanking {

    //Return the new ELO Score of the owner of the player
    //matchResult is 0 if defeat of player, 0.5 if draw, 1 if victory

    public static int CalculateELORanks(int playerRanking, int opponentRanks, float matchResult)
    {
        int newScore = 0;
        int coefK = 20;
        float pD;
        pD = 1 / (1 + Mathf.Pow(10, -(playerRanking - opponentRanks) / 400));
        newScore = (int) (playerRanking + coefK * (matchResult - pD));

        return newScore;
    }
}
