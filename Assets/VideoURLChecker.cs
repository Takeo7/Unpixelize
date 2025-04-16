using UnityEngine;
using UnityEngine.Video;

public class VideoURLChecker : MonoBehaviour
{
    void Start()
    {
        VideoPlayer vp = GetComponent<VideoPlayer>();
        if (vp != null && vp.source == VideoSource.Url)
        {
            Debug.Log("VIDEO URL: " + vp.url);
        }
    }
}