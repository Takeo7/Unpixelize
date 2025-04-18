using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class ApiClient : MonoBehaviour
{
    public static ApiClient Instance;

    [Header("API Settings")]
    public string baseUrl = "https://unpixelize-api.onrender.com/api";
    public string authToken;
    public bool Testing;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Login(string email, string password, System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(LoginCoroutine(email, password, onSuccess, onError));
    }

    public void GetLevels(System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(GetLevelsCoroutine(onSuccess, onError));
    }

    public void GetLevel(int levelId, System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(GetLevelCoroutine(levelId, onSuccess, onError));
    }

    private IEnumerator LoginCoroutine(string email, string password, System.Action<string> onSuccess, System.Action<string> onError)
    {
        string url = baseUrl + "/login";

        var json = JsonUtility.ToJson(new LoginRequest { email = email, password = password });
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            onError?.Invoke(request.error);
        }
    }

    private IEnumerator GetLevelsCoroutine(System.Action<string> onSuccess, System.Action<string> onError)
    {
        string url = baseUrl + "/levels";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + authToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            LevelApiResponse[] apiLevels = JsonHelper.FromJson<LevelApiResponse>(JsonHelper.FixJsonArray(json));

            List<LevelProgress> levelsProgress = new List<LevelProgress>();
            foreach (var level in apiLevels)
            {
                LevelProgress levelProgress = new LevelProgress
                {
                    levelId = level.level,
                    levelName = $"Nivel {level.level}",
                    unlocked = level.unlocked,
                    solved = false,
                    subLevels = new List<SubLevelData>()
                };

                yield return GetLevelCoroutine(level.level, levelJson =>
                {
                    Debug.Log($"[Raw JSON Level {level.level}] {levelJson}");
                    string fixedJson = JsonHelper.FixJsonArray(levelJson);
                    Debug.Log($"[Fixed JSON Level {level.level}] {fixedJson}");

                    try
                    {
                        SubLevelData[] sublevels = JsonHelper.FromJson<SubLevelData>(fixedJson);
                        if (sublevels != null)
                        {
                            foreach (var sub in sublevels)
                            {
                                if (sub != null)
                                {
                                    if (sub.film != null && sub.film.names != null && sub.film.names.Count > 0)
                                    {
                                        sub.film.name = sub.film.names[0];
                                    }

                                    Debug.Log($"[Parsed SubLevel] Nivel {level.level}, SubLevel {sub.sublevel_id}, Film ID: {sub.film?.id}, Solved: {sub.solved}");
                                    levelProgress.subLevels.Add(sub);
                                }
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[ERROR deserializando subniveles nivel {level.level}] {e.Message}");
                    }
                }, error =>
                {
                    Debug.LogError("Error al cargar subniveles del nivel " + level.level + ": " + error);
                });

                Debug.Log($"[Level Complete] Nivel {level.level} con {levelProgress.subLevels.Count} subniveles cargados.");
                levelsProgress.Add(levelProgress);
            }

            PlayerInfoController.Player_Instance.playerData.levelsProgress = levelsProgress;
            onSuccess?.Invoke(json);
        }
        else
        {
            onError?.Invoke(request.error);
        }
    }

    private IEnumerator GetLevelCoroutine(int levelId, System.Action<string> onSuccess, System.Action<string> onError)
    {
        string url = baseUrl + "/level/" + levelId;

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + authToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string fixedJson = request.downloadHandler.text;
            onSuccess?.Invoke(fixedJson);
        }
        else
        {
            onError?.Invoke(request.error);
        }
    }

    [System.Serializable]
    public class LoginRequest
    {
        public string email;
        public string password;
    }

    [System.Serializable]
    public class LevelApiResponse
    {
        public int id;
        public int level;
        public bool unlocked;
    }
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string FixJsonArray(string json)
    {
        return "{\"Items\":" + json + "}";
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}
