//ダイスのアニメーション

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class DiceAnimation : MonoBehaviour
{
    public float rollDuration = 1.5f;
    private bool isRolling = false;

    //ダイスの出目ごとの回転角度
    private readonly Dictionary<int, Vector3> diceRotations = new Dictionary<int, Vector3>()
    {
        { 1,  new Vector3(-23.928f, 115.571f, -127.416f) },
        { 2,  new Vector3(-32.612f, 317.764f, -565.357f) },
        { 3,  new Vector3(73.227f, 97.711f, -545.510f) },
        { 4,  new Vector3(-46.300f, 145.925f, -391.397f) },
        { 5,  new Vector3(58.072f, 88.929f, -50.537f) },
        { 6,  new Vector3(14.781f, 125.114f, 116.812f) },
        { 7,  new Vector3(-47.067f, 58.837f, 265.117f) },
        { 8,  new Vector3(23.840f, 306.947f, 656.694f) },
        { 9,  new Vector3(-1.227f, 198.895f, 692.780f) },
        { 10, new Vector3(17.137f, 364.963f, 1149.802f) },
        { 11, new Vector3(-18.733f, 192.911f, 1272.439f) },
        { 12, new Vector3(-12.242f, 19.744f, 1447.817f) },
        { 13, new Vector3(-51.400f, 149.518f, 1309.290f) },
        { 14, new Vector3(47.047f, -244.211f, 1514.298f) },
        { 15, new Vector3(32.481f, -50.035f, 1564.197f) },
        { 16, new Vector3(73.621f, 99.810f, 1576.738f) },
        { 17, new Vector3(58.143f, -6.179f, 1531.458f) },
        { 18, new Vector3(-4.298f, -90.572f, 1609.442f) },
        { 19, new Vector3(31.930f, 130.472f, 1654.612f) },
        { 20, new Vector3(27.856f, 68.83f, 50.347f) }
    };

    public bool RollDice(int roll, Action<int> onComplete)
    {
        if(isRolling) return false;

        if(rollDuration <= 0f)
        {
            Debug.LogWarning("ダイスのアニメーション時間は0より大きく設定してね");
            return false;
        }

        if(!diceRotations.ContainsKey(roll))
        {
            Debug.LogWarning("この出目の角度はまだ登録されていないよ：" + roll);
            return false;
        }

        StartCoroutine(RollAnimation(roll, onComplete));
        return true;
    }

    private IEnumerator RollAnimation(int roll, Action<int> onComplete)
    {
        isRolling = true;
        Quaternion startRotation = transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(diceRotations[roll]);

        //開始時と終了時の回転差を求める
        Quaternion delta = targetRotation * Quaternion.Inverse(startRotation);
        delta.ToAngleAxis(out float angle, out Vector3 axis);
        
        if(axis.sqrMagnitude < 0.001f) axis = Vector3.up; //回転軸が不安定な場合の対策
        float totalAngle = angle + 720f; //余分な回転を加えて勢いよく回す
        float elapsed = 0f; //アニメーションの経過時間を保存

        //アニメーションを繰り返す
        while(elapsed < rollDuration)
        {
            elapsed += Time.deltaTime; //アニメーションを進める
            float t = Mathf.Clamp01(elapsed / rollDuration); //アニメーション進行度の計算
            float easedT = 1f - Mathf.Pow(1f - t, 3f); //最初は速く最後はゆっくり減速

            //同じ回転軸で連続的に回転
            Quaternion rotation = Quaternion.AngleAxis(totalAngle * easedT, axis);
            transform.localRotation = rotation * startRotation;

            yield return null;
        }

        //最後は登録済みの正確な向きに合わせる
        transform.localRotation = targetRotation;

        Debug.Log("今回の出目：" + roll);
        isRolling = false;
        if(onComplete != null) onComplete.Invoke(roll);
    }

}