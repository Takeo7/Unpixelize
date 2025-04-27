using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MovieGuess_VideoController : MonoBehaviour
{
    [Space]
    public RawImage rawImage;
    public VideoPlayer videoPlayer;

    [Space]
    public AssetReference fallbackVideoReference; // Fallback como Addressable
    public float timeoutSeconds = 5f;

    private bool videoLoaded = false;
    private RenderTexture renderTexture;

    void Start()
    {
        PlayFallbackVideo();
    }

    public void PlayVideoFromAddressables(string address)
    {
        Debug.Log("🔍 Buscando video en Addressables: " + address);

        PrepareVideoPlayer();

        Addressables.LoadAssetAsync<VideoClip>(address).Completed += (AsyncOperationHandle<VideoClip> handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("✅ Video encontrado y cargado: " + address);
                videoPlayer.clip = handle.Result;
                StartCoroutine(PrepareAndPlay());
            }
            else
            {
                Debug.LogWarning("❌ No se encontró el video: " + address + ". Usamos fallback.");
                PlayFallbackVideo();
            }
        };
    }

    private void PrepareVideoPlayer()
    {
        if (videoPlayer != null)
        {
            Destroy(videoPlayer);
        }

        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(512, 512, 0);
            renderTexture.Create();
        }

        videoPlayer.targetTexture = renderTexture;
        rawImage.texture = renderTexture;

        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.source = VideoSource.VideoClip;
    }

    private IEnumerator PrepareAndPlay()
    {
        videoPlayer.Prepare();

        float timeout = timeoutSeconds;
        while (!videoPlayer.isPrepared && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (videoPlayer.isPrepared)
        {
            Debug.Log($"🎬 Video preparado correctamente: {videoPlayer.clip?.name}");

            videoLoaded = true;
            videoPlayer.Play();
        }
        else
        {
            Debug.LogError("❌ No se pudo preparar el VideoClip. Usamos fallback.");
            PlayFallbackVideo();
        }
    }

    public void PlayFallbackVideo()
    {
        Debug.Log("⚠️ Activamos fallback video.");

        PrepareVideoPlayer();

        fallbackVideoReference.LoadAssetAsync<VideoClip>().Completed += (AsyncOperationHandle<VideoClip> handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                videoPlayer.clip = handle.Result;
                StartCoroutine(PrepareAndPlay());
            }
            else
            {
                Debug.LogError("❌ No se pudo cargar el fallback video.");
            }
        };
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
