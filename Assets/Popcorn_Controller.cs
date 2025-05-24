using UnityEngine;
using TMPro;
using System.Collections;

public class Popcorn_Controller : MonoBehaviour
{
    [Header("Referencia al Texto de Popcorn")]
    public TextMeshProUGUI popcornText;
    [Space]
    public Color win_color;
    public Color lose_color;
    [Space]
    [Header("Segundos de permanencia")]
    [Range(0f, 5f)]
    public float seconds = 2f;

    private Coroutine hideCoroutine;


    public void ShowPopcornChange(int amount, bool isGain)
    {
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        string prefix = isGain ? "+" : "-";
        Color color = isGain ? win_color : lose_color;

        popcornText.text = prefix + amount.ToString();
        popcornText.color = color;
        popcornText.gameObject.SetActive(true);

        hideCoroutine = StartCoroutine(HideAfterSeconds(seconds));
    }

    private IEnumerator HideAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        popcornText.gameObject.SetActive(false);
    }
}
