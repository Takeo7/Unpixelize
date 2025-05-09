using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;

public class MovieInfo_Button : MonoBehaviour
{
    public int movie_id = 0;
    public bool solved = false;
    public string movie_video_path;

    [Space]
    public Scene_Controller sc;
    public Button movie_bt;

    [Header("Visual Settings")]
    public RawImage videoDisplay;
    public Vector2 renderTextureSize = new Vector2(512, 512);
    public Material videoMaterial;
    public float scaleMultiplier = 1.2f;

    private VideoPlayer videoPlayer;

    public void LoadMovieID(int id, bool sol, string pathToVideo)
    {
        movie_id = id;
        solved = sol;
        movie_video_path = pathToVideo;

        string videoKey = GetVideoKeyFromPath(pathToVideo);
        UpdateMovie(videoKey);
    }

    private void UpdateMovie(string videoKey)
    {
        SetupVideoDisplay();
        LoadAndPrepareVideo(videoKey);

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

    private void SetupVideoDisplay()
    {
        if (videoDisplay == null)
        {
            GameObject displayGO = new GameObject("VideoPreview", typeof(RawImage));
            displayGO.transform.SetParent(this.transform, false);
            videoDisplay = displayGO.GetComponent<RawImage>();

            RectTransform rt = videoDisplay.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            // Aplicar RectMask2D como máscara
            if (this.GetComponent<RectMask2D>() == null)
                this.gameObject.AddComponent<RectMask2D>();

            if (this.GetComponent<Image>() == null)
            {
                Image bg = this.gameObject.AddComponent<Image>();
                bg.color = Color.black; // El color puede ser oscuro para enmascarar bien
            }
        }

        if (videoMaterial != null)
        {
            videoDisplay.material = videoMaterial;
        }

        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            videoPlayer.source = VideoSource.VideoClip;
        }

        RenderTexture rtTex = new RenderTexture((int)renderTextureSize.x, (int)renderTextureSize.y, 0);
        rtTex.Create();
        videoPlayer.targetTexture = rtTex;
        videoDisplay.texture = rtTex;
    }

    private void LoadAndPrepareVideo(string address)
    {
        Debug.Log("🔍 Buscando video en Addressables: " + address);

        Addressables.LoadAssetAsync<VideoClip>(address).Completed += (AsyncOperationHandle<VideoClip> handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                videoPlayer.clip = handle.Result;
                StartCoroutine(PrepareAndDisplayFirstFrame());
            }
            else
            {
                Debug.LogWarning($"❌ No se encontró el video '{address}' para Movie ID {movie_id}.");
                SetRawImageColor(Color.red);
            }
        };
    }

    private IEnumerator PrepareAndDisplayFirstFrame()
    {
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
            yield return null;

        videoPlayer.Play();
        videoPlayer.Pause();

        Debug.Log($"🖼️ Primer frame preparado: {videoPlayer.clip?.name}");

        AdjustRawImageScaleToAspect(videoPlayer.clip);

        if (solved)
        {
            SetGreenBorder(true);
        }
    }

    private void AdjustRawImageScaleToAspect(VideoClip clip)
    {
        float videoAspect = (float)clip.width / clip.height;
        float renderAspect = renderTextureSize.x / renderTextureSize.y;

        RectTransform rt = videoDisplay.rectTransform;
        if (videoAspect > renderAspect)
        {
            rt.sizeDelta = new Vector2(renderTextureSize.x * scaleMultiplier, renderTextureSize.y * (renderAspect / videoAspect) * scaleMultiplier);
        }
        else
        {
            rt.sizeDelta = new Vector2(renderTextureSize.x * (videoAspect / renderAspect) * scaleMultiplier, renderTextureSize.y * scaleMultiplier);
        }
    }

    public void SetGreenBorder(bool enabled)
    {
        Outline outline = videoDisplay.GetComponentInParent<Outline>();

        if (enabled)
        {
            if (outline == null)
                outline = videoDisplay.GetComponentInParent<Outline>();

            outline.effectColor = Color.green;
            outline.effectDistance = new Vector2(5, 5);
        }
        else
        {
            if (outline != null)
                Destroy(outline);
        }
    }

    private string GetVideoKeyFromPath(string pathToVideo)
    {
        if (string.IsNullOrEmpty(pathToVideo))
            return string.Empty;

        string filename = System.IO.Path.GetFileNameWithoutExtension(pathToVideo);
        return $"Videos/{RemoveVoidsAndLower(filename)}";
    }

    public string RemoveVoidsAndLower(string s)
    {
        return s.Replace(" ", "").ToLower();
    }

    private void SetRawImageColor(Color color)
    {
        Texture2D fallbackTexture = new Texture2D(1, 1);
        fallbackTexture.SetPixel(0, 0, color);
        fallbackTexture.Apply();
        videoDisplay.texture = fallbackTexture;
        videoDisplay.material = null;
    }
}
