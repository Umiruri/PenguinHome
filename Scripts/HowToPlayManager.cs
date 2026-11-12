//あそびかた画面

using UnityEngine;
using UnityEngine.SceneManagement;

public class HowToPlayManager : MonoBehaviour
{

    //タイトル画面から開く
    public void OpenFromTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("HowToPlayScene", LoadSceneMode.Additive);
    }

    //元の画面に戻る
    public void ReturnToPreviousScene()
    {
        SceneManager.UnloadSceneAsync("HowToPlayScene");
        Time.timeScale = 1f;
    }

}
