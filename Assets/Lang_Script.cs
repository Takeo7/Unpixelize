using UnityEngine;
using UnityEngine.UI;

public class Lang_Script : MonoBehaviour
{
    public Sprite es_img;
    public Sprite en_img;

    public Image butt_img;
    public GameObject selector_lang;
    public void ChangeLang(string s)
    {
        if (MovieGuess_Controller.MovieGuess_instance.ChangeLanguage())
        {
            switch (s)
            {
                case "es":
                    butt_img.sprite = es_img;
                    break;
                case "en":
                    butt_img.sprite = en_img;
                    break;
                default:
                    break;
            }
            selector_lang.SetActive(false);
        }
        
    }
}
