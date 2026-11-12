//ペンギンが仲間になったと表示

using System.Collections;
using UnityEngine;
using TMPro;

public class FriendUI : MonoBehaviour
{
    [Header("通知UI")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TMP_Text notificationText;

    [Header("表示時間")]
    [SerializeField] private float displayDuration = 3f;

    private Coroutine hideCoroutine;

    private void Start()
    {
        //ゲーム開始時は通知を非表示に
        if(notificationPanel != null) notificationPanel.SetActive(false);
    }

    //通知を表示
    public void ShowNotification(string message)
    {
        if(notificationPanel == null || notificationText == null)
        {
            Debug.LogWarning("通知UIが設定されていないよ");
            return;
        }

        notificationText.text = message;
        notificationPanel.SetActive(true);

        //前の通知が残っていたらタイマーをリセット
        if(hideCoroutine != null) StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideAfterSeconds());
    }

    // 指定時間後に通知を消す
    private IEnumerator HideAfterSeconds()
    {
        yield return new WaitForSeconds(displayDuration);

        notificationPanel.SetActive(false);
        hideCoroutine = null;
    }

}