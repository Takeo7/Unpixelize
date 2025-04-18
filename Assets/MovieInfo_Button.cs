using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class MovieInfo_Button : MonoBehaviour
{
    public int movie_id = 0;
    public bool solved = false;
    public string movie_img;
    [Space]
    public Scene_Controller sc;
    [Space]
    public Button movie_bt;

    [Space]
    [Space]

    public Sprite default_img;
   

    public void LoadMovieID(int id, bool sol, string url)
    {
        Debug.Log("Load Movie Info: " + id);
        movie_id = id;

        solved = sol;

        movie_img = url;

        UpdateMovie();
    }

    public void UpdateMovie()
    {
        StartCoroutine(CargarImagenDesdeURL(movie_img));

        if (solved)
        {
            ColorBlock cb = movie_bt.colors;
            cb.normalColor = Color.green;
            movie_bt.colors = cb;
        }

    }

    public void EnterMovie()
    {
        PlayerInfoController.Player_Instance.currentMovie = movie_id;

        sc.ChangeScene(Scene_Controller.Scenes.MovieGuess);
    }


    IEnumerator CargarImagenDesdeURL(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al cargar imagen: " + request.error);
            Debug.LogError("Tipo de contenido: " + request.GetResponseHeader("Content-Type"));
            movie_bt.image.sprite = default_img;
        }
        else
        {
            Texture2D textura = DownloadHandlerTexture.GetContent(request);
            Sprite sprite = Sprite.Create(textura, new Rect(0, 0, textura.width, textura.height), new Vector2(0.5f, 0.5f));
            movie_bt.image.sprite = sprite;
        }
    }
}
