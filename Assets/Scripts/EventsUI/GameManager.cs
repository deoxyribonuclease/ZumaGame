using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text resultText;
    public int score = 0;

    public void BallDestroyed(int i)
    {
        score += (i >= 3) ? (100 + 20 * i) : 100;
        UpdateScoreText(); 
    }
    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + score.ToString();
        resultText.text = "Score: " + score.ToString();
    }
    public void ScoreReset()
    {
        score = 0;
        scoreText.text = "Score: " + score.ToString();
        resultText.text = "Score: " + score.ToString();
    }
}
