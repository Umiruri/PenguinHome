//能力値を管理

using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class StatusManager : MonoBehaviour
{
    public TextMeshProUGUI strText;
    public TextMeshProUGUI dexText;
    public TextMeshProUGUI intText;
    public TextMeshProUGUI totalPointsText;

    //能力値の初期値
    private int str = 10;
    private int dex = 10;
    private int intelligence = 10;

    //つよさを増やす
    public void AddSTR()
    {
        if(str >= 20) return;
        if(GetTotal() >= 30) return;
        str++;
        GameData.STR = str;
        UpdateTexts();
    }

    //つよさを減らす
    public void SubtractSTR()
    {
        if(str <= 0) return;
        str--;
        GameData.STR = str;
        UpdateTexts();
    }

    //すばやさを増やす
    public void AddDEX()
    {
        if(dex >= 20) return;
        if(GetTotal() >= 30) return;
        dex++;
        GameData.DEX = dex;
        UpdateTexts();
    }

    //すばやさを減らす
    public void SubtractDEX()
    {
        if(dex <= 0)return;
        dex--;
        GameData.DEX = dex;
        UpdateTexts();
    }

    //かしこさを増やす
    public void AddINT()
    {
        if(intelligence >= 20) return;
        if(GetTotal() >= 30) return;
        intelligence++;
        GameData.INT = intelligence;
        UpdateTexts();
    }
    //かしこさを減らす
    public void SubtractINT()
    {
        if(intelligence <= 0) return;
        intelligence--;
        GameData.INT = intelligence;
        UpdateTexts();
    }

    //合計値を取得
    private int GetTotal()
    {
        return str + dex + intelligence;
    }

    //画面の数字を更新
    private void UpdateTexts()
    {
        if(strText != null) strText.text = "つよさ　 : " + str;
        if(dexText != null) dexText.text = "すばやさ : " + dex;
        if(intText != null) intText.text = "かしこさ : " + intelligence;

        if(totalPointsText != null)
        {
            int total = GetTotal();
            totalPointsText.text = "使用ポイント : " + total + " / 30";
        }
    }

    public void StartExploration()
    {
        SceneManager.LoadScene("SampleScene_StageDesign");
    }

    //能力値設定画面が開いたとき
    private void Start()
    {
        //能力値を初期値の10に戻す
        str = 10;
        dex = 10;
        intelligence = 10;

        //GameDataにも初期値を反映
        GameData.STR = str;
        GameData.DEX = dex;
        GameData.INT = intelligence;

        UpdateTexts();
    }

}