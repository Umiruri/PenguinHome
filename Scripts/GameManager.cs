//ゲーム全体の進行状態を管理

using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    //共通設定
    private const int MaxPenguins = 11;
    private const float LoseMinDistance = 3f;
    private const float LoseMaxDistance = 6f;
    private const float RayStartHeight = 20f;
    private const float RayDistance = 50f;

    public static GameManager Instance { get; private set; }

    public bool IsExploring { get; private set; } = true; //探索中かどうか

    //仲間になったペンギンを順番に管理
    public List<PenguinController> Friends { get; private set; } = new List<PenguinController>();

    //フィールドに存在する全ペンギンを管理
    public List<PenguinController> AllPenguins { get; private set; } = new List<PenguinController>();

    //使用済みイベントオーブのIDを管理
    private HashSet<string> usedOrbIds = new HashSet<string>();

    //イベント回数
    public int EventCount { get; private set; } = 0;
    public int SuccessEventCount { get; private set; } = 0;
    public int FailureEventCount { get; private set; } = 0;

    //仲間のペンギン数
    public int FriendCount => Friends.Count;

    private GameTimer gameTimer;

    private void Awake()
    {
        if(Instance!=null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        gameTimer = GetComponent<GameTimer>();

        //プレイ時間を復元
        if(gameTimer != null)
        {
            gameTimer.LoadPlayTime(
                GameData.PlayTime,
                GameData.CurrentExplorationTime
            );
        }
        else Debug.LogWarning("GameTimerが見つかりません");

        //イベント回数を復元
        LoadEventCounts(
            GameData.EventCount,
            GameData.SuccessEventCount,
            GameData.FailureEventCount
        );

        //使用済みイベントオーブを復元
        if(GameData.LoadedSaveData != null)
        {
            LoadUsedOrbs(GameData.LoadedSaveData);
            Debug.Log("使用済みイベントオーブを復元しました");
        }
        else
        {
            ResetUsedOrbs();
            Debug.Log("使用済みイベントオーブをリセットしました");
        }
    }

    private void OnDestroy()
    {
        if(Instance == this) Instance = null;
    }

    //ペンギンを全体リストに登録する
    public void RegisterPenguin(PenguinController penguin)
    {
        if(penguin == null || AllPenguins.Contains(penguin)) return;
        
        //破棄されたペンギンをリストから除外
        AllPenguins.RemoveAll(p=>p == null);

        if(AllPenguins.Count >= MaxPenguins)
        {
            Debug.LogWarning("フィールドのペンギンは最大11羽までです");
            return;
        }

        AllPenguins.Add(penguin);

        Debug.Log($"ペンギン登録：ID={penguin.penguinId} / 登録数={AllPenguins.Count}");
    }

    //ペンギンを仲間に登録する
    public void AddFriend(PenguinController penguin)
    {
        if(penguin == null || Friends.Contains(penguin)) return;

        if(Friends.Count >= MaxPenguins)
        {
            Debug.LogWarning("仲間のペンギンは最大11羽までです");
            return;
        }

        Friends.Add(penguin);
    }

    //仲間リストからペンギンを外す
    public void RemoveFriend(PenguinController penguin)
    {
        if(penguin == null) return;
        Friends.Remove(penguin);
    }

    //ランダムに1羽を仲間にする
    public PenguinController RecruitRandomPenguin() //まだ仲間になっていないペンギンの中から1羽ランダムに選んで仲間にする
    {
        Friends.RemoveAll(p=>p == null);
        AllPenguins.RemoveAll(p=>p == null);

        if(Friends.Count >= MaxPenguins) return null;

        List<PenguinController> candidates =
            AllPenguins.FindAll(p => p != null && !p.IsFriend);

        if(candidates.Count == 0) return null;

        PenguinController penguin =
            candidates[Random.Range(0, candidates.Count)];

        penguin.SetFriendStatus(true);

        return penguin;
    }

    //ランダムに1羽を仲間から外す
    //イベント失敗後のペンギンの処理
    public PenguinController LoseRandomFriend()
    {
        Friends.RemoveAll(p => p == null);

        if (Friends.Count == 0) return null;

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if(player == null)
        {
            Debug.LogWarning("Playerが見つかりません");
            return null;
        }

        PenguinController penguin =
            Friends[Random.Range(0, Friends.Count)];

        //プレイヤーの周囲にランダムな位置を作成
        Vector2 direction = Random.insideUnitCircle.normalized;

        if(direction.sqrMagnitude < 0.001f) direction = Vector2.right;

        float distance =
            Random.Range(LoseMinDistance, LoseMaxDistance);

        Vector2 offset = direction * distance;

        Vector3 position =
            player.transform.position
            + new Vector3(offset.x, 0f, offset.y);

        //上空から地面を探す
        Vector3 rayStart =
            position + Vector3.up * RayStartHeight;

        if(Physics.Raycast(
            rayStart,
            Vector3.down,
            out RaycastHit hit,
            RayDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            position.y = hit.point.y;
        }
        else position = penguin.transform.position;

        penguin.ReleaseAt(position);

        return penguin;
    }

    //イベント結果を記録
    public void RecordEventResult(bool success)
    {
        if(!IsExploring) return;
        
        EventCount++;

        if(success) SuccessEventCount++;
        else FailureEventCount++;
    }

    //オーブが使用済みか確認
    public bool IsOrbUsed(string orbId)
    {
        if(string.IsNullOrEmpty(orbId)) return false;

        return usedOrbIds.Contains(orbId);
    }

    //オーブを使用済みに
    public void MarkOrbUsed(string orbId)
    {
        if(string.IsNullOrEmpty(orbId)) return;

        usedOrbIds.Add(orbId);
    }

    //使用済みオーブをセーブデータに保存
    public void SaveUsedOrbs(SaveData data)
    {
        if(data == null) return;

        List<string> ids = new List<string>(usedOrbIds);
        ids.Sort();

        data.usedOrbIds = ids.ToArray();
        data.usedOrbCount = data.usedOrbIds.Length;
    }

    //セーブデータから使用済みオーブを復元
    public void LoadUsedOrbs(SaveData data)
    {
        usedOrbIds.Clear();
        if(data == null || data.usedOrbIds == null) return;

        foreach(string orbId in data.usedOrbIds)
        {
            if(!string.IsNullOrEmpty(orbId)) usedOrbIds.Add(orbId);
        }
    }

    //新しい探索では使用済みオーブをリセット
    public void ResetUsedOrbs()
    {
        usedOrbIds.Clear();
    }

    //保存データからイベント回数を復元
    public void LoadEventCounts(
        int eventCount,
        int successCount,
        int failureCount)
    {
        EventCount = Mathf.Max(0, eventCount);

        SuccessEventCount = Mathf.Clamp(
            successCount,
            0,
            EventCount
        );

        FailureEventCount = Mathf.Clamp(
            failureCount,
            0,
            EventCount - SuccessEventCount
        );
    }

    //フィールドにいる全ペンギンの状態を保存
    public void SavePenguins(SaveData data)
    {
        if(data == null) return;

        List<PenguinSaveData> savedPenguins =
            new List<PenguinSaveData>();

        List<int> friendIds = new List<int>();

        HashSet<int> savedIds = new HashSet<int>();

        AllPenguins.RemoveAll(p => p == null);
        Friends.RemoveAll(p => p == null);

        foreach(PenguinController penguin in AllPenguins)
        {
            if(penguin == null) continue;

            //同じIDの重複保存を防ぐ
            if (!savedIds.Add(penguin.penguinId))
            {
                Debug.LogWarning("重複したペンギンIDを検出しました。ID：" + penguin.penguinId);
                continue;
            }

            Vector3 position = penguin.transform.position;
            Vector3 rotation = penguin.transform.eulerAngles;

            if(!IsValidVector(position) || !IsValidVector(rotation))
            {
                Debug.LogWarning("異常な位置または向きのペンギンを保存から除外しました。ID：" + penguin.penguinId);
                continue;
            }

            PenguinSaveData penguinData = new PenguinSaveData();

            penguinData.penguinId = penguin.penguinId;

            penguinData.posX = position.x;
            penguinData.posY = position.y;
            penguinData.posZ = position.z;

            penguinData.rotX = rotation.x;
            penguinData.rotY = rotation.y;
            penguinData.rotZ = rotation.z;

            penguinData.isFriend = penguin.IsFriend;

            savedPenguins.Add(penguinData);

            if (penguin.IsFriend) friendIds.Add(penguin.penguinId);
        }

        //実際に保存できた数を記録
        data.penguins = savedPenguins.ToArray();
        data.penguinCount = data.penguins.Length;

        data.friendPenguinIds = friendIds.ToArray();
        data.friendPenguinCount = data.friendPenguinIds.Length;

        Debug.Log($"ペンギンの保存データを作成：{data.penguinCount}羽 / 仲間：{data.friendPenguinCount}羽");
    }

    //探索を終了
    public void FinishExploration()
    {
        if(!IsExploring) return;

        //探索を終了する
        IsExploring = false;    
        string playTime = "00:00";

        if(gameTimer != null)
        {
            playTime = gameTimer.GetFormattedTime();

            //累積時間と今回の探索時間を保存
            GameData.PlayTime = gameTimer.PlayTime;
            GameData.CurrentExplorationTime =
                gameTimer.CurrentExplorationTime;
        }

        //イベント回数もGameDataに反映
        GameData.EventCount = EventCount;
        GameData.SuccessEventCount = SuccessEventCount;
        GameData.FailureEventCount = FailureEventCount;

        Debug.Log("===== 探索結果 =====");
        Debug.Log("仲間のペンギン：" + FriendCount + "羽");
        Debug.Log("発生したイベント：" + EventCount + "回");
        Debug.Log("成功したイベント：" + SuccessEventCount + "回");
        Debug.Log("失敗したイベント：" + FailureEventCount + "回");
        Debug.Log("探索時間：" + playTime);
        Debug.Log("====================");

        //結果画面
        UIManager uiManager = GetComponent<UIManager>();

        if (uiManager != null)
        {
            uiManager.ShowResults(
                FriendCount,
                EventCount,
                SuccessEventCount,
                FailureEventCount,
                playTime
            );
        }
    }

    //新しい探索を始めるために記録をリセット
    public void ResetExplorationData()
    {
        //全ペンギンの仲間状態を解除
        foreach(PenguinController penguin in AllPenguins)
        {
            if(penguin != null) penguin.SetFriendStatus(false);
        }

        Friends.Clear();

        //イベント記録をリセット
        EventCount = 0;
        SuccessEventCount = 0;
        FailureEventCount = 0;

        //使用済みオーブをリセット
        ResetUsedOrbs();

        //GameData側のイベント記録もリセット
        GameData.EventCount = 0;
        GameData.SuccessEventCount = 0;
        GameData.FailureEventCount = 0;

        //探索中の状態に戻す
        IsExploring = true;

        Debug.Log("探索記録をリセットしました");
    }

    //Vector3の異常値を確認する
    private bool IsValidVector(Vector3 value)
    {
        return
            !float.IsNaN(value.x) &&
            !float.IsNaN(value.y) &&
            !float.IsNaN(value.z) &&
            !float.IsInfinity(value.x) &&
            !float.IsInfinity(value.y) &&
            !float.IsInfinity(value.z);
    }

}