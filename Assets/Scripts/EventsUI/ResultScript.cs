using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResultScript : MonoBehaviour
{
    public TMP_Text resultText;
    public GameObject targetObject;
    public AudioSource loseSound;
    public AudioSource winSound;
    public TMP_InputField _usernameInputField;
    public GameManager gameManager;


    public void StopGame()
    {
        Time.timeScale = 0f;
    }
    public void SetObjectActive(bool isActive)
    {
        if (targetObject != null)
            targetObject.SetActive(isActive);
    }

    public void Lose()
    {
        loseSound.Play();
        resultText.text = "You lose.";
        SetObjectActive(true);
        Invoke("StopGame", 2f);
    }

    public void Won()
    {
        winSound.Play();
        resultText.text = "You won!";
        SetObjectActive(true);
        HighScores.UploadScore(_usernameInputField.text, gameManager.score);
        Invoke("StopGame", 2f);
    }
}
