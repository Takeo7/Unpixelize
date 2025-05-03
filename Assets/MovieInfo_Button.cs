using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;
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

    private VideoPlayer videoPlayer;

    public void LoadMovieID(int id, bool sol, string pathToVideo)
    {
        movie_id = id;
        solved = sol;
        movie_video_path = pathToVideo;

        string videoKey = GetVideoKeyFromPath(pathToVideo);
        UpdateMovie(videoKey);

        
    }

    public void SetGreenBorder(bool enabled)
    {
        Outline outline = videoDisplay.GetComponentInParent<Outline>();

        if (enabled)
        {
            if (outline == null)
                outline = videoDisplay.GetComponentInParent<Outline>();

            outline.effectColor = Color.green;
            outline.effectDistance = new Vector2(5, 5); // Puedes ajustar el grosor aquí
        }
        else
        {
            if (outline != null)
                Destroy(outline);
        }
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
            videoDisplay.rectTransform.anchorMin = Vector2.zero;
            videoDisplay.rectTransform.anchorMax = Vector2.one;
            videoDisplay.rectTransform.offsetMin = Vector2.zero;
            videoDisplay.rectTransform.offsetMax = Vector2.zero;
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

        RenderTexture rt = new RenderTexture((int)renderTextureSize.x, (int)renderTextureSize.y, 0);
        rt.Create();
        videoPlayer.targetTexture = rt;
        videoDisplay.texture = rt;
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
                SetRawImageColor(Color.red); // Fallback visual
            }
        };
    }

    private IEnumerator PrepareAndDisplayFirstFrame()
    {
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
            yield return null;

        videoPlayer.Play();
        videoPlayer.Pause(); // Muestra el primer frame
        Debug.Log($"🖼️ Primer frame preparado: {videoPlayer.clip?.name}");
        if (solved)
        {
            SetGreenBorder(solved);
        }
    }

    private string GetVideoKeyFromPath(string pathToVideo)
    {
        if (string.IsNullOrEmpty(pathToVideo))
            return string.Empty;

        string filename = System.IO.Path.GetFileNameWithoutExtension(pathToVideo);
        filename = RemoveVoidsAndLower(filename);
        return $"Videos/{filename}";
    }
    public string RemoveVoidsAndLower(string s)
    {
        int length = s.Length;

        string s_final = s.Replace(" ", "");

        s_final = s_final.ToLower();

        return s_final;
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
