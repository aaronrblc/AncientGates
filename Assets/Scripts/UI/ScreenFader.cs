using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.5f;

    private CanvasGroup cg;

    private void Awake() => cg = GetComponent<CanvasGroup>();

    public void FadeOut(System.Action onComplete = null) => StartCoroutine(Fade(0f, 1f, onComplete));
    public void FadeIn(System.Action onComplete = null) => StartCoroutine(Fade(1f, 0f, onComplete));

    private IEnumerator Fade(float from, float to, System.Action onComplete)
    {
        float t = 0f;
        cg.alpha = from;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        cg.alpha = to;
        onComplete?.Invoke();
    }
}
