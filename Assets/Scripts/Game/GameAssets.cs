using UnityEngine;

public class GameAssets : MonoBehaviour
{
    #region Assets
    public Sprite PipeHeadSprite;
    public Transform PipeHead;
    public Transform PipeBody;
    public Transform Ground;
    public Transform Clouds1;
    public Transform Clouds2;
    public Transform Clouds3;
    #endregion Assets

    public SoundAudioClip[] SoundAudioClips;

    private static GameAssets _instance;

    public static GameAssets GetInstance() => _instance;
    
    private void Awake()
    {
        _instance = this;
    }
}
