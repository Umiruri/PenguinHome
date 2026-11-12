//家のTriggerに入ったとき、相手のTagがPlayerなら探索終了

using UnityEngine;

public class HomeGoal : MonoBehaviour
{
    //探索終了済みか
    private bool isFinished = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isFinished) return;

        if(!other.CompareTag("Player")) return;

        if(GameManager.Instance == null)
        {
            Debug.LogWarning("GameManagerが見つかりません");
            return;
        }

        //探索終了
        isFinished = true;

        Debug.Log("おうちに着いたよ");

        GameManager.Instance.FinishExploration();
    }
    
}