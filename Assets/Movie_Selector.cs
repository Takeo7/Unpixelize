using UnityEngine;
using UnityEngine.Video;
using System.Linq;
using System.IO;

public class Movie_Selector : MonoBehaviour
{
    public VideoPlayer videoPlayer; // Asigna el VideoPlayer desde el Inspector
    public string movieName; // Aquí se guardará el nombre extraído
    
    private void Start()
    {
        LoadVideoById(MovieID_Persistant.Movie_ID._movieID);
    }

    public void LoadVideoById(int id)
    {
        Debug.Log("Movie id: " + id);
        // Carga todos los videos en la carpeta "Resources/Movies"
        VideoClip[] videoFiles = Resources.LoadAll<VideoClip>("Movies");

        foreach (var file in videoFiles)
        {
            if (file.name.Contains($"_{id}"))
            {
                Debug.Log("Esta es la peli seleccionada: " + file.name);
                movieName = file.name.Split('_')[0]; // Extrae el nombre antes del ID
                videoPlayer.clip = file; // Asigna el video al VideoPlayer
                Debug.Log($"Video encontrado: {movieName} (Archivo: {file.name})");
                Level_Controller.Instance.SetMovieData();
                return;
            }
        }

        Debug.LogWarning("Video no encontrado.");

    }
}
