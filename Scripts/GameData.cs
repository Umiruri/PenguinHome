//ゲーム中に使うデータを保管

public static class GameData
{
    //能力値
    public static int STR = SaveDataSettings.DefaultStat;
    public static int DEX = SaveDataSettings.DefaultStat;
    public static int INT = SaveDataSettings.DefaultStat;

    //セーブスロット関係
    public static int SaveSlot = 1;
    public static string SaveName = "";
    public static bool IsNewGame = true;

    //読み込んだセーブデータ全体
    public static SaveData LoadedSaveData = null;

    //プレイ時間
    public static float PlayTime = SaveDataSettings.DefaultPlayTime; //総プレイ時間（秒）
    public static float CurrentExplorationTime = SaveDataSettings.DefaultPlayTime; //プレイ中の時間（秒）

    //発生イベント関係
    public static int EventCount = SaveDataSettings.DefaultCount;
    public static int SuccessEventCount = SaveDataSettings.DefaultCount;
    public static int FailureEventCount = SaveDataSettings.DefaultCount;

    //読み込んだプレイヤーの位置
    public static bool HasSavedPlayerPosition = false;
    public static float PlayerPosX = 0f;
    public static float PlayerPosY = 0f;
    public static float PlayerPosZ = 0f;

    //読み込んだプレイヤーの向き
    public static float PlayerRotX = 0f;
    public static float PlayerRotY = 0f;
    public static float PlayerRotZ = 0f;

    //新しいゲーム用に初期化
    public static void Reset()
    {
        STR = SaveDataSettings.DefaultStat;
        DEX = SaveDataSettings.DefaultStat;
        INT = SaveDataSettings.DefaultStat;

        SaveName = "";
        IsNewGame = true;

        LoadedSaveData = null;

        PlayTime = SaveDataSettings.DefaultPlayTime;
        CurrentExplorationTime = SaveDataSettings.DefaultPlayTime;

        EventCount = SaveDataSettings.DefaultCount;
        SuccessEventCount = SaveDataSettings.DefaultCount;
        FailureEventCount = SaveDataSettings.DefaultCount;

        HasSavedPlayerPosition = false;

        PlayerPosX = 0f;
        PlayerPosY = 0f;
        PlayerPosZ = 0f;

        PlayerRotX = 0f;
        PlayerRotY = 0f;
        PlayerRotZ = 0f;
    }
    
}