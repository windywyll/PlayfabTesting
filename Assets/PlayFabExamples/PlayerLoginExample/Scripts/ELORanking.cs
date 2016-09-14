using UnityEngine;
using System.Collections;

public class ELORanking : MonoBehaviour {

    public static int CalculateELORanks(int myRanks, int opponentRanks, float matchResult)
    {
        int newScore = 0;
        int coefK = 20;
        float pD;
        pD = 1 / (1 + Mathf.Pow(10, -(myRanks - opponentRanks) / 400));
        newScore = (int) (myRanks + coefK * (matchResult - pD));

        return newScore;
    }
}
