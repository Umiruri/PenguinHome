//セーブ枠の表示をまとめて管理

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [Header("スロット設定")]
    public int slotNumber = 1;
    public SaveSelectManager manager;

    [Header("表示")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI playTimeText;

    [Header("削除ボタン")]
    public GameObject deleteButton;

    private Button slotButton;

    private void Awake()
    {
        //このオブジェクトに付いているボタンを取得
        slotButton = GetComponent<Button>();

        //自分のクリック処理だけを登録する
        if(slotButton != null)
        {
            slotButton.onClick.RemoveListener(OnSlotClicked);
            slotButton.onClick.AddListener(OnSlotClicked);
        }
    }

    private void Start()
    {
        Refresh();
    }

    //セーブデータの表示を更新
    public void Refresh()
    {
        if(slotNumber<SaveSystem.MinSlot || slotNumber>SaveSystem.MaxSlot)
        {
            ShowError("無効なスロットです");
            return;
        }

        SaveData data = SaveSystem.Load(slotNumber);

        //読み込みに失敗した場合
        if(data == null)
        {
            ShowError("セーブデータを読み込めません");
            return;
        }

        //空きスロットの場合
        if(!data.hasData)
        {
            ShowEmpty();
            return;
        }

        //保存済みスロットの場合
        ShowSaved(
            data.saveName,
            data.STR,
            data.DEX,
            data.INT,
            data.playTime
        );
    }

    //スロットがクリックされたとき
    private void OnSlotClicked()
    {
        if(manager == null)
        {
            Debug.LogError("SaveSelectManagerが設定されていません");
            return;
        }

        if(slotNumber < SaveSystem.MinSlot || slotNumber > SaveSystem.MaxSlot)
        {
            Debug.LogWarning("無効なスロット番号です");
            return;
        }

        //最新のセーブデータを確認
        SaveData data = SaveSystem.Load(slotNumber);

        if(data == null)
        {
            Debug.LogWarning("セーブデータを読み込めません");
            return;
        }

        //空きスロットなら新規作成
        if(!data.hasData) manager.SelectEmptySlot(slotNumber);
        //保存済みなら読み込み・上書き確認
        else manager.SelectSavedSlot(slotNumber);
    }

    //空き枠として表示する
    public void ShowEmpty()
    {
        SetText(nameText, "まだセーブデータがありません");
        SetText(statsText, "");
        SetText(playTimeText, "");

        //空きスロットでは削除ボタンを非表示
        if(deleteButton != null) deleteButton.SetActive(false);

        SetButtonInteractable(true);
    }

    //保存済みデータとして表示
    public void ShowSaved(
        string saveName,
        int str,
        int dex,
        int intelligence,
        float playTime)
    {
        SetText(nameText, "名前：" + saveName);

        SetText(
            statsText,
            "STR " + str +
            " / DEX " + dex +
            " / INT " + intelligence
        );

        //プレイ時間を時:分:秒で表示
        if(float.IsNaN(playTime) || float.IsInfinity(playTime))
        {
            playTime = 0f;
        }

        playTime = Mathf.Max(0f, playTime);

        long totalSeconds = (long)playTime;

        long hours = totalSeconds / 3600;
        long minutes = (totalSeconds % 3600) / 60;
        long seconds = totalSeconds % 60;

        string timeText =
            hours.ToString("00") + ":" +
            minutes.ToString("00") + ":" +
            seconds.ToString("00");

        SetText(playTimeText, "総プレイ時間：" + timeText);

        //保存済みスロットでは削除ボタンを表示
        if(deleteButton != null)
        {
            deleteButton.SetActive(true);
        }

        SetButtonInteractable(true);
    }

    //読み込みエラーとして表示
    private void ShowError(string message)
    {
        SetText(nameText, message);
        SetText(statsText, "");
        SetText(playTimeText, "");

        if(deleteButton != null) deleteButton.SetActive(false);

        //読み込みできないスロットはクリック不可
        SetButtonInteractable(false);
    }

    //Textの安全な更新
    private void SetText(TextMeshProUGUI target, string message)
    {
        if(target != null) target.text = message;
        else Debug.LogWarning("表示用Textが設定されていません");
    }

    //ボタンの有効・無効を切り替える
    private void SetButtonInteractable(bool interactable)
    {
        if(slotButton != null) slotButton.interactable = interactable;
    }

}