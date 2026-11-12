//地面が見つかった場合だけ、その場所にペンギンを生成

using UnityEngine;

public class PenguinSpawner : MonoBehaviour
{
    [Header("ペンギン設定")]
    public GameObject penguinPrefab;
    public int penguinCount = 5;

    [Header("配置範囲")]
    public float areaWidth = 45f;
    public float areaDepth = 45f;

    [Header("地面設定")]
    public LayerMask groundLayer;
    public float rayStartHeight = 50f;
    public float spawnHeightOffset = 0.5f;

    [Header("Home周辺の出現禁止設定")]
    public Transform home;
    public float homeSafeRadius = 5f;

    [Header("配置チェック")]
    public float checkRadius = 0.8f;
    public LayerMask obstacleLayer;

    private void Start()
    {
        if(GameData.LoadedSaveData != null) SpawnSavedPenguins(GameData.LoadedSaveData);
        else SpawnPenguins();
    }

    public void SpawnPenguins()
    {
        if(penguinPrefab == null)
        {
            Debug.LogError("ペンギンのPrefabが設定されていないよ");
            return;
        }

        int spawnedCount = 0;
        int attempts = 0;
        int maxAttempts = penguinCount * 20;

        while(spawnedCount < penguinCount && attempts < maxAttempts)
        {
            attempts++;

            float x = Random.Range(-areaWidth / 2f, areaWidth / 2f);
            float z = Random.Range(-areaDepth / 2f, areaDepth / 2f);

            Vector3 rayOrigin = transform.position + new Vector3(x, rayStartHeight, z);

            if(!Physics.Raycast(rayOrigin,Vector3.down,out RaycastHit hit,rayStartHeight * 2f,groundLayer)) continue;
            Vector3 spawnPosition = hit.point + Vector3.up * spawnHeightOffset;

            //氷などの障害物と重なる場所には生成しない
            if(Physics.CheckSphere(spawnPosition + Vector3.up * checkRadius,checkRadius,obstacleLayer,QueryTriggerInteraction.Ignore)) continue;

            //Homeからの水平距離を確認する
            if(home != null)
            {
                Vector2 spawnXZ = new Vector2(spawnPosition.x, spawnPosition.z);
                Vector2 homeXZ = new Vector2(home.position.x, home.position.z);

                if(Vector2.Distance(spawnXZ, homeXZ) < homeSafeRadius) continue; 
            }

            GameObject penguinObject = Instantiate(penguinPrefab,spawnPosition,Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));

            //生成したペンギンに個別のIDを設定する
            PenguinController penguin = penguinObject.GetComponent<PenguinController>();
            if(penguin != null) penguin.penguinId = spawnedCount + 1;
            spawnedCount++;
        }
    }

    //既存と同じ配置範囲・地面判定・Home周辺の禁止範囲を使って、新しいペンギンを生成
    //生成直後に仲間状態にしてプレイヤーに追従するように
    //イベント成功時に新しいペンギンをプレイヤーの頭上へ生成
    public PenguinController SpawnNewFriend()
    {
        if(penguinPrefab == null)
        {
            Debug.LogError("ペンギンのPrefabが設定されていないよ");
            return null;
        }

        GameManager gm = GameManager.Instance;

        if(gm == null || gm.AllPenguins.Count >= 11)
        {
            Debug.Log("ペンギンはこれ以上増やせないよ");
            return null;
        }

        if(gm.Friends.Count >= 11)
        {
            Debug.Log("仲間は11匹までだよ");
            return null;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if(playerObject == null)
        {
            Debug.LogError("Playerが見つからないよ");
            return null;
        }

        //既存ペンギンと重複しないIDを決める
        int newId = 1;
        foreach(PenguinController p in gm.AllPenguins)
        {
            if(p != null && p.penguinId >= newId) newId = p.penguinId + 1;
        }

        //プレイヤーの5メートル上に生成
        Vector3 spawnPosition = playerObject.transform.position + Vector3.up * 5f;
        GameObject obj = Instantiate(penguinPrefab,spawnPosition,Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        PenguinController penguin = obj.GetComponent<PenguinController>();

        if(penguin == null)
        {
            Destroy(obj);
            Debug.LogError("PrefabにPenguinControllerがないよ");
            return null;
        }

        penguin.penguinId = newId;

        //GameManagerに登録して仲間にする
        gm.RegisterPenguin(penguin);
        penguin.SetFriendStatus(true);
        Debug.Log("新しいペンギンが仲間になったよ ID=" + newId);

        return penguin;
    }

    public void SpawnSavedPenguins(SaveData data)
    {
        if(data == null || data.penguins == null)
        {
            Debug.LogWarning("ペンギンの保存データが見つからないよ");
            return;
        }

        if(penguinPrefab == null)
        {
            Debug.LogError("ペンギンのPrefabが設定されていないよ");
            return;
        }

        GameManager gameManager = GameManager.Instance;

        if(gameManager == null)
        {
            Debug.LogError("GameManagerが見つからないよ");
            return;
        }

        //保存データにあるペンギンを順番に生成する
        foreach(PenguinSaveData penguinData in data.penguins)
        {
            if(penguinData == null) continue;
        
            Vector3 savedPosition = new Vector3(penguinData.posX,penguinData.posY,penguinData.posZ);
            Quaternion savedRotation = Quaternion.Euler(penguinData.rotX,penguinData.rotY,penguinData.rotZ);
            GameObject penguinObject = Instantiate(penguinPrefab,savedPosition,savedRotation);
            PenguinController penguin = penguinObject.GetComponent<PenguinController>();

            if(penguin == null)
            {
                Debug.LogError("PrefabにPenguinControllerがないよ");
                Destroy(penguinObject);
                continue;
            }

            penguin.penguinId = penguinData.penguinId; //保存データと同じIDを設定する
            penguin.LoadPenguinData(penguinData); //保存した位置・向き・仲間状態を復元する
            gameManager.RegisterPenguin(penguin); //GameManagerに登録する

            //仲間だったペンギンを仲間リストにも登録する
            if(penguinData.isFriend) gameManager.AddFriend(penguin);
        }

        //復元が終わったため、読み込み用データをクリアする
        GameData.LoadedSaveData = null;
        Debug.Log("保存したペンギンの復元が完了したよ");
    }

    //指定位置の周辺から安全な再配置場所を探す
    public bool TryFindSafePosition(Vector3 targetPosition, out Vector3 safePosition)
    {
        safePosition = targetPosition;

        for(int i = 0; i < 20; i++)
        {
            Vector3 checkPosition = targetPosition;

            //2回目以降は少しずつ周囲の位置を調べる
            if(i > 0)
            {
                Vector2 offset = Random.insideUnitCircle * 3f;
                checkPosition += new Vector3(offset.x, 0f, offset.y);
            }

            Vector3 rayOrigin = checkPosition + Vector3.up * rayStartHeight;

            //GroundまたはJumpObstacleの地面を探す
            if(!Physics.Raycast(rayOrigin,Vector3.down,out RaycastHit hit,rayStartHeight * 2f,groundLayer,QueryTriggerInteraction.Ignore)) continue;

            Vector3 candidate = hit.point + Vector3.up * spawnHeightOffset;

            //氷などの障害物と重ならないか確認
            if(Physics.CheckSphere(candidate + Vector3.up * checkRadius,checkRadius,obstacleLayer,QueryTriggerInteraction.Ignore)) continue;

            safePosition = candidate;
            return true;
        }
        return false;
    }

}