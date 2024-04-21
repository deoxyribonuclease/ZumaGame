using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighScores : MonoBehaviour
{
    const string privateCode = "Bh-V5Sk_o0Ozi7eBVw5FaQDri-JpcMi0WSeDNBFbKogQ";  //Key to Upload New Info
    const string publicCode = "662403198f40bb122c88108f";   //Key to download
    const string webURL = "http://dreamlo.com/lb/"; 

    public PlayerScore[] scoreList;
    DisplayHighscores myDisplay;

    static HighScores instance; 
    void Awake()
    {
        instance = this; 
        myDisplay = GetComponent<DisplayHighscores>();
    }

 
    public static void UploadScore(string username, int score)
    {
        instance.StartCoroutine(instance.DatabaseUpload(username,score)); 
    }

    #pragma warning disable CS0618
    IEnumerator DatabaseUpload(string userame, int score)
    {
        WWW www = new WWW(webURL + privateCode + "/add/" + WWW.EscapeURL(userame) + "/" + score);
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            print("Upload Successful");
            DownloadScores();
        }
        else print("Error uploading" + www.error);
    }

    public void DownloadScores()
    {
        StartCoroutine("DatabaseDownload");
    }

    IEnumerator DatabaseDownload()
    {
        WWW www = new WWW(webURL + publicCode + "/pipe/0/12"); 
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            OrganizeInfo(www.text);
            myDisplay.SetScoresToMenu(scoreList);
        }
        else print("Error uploading" + www.error);
    }
    #pragma warning restore CS0618

    void OrganizeInfo(string rawData) 
    {
        string[] entries = rawData.Split(new char[] {'\n'}, System.StringSplitOptions.RemoveEmptyEntries);
        scoreList = new PlayerScore[entries.Length];
        for (int i = 0; i < entries.Length; i ++)
        {
            string[] entryInfo = entries[i].Split(new char[] {'|'});
            string username = entryInfo[0];
            int score = int.Parse(entryInfo[1]);
            scoreList[i] = new PlayerScore(username,score);
            print(scoreList[i].username + ": " + scoreList[i].score);
        }
    }
}

public struct PlayerScore
{
    public string username;
    public int score;

    public PlayerScore(string _username, int _score)
    {
        username = _username;
        score = _score;
    }
}