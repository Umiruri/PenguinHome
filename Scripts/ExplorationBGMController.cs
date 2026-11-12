//イベントBGM・探索中のBGM・結果画面のBGM

using UnityEngine;

public class ExplorationBGMController : MonoBehaviour
{
    [Header("BGM")]
    [SerializeField] private AudioSource explorationBGM;
    [SerializeField] private AudioSource eventBGM;
    [SerializeField] private AudioSource resultBGM;

    private void Start()
    {
        BGMManager titleBGM = FindAnyObjectByType<BGMManager>();
        if(titleBGM != null) titleBGM.StopTitleBGM();
        PlayExplorationBGM();
    }

    //探索BGMを再生する
    public void PlayExplorationBGM()
    {
        StopAllBGM();
        if(explorationBGM != null) explorationBGM.Play();
    }

    //イベントBGMを再生する
    public void PlayEventBGM()
    {
        StopAllBGM();
        if(eventBGM != null) eventBGM.Play();
    }

    // リザルトBGMを再生する
    public void PlayResultBGM()
    {
        StopAllBGM();
        if(resultBGM != null) resultBGM.Play();
    }

    //すべてのBGMを停止する
    private void StopAllBGM()
    {
        if(explorationBGM != null) explorationBGM.Stop();
        if(eventBGM != null) eventBGM.Stop();
        if(resultBGM != null) resultBGM.Stop();
    }
    
}