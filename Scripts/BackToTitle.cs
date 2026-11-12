//タイトルへ戻る

using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToTitle : MonoBehaviour
{
    public void ReturnToTitle()
    {
        //時間停止を解除
        Time.timeScale = 1f;

        //タイトル画面へ移動
        SceneManager.LoadScene("TitleScene");
    }
}