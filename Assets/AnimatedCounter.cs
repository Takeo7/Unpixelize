using UnityEngine;
using TMPro;
using System.Collections;
using System;

public class AnimatedCounter : MonoBehaviour
{
    public TextMeshProUGUI pointsText;

    [Header("Velocidad")]
    public float minUpdateSpeed = 0.01f;    // Velocidad m�s r�pida (menor delay)
    public float maxUpdateSpeed = 0.05f;    // Velocidad m�s lenta (mayor delay)
    public float speedCurvePower = 0.5f;    // Controla c�mo escala la velocidad seg�n la diferencia

    [Header("Colores")]
    public Color normalColor = Color.white;
    public Color increaseColor = Color.green;
    public Color decreaseColor = Color.red;

    private Coroutine animationCoroutine;

    public void SetPoints(int amount)
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        int currentPoints = int.TryParse(pointsText.text, out var value) ? value : 0;
        int targetPoints = amount;

        animationCoroutine = StartCoroutine(AnimatePoints(currentPoints, targetPoints));
    }

    public void SetDailyReward(int daily)
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        int currentPoints = (int.TryParse(pointsText.text, out var value) ? value : 0) - daily;
        int targetPoints = currentPoints + daily;

        animationCoroutine = StartCoroutine(AnimatePoints(currentPoints, targetPoints));
    }

    private IEnumerator AnimatePoints(int from, int to)
    {

        int delta = to - from;
        int direction = delta > 0 ? 1 : -1;
        pointsText.color = delta > 0 ? increaseColor : decreaseColor;

        int currentValue = from;
        int totalSteps = Mathf.Abs(delta);

        Debug.Log("From: " + from + "-- to: " + to);

        while (currentValue != to)
        {
            currentValue += direction;
            pointsText.text = currentValue.ToString();

            float t = 1f / Mathf.Pow(totalSteps, speedCurvePower);
            float stepDelay = Mathf.Clamp(t * maxUpdateSpeed, minUpdateSpeed, maxUpdateSpeed);

            yield return new WaitForSeconds(stepDelay);
        }

        if (to <= 0)
        {
            Debug.Log("TU AMOUNT ES 0");
            pointsText.color = decreaseColor;
        }
        else
        {
            pointsText.color = normalColor;
        }

        animationCoroutine = null;
    }
}
