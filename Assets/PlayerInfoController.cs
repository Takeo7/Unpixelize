using System.Collections.Generic;
using UnityEngine;


public class PlayerInfoController : MonoBehaviour
{
    #region Change Scene Info

    public int currentLevel;
    public int currentMovie;

    public int popcorns=1000;

    #endregion
    public static PlayerInfoController Player_Instance { get; private set; }

    public PlayerData playerData;

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

        //PlayerData mockData = MockDataGenerator.GetMockPlayerData();
        //PlayerInfoController.Player_Instance.LoadPlayerData(mockData);
    }

    private void Start()
    {
        popcorns = 1000;
    }

    public void LoadPlayerData(PlayerData data)
    {
        playerData = data;
    }

    public int GetPopcorns()
    {
        return popcorns;
    }

    public void LoadPopcornsText(TMPro.TextMeshProUGUI text)
    {
        Debug.Log("LoadPopcornsText: "+popcorns);
        text.text = popcorns.ToString();
    }

    public void LoadHelpersText(TMPro.TextMeshProUGUI text, Purchase_Type type)
    {
        text.text = prices[(int)type].ToString();
    }

    public bool SetPopcorns(Purchase_Type pt)
    {
        if (prices[(int)pt] <= popcorns)
        {
            popcorns -= prices[(int)pt];
            //Llamada para updatear popcorns
            return true;
        }
        else
        {
            return false;
        }
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
}


#region Player Data
[System.Serializable]
public class PlayerData
{
    public string playerId;
    public string playerName;
    public string authToken; // ← Aquí se guardará el token

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
    public string platform;
    public string path_to_video;
    public string path_to_photo;

    public List<FilmName> names; // ← usado al deserializar del JSON
    public FilmName name; // para uso interno
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



#endregion

/*#region Player Mock Data

public static class MockDataGenerator
{
    public static PlayerData GetMockPlayerData()
    {
        PlayerData playerData = new PlayerData
        {
            playerId = "test123",
            playerName = "Tester",
            levelsProgress = new List<LevelProgress>()
        };

        for (int levelId = 1; levelId <= 9; levelId++)
        {
            LevelProgress level = new LevelProgress
            {
                levelId = levelId,
                levelName = $"Nivel {levelId}",
                subLevels = new List<SubLevelData>()
            };

            int completedCount = 0;

            for (int subIndex = 0; subIndex < 9; subIndex++)
            {
                int filmId = (levelId - 1) * 9 + subIndex + 1;
                bool isSolved = Random.value > 0.5f;
                if (isSolved) completedCount++;

                SubLevelData subLevel = new SubLevelData
                {
                    sublevel_id = subIndex + 1,
                    solved = isSolved,
                    film = new Film
                    {
                        id = filmId,
                        director = $"Director {filmId}",
                        actor = $"Actor {filmId}",
                        year = 2000 + filmId % 20,
                        platform = "Netflix",
                        path_to_video = $"videos/video_{filmId}.mp4",
                        path_to_photo = $"https://placehold.co/200x200.png?text=Film+{filmId}&format=png",


                        names = new FilmName
                            {
                                id = filmId,
                                film_id = filmId,
                                en = $"Film {filmId}",
                                es = $"Película {filmId}"
                            }
                    },
                    help = new HelpData
                    {
                        help_pixel = new HelpPixel
                        {
                            id = filmId,
                            user_id = 1,
                            film_id = filmId,
                            level_id = levelId,
                            sublevel_id = subIndex + 1,
                            pixel_count = Random.Range(1, 6),
                            created_at = System.DateTime.UtcNow.ToString("o"),
                            updated_at = System.DateTime.UtcNow.ToString("o")
                        },
                        help_clues = new List<HelpClue>
                        {
                            new HelpClue
                            {
                                id = filmId,
                                user_id = 1,
                                film_id = filmId,
                                level_id = levelId,
                                sublevel_id = subIndex + 1,
                                type = "director",
                                created_at = System.DateTime.UtcNow.ToString("o"),
                                updated_at = System.DateTime.UtcNow.ToString("o")
                            }
                        },
                        help_bombs = null
                    }
                };

                level.subLevels.Add(subLevel);
            }

            level.solved = completedCount == 9;

            if (levelId <= 2)
            {
                level.unlocked = true;
            }
            else
            {
                bool previousSolved = playerData.levelsProgress[levelId - 2].solved;
                level.unlocked = previousSolved;
            }

            playerData.levelsProgress.Add(level);
        }

        return playerData;
    }
}







#endregion
*/
