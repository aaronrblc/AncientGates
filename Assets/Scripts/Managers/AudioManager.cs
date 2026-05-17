using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioMixer masterMixer;

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup ambienceGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    private AudioSource _bgmSource;
    private AudioSource _ambienceSource;
    private AudioSource _sfxSource;

    protected override void Awake()
    {
        base.Awake();
        _bgmSource      = AddSource(bgmGroup, loop: true);
        _ambienceSource = AddSource(ambienceGroup, loop: true);
        _sfxSource      = AddSource(sfxGroup, loop: false);
    }

    // --- BGM ---

    public void PlayBgm(AudioClip clip)
    {
        if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;
        _bgmSource.clip = clip;
        _bgmSource.Play();
    }

    public void StopBgm() => _bgmSource.Stop();

    // --- Ambience global ---

    public void PlayAmbience(AudioClip clip)
    {
        _ambienceSource.clip = clip;
        _ambienceSource.Play();
    }

    public void StopAmbience() => _ambienceSource.Stop();

    // --- SFX 2D (no posicional) ---

    public void PlaySfx(AudioClip clip) => _sfxSource.PlayOneShot(clip);

    // --- Volúmenes (0-1 normalizado) ---

    public void SetMasterVolume(float value)   => SetVolume("Master_Volume", value);
    public void SetBgmVolume(float value)      => SetVolume("BGM_Volume", value);
    public void SetAmbienceVolume(float value) => SetVolume("Ambience_Volume", value);
    public void SetSfxVolume(float value)      => SetVolume("SFX_Volume", value);

    // --- Internal ---

    private AudioSource AddSource(AudioMixerGroup group, bool loop)
    {
        var src = gameObject.AddComponent<AudioSource>();
        src.outputAudioMixerGroup = group;
        src.loop = loop;
        src.spatialBlend = 0f;
        src.playOnAwake = false;
        return src;
    }

    // Convierte 0-1 a decibelios (-80 dB .. 0 dB)
    private void SetVolume(string parameter, float normalized)
    {
        float db = normalized > 0.0001f ? Mathf.Log10(normalized) * 20f : -80f;
        masterMixer.SetFloat(parameter, db);
    }
}
