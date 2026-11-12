//カメラ設定

using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("追いかける対象")]
    public Transform target;

    [Header("カメラ設定")]
    public float distance = 8f;
    public float height = 2f;

    [Header("マウス操作")]
    public float mouseSensitivity = 0.08f;
    public float minVerticalAngle = -10f;
    public float maxVerticalAngle = 40f;

    [Header("上下の追従")]
    public float verticalFollowSpeed = 5f;

    private float currentCameraHeight;

    //結果画面でカメラが動かないように
    [SerializeField]
    private GameObject resultPanel;

    //カメラの水平方向・垂直方向の角度を保存
    private float horizontalAngle;
    private float verticalAngle = 20f;

    private GameMenuManager menuManager;
    private EventManager eventManager;

    void Start()
    {
        menuManager = FindAnyObjectByType<GameMenuManager>();
        eventManager = FindAnyObjectByType<EventManager>();

        if(target != null)
        {
            Vector3 direction = transform.position - target.position;
            horizontalAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            currentCameraHeight = target.position.y;
        }

        //カーソルは表示したままにする
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void LateUpdate()
    {
        //メニュー画面が開いている間はカメラを動かさない
        if(menuManager != null && menuManager.IsMenuOpen) return;

        //イベント中はカメラを固定する
        if(eventManager != null && eventManager.IsEventActive) return;

        //結果画面はカメラを固定
        if(resultPanel != null && resultPanel.activeInHierarchy) return;

        //追いかける対象がいないときは処理を終了
        if(target == null) return;

        //マウスの移動量を取得
        if(Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            horizontalAngle += mouseDelta.x * mouseSensitivity;
            verticalAngle -= mouseDelta.y * mouseSensitivity;

            verticalAngle = Mathf.Clamp(
                verticalAngle,
                minVerticalAngle,
                maxVerticalAngle
            );
        }

        //カメラの回転を計算
        Quaternion rotation = Quaternion.Euler(
            verticalAngle,
            horizontalAngle,
            0f
        );

        //プレイヤーの上下移動に少しずつ追従
        currentCameraHeight = Mathf.Lerp(
            currentCameraHeight,
            target.position.y,
            verticalFollowSpeed * Time.deltaTime
        );

        //カメラの目標位置
        Vector3 targetPosition = new Vector3(
            target.position.x,
            currentCameraHeight + height,
            target.position.z
        ) - rotation * Vector3.forward * distance;

        //カメラを直接移動
        transform.position = targetPosition;

        //注視点の高さ
        Vector3 lookPosition = new Vector3(
            target.position.x,
            currentCameraHeight + height,
            target.position.z
        );

        transform.LookAt(lookPosition);
    }

}