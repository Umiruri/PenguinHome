//MENUボタンを押すとメニューが開く
//各ボタンで閉じたり移動したり

using UnityEngine;
using UnityEngine.SceneManagement;
public class GameMenuManager : MonoBehaviour
{
    [Header("メニュー画面")]
    public GameObject menuPanel;

    [Header("メニューボタン")]
    public GameObject menuOpenButton;

    //メニューが開いているか
    public bool IsMenuOpen { get; private set; } = false;

    //メニューを開く
    public void OpenMenu()
    {
        if(menuPanel == null)
        {
            Debug.LogError("メニューパネルが設定されていません");
            return;
        }

        menuPanel.SetActive(true);

        if(menuOpenButton != null) menuOpenButton.SetActive(false);

        IsMenuOpen = true;
        Time.timeScale = 0f;
    }

    //メニューを閉じてゲームに戻る
    public void ResumeGame()
    {
        if(menuPanel == null)
        {
            Debug.LogError("メニューパネルが設定されていません");
            return;
        }

        menuPanel.SetActive(false);

        if(menuOpenButton != null) menuOpenButton.SetActive(true);

        IsMenuOpen = false;
        Time.timeScale = 1f;
    }

    //タイトルへ戻る
    public void ReturnToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }

    //あそびかた画面を開く
    public void OpenHowToPlay()
    {
        Time.timeScale = 0f; //探索を一時的に停止
        SceneManager.LoadScene("HowToPlayScene",LoadSceneMode.Additive);
    }

}