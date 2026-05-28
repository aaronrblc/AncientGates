using UnityEngine;

public class LocalAxisBob : MonoBehaviour
{
    public float bobbingHeight = 0.08f;
    public float bobbingSpeed = 1.5f;
    public float rotationAmount = 0.8f;
    public bool randomOffset = true;
    public Vector2 randomRange = new Vector2(0.1f, 1f);

    private Vector3 startPos;
    private Vector3 startUp;
    private Quaternion startRotation;

    void Start()
    {
        startPos = transform.position;
        startRotation = transform.rotation;
        startUp = startRotation * Vector3.up;

        if (randomOffset)
        {
            bobbingSpeed += Random.Range(randomRange.x, randomRange.y);
            rotationAmount += Random.Range(randomRange.x, randomRange.y);
        }
    }

    void Update()
    {
        transform.position = startPos + startUp * (Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight);

        float rotationX = Mathf.Sin(Time.time * bobbingSpeed * 0.5f) * rotationAmount;
        float rotationY = Mathf.Sin(Time.time * bobbingSpeed * 0.7f) * rotationAmount;
        float rotationZ = Mathf.Sin(Time.time * bobbingSpeed * 0.9f) * rotationAmount;

        transform.rotation = startRotation * Quaternion.Euler(rotationX, rotationY, rotationZ);
    }
}
