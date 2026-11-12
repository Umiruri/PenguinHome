//タイトルシーン切り替え用
//あそびかた画面はHowToPlayManager.cs

using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    //はじめから
    public void StartNewGame()
    {
        GameData.IsNewGame = true;
        SceneManager.LoadScene("SaveSelectScene");
    }

    //つづきから
    public void ContinueGame()
    {
        GameData.IsNewGame = false;
        SceneManager.LoadScene("SaveSelectScene");
    }

    //ゲーム終了
    public void QuitGame()
    {
        Debug.Log("ゲームを終了します");
        Application.Quit();
    }
    
}