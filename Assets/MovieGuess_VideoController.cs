using System.Collections;
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
    public VideoClip fallbackVideoClip;
    public float timeoutSeconds = 5f;   // Tiempo máximo de espera para cargar el video remoto

    private bool videoLoaded = false;

    void Start()
    {
        PlayFallbackVideo();
    }

    public void PlayVideoFromURL(string url)
    {
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = url;

        videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // 🔇 Silenciar el video

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.errorReceived += OnVideoError;

        videoPlayer.Prepare();

        StartCoroutine(CheckVideoTimeout());
    }

    IEnumerator CheckVideoTimeout()
    {
        float timer = 0f;
        while (!videoLoaded && timer < timeoutSeconds)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!videoLoaded)
        {
            Debug.LogWarning("⏱️ Timeout al preparar el video. Usamos fallback.");
            PlayFallbackVideo();
        }
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        videoLoaded = true;
        rawImage.texture = vp.texture;
        vp.Play();
        Debug.Log("✅ Video cargado desde URL.");
    }

    void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogWarning("❌ Error cargando video por URL: " + message);
        PlayFallbackVideo();
    }

    void PlayFallbackVideo()
    {
        videoLoaded = true;

        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.errorReceived -= OnVideoError;

        videoPlayer.Stop();
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = fallbackVideoClip;

        videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // 🔇 También en fallback

        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnVideoPrepared;
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
