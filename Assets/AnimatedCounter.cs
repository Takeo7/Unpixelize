using UnityEngine;
using TMPro;
using System.Collections;

public class AnimatedCounter : MonoBehaviour
{
    public TextMeshProUGUI pointsText;

    [Header("Velocidad")]
    public float minUpdateSpeed = 0.01f;    // Velocidad más rápida (menor delay)
    public float maxUpdateSpeed = 0.05f;    // Velocidad más lenta (mayor delay)
    public float speedCurvePower = 0.5f;    // Controla cómo escala la velocidad según la diferencia

    [Header("Colores")]
    public Color normalColor = Color.white;
    public Color increaseColor = Color.green;
    public Color decreaseColor = Color.red;

    private Coroutine animationCoroutine;

    public void SetPoints(int delta)
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        int currentPoints = int.TryParse(pointsText.text, out var value) ? value : 0;
        int targetPoints = currentPoints + delta;

        animationCoroutine = StartCoroutine(AnimatePoints(currentPoints, targetPoints, delta));
    }

    private IEnumerator AnimatePoints(int from, int to, int delta)
    {
        int direction = delta > 0 ? 1 : -1;
        pointsText.color = delta > 0 ? increaseColor : decreaseColor;

        int currentValue = from;
        int totalSteps = Mathf.Abs(delta);

        while (currentValue != to)
        {
            currentValue += direction;
            pointsText.text = currentValue.ToString();

            float t = 1f / Mathf.Pow(totalSteps, speedCurvePower);
            float stepDelay = Mathf.Clamp(t * maxUpdateSpeed, minUpdateSpeed, maxUpdateSpeed);

            yield return new WaitForSeconds(stepDelay);
        }

        pointsText.color = normalColor;
        animationCoroutine = null;
    }
}
