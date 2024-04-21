using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    public void PauseMethod()
    {
        Time.timeScale = 0f;
    }
    public void UnPauseMethod()
    {
        Time.timeScale = 1f;
    }
}
