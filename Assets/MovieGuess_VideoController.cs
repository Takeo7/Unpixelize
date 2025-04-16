using UnityEngine;
using UnityEngine.UI;

public class MovieGuess_VideoController : MonoBehaviour
{
    public Image movie_img;

    public void UnPixelice(int incremental)
    {
        float pixelsize = movie_img.material.GetFloat("_PixelSize");
        movie_img.material.SetFloat("_PixelSize", pixelsize + incremental);
    }

    public void SetPixelice(int p)
    {
        movie_img.material.SetFloat("_PixelSize", p);
    }
}
