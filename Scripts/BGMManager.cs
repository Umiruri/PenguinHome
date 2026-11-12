//タイトル画面BGM

using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    private static BGMManager instance;
    private AudioSource audioSource;

    private void Awake()
    {
        //別のBGMManagerが存在する場合は削除
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //シーンが読み込まれたとき
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "TitleScene") PlayTitleBGM();
    }

    //タイトルBGMを再生
    public void PlayTitleBGM()
    {
        if(audioSource != null && !audioSource.isPlaying) audioSource.Play();
    }

    //タイトルBGMを停止
    public void StopTitleBGM()
    {
        if(audioSource != null) audioSource.Stop();
    }

}