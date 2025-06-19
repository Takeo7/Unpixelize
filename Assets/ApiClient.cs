using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Security.Cryptography;
using System;

public class ApiClient : MonoBehaviour
{
    public static ApiClient Instance;

    [Header("API Settings")]
    public string baseUrl = "http://13.53.108.213/api";
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
        PlayerPrefs.SetString("auth_secret", "VISIBLE_CHECK_123");
        PlayerPrefs.Save();
    }

    public string GetPersistentUserId()
    {
        if (PlayerPrefs.HasKey("user_uuid"))
        {
            return PlayerPrefs.GetString("user_uuid");
        }
        else
        {
            //first time
            string newId = SystemInfo.deviceUniqueIdentifier;

            // Fallback si el identificador no es confiable
            if (string.IsNullOrEmpty(newId) || newId == "00000000000000000000000000000000")
            {
                newId = System.Guid.NewGuid().ToString();
            }

            PlayerPrefs.SetString("user_uuid", newId);
            PlayerPrefs.Save();
            return newId;
        }
            

        
    }

    public string GenerateSignature(string uuid, string secret)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
        {
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(uuid));
            return System.BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    public static class PasswordGenerator
    {
        private static System.Random rng = new System.Random();

        public static string GeneratePassword(int length = 10)
        {
            if (length < 8) length = 8;

            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string all = lower + upper + digits;

            StringBuilder password = new StringBuilder();

            // Añadir al menos 2 mayúsculas
            for (int i = 0; i < 2; i++)
                password.Append(upper[rng.Next(upper.Length)]);

            // Añadir al menos 2 minusculas
            for (int i = 0; i < 2; i++)
                password.Append(lower[rng.Next(lower.Length)]);

            // Añadir al menos 1 número
            password.Append(digits[rng.Next(digits.Length)]);

            // Rellenar el resto
            while (password.Length < length)
                password.Append(all[rng.Next(all.Length)]);

            // Mezclar los caracteres para que no queden las mayúsculas al principio
            return ShuffleString(password.ToString());
        }

        private static string ShuffleString(string input)
        {
            char[] array = input.ToCharArray();
            int n = array.Length;
            for (int i = 0; i < n; i++)
            {
                int j = rng.Next(i, n);
                (array[i], array[j]) = (array[j], array[i]);
            }
            return new string(array);
        }
    }

    #region Login
    public void Login(string email, string password, System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(LoginCoroutine(email, password, onSuccess, onError));
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
        Debug.Log($"[⏱️ API] Login completado en {sw.ElapsedMilliseconds} ms" + "--- URL: " + url);

        if (request.result == UnityWebRequest.Result.Success)
        {
            LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);

            PlayerInfoController.Player_Instance.playerData.authToken = response.token;
            authToken = response.token;

            if (response.daily_reward)
            {
                PlayerInfoController.Player_Instance.HandleDailyReward();
            }

            onSuccess?.Invoke(response.token);
        }
        else
        {
            Debug.LogError("❌ Error en Login: " + request.downloadHandler.text);
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
    public class LoginResponse
    {
        public string token;
        public bool daily_reward;
    }

    #endregion
    
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

    public void UseHelpKey(int levelId, int subLevelId, System.Action<HelpKeyResponse> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(PostRequest($"/help/key/{levelId}/{subLevelId}", onSuccess, onError));
    }

    public void UseHelpLetter(int levelId, int subLevelId, string lang, System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(PutRequest($"/help/letter/{levelId}/{subLevelId}/{lang}", onSuccess, onError));
    }

    #region Solved Sublevel

    public void MarkSubLevelSolved(int levelId, int subLevelId, System.Action<SolvedSublevelResponse> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(PostSolvedSublevelCoroutine(levelId, subLevelId, onSuccess, onError));
    }

    private IEnumerator PostSolvedSublevelCoroutine(int levelId, int subLevelId, System.Action<SolvedSublevelResponse> onSuccess, System.Action<string> onError)
    {
        string url = $"{baseUrl}/solved_sublevel/{levelId}/{subLevelId}";

        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + authToken);

        Stopwatch sw = Stopwatch.StartNew();
        yield return request.SendWebRequest();
        sw.Stop();
        Debug.Log($"[⏱️ API] POST solved_sublevel/{levelId}/{subLevelId} en {sw.ElapsedMilliseconds} ms");

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                SolvedSublevelResponse response = JsonUtility.FromJson<SolvedSublevelResponse>(request.downloadHandler.text);
                Debug.Log($"✅ Subnivel resuelto: {response.message} | Nivel completo: {response.level_completed} | Siguiente desbloqueado: {response.next_level_unlocked}");
                onSuccess?.Invoke(response);
            }
            catch (System.Exception e)
            {
                Debug.LogError("❌ Error al parsear solved_sublevel: " + e.Message);
                onError?.Invoke("Parse error");
            }
        }
        else
        {
            Debug.LogError("❌ Error en POST solved_sublevel: " + request.downloadHandler.text);
            onError?.Invoke(request.error);
        }
    }


    

    #endregion

    public void GetPlayerAmount(System.Action<int> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(GetAmountCoroutine(onSuccess, onError));
    }

    #region BugReport

    public void SendReport(string message, System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(PostReportCoroutine(message, onSuccess, onError));
    }


    private IEnumerator PostReportCoroutine(string message, System.Action<string> onSuccess, System.Action<string> onError)
    {
        string url = baseUrl + "/report";

        ReportRequest report = new ReportRequest { report = message };
        string json = JsonUtility.ToJson(report);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + authToken); // si se requiere autenticación

        Stopwatch sw = Stopwatch.StartNew();
        yield return request.SendWebRequest();
        sw.Stop();

        Debug.Log($"[⏱️ API] POST /report completado en {sw.ElapsedMilliseconds} ms");

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("📨 Reporte enviado con éxito.");
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("❌ Error al enviar el reporte: " + request.downloadHandler.text);
            onError?.Invoke(request.error);
        }
    }


    #endregion


    #region Register

    public void Register(System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(RegisterCoroutine(onSuccess, onError));
    }

    private IEnumerator RegisterCoroutine(System.Action<string> onSuccess, System.Action<string> onError)
    {
        string uuid = GetPersistentUserId();
        Debug.Log("UUID: " + uuid);
        string password = PasswordGenerator.GeneratePassword(10);
        PlayerPrefs.SetString("password", password);
        Debug.Log("Password: " + password);
        string secret = PlayerPrefs.GetString("auth_secret");
        Debug.Log("SECRET: " + secret);

        // Generar HMAC SHA256
        byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
        byte[] dataBytes = Encoding.UTF8.GetBytes(uuid);
        string signature = GenerateSignature(uuid, secret);
        Debug.Log("SIGNATURE1: " + signature);

        using (var hmac = new System.Security.Cryptography.HMACSHA256(keyBytes))
        {
            byte[] hashBytes = hmac.ComputeHash(dataBytes);
            signature = System.BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            Debug.Log("SIGNATURE2: " + signature);
        }

        RegisterRequest registerRequest = new RegisterRequest
        {
            device_uuid = uuid,
            password = password,
            signature = signature
        };

        string json = JsonUtility.ToJson(registerRequest);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        string url = baseUrl + "/auth/anonymous/register";
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ Registro completado: " + request.downloadHandler.text);

            RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(request.downloadHandler.text);

            // Guardar en PlayerPrefs
            PlayerPrefs.SetString("REGISTERED_EMAIL", response.email);
            Debug.Log("EMAIL: " + response.email);
            authToken = response.token;
            PlayerInfoController.Player_Instance.playerData.authToken = authToken;
            PlayerPrefs.Save();

            // Guardar en PlayerInfoController
            PlayerInfoController.Player_Instance.playerData.authToken = response.token;
            authToken = response.token;

            onSuccess?.Invoke(response.email);
        }
        else
        {

            Debug.LogError("❌ Error en registro: " + request.downloadHandler.text);
            onError?.Invoke(request.error);
        }
    }

    [System.Serializable]
    private class RegisterRequest
    {
        public string device_uuid;
        public string signature;
        public string password;
    }

    [System.Serializable]
    private class RegisterResponse
    {
        public string email;
        public string token;
        public string message;
    }

    #endregion


    #region Help Data

    public void GetHelpData(int levelId, int subLevelId, System.Action<string> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(GetHelpDataCoroutine(levelId, subLevelId, onSuccess, onError));
    }


    private IEnumerator GetHelpDataCoroutine(int levelId, int subLevelId, System.Action<string> onSuccess, System.Action<string> onError)
    {
        string url = $"{baseUrl}/helps/{levelId}/{subLevelId}";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + authToken);

        Stopwatch sw = Stopwatch.StartNew();
        yield return request.SendWebRequest();
        sw.Stop();
        Debug.Log($"[⏱️ API] GetHelps {levelId}/{subLevelId} completado en {sw.ElapsedMilliseconds} ms");

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                string jsonFixed = JsonHelper.FixJsonArray(request.downloadHandler.text);
                HelpServerItem[] helps = JsonHelper.FromJson<HelpServerItem>(jsonFixed);

                var levelData = PlayerInfoController.Player_Instance.playerData.levelsProgress
                    .Find(l => l.levelId == levelId);
                var subData = levelData?.subLevels.Find(s => s.sublevel_id == subLevelId);

                if (subData != null && helps.Length > 0)
                {
                    HelpServerItem serverData = helps[0];
                    subData.help.help_pixel = serverData.help_pixel;
                    subData.help.help_clues = serverData.help_clues;
                    subData.help.help_bombs = serverData.help_bombs;

                    if (serverData.help_letters != null && serverData.help_letters.Count > 0)
                    {
                        foreach (var letter in serverData.help_letters)
                        {
                            if (letter.lang == "es")
                                subData.help.helpLetters_es = new HelpLetters { letters = letter.letter_count };
                            else if (letter.lang == "en")
                                subData.help.helpLetters_en = new HelpLetters { letters = letter.letter_count };
                        }
                    }


                    Debug.Log($"✅ Ayudas cargadas para L{levelId}-S{subLevelId}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ No se encontró SubLevelData para L{levelId}-S{subLevelId}");
                }

                onSuccess?.Invoke(request.result.ToString());
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error parseando ayuda: {e.Message}");
                onError?.Invoke("Parse error");
            }
        }
        else
        {
            onError?.Invoke(request.error);
        }
    }

    [System.Serializable]
    public class HelpServerItem
    {
        public int id;
        public int film_id;
        public int level_id;
        public HelpPixel help_pixel;
        public List<HelpClue> help_clues;
        public HelpBomb help_bombs;
        public List<HelpLetterServerData> help_letters;
    }

    [System.Serializable]
    public class HelpLetterServerData
    {
        public int id;
        public int user_id;
        public int level_id;
        public int sublevel_id;
        public int letter_count;
        public string lang;
        public string created_at;
        public string updated_at;
    }

