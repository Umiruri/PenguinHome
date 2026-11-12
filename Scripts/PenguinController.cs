//ペンギン

using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PenguinController : MonoBehaviour
{
    [Header("仲間になる設定")]
    public float recruitDistance = 2f;

    [Header("追従設定")]
    public float followSpeed = 4f;
    public float followStartDistance = 2f;
    public float stopDistance = 1.5f;
    public float followSmoothTime = 0.25f;

    [Header("重なり防止")]
    public float separationDistance = 2.5f;
    public float separationStrength = 4.0f;

    [Header("地面・重力")]
    public float gravity = -20f;

    [Header("段差・ジャンプ")]
    public float jumpForce = 8f;
    public float jumpHeightDifference = 1.0f;
    public float jumpDistance = 4.0f;

    [Header("ペンギン識別設定")]
    public int penguinId;

    [Header("落下時の復帰設定")]
    [SerializeField] private float fallThreshold = -2f;
    [SerializeField] private float recoveryHeight = 5f;
    private bool isRecovering = false;

    public bool IsFriend { get; private set; }
    private Transform player;
    private CharacterController controller;
    private Vector3 smoothVelocity;
    private float verticalVelocity;
    private bool isJumping = false;

    public void SetFriendStatus(bool isFriend)
    {
        //新しく仲間になったか確認する
        bool becameFriend = !IsFriend && isFriend;
        IsFriend = isFriend;

        if(GameManager.Instance != null)
        {
            if(isFriend) GameManager.Instance.AddFriend(this);
            else GameManager.Instance.RemoveFriend(this);
        }

        //新しく仲間になったときだけ通知を表示
        if(becameFriend)
        {
            FriendUI friendUI = FindAnyObjectByType<FriendUI>();
            if(friendUI != null) friendUI.ShowNotification("ペンギンが仲間になったよ");
        }
    }

    //ペンギンを仲間から外し、指定した位置へ移動
    public void ReleaseAt(Vector3 position)
    {
        //仲間から外す
        SetFriendStatus(false);

        //追従・ジャンプの速度をリセット
        smoothVelocity = Vector3.zero;
        verticalVelocity = 0f;
        isJumping = false;

        //PenguinSpawnerを探す
        PenguinSpawner spawner = FindAnyObjectByType<PenguinSpawner>();

        if(spawner != null && spawner.TryFindSafePosition(position, out Vector3 safePosition))
        {
            //安全な場所が見つかった場合
            controller.enabled = false;
            transform.position = safePosition;
            controller.enabled = true;
        }
        else Debug.LogWarning("安全な再配置場所が見つからなかったよ");
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if(playerObject != null) player = playerObject.transform;
        else Debug.LogWarning("Playerが見つからないよ");

        if(GameManager.Instance != null) GameManager.Instance.RegisterPenguin(this);
    }

    private void Update()
    {
        CheckFall();
        
        if(isRecovering) return;
        if(player == null) return;
        if(!IsFriend)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if(distance <= recruitDistance) Recruit();
        }
        if(IsFriend) FollowPlayer();
    }

    private void Recruit()
    {
        if(GameManager.Instance == null) return;

        if(GameManager.Instance.Friends.Count >= 11)
        {
            Debug.Log("仲間は11匹までだよ");
            return;
        }

        //仲間状態を変更する
        SetFriendStatus(true);

        Debug.Log("ペンギンが仲間になったよ");
    }

    private void FollowPlayer()
    {
        //プレイヤーとの距離を確認
        if(GameManager.Instance == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position,player.position);
        Vector3 horizontalMove = Vector3.zero;

        //プレイヤーを追いかける
        if(distanceToPlayer > followStartDistance)
        {
            Vector3 direction = player.position - transform.position;
            direction.y = 0f;

            if(direction.magnitude > 0.01f)
            {
                Vector3 desiredVelocity = direction.normalized * followSpeed;
                smoothVelocity = Vector3.Lerp(smoothVelocity,desiredVelocity,Time.deltaTime * 3f);
                horizontalMove = smoothVelocity * Time.deltaTime;

                //移動方向に体を向ける
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation,targetRotation,Time.deltaTime * 8f);
            }
        }
        else smoothVelocity = Vector3.zero;

        //仲間同士が近すぎる場合は距離を取る
        Vector3 separation = Vector3.zero;
        foreach(PenguinController other in GameManager.Instance.Friends)
        {
            if(other == null || other == this) continue;

            Vector3 difference = transform.position - other.transform.position;
            difference.y = 0f;

            float distance = difference.magnitude;

            if(distance > 0.01f && distance < separationDistance) separation += difference.normalized * (separationDistance - distance);
        }

        if(separation.sqrMagnitude > 0.001f) horizontalMove += separation.normalized * separationStrength * Time.deltaTime;

        // 進行方向に障害物があるときだけジャンプする
        if(!isJumping && controller.isGrounded && smoothVelocity.sqrMagnitude > 0.1f && IsObstacleAhead())
        {
            verticalVelocity = jumpForce;
            isJumping = true;
            Debug.Log("ペンギンがジャンプを開始");
        }

        //一度ジャンプしたら着地するまで再発動しないように
        if(isJumping && controller.isGrounded && verticalVelocity <= 0f)
        {
            isJumping = false;
            Debug.Log("ジャンプ終了・着地したよ");
        }

        //重力を適用する
        if(controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 movement = horizontalMove + Vector3.up * verticalVelocity * Time.deltaTime;
        controller.Move(movement);

        //Debug.Log($"移動後 Grounded: {controller.isGrounded}, Jumping: {isJumping}, Y速度: {verticalVelocity:F2}");
    }

    private bool IsObstacleAhead()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if(direction.sqrMagnitude < 0.01f) return false;
        direction.Normalize();

        //JumpObstacleレイヤーだけを判定する
        int obstacleLayer = LayerMask.GetMask("JumpObstacle");
        Vector3 lowerOrigin = transform.position + Vector3.up * 0.2f; //低い位置から前方を調べる
        Vector3 upperOrigin = transform.position + Vector3.up * 0.7f; //高い位置から前方を調べる

        bool lowerHit = Physics.Raycast(lowerOrigin, direction, 3.0f, obstacleLayer,QueryTriggerInteraction.Ignore);
        bool upperHit = Physics.Raycast(upperOrigin, direction, 3.0f, obstacleLayer,QueryTriggerInteraction.Ignore);

        return lowerHit || upperHit;
    }

    //セーブデータからペンギンの状態を復元する
    public void LoadPenguinData(PenguinSaveData data)
    {
        if(data == null) return;

        //位置と向きを復元
        controller.enabled = false;
        transform.position = new Vector3(data.posX,data.posY,data.posZ);
        transform.rotation = Quaternion.Euler(data.rotX,data.rotY,data.rotZ);
        controller.enabled = true;

        //仲間状態を復元
        IsFriend = data.isFriend;

        //追従処理の速度をリセット
        smoothVelocity = Vector3.zero;
        verticalVelocity = 0f;
        isJumping = false;
    }

    //海に落ちたペンギンを復帰させる
    private void CheckFall()
    {
        //復帰中の場合
        if(isRecovering)
        {
            //重力を適用する
            verticalVelocity += gravity * Time.deltaTime;

            //下方向へ移動する
            Vector3 movement = Vector3.up * verticalVelocity * Time.deltaTime;
            controller.Move(movement);

            //地面に着地したら復帰完了
            if(controller.isGrounded && verticalVelocity <= 0f)
            {
                isRecovering = false;
                verticalVelocity = -2f;
                smoothVelocity = Vector3.zero;
                isJumping = false;
                Debug.Log("ペンギンが着地したよ");
            }
            return;
        }

        if(transform.position.y >= fallThreshold) return; //落下していなければ何もしない
        if(player == null) return; //プレイヤーが見つからなければ復帰できない

        //復帰開始
        isRecovering = true;

        //追従・ジャンプの速度をリセット
        verticalVelocity = 0f;
        smoothVelocity = Vector3.zero;
        isJumping = false;

        //プレイヤーの少し上に移動させる
        controller.enabled = false;
        transform.position = player.position + Vector3.up * recoveryHeight;
        controller.enabled = true;
        Debug.Log("ペンギンが上空から復帰するよ");
    }

    //プレイヤーが落下したとき一緒に復帰する
    public void ReturnWithPlayer(Vector3 position)
    {
        //復帰中の処理をリセット
        isRecovering = true;
        verticalVelocity = 0f;
        smoothVelocity = Vector3.zero;
        isJumping = false;

        //ペンギンごとに異なる方向へ配置する
        float angle = penguinId * 137.5f * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * 3f,
            0f,
            Mathf.Sin(angle) * 3f
        );

        //プレイヤーの周囲の上空に移動させる
        controller.enabled = false;

        transform.position = position + offset + Vector3.up * recoveryHeight;

        controller.enabled = true;

        Debug.Log("ペンギンがプレイヤーの周囲から復帰するよ");
    }

}