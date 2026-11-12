//保存データ・セーブデータ

//セーブデータの共通設定(初期設定)
public static class SaveDataSettings
{
    //各ステータスの初期値
    public const int DefaultStat = 10;

    //プレイ時間の初期値
    public const float DefaultPlayTime = 0f;
    
    //カウントの初期値
    public const int DefaultCount = 0;
}

//ペンギン1羽分の保存データ
[System.Serializable]
public class PenguinSaveData
{
    //ペンギンを識別する固定ID
    public int penguinId;

    //フィールド上の位置
    public float posX;
    public float posY;
    public float posZ;

    //ペンギンの向き
    public float rotX;
    public float rotY;
    public float rotZ;

    //仲間になっているか
    public bool isFriend;
}

[System.Serializable]
public class SaveData
{
    //セーブデータが入っているか
    public bool hasData = false;

    //セーブ名
    public string saveName = "";

    //能力値
    public int STR = SaveDataSettings.DefaultStat;
    public int DEX = SaveDataSettings.DefaultStat;
    public int INT = SaveDataSettings.DefaultStat;

    //総プレイ時間（秒）
    public float playTime = SaveDataSettings.DefaultPlayTime;
    
    //今回の探索時間（秒）
    public float currentExplorationTime = SaveDataSettings.DefaultPlayTime;

    //プレイヤーの位置
    public float playerPosX = 0f;
    public float playerPosY = 0f;
    public float playerPosZ = 0f;

    //プレイヤーの向き
    public float playerRotX = 0f;
    public float playerRotY = 0f;
    public float playerRotZ = 0f;

    //仲間のペンギンの数
    public int friendPenguinCount = SaveDataSettings.DefaultCount;

    //仲間のペンギンを識別するIDの一覧
    public int[] friendPenguinIds = new int[0];
    
    //フィールドにいるペンギン全員のデータ
    public PenguinSaveData[] penguins = new PenguinSaveData[0];
    
    //フィールドにいるペンギンの数
    public int penguinCount = SaveDataSettings.DefaultCount;

    //イベントの数
    public int eventCount = SaveDataSettings.DefaultCount;
    public int successEventCount = SaveDataSettings.DefaultCount;
    public int failureEventCount = SaveDataSettings.DefaultCount;

    //使用済みイベントオーブの数
    public int usedOrbCount = SaveDataSettings.DefaultCount;

    //使用済みイベントオーブのID一覧
    public string[] usedOrbIds = new string[0];
}