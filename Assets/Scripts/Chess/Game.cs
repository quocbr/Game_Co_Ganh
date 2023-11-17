using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    private static Game instance;
    public static Game Instance => instance;
    
    public Text TySo;
    public Text Luot;
    public Text Thoigian;
    private double elapsedTime = 0f;
    public GameObject PanelPause;
    public GameObject PanelWin;
    public Text whoWin;

    private bool luot = true;

    void Start()
    {
        instance = this;
        Luot.text="Lượt của: quân Xanh";
        luot = true;
        StartCoroutine(UpdateTimer());
    }

    IEnumerator UpdateTimer()
    {
        while (true)
        {
            elapsedTime += 1f;
            
            // Format thời gian đã chạy thành giờ:phút:giây
            string formattedTime = FormatTime(elapsedTime);
    
            // Gán thời gian đã chạy vào Text UI element
            Thoigian.text = "Thời gian: " + formattedTime;

            yield return new WaitForSeconds(1f);
        }
    }

    string FormatTime(double timeInSeconds)
    {
        int minutes = (int)((timeInSeconds % 3600) / 60);
        int seconds = (int)(timeInSeconds % 60);

        return string.Format("{0:D2}:{1:D2}", minutes, seconds);
    }

    public void EndGame(bool ck)
    {
        string s = TySo.text;
        string[] a = s.Split("-");
        int trang = int.Parse(a[0]);
        int den = int.Parse(a[1]);
        if (!ck)
        {
            whoWin.text = "Trắng win";
            trang++;
        }
        else {
            whoWin.text = "Đen win";
            den++;
        }
        TySo.text = trang + "-" + den;
        SoundController.Instance.Win();
        PanelWin.SetActive(true);
        Time.timeScale = 0;
        BoardManager.Instance.SetPause(0);
    }
    
    public void PauseGame()
    {
        Time.timeScale = 0;
        BoardManager.Instance.SetPause(0);
        PanelPause.SetActive(true);
        //isGamePaused = true;
        // Thêm các tác vụ khác khi tạm dừng trò chơi (nếu cần)
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        BoardManager.Instance.SetPause(1);
        PanelPause.SetActive(false);
        //isGamePaused = false;
        // Thêm các tác vụ khác khi trò chơi tiếp tục (nếu cần)
    }

    public void PlayBack()
    {
        BoardManager.Instance.PlayBack();
        elapsedTime = 0f;
        SetLuot("quan xanh");
    }

    public void DauHang()
    {
        PlayBack();
        EndGame(!luot);
        luot = true;
    }

    public void OutMenu()
    {
        SceneManager.LoadScene("Main");
    }

    public void SetLuot(string text)
    {
        Luot.text = "Lượt của: " + text;
        luot = !luot;
    }

    public void HidePanel()
    {
        PanelWin.SetActive(false);
        ResumeGame();
    }
}
