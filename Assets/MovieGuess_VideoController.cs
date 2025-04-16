using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MovieGuess_VideoController : MonoBehaviour
{

    [Space]

    public RawImage rawImage;         // Solo si usas UI
    public VideoPlayer videoPlayer;

    [Space]
    // Aquí puedes poner la URL de tu servidor en producción más adelante
    public string videoURL = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";

    void Start()
    {
        //PlayVideoFromURL(videoURL);
    }

    public void PlayVideoFromURL(string url)
    {
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = url;

        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare(); // Prepara el video antes de reproducirlo
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        rawImage.texture = vp.texture;
        vp.Play();
    }

    public void UnPixelice(int incremental)
    {
        float pixelsize = rawImage.material.GetFloat("_PixelSize");
        rawImage.material.SetFloat("_PixelSize", pixelsize + incremental);
    }

    public void SetPixelice(int p)
    {
        rawImage.material.SetFloat("_PixelSize", p);
    }
}
