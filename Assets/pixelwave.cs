using UnityEngine;

public class pixelwave : MonoBehaviour
{
    public Material pixelRevealMat;

    void Update()
    {
        float progress = Mathf.Clamp01(Time.time * 0.2f); // avanza progresivamente
        pixelRevealMat.SetFloat("_RevealProgress", progress);
    }

}