#endregion


    #region Info

    public void GetPlayerInfo(System.Action<InfoResponse> onSuccess, System.Action<string> onError)
    {
        StartCoroutine(GetPlayerInfoCoroutine(onSuccess, onError));
    }

    private IEnumerator GetPlayerInfoCoroutine(System.Action<InfoResponse> onSuccess, System.Action<string> onError)
    {
        string url = baseUrl + "/info";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + authToken);

        Stopwatch sw = Stopwatch.StartNew();
        yield return request.SendWebRequest();
        sw.Stop();
        Debug.Log($"[⏱️ API] GetInfo completado en {sw.ElapsedMilliseconds} ms");

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                InfoResponse response = JsonUtility.FromJson<InfoResponse>(request.downloadHandler.text);

                // Asignar en PlayerInfoController
                var pic = PlayerInfoController.Player_Instance;
                pic.playerData.amount = response.amount;
                pic.playerData.daily_reward = response.daily_reward;
                pic.playerData.px_limit = response.limit_pixel;
                pic.prices = new List<int>
            {
                response.help_prices.letter,
                response.help_prices.bomb,
                response.help_prices.unpixel,
                response.help_prices.clues,
                response.help_prices.key
            };
                pic.win_amount = new List<int>
            {
                response.rewards.resolved_sublevel,
                response.rewards.resolved_level
            };

                Debug.Log($"✅ Info actualizada: amount={response.amount}");

                onSuccess?.Invoke(response);
            }
            catch (System.Exception e)
            {
                Debug.LogError("❌ Error al parsear /info: " + e.Message);
                onError?.Invoke("Parse error");
            }
        }
        else
        {
            onError?.Invoke(request.error);
        }
    }


    [System.Serializable]
    public class InfoResponse
    {
        public int amount;
        public int daily_reward;
        public int limit_pixel;
        public HelpPrices help_prices;
        public Rewards rewards;
    }
    #endregion

    private IEnumerator GetAmountCoroutine(System.Action<int> onSuccess, System.Action<string> onError)
    {
        string url = baseUrl + "/amount";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + authToken);

        Stopwatch sw = Stopwatch.StartNew();
        yield return request.SendWebRequest();
        sw.Stop();
        Debug.Log($"[⏱️ API] GetAmount completado en {sw.ElapsedMilliseconds} ms");

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                AmountResponse response = JsonUtility.FromJson<AmountResponse>(request.downloadHandler.text);
                PlayerInfoController.Player_Instance.playerData.amount = response.amount;
                Debug.Log($"💰 Amount recibido: {response.amount}");
                onSuccess?.Invoke(response.amount);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error al parsear GetAmount: {e.Message}");
                onError?.Invoke("Parse error");
            }
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

    [System.Serializable]
