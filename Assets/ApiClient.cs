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
    public string baseUrl = "http://13.61.154.102/api";
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

        baseUrl = "http://13.61.154.102/api";
    }

    public void Login(string email, string password, System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(LoginCoroutine(email, password, onSuccess, onError));
    }

    public void Logout(System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(LogoutCoroutine(onSuccess, onError));
    }

    public void GetLevels(System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(GetLevelsCoroutine(onSuccess, onError));
    }

    public void GetLevel(int levelId, System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(GetLevelCoroutine(levelId, onSuccess, onError));
    }

    public void UseHelpPixel(int levelId, int subLevelId, System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(PutRequest($"/help/pixel/{levelId}/{subLevelId}", onSuccess, onError));
    }

    public void UseHelpClue(int levelId, int subLevelId, string clueType, System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(PostRequest($"/help/clue/{levelId}/{subLevelId}/{clueType}", onSuccess, onError));
    }

    public void UseHelpBomb(int levelId, int subLevelId, System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(PostRequest($"/help/bomb/{levelId}/{subLevelId}", onSuccess, onError));
    }

    public void UseHelpKey(int levelId, int subLevelId, System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(PostRequest($"/help/key/{levelId}/{subLevelId}", onSuccess, onError));
    }

    public void UseHelpLetter(int levelId, int subLevelId, string lang, System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(PutRequest($"/help/letter/{levelId}/{subLevelId}/{lang}", onSuccess, onError));
    }

    public void MarkSubLevelSolved(int levelId, int subLevelId, System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(PostRequest($"/solved_sublevel/{levelId}/{subLevelId}", onSuccess, onError));
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

        Stopwatch sw = Stopwatch.StartNew();
        yield return request.SendWebRequest();
        sw.Stop();
        Debug.Log($"[⏱️ API] Login completado en {sw.ElapsedMilliseconds} ms"+ "--- URL: "+url);

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            onError?.Invoke(request.error);
        }
    }

    private IEnumerator LogoutCoroutine(System.Action<string> onSuccess, System.Action<string> onError)
    {
        string url = baseUrl + "/logout";

        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + authToken);

        Stopwatch sw = Stopwatch.StartNew();
        yield return request.SendWebRequest();
        sw.Stop();
        Debug.Log($"[⏱️ API] Logout completado en {sw.ElapsedMilliseconds} ms");

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

        Stopwatch sw = Stopwatch.StartNew();
        yield return request.SendWebRequest();
        sw.Stop();
        Debug.Log($"[⏱️ API] GetLevels completado en {sw.ElapsedMilliseconds} ms");

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

        Stopwatch sw = Stopwatch.StartNew();
        yield return request.SendWebRequest();
        sw.Stop();
        Debug.Log($"[⏱️ API] GetLevel {levelId} completado en {sw.ElapsedMilliseconds} ms");

        if (request.result == UnityWebRequest.Result.Success)
        {
            string fixedJson = JsonHelper.FixJsonArray(request.downloadHandler.text);
            try
            {
                SubLevelData[] sublevels = JsonHelper.FromJson<SubLevelData>(fixedJson);
                var level = PlayerInfoController.Player_Instance.playerData.levelsProgress.Find(l => l.levelId == levelId);

                if (level != null)
                {
                    level.subLevels.Clear();
                    foreach (var sub in sublevels)
                    {
                        if (sub.film != null && sub.film.name != null)
                        {
                            sub.film.names = new List<FilmName> { sub.film.name };
                        }
                        level.subLevels.Add(sub);
                    }
                }

                Debug.Log($"✅ Subniveles del nivel {levelId} cargados: {sublevels.Length}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ERROR deserializando subniveles nivel {levelId}] {e.Message}");
            }

            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            onError?.Invoke(request.error);
        }
    }

    private IEnumerator PostRequest(string endpoint, System.Action<string> onSuccess, System.Action<string> onError)
    {
        string url = baseUrl + endpoint;

        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + authToken);

        Stopwatch sw = Stopwatch.StartNew();
        yield return request.SendWebRequest();
        sw.Stop();
        Debug.Log($"[⏱️ API] POST {endpoint} completado en {sw.ElapsedMilliseconds} ms");

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            onError?.Invoke(request.error);
        }
    }

    private IEnumerator PutRequest(string endpoint, System.Action<string> onSuccess, System.Action<string> onError)
    {
        string url = baseUrl + endpoint;

        UnityWebRequest request = UnityWebRequest.Put(url, "");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + authToken);

        Stopwatch sw = Stopwatch.StartNew();
        yield return request.SendWebRequest();
        sw.Stop();
        Debug.Log($"[⏱️ API] PUT {endpoint} completado en {sw.ElapsedMilliseconds} ms");

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(request.downloadHandler.text);
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
