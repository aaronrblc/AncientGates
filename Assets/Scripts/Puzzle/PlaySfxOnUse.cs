using UnityEngine;

public class PlaySfxOnUse : MonoBehaviour
{
    [SerializeField] private AudioClip clip;

    public void Play() => AudioManager.Instance.PlaySfx(clip);

    //hola
}
