using System.Collections;
using UnityEngine;

/// <summary>
/// Handles camera shake effects for impact and explosion events
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Coroutine currentShake;
    private Vector3 originalPos;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Initiates a camera shake effect with specified duration and intensity
    /// </summary>
    /// <param name="duration">How long the shake should last</param>
    /// <param name="magnitude">How intense the shake should be</param>
    public void ShakeCamera(float duration, float magnitude)
    {
        if (currentShake != null)
        {
            StopCoroutine(currentShake);
            transform.localPosition = originalPos;
        }
        currentShake = StartCoroutine(Shake(duration, magnitude));
    }

    /// <summary>
    /// Coroutine that handles the camera shake animation
    /// </summary>
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