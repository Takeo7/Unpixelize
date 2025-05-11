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
    public Movie_Buttons_Controller mbc;

    [Space]
    public GameObject solved_img;
    public GameObject mask_img;
    public GameObject shadow_img;

    [Header("Visual Settings")]
    public RawImage videoDisplay;
    public Vector2Int renderTextureSize = new Vector2Int(512, 512);
    public float scaleMultiplier = 1.2f;
    public int pixelBlockSize = 8;

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
            displayGO.transform.SetParent(mask_img.transform, false);
            displayGO.transform.SetSiblingIndex(0);
            videoDisplay = displayGO.GetComponent<RawImage>();

            RectTransform rt = videoDisplay.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            // Aplicar máscara
            if (mask_img.GetComponent<Mask>() == null)
                mask_img.gameObject.AddComponent<Mask>();

            if (this.GetComponent<Image>() == null)
            {
                Image bg = this.gameObject.AddComponent<Image>();
                bg.color = Color.black;
            }
        }

        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            videoPlayer.source = VideoSource.VideoClip;
        }

        RenderTexture rtTex = new RenderTexture(renderTextureSize.x, renderTextureSize.y, 0);
        rtTex.Create();
        videoPlayer.targetTexture = rtTex;
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
        yield return new WaitForSeconds(0.1f); // Asegura que un frame se ha renderizado

        Texture2D snapshot = new Texture2D(renderTextureSize.x, renderTextureSize.y, TextureFormat.RGB24, false);
        RenderTexture.active = videoPlayer.targetTexture;
        snapshot.ReadPixels(new Rect(0, 0, renderTextureSize.x, renderTextureSize.y), 0, 0);
        snapshot.Apply();
        RenderTexture.active = null;

        videoPlayer.Pause();

        Texture2D pixelated = PixelateTexture(snapshot, pixelBlockSize);
        videoDisplay.texture = pixelated;
        videoDisplay.material = null;

        AdjustRawImageScaleToAspect(videoPlayer.clip);

        if (solved)
        {
            //SetGreenBorder(true);
            solved_img.SetActive(true);
            shadow_img.SetActive(false);
        }

        mbc.MovieLoaded();
    }

    private Texture2D PixelateTexture(Texture2D original, int blockSize)
    {
        int width = original.width;
        int height = original.height;
        Texture2D pixelated = new Texture2D(width, height);

        for (int y = 0; y < height; y += blockSize)
        {
            for (int x = 0; x < width; x += blockSize)
            {
                Color avgColor = original.GetPixel(x, y);
                for (int dy = 0; dy < blockSize && y + dy < height; dy++)
                {
                    for (int dx = 0; dx < blockSize && x + dx < width; dx++)
                    {
                        pixelated.SetPixel(x + dx, y + dy, avgColor);
                    }
                }
            }
        }

        pixelated.Apply();
        return pixelated;
    }

    private void AdjustRawImageScaleToAspect(VideoClip clip)
    {
        float videoAspect = (float)clip.width / clip.height;
        float renderAspect = (float)renderTextureSize.x / renderTextureSize.y;

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
