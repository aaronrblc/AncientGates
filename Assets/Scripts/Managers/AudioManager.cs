using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    protected override void Awake() => base.Awake();

    public void PlaySfx(string soundId) { /* TODO */ }
    public void PlayBgm(string soundId) { /* TODO */ }
    public void StopAllBgm() { /* TODO */ }
}
