//探索シーン開始時にプレイヤーの位置と向きを復元

using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerSaveLoader : MonoBehaviour
{
    private void Start()
    {
        //セーブした位置がなければ何もしない
        if(!GameData.HasSavedPlayerPosition) return;

        //CharacterControllerを取得
        CharacterController controller = GetComponent<CharacterController>();

        if(controller == null)
        {
            Debug.LogError("CharacterControllerが見つかりません");
            return;
        }

        //保存位置の数値を確認
        Vector3 savedPosition = new Vector3(
            GameData.PlayerPosX,
            GameData.PlayerPosY,
            GameData.PlayerPosZ
        );

        Vector3 savedRotation = new Vector3(
            GameData.PlayerRotX,
            GameData.PlayerRotY,
            GameData.PlayerRotZ
        );

        if(!IsValidVector(savedPosition) || !IsValidVector(savedRotation))
        {
            Debug.LogError("保存されたプレイヤーの位置または向きが不正です");
            GameData.HasSavedPlayerPosition = false;
            return;
        }

        bool wasEnabled = controller.enabled;

        try
        {
            //CharacterControllerを一時的に無効化
            controller.enabled = false;

            //プレイヤーの位置と向きを復元
            transform.SetPositionAndRotation(
                savedPosition,
                Quaternion.Euler(savedRotation)
            );

            //復元した位置を落下時の復帰地点に設定
            PlayerMovement playerMovement = GetComponent<PlayerMovement>();

            if(playerMovement != null)
            {
                playerMovement.SetRespawnPosition(
                    savedPosition,
                    Quaternion.Euler(savedRotation)
                );
            }

            Debug.Log("プレイヤーの位置を復元しました");

            //復元成功後にフラグを解除
            GameData.HasSavedPlayerPosition = false;
        }
        catch(System.Exception e)
        {
            Debug.LogError("プレイヤーの位置復元に失敗しました: " + e.Message);
        }
        finally
        {
            //元の有効状態に戻す
            controller.enabled = wasEnabled;
        }
    }

    //Vector3の数値が正常か確認
    private bool IsValidVector(Vector3 value)
    {
        return
            !float.IsNaN(value.x) &&
            !float.IsNaN(value.y) &&
            !float.IsNaN(value.z) &&
            !float.IsInfinity(value.x) &&
            !float.IsInfinity(value.y) &&
            !float.IsInfinity(value.z);
    }

}