using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MovieGuess_VideoController : MonoBehaviour
{
    [Header("UI & Video")]
    public RawImage rawImage;
    public VideoPlayer videoPlayer;

    [Header("Render Texture Settings")]
    public int defaultWidth = 1280;
    public int defaultHeight = 720;
    public float scaleMultiplier;

    private bool videoLoaded = false;
    private RenderTexture renderTexture;
    private Coroutine currentPreparation;

    void Awake()
{
    Debug.Log("✅ MovieGuess scene started");
}

    


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
        //Debug.Log("🔍 Buscando video en Addressables: " + address);

        PrepareVideoPlayer();

        Addressables.LoadAssetAsync<VideoClip>(address).Completed += (AsyncOperationHandle<VideoClip> handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                //Debug.Log("✅ Video encontrado: " + address);
                videoPlayer.clip = handle.Result;
                currentPreparation = StartCoroutine(PrepareAndPlayUntilSuccess());
            }
            else
            {
                Debug.LogWarning("❌ No se encontró el video en Addressables: " + address);
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
        videoPlayer.isLooping = true;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.aspectRatio = VideoAspectRatio.FitInside;

        

        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(defaultWidth, defaultHeight, 0);
            renderTexture.Create();
        }

        videoPlayer.targetTexture = renderTexture;
        rawImage.texture = renderTexture;

        //Debug.Log("🎥 VideoPlayer preparado con RenderTexture " + renderTexture.width + "x" + renderTexture.height);
    }

    private void AdjustRawImageScaleToAspect(VideoClip clip, RawImage display)
    {
        float videoAspect = (float)clip.width / clip.height;
        float renderAspect = (float)defaultWidth / defaultHeight;

        RectTransform rt = display.rectTransform;
        if (videoAspect > renderAspect)
        {
            rt.sizeDelta = new Vector2(defaultWidth * scaleMultiplier, defaultHeight * (renderAspect / videoAspect) * scaleMultiplier);
        }
        else
        {
            rt.sizeDelta = new Vector2(defaultWidth * (videoAspect / renderAspect) * scaleMultiplier, defaultHeight * scaleMultiplier);
        }

        display.transform.position = new Vector2(display.transform.position.x, display.transform.position.y- 20);
    }

    private IEnumerator PrepareAndPlayUntilSuccess()
    {
        videoPlayer.Prepare();
        //Debug.Log("⌛ Esperando preparación del video...");

        float timeout = 5f;
        while (!videoPlayer.isPrepared && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (!videoPlayer.isPrepared)
        {
            Debug.LogError("❌ Error: el video no se preparó correctamente dentro del tiempo límite.");
            SetRawImageToRed();
            yield break;
        }

        //Debug.Log("🎬 Video preparado: " + videoPlayer.clip.name);

        videoLoaded = true;
        videoPlayer.Play();

        AdjustRawImageScale();
    }

    private void AdjustRawImageScale()
    {
        if (videoPlayer.clip == null || rawImage == null)
        {
            Debug.LogWarning("⚠️ No se pudo ajustar escala: video o imagen nulo.");
            return;
        }

        AdjustRawImageScaleToAspect(videoPlayer.clip, rawImage);

        //float videoAspect = (float)videoPlayer.clip.width / videoPlayer.clip.height;

    }

    private void SetRawImageToRed()
    {
        Texture2D redTex = new Texture2D(1, 1);
        redTex.SetPixel(0, 0, Color.red);
        redTex.Apply();
        rawImage.texture = redTex;
    }

    public void SetPixelice(int p)
    {
        if (rawImage.material != null && rawImage.material.HasProperty("_PixelSize"))
        {
            rawImage.material.SetFloat("_PixelSize", p);
            rawImage.SetMaterialDirty(); // Forzar redibujado
        }
    }
}
