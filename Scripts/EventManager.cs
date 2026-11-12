//イベント関係

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventManager : MonoBehaviour
{
    //ダイスのアニメーション関係
    private DiceRoller diceRoller;
    private DiceAnimation diceAnimation;
    private AudioSource diceAudioSource;

    //ペンギンの出現管理
    private PenguinSpawner penguinSpawner;

    //BGMの管理
    private ExplorationBGMController bgmController;
    
    //イベントパネル関係
    public GameObject eventPanel;
    public bool IsEventActive {get; private set;}

    //イベント画面のボタン
    private Button rollDiceButton;
    private Button returnButton;

    //イベント画面に表示するテキスト
    private TMP_Text descriptionText;
    private TMP_Text resultText;

    //現在進行中のイベント名と判定する能力値
    private string currentEventName;
    private DiceRoller.StatType currentStatType;
    private bool hasRolled; //ダイスを振ったかどうか
    private bool isWaitingForResult; //判定結果

    private void Awake()
    {
        diceRoller = GetComponent<DiceRoller>();
        penguinSpawner = FindAnyObjectByType<PenguinSpawner>();
        bgmController = FindAnyObjectByType<ExplorationBGMController>();

        Transform diceCamera = eventPanel != null ? eventPanel.transform.Find("DiceCamera"): null;

        diceAnimation = diceCamera != null ? diceCamera.GetComponentInChildren<DiceAnimation>(true) : null;

        if(diceRoller == null) Debug.LogError("DiceRollerが見つからないよ");
        if(eventPanel == null)
        {
            Debug.LogError("EventPanelを登録しよう");
            return;
        }

        //ダイスの音
        GameObject diceSound = GameObject.Find("DiceSound");
        if(diceSound != null) diceAudioSource = diceSound.GetComponent<AudioSource>();
        else Debug.LogWarning("DiceSoundが見つからないよ");

        //EventPanel内のUIを取得
        Transform description = eventPanel.transform.Find("EventDescriptionText");
        Transform result = eventPanel.transform.Find("EventResultText");
        Transform rollButton = eventPanel.transform.Find("RollDiceButton");
        Transform backButton = eventPanel.transform.Find("ReturnButton");

        if(description != null) descriptionText = description.GetComponent<TMP_Text>();
        if(result != null) resultText = result.GetComponent<TMP_Text>();
        if(rollButton != null) rollDiceButton = rollButton.GetComponent<Button>();
        if(backButton != null) returnButton = backButton.GetComponent<Button>();
        if(rollDiceButton != null) rollDiceButton.onClick.AddListener(OnRollDiceButtonClicked);
        if(returnButton != null) returnButton.onClick.AddListener(ReturnToExploration);
        
        eventPanel.SetActive(false);
    }

    //イベント画面を開く
    public bool StartEvent(string eventName, DiceRoller.StatType statType)
    {
        if(diceRoller == null || eventPanel == null) return false;
        if(IsEventActive) return false;
        if(GameManager.Instance != null && !GameManager.Instance.IsExploring) return false;

        currentEventName = eventName;
        currentStatType = statType;
        hasRolled = false;
        isWaitingForResult = false;

        eventPanel.SetActive(true);
        IsEventActive = true;

        if(bgmController != null) bgmController.PlayEventBGM();

        //能力値を目標値として取得
        int targetValue = 0;
        switch(currentStatType)
        {
            case DiceRoller.StatType.DEX:
                targetValue = GameData.DEX;
                break;

            case DiceRoller.StatType.INT:
                targetValue = GameData.INT;
                break;

            case DiceRoller.StatType.STR:
                targetValue = GameData.STR;
                break;
        }

        //イベントの説明を表示する
        if(descriptionText != null) descriptionText.text = "イベント発生！\n" + currentEventName + "\n" + "ダイスを振って判定しよう！\n" + "目標値：" + targetValue + " 以下なら成功！";

        // 結果表示とボタンを初期状態にする
        if(resultText != null) resultText.text = "";
        if(rollDiceButton != null) rollDiceButton.interactable = true;
        if(returnButton != null) returnButton.gameObject.SetActive(false);

        return true;
    }

    //「ダイスを振る！」ボタン
    private void OnRollDiceButtonClicked()
    {
        if(!IsEventActive || hasRolled || isWaitingForResult) return;

        if(diceAnimation == null)
        {
            Debug.LogError("DiceAnimationが見つからないよ");
            return;
        }

        int roll = Random.Range(1, 21);

        //ダイスアニメーションを開始
        bool started = diceAnimation.RollDice(roll, OnDiceAnimationComplete);

        //開始できなかった場合はここで終了
        if(!started) return;

        hasRolled = true;
        isWaitingForResult = true;

        if(rollDiceButton != null) rollDiceButton.interactable = false;
        if(diceAudioSource != null) diceAudioSource.Play();
    }

    //ダイスの回転が終わってから呼ばれる
    private void OnDiceAnimationComplete(int roll)
    {
        bool success = diceRoller.RollCheck(currentStatType, roll);

        if(GameManager.Instance != null) GameManager.Instance.RecordEventResult(success);
        string message = "";

        if(success)
        {
            PenguinController penguin = null;

            if(penguinSpawner != null) penguin = penguinSpawner.SpawnNewFriend();

            if(penguin != null) message = "\n新しいペンギンが仲間になったよ！";
            else message = "\nペンギンを増やせなかったよ……";

            if(resultText != null) resultText.text = "出目：" + roll + "\n判定成功！" + message;
        }
        else
        {
            PenguinController penguin = GameManager.Instance != null ? GameManager.Instance.LoseRandomFriend() : null;

            if(resultText != null)
            {
                if (penguin != null) resultText.text = "出目：" + roll + "\n判定失敗……\n仲間のペンギンがはぐれてしまったよ……";
                else resultText.text = "出目：" + roll + "\n判定失敗……\n仲間のペンギンはいなくなってしまったよ……";
            }
        }

        isWaitingForResult = false;
        if(returnButton != null) returnButton.gameObject.SetActive(true);
    }

    //探索画面に戻る
    public void ReturnToExploration()
    {
        if(!IsEventActive || isWaitingForResult || !hasRolled) return;
        IsEventActive = false;
        if(eventPanel != null) eventPanel.SetActive(false);
        if(bgmController != null) bgmController.PlayExplorationBGM();
    }

}