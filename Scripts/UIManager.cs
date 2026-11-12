//結果画面を操作

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("結果画面")]
    public GameObject resultPanel;
    public TMP_Text resultText;

    private ExplorationBGMController bgmController;


    private void Awake()
    {
        bgmController = FindAnyObjectByType<ExplorationBGMController>();
    }

    //結果画面を表示
    public void ShowResults(
        int friendCount,
        int eventCount,
        int successEventCount,
        int failureEventCount,
        string playTime)
    {
        if(resultPanel != null) resultPanel.SetActive(true);
        if(bgmController != null) bgmController.PlayResultBGM();
        if(resultText != null)
        {
            resultText.text =
                "探索結果\n\n" +
                "仲間のペンギン：" + friendCount + "羽\n" +
                "発生したイベント：" + eventCount + "回\n" +
                "成功したイベント：" + successEventCount + "回\n" +
                "失敗したイベント：" + failureEventCount + "回\n" +
                "探索時間：" + playTime;
        }
    }

    //タイトル画面へ戻る
    public void ReturnToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }

    //現在のセーブデータを保存
    public void SaveCurrentData()
    {
        //セーブスロットの確認
        if(GameData.SaveSlot < SaveSystem.MinSlot || GameData.SaveSlot > SaveSystem.MaxSlot)
        {
            Debug.LogError("無効なセーブスロットです");
            return;
        }

        //現在のセーブデータを読み込む
        SaveData data = SaveSystem.Load(GameData.SaveSlot);

        if(data == null)
        {
            Debug.LogError("セーブデータを読み込めませんでした");
            return;
        }

        //管理オブジェクトを取得
        GameManager gameManager = GameManager.Instance;

        GameTimer gameTimer = gameManager != null
            ? gameManager.GetComponent<GameTimer>()
            : null;

        //プレイ時間を取得
        if(gameTimer != null)
        {
            GameData.PlayTime = gameTimer.PlayTime;
            GameData.CurrentExplorationTime = gameTimer.CurrentExplorationTime;  
        }

        //基本情報を設定
        data.hasData = true;
        data.saveName = GameData.SaveName;
        data.STR = GameData.STR;
        data.DEX = GameData.DEX;
        data.INT = GameData.INT;

        //プレイ時間を保存
        data.playTime = GameData.PlayTime;
        data.currentExplorationTime = GameData.CurrentExplorationTime;

        //イベント回数を保存
        if(GameManager.Instance != null)
        {
            data.eventCount = gameManager.EventCount;
            data.successEventCount = gameManager.SuccessEventCount;
            data.failureEventCount = gameManager.FailureEventCount;
        }

        //プレイヤーの位置と向きを保存
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if(player != null)
        {
            Vector3 position = player.transform.position;
            Vector3 rotation = player.transform.eulerAngles;

            data.playerPosX = position.x;
            data.playerPosY = position.y;
            data.playerPosZ = position.z;

            data.playerRotX = rotation.x;
            data.playerRotY = rotation.y;
            data.playerRotZ = rotation.z;
        }
        else
        {
            Debug.LogWarning("プレイヤーが見つからないため、保存済みの位置を維持します");
        }

        //ペンギンと使用済みオーブの状態を保存
        if(gameManager != null)
        {
            gameManager.SavePenguins(data);
            gameManager.SaveUsedOrbs(data);
        }
        else Debug.LogWarning("GameManagerが見つからないため、ペンギンとオーブの状態は更新されません");

        //ファイルに保存
        if(SaveSystem.Save(GameData.SaveSlot, data))
        {
            Debug.Log("セーブデータを保存しました！");

            //セーブに成功したら、現在の位置を復帰地点に設定
            if(player != null)
            {
                PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

                if(playerMovement != null)
                {
                    playerMovement.SetRespawnPosition(
                        player.transform.position,
                        player.transform.rotation
                    );
                }
                else Debug.LogWarning("PlayerMovementが見つかりません");
            }
        }
        else Debug.LogError("セーブデータの保存に失敗しました");
    }

    //探索をやり直す
    public void RetryExploration()
    {
        GameManager gameManager = GameManager.Instance;

        if(gameManager != null) gameManager.ResetExplorationData();
        else Debug.LogWarning("GameManagerが見つかりません");

        //今回の探索時間だけをリセット
        GameTimer gameTimer = gameManager != null
            ? gameManager.GetComponent<GameTimer>()
            : null;

        if(gameTimer != null) gameTimer.ResetExplorationTime();

        //次の探索でも0秒から始める
        GameData.CurrentExplorationTime = 0f;

        //累積プレイ時間は維持したまま、探索フィールドを再読み込み
        SceneManager.LoadScene("SampleScene_StageDesign");
    }

}