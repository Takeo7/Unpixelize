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
    private bool videoLoaded = false;
    private RenderTexture renderTexture;
    private Coroutine currentPreparation;

    void OnDestroy()
    {
        if (currentPreparation != null)
        {
            StopCoroutine(currentPreparation);
        }

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
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
                currentPreparation = StartCoroutine(PrepareAndPlayUntilSuccess());
            }
            else
            {
                Debug.LogWarning("❌ No se encontró el video: " + address + ". Mostramos fallback visual rojo.");
                SetRawImageToRed();
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
        videoPlayer.isLooping = true; // 🔁 Repetir video

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

    private IEnumerator PrepareAndPlayUntilSuccess()
    {
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        Debug.Log($"🎬 Video preparado correctamente: {videoPlayer.clip?.name}");
        videoLoaded = true;
        videoPlayer.Play();
    }

    private void SetRawImageToRed()
    {
        Texture2D redTex = new Texture2D(1, 1);
        redTex.SetPixel(0, 0, Color.red);
        redTex.Apply();
        rawImage.texture = redTex;
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
