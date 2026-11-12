//プレイヤーの能力値

using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    //能力値の設定範囲
    private const int MinStat = 0;
    private const int MaxStat = 20;
    private const int MaxTotalStats = 30;

    //能力値が設定済みかどうか
    public bool IsConfigured { get; private set; }

    //能力値
    public int DEX { get; private set; } //すばやさ
    public int INT { get; private set; } //かしこさ
    public int STR { get; private set; } //つよさ

    public int TotalStats => DEX + INT + STR; //能力値の合計

    //プレイヤーの能力値を設定
    public bool SetStats(int dex, int intelligence, int str)
    {
        //各能力値の範囲を確認
        if (dex < MinStat || dex > MaxStat ||
            intelligence < MinStat || intelligence > MaxStat ||
            str < MinStat || str > MaxStat)
        {
            Debug.LogWarning("能力値が設定可能な範囲を超えているよ");
            return false;
        }

        //合計ポイントを確認
        if (dex + intelligence + str > MaxTotalStats)
        {
            Debug.LogWarning("能力値の合計が30を超えているよ");
            return false;
        }

        //能力値を反映
        DEX = dex;
        INT = intelligence;
        STR = str;

        IsConfigured = true;
        
        Debug.Log( $"能力値設定完了：DEX={DEX}, INT={INT}, STR={STR}");
        return true;
    }
    
}