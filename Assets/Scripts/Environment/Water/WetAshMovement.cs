using UnityEngine;

public class WetAshMovement : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;

    [Header("Texture Movement")]
    [SerializeField] private float speedX = 0.04f;
    [SerializeField] private float speedY = 0.015f;

    [Header("Surface Motion")]
    [SerializeField] private float waveAmplitude = 0.02f;
    [SerializeField] private float waveSpeed = 1.2f;

    [Header("Tiling Pulse")]
    [SerializeField] private Vector2 baseTiling = new Vector2(1f, 1f);
    [SerializeField] private float tilingPulse = 0.04f;
    [SerializeField] private float tilingSpeed = 0.6f;

    private Material materialInstance;
    private Vector2 offset;
    private Vector3 startLocalPosition;

    private void Start()
    {
        materialInstance = targetRenderer.material;
        startLocalPosition = transform.localPosition;
        baseTiling = materialInstance.mainTextureScale;
    }

    private void Update()
    {
        offset.x += speedX * Time.deltaTime;
        offset.y += speedY * Time.deltaTime;
        materialInstance.mainTextureOffset = offset;

        // Pulso de tiling para sensación de profundidad
        float pulse = 1f + Mathf.Sin(Time.time * tilingSpeed) * tilingPulse;
        materialInstance.mainTextureScale = baseTiling * pulse;

        // Suma de ondas con frecuencias distintas — movimiento más orgánico
        float wave =
            Mathf.Sin(Time.time * waveSpeed)               * waveAmplitude +
            Mathf.Sin(Time.time * waveSpeed * 1.7f + 0.8f) * waveAmplitude * 0.35f +
            Mathf.Sin(Time.time * waveSpeed * 2.5f + 1.5f) * waveAmplitude * 0.15f;

        transform.localPosition = startLocalPosition + transform.up * wave;
    }
}