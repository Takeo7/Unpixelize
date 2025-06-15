using UnityEngine;
using UnityEngine.UI;

public class UnpixeliceAnimation : MonoBehaviour
{
    public Image unpixel_img;


    public void Unpixelice()
    {
        unpixel_img.fillAmount = unpixel_img.fillAmount - 0.2f;
    }
    public void Unpixelice(int count)
    {
        unpixel_img.fillAmount = unpixel_img.fillAmount - (0.2f * count);
    }
}