public class PostErrorWithMessage
{
    public string message;
}

[System.Serializable]
public class AmountWrapper
{
    public int amount;
}

private void TryUpdateAmountFromJson(string json)
{
    try
    {
        AmountWrapper aw = JsonUtility.FromJson<AmountWrapper>(json);
        if (aw.amount >= 0) // permite amount = 0 como en solved_sublevel
        {
            PlayerInfoController.Player_Instance.SetPopcorns(aw.amount);
            Debug.Log("💰 Amount actualizado desde respuesta POST: " + aw.amount);
        }
    }
    catch
    {
        // El JSON no tiene "amount", no pasa nada
    }
}

public IEnumerator PostRequest<T>(string endpoint, System.Action<T> onSuccess, System.Action<string> onError)
{
    string url = baseUrl + endpoint;

    UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "");
    request.downloadHandler = new DownloadHandlerBuffer();
    request.SetRequestHeader("Authorization", "Bearer " + authToken);

    Stopwatch sw = Stopwatch.StartNew();
    yield return request.SendWebRequest();
    sw.Stop();

    string responseText = request.downloadHandler.text;
    Debug.Log($"[⏱️ API] POST {endpoint} en {sw.ElapsedMilliseconds} ms\nRespuesta: {responseText}");

    if (request.result == UnityWebRequest.Result.Success)
    {
            try
            {

                // 2. Intenta extraer amount, si lo hay
                TryUpdateAmountFromJson(responseText);

                // 1. Deserializa en el tipo esperado (T)
                T result = JsonUtility.FromJson<T>(responseText);
                onSuccess?.Invoke(result);
        }
            catch (Exception e)
            {
                Debug.LogError("❌ Error parseando respuesta POST: " + e.Message);
                onError?.Invoke("Parse error");
            }
    }
    else
    {
        if ((int)request.responseCode == 400)
        {
            try
            {
                PostErrorWithMessage errorObj = JsonUtility.FromJson<PostErrorWithMessage>(responseText);
                Debug.LogWarning("⚠️ Error 400: " + errorObj.message);
                onError?.Invoke("poor");
            }
            catch
            {
                Debug.LogError("❌ Error 400 sin mensaje parseable");
                onError?.Invoke("Error 400");
            }
        }
        else
        {
            Debug.LogError("❌ Error HTTP: " + request.error);
            onError?.Invoke(request.error);
        }
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

    string responseText = request.downloadHandler.text;
    Debug.Log($"[⏱️ API] PUT {endpoint} completado en {sw.ElapsedMilliseconds} ms\nRespuesta: {responseText}");

        if (request.result == UnityWebRequest.Result.Success)
        {

            TryUpdateAmountFromJson(responseText); // 💰 Intentar actualizar amount si lo hay
            onSuccess?.Invoke(responseText);
        }
        else
        {
            if ((int)request.responseCode == 400)
            {
                try
                {
                    PostErrorWithMessage errorObj = JsonUtility.FromJson<PostErrorWithMessage>(responseText);
                    Debug.LogWarning("⚠️ Error 400: " + errorObj.message);
                    onError?.Invoke("poor");
                }
                catch
                {
                    Debug.LogError("❌ Error 400 sin mensaje parseable");
                    onError?.Invoke("Error 400");
                }
            }
            else
            {
                Debug.LogError("❌ Error HTTP: " + request.error);
                onError?.Invoke(request.error);
            }
        }
}

