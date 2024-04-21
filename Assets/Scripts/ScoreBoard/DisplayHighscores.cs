using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayHighscores : MonoBehaviour 
{
    public TMPro.TextMeshProUGUI[] rNames;
    HighScores myScores;

    void Start()
    {
        for (int i = 0; i < rNames.Length;i ++)
        {
            rNames[i].text = i + 1 + ". Fetching...";
        }
        myScores = GetComponent<HighScores>();
        StartCoroutine("RefreshHighscores");
    }
    public void SetScoresToMenu(PlayerScore[] highscoreList)
    {
        for (int i = 0; i < rNames.Length;i ++)
        {
            rNames[i].text = i + 1 + ". ";
            if (highscoreList.Length > i)
            {
                if(highscoreList[i].username.Length >= 3)
                rNames[i].text = i + 1 + ". " + highscoreList[i].username + "\t" + highscoreList[i].score.ToString();
                else
                rNames[i].text = i + 1 + ". " + highscoreList[i].username + "   \t" + highscoreList[i].score.ToString();
            }
        }
    }
    IEnumerator RefreshHighscores()
    {
        while(true)
        {
            myScores.DownloadScores();
            yield return new WaitForSeconds(30);
        }
    }
}
