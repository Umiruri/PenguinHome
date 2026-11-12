//探索中の時間を計測

using UnityEngine;

public class GameTimer : MonoBehaviour
{
    //累計プレイ時間（秒）
    public float PlayTime{ get; private set; }

     //今回の探索時間（秒）
    public float CurrentExplorationTime { get; private set; }

    private void Update()
    {
        //GameManagerが存在しないまたは探索中でなければ計測しない
        if (GameManager.Instance == null || !GameManager.Instance.IsExploring) return;
        
        float elapsed = Mathf.Max(0f, Time.deltaTime);

        //累積時間と今回の探索時間を計測
        PlayTime += elapsed;
        CurrentExplorationTime += elapsed;
    }

    //セーブデータから時間を復元
    public void LoadPlayTime(float savedTime, float savedExplorationTime)
    {
        PlayTime = NormalizeTime(savedTime);
        CurrentExplorationTime = NormalizeTime(savedExplorationTime);
    }

    //今回の探索時間をリセットする
    public void ResetExplorationTime()
    {
        CurrentExplorationTime = 0f;
    }

    //時間の異常値を補正する
    private float NormalizeTime(float time)
    {
        if(float.IsNaN(time) || float.IsInfinity(time))
        {
            Debug.LogWarning("異常なプレイ時間を検出したため、0秒に補正しました");
            return 0f;
        }

        return Mathf.Max(0f, time);
    }

    //時間を「分:秒」に変換する
    private string FormatTime(float time)
    {
        time = NormalizeTime(time);

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        return $"{minutes:00}:{seconds:00}";
    }

    //今回の探索時間を取得する
    public string GetFormattedTime()
    {
        return FormatTime(CurrentExplorationTime);
    }

    //累積プレイ時間を取得する
    public string GetFormattedTotalTime()
    {
        return FormatTime(PlayTime);
    }

}