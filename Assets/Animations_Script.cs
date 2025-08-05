using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animations_Script : MonoBehaviour
{


    [Header("Objetos a popear")]
    public GameObject[] objectsToPop;

    [Header("Configuración del efecto")]
    public float popScale = 1.2f;
    public float popDuration = 0.2f;
    public float delayBetweenEach = 0.05f; // opcional: para hacer un pop escalonado

    public void StartPopping()
    {
        StartCoroutine(PlayPopEffectOnAll());
    }

    IEnumerator PlayPopEffectOnAll()
    {
        foreach (GameObject obj in objectsToPop)
        {
            StartCoroutine(PopEffect(obj.transform));
            yield return new WaitForSeconds(delayBetweenEach); // opcional
        }
    }

    IEnumerator PopEffect(Transform target)
    {
        Vector3 originalScale = target.localScale;

        // Escala hacia arriba
        float timer = 0;
        while (timer < popDuration)
        {
            target.localScale = Vector3.Lerp(originalScale, originalScale * popScale, timer / popDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        // Escala hacia abajo
        timer = 0;
        while (timer < popDuration)
        {
            target.localScale = Vector3.Lerp(originalScale * popScale, originalScale, timer / popDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        target.localScale = originalScale;
    }
}
