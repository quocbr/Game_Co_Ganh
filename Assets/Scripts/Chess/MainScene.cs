using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScene : MonoBehaviour
{
    public void ChoiVsMay()
    {
        SceneManager.LoadScene("Game_Co");
        PlayerPrefs.SetInt("ChoiVsMay",1);
    }
    
    public void ChoiVsNguoi()
    {
        SceneManager.LoadScene("Game_Co");
        PlayerPrefs.SetInt("ChoiVsMay",0);
    }

    public void OutGame()
    {
        Application.Quit();
    }
}
