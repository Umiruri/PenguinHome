//プレイヤーの能力値設定を管理

using UnityEngine;

public class CharacterSetup : MonoBehaviour
{
    private CharacterStats characterStats;
    private bool FindCharacterStats()
    {
        if(characterStats == null) characterStats = GetComponent<CharacterStats>();
        if(characterStats == null)
        {
            Debug.LogError("同じPlayerにCharacterStatsが見つからないよ");
            return false;
        }
        return true;
    }

    private void Awake()
    {
        FindCharacterStats();
        if(characterStats != null && !characterStats.IsConfigured) ConfigureCharacter(GameData.DEX,GameData.INT,GameData.STR);
    }

    public bool ConfigureCharacter(int dex,int intelligence,int str)
    {
        if(!FindCharacterStats()) return false;
        return characterStats.SetStats(dex,intelligence,str);
    }

}