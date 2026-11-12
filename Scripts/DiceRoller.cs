//能力値を使った1〜20のダイス判定

using UnityEngine;

public class DiceRoller : MonoBehaviour
{
    public enum StatType {DEX,INT,STR}

    public bool RollCheck(StatType statType, int roll)
    {
        Debug.Log("ダイス判定処理に入ったよ");

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if(player == null)
        {
            Debug.LogWarning("Playerが見つからないよ");
            return false;
        }

        CharacterStats stats = player.GetComponent<CharacterStats>();

        if(stats == null || !stats.IsConfigured)
        {
            Debug.LogWarning("能力値がまだ設定されていないよ");
            return false;
        }

        if(roll < 1 || roll > 20)
        {
            Debug.LogWarning("ダイスの出目が不正だよ");
            return false;
        }

        int targetValue = 0;

        switch(statType)
        {
            case StatType.DEX:
                targetValue = stats.DEX;
                break;

            case StatType.INT:
                targetValue = stats.INT;
                break;

            case StatType.STR:
                targetValue = stats.STR;
                break;
        }

        //出目と能力値を比較
        bool success = roll <= targetValue;
        
        Debug.Log($"{statType}判定：出目={roll}、目標値={targetValue}、" + (success ? "成功" : "失敗"));
        return success;
    }

}