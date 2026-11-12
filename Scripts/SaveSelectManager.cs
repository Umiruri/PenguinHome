//空のセーブスロットをクリックしたときに、セーブ名入力パネルを表示する

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SaveSelectManager : MonoBehaviour
{
    //セーブスロットの範囲
    private const int MinSlot = 1;
    private const int MaxSlot = 3;

    [Header("セーブ名入力")]
    public GameObject saveNamePanel;
    public TMP_InputField saveNameInput;

    [Header("削除確認")]
    public GameObject deleteConfirmPanel;
    public TMP_Text deleteConfirmText;

    [Header("上書き確認")]
    public GameObject overwriteConfirmPanel;
    public TMP_Text overwriteConfirmText;

    private int selectedSlot = 0;
    private int deleteTargetSlot = 0;

    private void Start()
    {
        //パネルを初期状態で非表示に
        SetPanelActive(saveNamePanel, false);
        SetPanelActive(deleteConfirmPanel, false);
        SetPanelActive(overwriteConfirmPanel, false);
    }

    //スロット番号が有効か確認
    private bool IsValidSlot(int slotNumber)
    {
        return slotNumber >= MinSlot && slotNumber <= MaxSlot;
    }

    //パネルの表示・非表示
    private void SetPanelActive(GameObject panel, bool active)
    {
        if(panel == null)
        {
            Debug.LogWarning("パネルがInspectorに設定されていません");
            return;
        }

        panel.SetActive(active);
    }

    //空のセーブスロットを選択
    public void SelectEmptySlot(int slotNumber)
    {
        if(!GameData.IsNewGame)
        {
            Debug.Log("新規ゲーム以外では空のスロットを選択できません");
            return;
        }

        if(!IsValidSlot(slotNumber))
        {
            Debug.LogWarning("無効なセーブスロット番号です");
            return;
        }

        if(saveNamePanel == null || saveNameInput == null)
        {
            Debug.LogError("セーブ名入力欄が設定されていません");
            return;
        }

        //本当に空のスロットか確認
        SaveData data = SaveSystem.Load(slotNumber);

        if(data == null)
        {
            Debug.LogError("セーブデータを確認できません");
            return;
        }

        if (data.hasData)
        {
            Debug.LogWarning("このスロットには既にデータがあります");
            return;
        }
        
        selectedSlot = slotNumber;
        saveNameInput.text = "";

        saveNamePanel.SetActive(true);
        saveNameInput.ActivateInputField();

        Debug.Log("選択したスロット : " + selectedSlot);
    }

    //セーブ名入力をキャンセル（ボタン）
    public void CancelInput()
    {
        SetPanelActive(saveNamePanel, false);
        selectedSlot = 0;
    }

    //セーブ名入力を決定（ボタン）
    public void ConfirmInput()
    {
        if (!IsValidSlot(selectedSlot))
        {
            Debug.LogWarning("保存先のスロットが選択されていません");
            return;
        }

        if(saveNamePanel == null || saveNameInput == null)
        {
            Debug.LogError("セーブ名入力欄が設定されていません");
            return;
        }

        string saveName = saveNameInput.text.Trim();

        //セーブ名入力欄が空欄のままでは保存できない
        if(string.IsNullOrEmpty(saveName))
        {
            Debug.Log("セーブ名を入力してください");
            saveNameInput.ActivateInputField();
            return;
        }


        //新しいセーブデータを作成
        SaveData data = new SaveData();

        data.hasData = true;
        data.saveName = saveName;
        
        data.STR = GameData.STR;
        data.DEX = GameData.DEX;
        data.INT = GameData.INT;

        //総プレイ時間をリセット
        data.playTime = 0f;

        //今回の探索時間はリセット
        data.currentExplorationTime = 0f;

        //保存に失敗した場合は次のシーンに進まないように
        if(!SaveSystem.Save(selectedSlot, data))
        {
            Debug.LogError("セーブデータの作成に失敗しました");
            return;
        }

        Debug.Log("セーブデータを作成しました:" + saveName);

        //ゲーム中に使用する情報を更新
        GameData.SaveSlot = selectedSlot;
        GameData.SaveName = saveName;
        GameData.PlayTime = data.playTime;
        GameData.CurrentExplorationTime = 0f;

        //読み込み情報をリセット
        GameData.LoadedSaveData = null;
        GameData.HasSavedPlayerPosition = false;

        //イベント回数をリセット
        GameData.EventCount = 0;
        GameData.SuccessEventCount = 0;
        GameData.FailureEventCount = 0;

        //入力パネルを閉じる
        SetPanelActive(saveNamePanel, false);

        //スロット選択画面からステータス画面へ
        SceneManager.LoadScene("StatusScene");
    }

    //削除対象のスロットを選択
    public void SelectDeleteSlot(int slotNumber)
    {
        if(!IsValidSlot(slotNumber))
        {
            Debug.LogWarning("無効なセーブスロット番号です");
            return;
        }

        SaveData data = SaveSystem.Load(slotNumber);

        if(data==null || !data.hasData)
        {
            Debug.LogWarning("削除できるセーブデータがありません");
            return;
        }

        if(deleteConfirmPanel == null || deleteConfirmText == null)
        {
            Debug.LogError("削除確認画面が設定されていません");
            return;
        }

        deleteTargetSlot = slotNumber;

        deleteConfirmText.text = "「" + data.saveName + "」を削除しますか？";
        SetPanelActive(deleteConfirmPanel, true);
    }

    //セーブデータを削除
    public void ConfirmDelete()
    {
        if(!IsValidSlot(deleteTargetSlot))
        {
            Debug.LogWarning("削除対象のスロットが選択されていません");
            return;
        }

        if (!SaveSystem.Delete(deleteTargetSlot))
        {
            Debug.LogError("セーブデータの削除に失敗しました");
            return;
        }

        Debug.Log("セーブデータを削除しました：" + deleteTargetSlot); //削除対象の番号をリセット

        deleteTargetSlot = 0;

        //セーブスロット画面を再読み込み
        SceneManager.LoadScene("SaveSelectScene"); //セーブデータ選択画面に戻る
    }

    //削除をキャンセル
    public void CancelDelete()
    {
        deleteTargetSlot = 0;
        SetPanelActive(deleteConfirmPanel, false);
    }

    //保存済みのスロットを選択
    public void SelectSavedSlot(int slotNumber)
    {
        if (!IsValidSlot(slotNumber))
        {
            Debug.LogWarning("無効なセーブスロット番号です");
            return;
        }

        SaveData data = SaveSystem.Load(slotNumber);

        if(data == null || !data.hasData){
            Debug.LogWarning("読み込めるセーブデータがありません");
            return;
        }

        //はじめからの場合は上書き確認を表示
        if(GameData.IsNewGame)
        {
            if(overwriteConfirmPanel == null || overwriteConfirmText == null)
            {
                Debug.LogError("上書き確認画面が設定されていません");
                return;
            }

            selectedSlot = slotNumber;

            overwriteConfirmText.text = "「" + data.saveName + "」に上書きしますか？";

            SetPanelActive(overwriteConfirmPanel, true);
            return;
        }

        //つづきからの場合は保存データを読み込む
        GameData.STR = data.STR;
        GameData.DEX = data.DEX;
        GameData.INT = data.INT;

        //選択中のセーブ情報を復元する
        GameData.SaveSlot = slotNumber;
        GameData.SaveName = data.saveName;

        //プレイ時間を復元
        GameData.PlayTime = Mathf.Max(0f, data.playTime);
        GameData.CurrentExplorationTime = Mathf.Max(0f, data.currentExplorationTime);

        //イベント回数を復元
        GameData.EventCount = Mathf.Max(0, data.eventCount);
        GameData.SuccessEventCount = Mathf.Max(0, data.successEventCount);
        GameData.FailureEventCount = Mathf.Max(0, data.failureEventCount);

        //プレイヤーの位置を復元
        GameData.PlayerPosX = data.playerPosX;
        GameData.PlayerPosY = data.playerPosY;
        GameData.PlayerPosZ = data.playerPosZ;

        //プレイヤーの向きを復元
        GameData.PlayerRotX = data.playerRotX;
        GameData.PlayerRotY = data.playerRotY;
        GameData.PlayerRotZ = data.playerRotZ;

        //保存した位置があることを示す
        GameData.HasSavedPlayerPosition = true;

        //読み込んだセーブデータ全体を保持
        GameData.LoadedSaveData = data;

        Debug.Log("セーブデータを読み込みました : " + data.saveName);

        //探索シーンへ直接移動
        SceneManager.LoadScene("SampleScene_StageDesign");
    }

    //上書き→セーブ名入力へ
    public void ConfirmOverwrite()
    {
        if (!IsValidSlot(selectedSlot))
        {
            Debug.LogWarning("上書き対象のスロットが選択されていません");
            return;
        }

        if(saveNamePanel == null || saveNameInput == null)
        {
            Debug.LogError("セーブ名入力欄が設定されていません");
            return;
        }

        SetPanelActive(overwriteConfirmPanel, false);

        saveNameInput.text = "";
        SetPanelActive(saveNamePanel, true);

        saveNameInput.ActivateInputField();
    }

    //上書きをキャンセル
    public void CancelOverwrite()
    {
        SetPanelActive(overwriteConfirmPanel, false);
        selectedSlot = 0;
    }

    //上書き確認からセーブ名入力へ
    public void OpenSaveNameInput()
    {
        ConfirmOverwrite();
    }

}