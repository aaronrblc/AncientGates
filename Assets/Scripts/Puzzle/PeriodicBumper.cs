using System.Collections;
using UnityEngine;

public class PeriodicBumper : MonoBehaviour
{
    [SerializeField] private Vector3 bumpOffset = Vector3.up * 0.05f;
    [SerializeField] private float bumpDuration = 0.12f;
    [SerializeField] private float interval = 1.2f;
    [SerializeField] private float intervalVariation = 0.25f;
    [SerializeField] private bool playOnEnable = true;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bumpClip;

    private Vector3 _origin;
    private Coroutine _loop;

    private void Awake() => _origin = transform.localPosition;

    private void OnEnable()  { if (playOnEnable) StartBumping(); }
    private void OnDisable() => StopBumping();

    public void StartBumping()
    {
        if (_loop != null) StopCoroutine(_loop);
        _loop = StartCoroutine(BumpLoop());
    }

    public void StopBumping()
    {
        if (_loop != null) StopCoroutine(_loop);
        _loop = null;
        transform.localPosition = _origin;
    }

    private IEnumerator BumpLoop()
    {
        while (true)
        {
            yield return StartCoroutine(Bump());
            float wait = interval + Random.Range(-intervalVariation, intervalVariation);
            yield return new WaitForSeconds(Mathf.Max(0f, wait));
        }
    }

    private IEnumerator Bump()
    {
        Debug.Log($"[PeriodicBumper] source={audioSource} clip={bumpClip}");
        if (audioSource != null && bumpClip != null)
            audioSource.PlayOneShot(bumpClip);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / bumpDuration;
            float strength = Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI);
            transform.localPosition = _origin + bumpOffset * strength;
            yield return null;
        }
        transform.localPosition = _origin;
    }
}