public void ResetUser(System.Action onSuccess, System.Action<string> onError)
{
    StartCoroutine(PostRequest_ResetUser(onSuccess, onError));
}

private IEnumerator PostRequest_ResetUser(System.Action onSuccess, System.Action<string> onError)
{
    string url = baseUrl + "/user/reset";

    UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "");
    request.downloadHandler = new DownloadHandlerBuffer();
    request.SetRequestHeader("Authorization", "Bearer " + authToken);

    Stopwatch sw = Stopwatch.StartNew();
    yield return request.SendWebRequest();
    sw.Stop();

    Debug.Log($"[⏱️ API] POST /user/reset completado en {sw.ElapsedMilliseconds} ms");

    if (request.result == UnityWebRequest.Result.Success)
    {
        // Opcional: log
        Debug.Log("✅ Usuario reseteado correctamente");
        onSuccess?.Invoke();
    }
    else
    {
        if ((int)request.responseCode == 400)
        {
            try
            {
                PostErrorWithMessage errorObj = JsonUtility.FromJson<PostErrorWithMessage>(request.downloadHandler.text);
                onError?.Invoke(errorObj.message);
            }
            catch
            {
                onError?.Invoke("Error 400");
            }
        }
        else
        {
            onError?.Invoke(request.error);
        }
    }
}

   

    [System.Serializable]
    public class LevelApiResponse
    {
        public int id;
        public int level;
        public bool unlocked;
    }

    [System.Serializable]
    public class AmountResponse
    {
        public int amount;
    }


   


    [System.Serializable]
    public class HelpPrices
    {
        public int key;
        public int clues;
        public int bomb;
        public int unpixel;
        public int letter;
    }

    [System.Serializable]
    public class Rewards
    {
        public int resolved_sublevel;
        public int resolved_level;
    }

   

   

}

[System.Serializable]
public class ReportRequest
{
    public string report;
}

[System.Serializable]
public class SolvedSublevelResponse
{
    public string message;
    public bool level_completed;
    public bool next_level_unlocked;
    public int amount;
}
[System.Serializable]
public class HelpKeyResponse
{
    public string message;
    public bool level_completed;
    public bool next_level_unlocked;
    public int amount;
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
