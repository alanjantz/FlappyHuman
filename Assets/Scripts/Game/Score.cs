using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Score
{
    public const string HIGHSCORE = "highscore";

    public static int GetHighScore() => PlayerPrefs.GetInt(HIGHSCORE);

    public static bool TrySetNewHighScore(int newScore)
    {
        if (newScore > GetHighScore())
        {
            PlayerPrefs.SetInt(HIGHSCORE, newScore);
            PlayerPrefs.Save();

            return true;
        }
        else
            return false;
    }

    public static void ResetHighScore()
    {
        PlayerPrefs.SetInt(HIGHSCORE, 0);
        PlayerPrefs.Save();
    }
}
