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

        PlayerData mockData = MockDataGenerator.GetMockPlayerData();
        PlayerInfoController.Player_Instance.LoadPlayerData(mockData);
    }


    void Start()
    {
        
    }

    public void LoadPlayerData(PlayerData data)
    {
        playerData = data;
    }
}


#region Player Data
[System.Serializable]
public class PlayerData
{
    public string playerId;
    public string playerName;

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
    public int subLevelIndex; // 0 - 8
    public string imageUrl;   // URL de la imagen para mostrar
    public bool isCompleted;  // ¿Está completado este subnivel?
}

#endregion

#region Player Mock Data

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
                bool isCompleted = Random.value > 0.5f;
                if (isCompleted) completedCount++;

                level.subLevels.Add(new SubLevelData
                {
                    subLevelIndex = subIndex,
                    imageUrl = $"https://placehold.co/200x200.png?text=N{levelId}-S{subIndex + 1}&format=png",
                    isCompleted = isCompleted
                });
            }

            level.solved = (completedCount == 9);

            // Nivel 1 y 2 siempre desbloqueados, el resto depende del anterior
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

