using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Coroutine currentShake;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShakeCamera(float duration, float magnitude)
    {
        if (currentShake != null)
        {
            StopCoroutine(currentShake);
            transform.localPosition = originalPos;
        }
        currentShake = StartCoroutine(Shake(duration, magnitude));
    }

    private Vector3 originalPos;

    private IEnumerator Shake(float duration, float magnitude)
    {
        originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        currentShake = null;
    }
}