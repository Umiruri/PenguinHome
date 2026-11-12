//イベントが正常に開始されたときだけオーブが消えるように

using UnityEngine;

public class EventOrb : MonoBehaviour
{
    [Header("イベント設定")]
    public string eventName = "雪道を飛び越えよう！";
    public DiceRoller.StatType statType = DiceRoller.StatType.DEX;
    public string orbId; //オーブごとに異なるIDを設定する

    private EventManager eventManager;

    private bool hasTriggered;

    private void Start()
    {
        //EventManagerはGameManagerと同じオブジェクトにある
        eventManager = GameManager.Instance != null ? GameManager.Instance.GetComponent<EventManager>() : null;
        if(eventManager == null) Debug.LogError("EventManagerが見つからないよ");
        
        //セーブデータで使用済みなら、オーブを消したままにする
        if(GameManager.Instance != null && GameManager.Instance.IsOrbUsed(orbId))
        {
            hasTriggered = true;
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player")) return; //プレイヤー以外との接触は無視する
        if(hasTriggered) return; // すでに使ったオーブは反応しない
        if(eventManager == null) return;

        //仲間のペンギンが1匹もいなければイベントを開始しない
        if(GameManager.Instance == null || GameManager.Instance.FriendCount < 1) return;
    
        //イベント開始に成功した場合だけオーブを消す
        if(eventManager.StartEvent(eventName, statType))
        {
            hasTriggered = true;

            if(GameManager.Instance != null) GameManager.Instance.MarkOrbUsed(orbId);
            
            gameObject.SetActive(false);
        }
    }
    
}