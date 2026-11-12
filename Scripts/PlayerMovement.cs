//プレイヤーの移動

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 12f;

    [Header("ジャンプ・重力")]
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("カメラ")]
    public Transform cameraTransform;
    private CharacterController controller;
    private float verticalVelocity;

    [Header("落下時の復帰")]
    public float fallLimit = -2f;
    private Vector3 startPosition;
    private Quaternion startRotation;

    //コンポーネントと初期復帰地点を設定
    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void Start()
    {
        //カメラが未設定ならMainCameraを探す
        if(cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        //イベント中は動かないように
        EventManager eventManager = GameManager.Instance != null ? GameManager.Instance.GetComponent<EventManager>() : null;
        if(eventManager != null && eventManager.IsEventActive) return;

        //海に落ちたら開始地点に戻す
        if(transform.position.y < fallLimit)
        {
            controller.enabled = false;

            transform.position = startPosition;
            transform.rotation = startRotation;

            verticalVelocity = 0f;

            controller.enabled = true;

            //仲間のペンギンもプレイヤーと一緒に復帰させる
            if(GameManager.Instance != null)
            {
                foreach(PenguinController penguin in GameManager.Instance.Friends)
                {
                    if(penguin != null) penguin.ReturnWithPlayer(startPosition);
                }
            }
            Debug.Log("復帰後の位置：" + transform.position);
            return;
        }

        //探索終了後は落下処理だけ行う
        if(GameManager.Instance != null && !GameManager.Instance.IsExploring)
        {
            if(!controller.isGrounded)
            {
                verticalVelocity += gravity * Time.deltaTime;
                controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
            }
            else verticalVelocity = 0f;
            return;
        }

        Vector2 input = Vector2.zero;

        if(Keyboard.current != null)
        {
            if(Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;
            if(Keyboard.current.sKey.isPressed ||  Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
            if(Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;
            if(Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
        }

        input = Vector2.ClampMagnitude(input, 1f);

        //カメラの前方向と右方向を取得
        Vector3 cameraForward = Vector3.forward;
        Vector3 cameraRight = Vector3.right;

        if(cameraTransform != null)
        {
            cameraForward = cameraTransform.forward;
            cameraRight = cameraTransform.right;

            //上下のカメラ角度は移動に影響させない
            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward.Normalize();
            cameraRight.Normalize();
        }

        //カメラの向きに合わせて移動方向を決める
        Vector3 movement = cameraForward * input.y + cameraRight * input.x;
        movement = Vector3.ClampMagnitude(movement, 1f);

        //移動方向にプレイヤーを向ける
        if(movement.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation,targetRotation,rotationSpeed * Time.deltaTime);
        }

        if(controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f; //地面にいるときの処理
        if(controller.isGrounded && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity); //Spaceキーでジャンプ

        //重力を適用
        verticalVelocity += gravity * Time.deltaTime;
        Vector3 velocity = movement * moveSpeed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    //復帰地点を設定する
    public void SetRespawnPosition(Vector3 position, Quaternion rotation)
    {
        startPosition = position;
        startRotation = rotation;
    }
    
}