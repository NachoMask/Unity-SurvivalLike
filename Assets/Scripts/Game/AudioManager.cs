using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("# BGM")]
    [SerializeField] private AudioClip bgmClip;
    [SerializeField, Min(0f)] private float bgmVolume;
    private AudioSource bgmPlayer;

    [Header("# SFX")]
    [SerializeField] private AudioClip[] sfxClips;
    [SerializeField, Min(0f)] private float sfxVolume;
    [SerializeField] private int channels;
    private AudioSource[] sfxPlayers;
    private int channelIndex;

    public enum Sfx
    {
        Arrow,
        GameClear,
        GameOver,
        HitEnemy,
        HitPlayer,
        Lance,
        LevelUp,
        Select
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (!TryValidateSettings(out string error))
        {
            Debug.LogError($"{nameof(AudioManager)} {name} : {error}");
            enabled = false;
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Init();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Init()
    {
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        bgmPlayer.clip = bgmClip;

        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];
        for (int i = 0; i < sfxPlayers.Length; ++i)
        {
            sfxPlayers[i] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[i].playOnAwake = false;
            sfxPlayers[i].volume = sfxVolume;
        }
    }

    public void PlaySfx(Sfx sfx)
    {
        if (!System.Enum.IsDefined(typeof(Sfx), sfx))
            throw new System.ArgumentOutOfRangeException(nameof(sfx));

        for (int i = 0; i < sfxPlayers.Length; ++i)
        {
            int loopIndex = (channelIndex + i) % sfxPlayers.Length;

            if (sfxPlayers[loopIndex].isPlaying) continue;

            channelIndex = loopIndex;
            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx];
            sfxPlayers[loopIndex].Play();
            break;
        }
    }

    public void PlayBgm()
    {
        bgmPlayer.Play();
    }

    public void StopBgm()
    {
        bgmPlayer.Stop();
    }

    private bool TryValidateSettings(out string error)
    {
        if (bgmClip == null)
        {
            error = $"{nameof(bgmClip)} is Invalid.";
            return false;
        }
        if (bgmVolume < 0f || bgmVolume > 1f)
        {
            error = $"{nameof(bgmVolume)} must be between 0 and 1.";
            return false;
        }

        if (sfxClips == null)
        {
            error = $"{nameof(sfxClips)} is Invalid.";
            return false;
        }
        for (int i = 0; i < sfxClips.Length; ++i)
        {
            if (sfxClips[i] == null)
            {
                error = $"{nameof(sfxClips)}[{i}] is Invalid.";
                return false;
            }
        }

        int sfxEnumLength = System.Enum.GetValues(typeof(Sfx)).Length;

        if (sfxClips.Length < sfxEnumLength)
        {
            error = $"{nameof(sfxClips)}'s Length must be at least SfxEnum's Length: {sfxEnumLength}";
            return false;
        }
        if (sfxVolume < 0f || sfxVolume > 1f)
        {
            error = $"{nameof(sfxVolume)} must be between 0 and 1.";
            return false;
        }
        if (channels <= 0)
        {
            error = $"{nameof(channels)} must be greater than zero";
            return false;
        }
        
        error = null;
        return true;
    }
}
