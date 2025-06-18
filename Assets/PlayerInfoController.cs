using System.Collections.Generic;
using UnityEngine;


public class PlayerInfoController : MonoBehaviour
{
    #region Change Scene Info

    public int currentLevel;
    public int currentMovie;

    #endregion
    public static PlayerInfoController Player_Instance { get; private set; }

    public PlayerData playerData;

    public bool dailyReward = false;

    private void Awake()
    {
        #region Singleton
        if (Player_Instance != null && Player_Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Player_Instance = this;
        DontDestroyOnLoad(gameObject);

        #endregion

        if (LoadPlayerDataFromDisk())
        {

        }
        else
        {

        }

        //PlayerData mockData = MockDataGenerator.GetMockPlayerData();
        //PlayerInfoController.Player_Instance.LoadPlayerData(mockData);
    }

   

    public void LoadPlayerData(PlayerData data)
    {
        playerData = data;
    }


    public void HandleDailyReward()
    {
        Debug.Log("🎁 Daily reward disponible");
        dailyReward = true;
    }


    public void SetPopcorns(int pop)
    {
        playerData.amount = pop;
    }

    public void LoadPopcornsText(TMPro.TextMeshProUGUI text)
    {
        Debug.Log("LoadPopcornsText: "+playerData.amount);
        text.text = playerData.amount.ToString();
    }

    public void LoadHelpersText(TMPro.TextMeshProUGUI text, Purchase_Type type)
    {
        text.text = prices[(int)type].ToString();
    }

    public bool SetPopcorns(Purchase_Type pt, AnimatedCounter pop_anim)
    {
        pop_anim.SetPoints(-prices[(int)pt]);
        return true;
    }

    public void SetPopcorns(Win_Type wt, AnimatedCounter pop_anim)
    {
        pop_anim.SetPoints(win_amount[(int)wt]);
    }


    public enum Purchase_Type
    {
        newLetter,
        tnt,
        pixel,
        clue,
        key
    }
    public List<int> prices = new List<int> {50, 100, 10, 50, 500};

    public enum Win_Type
    {
        movie_solved,
        level_unlocked
    }
    public List<int> win_amount = new List<int> { 500, 1000 };


    public SubLevelData GetCurrentMovieData()
    {
        var sub = playerData.levelsProgress[currentLevel - 1]
        .subLevels.Find(s => s.sublevel_id == currentMovie);

        if (sub != null)
        {
            return sub;
        }
        else
        {
            Debug.LogError("❌ No se encontró el subnivel con ID: " + currentMovie);
            return null;
        }
    }

    public void OpenReferral()
    {
        Application.OpenURL(GetCurrentMovieData().film.referral.link);
    }


    public void SavePlayerDataToDisk()
    {
        string json = JsonUtility.ToJson(playerData);
        string path = Application.persistentDataPath + "/playerdata.json";
        System.IO.File.WriteAllText(path, json);
        //Debug.Log($"💾 PlayerData guardado en: {path}");
    }

    private void OnApplicationQuit()
    {
        SavePlayerDataToDisk();
        //Debug.Log("💾 Datos guardados al cerrar la aplicación.");
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SavePlayerDataToDisk();
            //Debug.Log("💾 Datos guardados al ir al segundo plano.");
        }
    }

    public bool LoadPlayerDataFromDisk()
    {
        string path = Application.persistentDataPath + "/playerdata.json";

        if (System.IO.File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            PlayerData loadedData = JsonUtility.FromJson<PlayerData>(json);
            LoadPlayerData(loadedData);
            //Debug.Log("📂 PlayerData cargado desde archivo.");
            return true;
        }
        else
        {
            //Debug.LogWarning("❌ No se encontró archivo de PlayerData.");
            return false;
        }
    }

}


#region Player Data
[System.Serializable]
public class PlayerData
{
    public string playerId;
    public string playerName;
    public string authToken; // ← Aquí se guardará el token
    public int amount;
    public int daily_reward;
    public int px_limit;

    // Lista de niveles con progreso del jugador
    public List<LevelProgress> levelsProgress = new List<LevelProgress>();
}

[System.Serializable]
public class LevelProgress
{
    public int levelId; // ID del nivel (1, 2, 3...)
    public string levelName;
    public bool solved;
    public bool unlocked;
    // Lista de subniveles con su estado e información
    public List<SubLevelData> subLevels = new List<SubLevelData>();
}

[System.Serializable]
public class SubLevelData
{
    public int sublevel_id;
    public Film film;
    public HelpData help;
    public bool solved;
}

[System.Serializable]
public class Film
{
    public int id;
    public string director;
    public string actor;
    public int year;
    public Referral referral;

    public List<FilmName> names; // ← usado al deserializar del JSON
    public FilmName name; // para uso interno
}

[System.Serializable]
public class Referral
{
    public string link;
}

[System.Serializable]
public class FilmName
{
    public int id;
    public int film_id;
    public string en;
    public string es;
}

[System.Serializable]
public class HelpData
{
    public HelpPixel help_pixel;
    public List<HelpClue> help_clues;
    public HelpBomb help_bombs;
    public HelpLetters helpLetters_es;
    public HelpLetters helpLetters_en;
}

[System.Serializable]
public class HelpPixel
{
    public int id;
    public int user_id;
    public int level_id;
    public int sublevel_id;
    public int pixel_count;
    public string created_at;
    public string updated_at;
}

[System.Serializable]
public class HelpClue
{
    public int id;
    public int user_id;
    public int film_id;
    public int level_id;
    public int sublevel_id;
    public string type;
    public string created_at;
    public string updated_at;
}

[System.Serializable]
public class HelpBomb
{
    public int id;
    public int user_id;
    public int level_id;
    public int sublevel_id;
    public string created_at;
    public string updated_at;
}

[System.Serializable]
public class HelpLetters
{
    public int letters;
}

#endregion


