using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UnityEngine.UI;

public class MovieGuess_LettersController : MonoBehaviour
{

    #region Sigleton
    public static MovieGuess_LettersController MovieGuessLetters_instance { get; private set; }

    private void Awake()
    {
        if (MovieGuessLetters_instance == null)
        {
            MovieGuessLetters_instance = this;
            //DontDestroyOnLoad(gameObject); // Opcional: Mantiene la instancia entre escenas
        }
        else
        {
            Destroy(gameObject); // Si ya existe una instancia, destruye la nueva
        }
    }
    #endregion

    [Space]
    public MovieGuess_Controller mgc;

    [Space]
    public GameObject Grid_Empty_Letter_Squares;
    public GameObject empty_letter_square;
    public List<Transform> emptySlots = new List<Transform>();
    public int last_letter_count = 0;

    [Space]
    public GameObject Grid_Letter_Squares;
    public GameObject letter_square;

    [Space]
    public List<GameObject> originalLetters = new List<GameObject>();
    public List<GameObject> fakeLetters = new List<GameObject>();
    public List<GameObject> allLetters = new List<GameObject>();

    [Space]
    bool tnt;

    [Space]
    public Dictionary<int, string> buyedLetters_es = new Dictionary<int, string>();
    public Dictionary<int, string> buyedLetters_en = new Dictionary<int, string>();


    #region SetLetters

    public void SetAllLetters(int length, int correctLetterCount, string title, int fakeLetterCount)
    {
        Debug.Log("Set All letters");
        SetEmptySquares(length, correctLetterCount, title, fakeLetterCount);
    }
    public void SetEmptySquares(int length, int correctLetterCount, string title, int fakeLetterCount)
    {
        Debug.Log("Set Empty Letters");
        // Limpiar si ya hay algo
        foreach (Transform child in Grid_Empty_Letter_Squares.transform)
        {
            Destroy(child.gameObject);
        }

        emptySlots.Clear();

        
        for (int i = 0; i < length; i++)
        {
            GameObject g = Instantiate(empty_letter_square);
            g.transform.SetParent(Grid_Empty_Letter_Squares.transform, false);
            g.SetActive(true);

            // Guardar referencia al slot
            emptySlots.Add(g.transform);
        }
        ShowEmptySquaresWithoutSpacesDeferred(title);
        SetLetterSquares(correctLetterCount, title, fakeLetterCount);
    }

    public void HideEmptySquaresForSpaces(string title)
    {
        Transform grid = Grid_Empty_Letter_Squares.transform;

        for (int i = 0; i < title.Length && i < grid.childCount; i++)
        {
            if (title[i] == ' ')
            {
                Transform slot = grid.GetChild(i);
                Image image = slot.GetComponent<Image>();
                if (image != null)
                {
                    Debug.Log("Espacio en: " + i);
                    image.enabled = false;
                    Debug.Log("Image disabled");
                }
                
            }
        }

        Debug.Log("🔲 Ocultadas las casillas correspondientes a espacios en el título.");
    }

    public void HideEmptySquaresForSpacesDeferred(string title)
    {
        StartCoroutine(WaitAndHideSpaces(title));
    }

    private IEnumerator WaitAndHideSpaces(string title)
    {
        yield return null; // espera 1 frame

        Transform grid = Grid_Empty_Letter_Squares.transform;

        for (int i = 0; i < title.Length && i < grid.childCount; i++)
        {
            if (title[i] == ' ')
            {
                Transform slot = grid.GetChild(i);
                Image image = slot.GetComponent<Image>();
                if (image != null)
                {
                    image.enabled = false;
                    Debug.Log($"✅ Image ocultada en slot {i}: {slot.name}");
                }
                else
                {
                    Debug.LogWarning($"❌ No se encontró Image en slot {i}: {slot.name}");
                }
            }
        }

        Debug.Log("🔲 Espacios ocultados tras un frame.");
    }

    public void ShowEmptySquaresWithoutSpacesDeferred(string title)
    {
        StartCoroutine(WaitAndShowSpaces(title));
    }

    private IEnumerator WaitAndShowSpaces(string title)
    {
        yield return null; // Espera 1 frame

        Transform grid = Grid_Empty_Letter_Squares.transform;

        for (int i = 0; i < title.Length && i < grid.childCount; i++)
        {
            if (title[i] != ' ')
            {
                Transform slot = grid.GetChild(i);
                Image image = slot.GetComponent<Image>();
                if (image != null)
                {
                    image.enabled = true;
                    //Debug.Log($"✅ Image activado en slot {i}: {slot.name}");
                }
                else
                {
                    //Debug.LogWarning($"❌ No se encontró Image en slot {i}: {slot.name}");
                }
            }
        }

        //Debug.Log("🟩 Activadas casillas sin espacios tras esperar un frame.");
    }



    public void SetLetterSquares(int correctLetterCount, string title, int fakeLetterCount)
    {
        Debug.Log("Set Letters");
        originalLetters.Clear();
        fakeLetters.Clear();
        allLetters.Clear();

        foreach (Transform child in Grid_Letter_Squares.transform)
        {
            Destroy(child.gameObject);
        }

        // Crear letras correctas
        for (int i = 0; i < correctLetterCount; i++)
        {
            GameObject g = Instantiate(letter_square);
            string letter = title[i].ToString().ToUpper();
            if (letter != " ")
            {
                g.GetComponentInChildren<Sqr_letter_script>().letter = letter;
                g.GetComponentInChildren<TextMeshProUGUI>().SetText(letter);

                originalLetters.Add(g);
            }
            
        }

        allLetters.AddRange(originalLetters);

        // Crear letras falsas
        if (tnt == false)
        {
            for (int i = 0; i < fakeLetterCount; i++)
            {
                GameObject g = Instantiate(letter_square);
                string randomLetter = GetRandomLetter();
                g.GetComponentInChildren<Sqr_letter_script>().letter = randomLetter;
                g.GetComponentInChildren<TextMeshProUGUI>().SetText(randomLetter);

                fakeLetters.Add(g);
            }

            allLetters.AddRange(fakeLetters);
        }
        

        // Unir y mezclar todas       
        ShuffleList(allLetters);

        // Asignar al grid
        foreach (GameObject g in allLetters)
        {
            g.transform.SetParent(Grid_Letter_Squares.transform, false);
            g.SetActive(true);
        }

        //PlaceBuyedLetters();
        RestoreBuyedLettersFromServer();
        MovieGuess_Controller.MovieGuess_instance.CheckIsAlreadySolved();
    }

    private string GetRandomLetter()
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return alphabet[Random.Range(0, alphabet.Length)].ToString();
    }

    private void ShuffleList(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            GameObject temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    #endregion

    #region Main Letter Funcionality

    public Transform GetNextEmptySlot()
    {
        string title = MovieGuess_Controller.MovieGuess_instance.title;

        for (int i = 0; i < emptySlots.Count; i++)
        {
            // Saltar si el título tiene un espacio en esa posición
            if (title[i] == ' ')
                continue;

            if (emptySlots[i].childCount == 0)
                return emptySlots[i];
        }
        return null;
    }

    public Transform GetAvailableGridSlot()
    {
        return Grid_Letter_Squares.transform;
    }

    #endregion

    #region Add New Letter Func

    public void AutoPlaceNextCorrectLetter(string title)
    {
        Transform emptyGrid = Grid_Empty_Letter_Squares.transform;

        //Buscar huecos vacíos
        List<int> huecosLibres = new List<int>();
        for (int i = 0; i < title.Length; i++)
        {
            // Ignorar los espacios
            if (title[i] == ' ')
                continue;

            Transform targetSlot = emptyGrid.GetChild(i);
            if (targetSlot.childCount == 0)
            {
                huecosLibres.Add(i);
            }
        }

        if (huecosLibres.Count == 0)
        {
            return;
        }

        //Elegir uno al azar
        int randomIndex = huecosLibres[Random.Range(0, huecosLibres.Count)];
        string targetLetter = title[randomIndex].ToString().ToUpper();



        Debug.Log($"Hueco aleatorio elegido: {randomIndex}, letra buscada: {targetLetter}");

        mgc.PostAddNewLetter_API(randomIndex, targetLetter, mgc.tit_lang);


        RestoreBuyedLettersFromServer();
        PlaceBuyedLetter(targetLetter, randomIndex, MovieGuess_Controller.MovieGuess_instance.tit_lang);
        
    }


    public void RestoreBuyedLettersFromServer()
    {
        var title = MovieGuess_Controller.MovieGuess_instance.title;
        var lang = MovieGuess_Controller.MovieGuess_instance.tit_lang;
        var sublevel = PlayerInfoController.Player_Instance
            .playerData.levelsProgress[PlayerInfoController.Player_Instance.currentLevel - 1]
            .subLevels[PlayerInfoController.Player_Instance.currentMovie - 1];

        HelpLetters helpLetters = lang == MovieGuess_Controller.TitleLanguage.es
            ? sublevel.help.helpLetters_es
            : sublevel.help.helpLetters_en;

        Dictionary<int, string> targetDict = lang == MovieGuess_Controller.TitleLanguage.es
            ? buyedLetters_es
            : buyedLetters_en;

        if (helpLetters != null && helpLetters.letters > 0 && targetDict.Count == 0)
        {
            Debug.Log($"♻️ Restaurando {helpLetters.letters} letras compradas del servidor...");

            int placed = 0;

            for (int i = 0; i < title.Length && placed < helpLetters.letters; i++)
            {
                if (title[i] == ' ') continue;

                if (!targetDict.ContainsKey(i))
                {
                    string letter = title[i].ToString().ToUpper();
                    targetDict.Add(i, letter);
                    placed++;

                    // 🔄 Colocar visualmente también
                    StartCoroutine(MoverLetraConDelay(letter, i, lang));

                    Debug.Log($"🔁 Letra restaurada y colocada: {letter} en posición {i}");
                }
            }
        }
        else
        {
            Debug.Log("✅ Letras ya restauradas o no hay letras compradas desde servidor.");
        }
    }



    public void PlaceBuyedLetter(string letter, int place, MovieGuess_Controller.TitleLanguage lang)
    {
        StartCoroutine(MoverLetraConDelay(letter, place, lang));
        CheckTitle(MovieGuess_Controller.MovieGuess_instance.title);
    }

    public void PlaceBuyedLetters()
    {
        Debug.Log("Place Buyed Letters");
        switch (MovieGuess_Controller.MovieGuess_instance.tit_lang)
        {
            case MovieGuess_Controller.TitleLanguage.es:
                Debug.Log("Buyed Español");
                foreach (var item in buyedLetters_es)
                {
                    StartCoroutine(MoverLetraConDelay(item.Value, item.Key, MovieGuess_Controller.TitleLanguage.es));

                }

                break;
            case MovieGuess_Controller.TitleLanguage.en:
                Debug.Log("Buyed English");
                foreach (var item in buyedLetters_en)
                {
                    StartCoroutine(MoverLetraConDelay(item.Value, item.Key, MovieGuess_Controller.TitleLanguage.en));

                }
                break;
            default:
                break;
        }
    }

    public void AutoPlaceAllCorrectLetters(string title)
    {
        Transform emptyGrid = Grid_Empty_Letter_Squares.transform;
        Transform letterGrid = Grid_Letter_Squares.transform;

        Debug.Log("🔠 Colocando todas las letras correctas automáticamente...");

        for (int i = 0; i < title.Length; i++)
        {
            string targetLetter = title[i].ToString().ToUpper();
            Transform targetSlot = emptyGrid.GetChild(i);

            // Saltar si ya tiene una letra colocada
            if (targetSlot.childCount > 0)
                continue;

            for (int j = 0; j < letterGrid.childCount; j++)
            {
                Transform letterSlot = letterGrid.GetChild(j);
                Sqr_letter_script script = letterSlot.GetComponent<Sqr_letter_script>();

                if (script != null && script.letter == targetLetter)
                {
                    letterSlot.SetParent(targetSlot, false);
                    letterSlot.localPosition = Vector3.zero;

                    Debug.Log($"✅ Letra '{targetLetter}' colocada en posición {i}", letterSlot.gameObject);
                    break;
                }
            }
        }
    }


    public IEnumerator MoverLetraConDelay(string letter, int place, MovieGuess_Controller.TitleLanguage lang)
    {
        yield return null; // Esperar un frame para asegurarnos de que todo esté instanciado

        Debug.Log($"[MoverLetraConDelay] Letra: {letter} --- Place: {place} --- Lang: {lang}");

        Transform emptyGrid = Grid_Empty_Letter_Squares.transform;
        Transform letterGrid = Grid_Letter_Squares.transform;

        if (letter == " ")
        {
            Debug.Log("❌ Saltamos colocación de espacio en la posición " + place);
            yield break;
        }

        if (emptyGrid == null || letterGrid == null)
        {
            Debug.LogError("❌ Grid_Empty_Letter_Squares o Grid_Letter_Squares es NULL.");
            yield break;
        }

        if (place < 0 || place >= emptyGrid.childCount)
        {
            Debug.LogError("❌ Índice de place fuera de rango: " + place);
            yield break;
        }

        Transform slot = emptyGrid.GetChild(place);

        // Buscar una letra disponible en el grid principal
        for (int j = 0; j < letterGrid.childCount; j++)
        {
            Transform letterSlot = letterGrid.GetChild(j);
            if (letterSlot == null)
                continue;

            Sqr_letter_script letterScript = letterSlot.GetComponent<Sqr_letter_script>();
            if (letterScript == null)
                continue;

            // Comparación segura
            if (letterScript.letter.Trim().ToUpper() == letter.Trim().ToUpper())
            {
                Debug.Log("Letra encontrada, la colocamos.", letterSlot.gameObject);

                letterSlot.SetParent(slot, false);
                letterSlot.localPosition = Vector3.zero;

                // 🔒 Desactivar botón para que no se pueda devolver
                Button btn = letterSlot.GetComponentInChildren<Button>();
                if (btn != null)
                {
                    btn.interactable = false;
                    Debug.Log("🔒 Botón desactivado para letra comprada: " + letter);
                }

                Debug.Log("Slot destino:", slot.gameObject);

                // Guardar en diccionario según idioma
                switch (lang)
                {
                    case MovieGuess_Controller.TitleLanguage.es:
                        if (!buyedLetters_es.ContainsKey(place))
                        {
                            buyedLetters_es.Add(place, letter);
                            Debug.Log("✅ Letra añadida al español: " + letter + " en posición " + place);
                        }
                        break;

                    case MovieGuess_Controller.TitleLanguage.en:
                        if (!buyedLetters_en.ContainsKey(place))
                        {
                            buyedLetters_en.Add(place, letter);
                            Debug.Log("✅ Letra añadida al inglés: " + letter + " en posición " + place);
                        }
                        break;
                }
                CheckTitle(MovieGuess_Controller.MovieGuess_instance.title);
                yield break; // Salimos tras colocar una letra
            }
        }

        Debug.LogWarning("⚠️ No se encontró ninguna letra disponible para colocar.");
    }



    #endregion

    public void CheckTitle(string title)
    {
        Transform grid = Grid_Empty_Letter_Squares.transform;

        if (grid.childCount != title.Length)
        {
            Debug.LogError("ERROR: cantidad de casillas no coincide con la longitud del título.");
            return;
        }

        StringBuilder checkTitle = new StringBuilder();

        for (int i = 0; i < title.Length; i++)
        {
            Transform slot = grid.GetChild(i);

            if (slot.childCount == 0)
            {
                if (title[i] == ' ')
                {
                    checkTitle.Append(" ");

                }
                else
                {
                    Debug.Log("ERROR: hay casillas vacías.");
                    return;
                }

            }
            else
            {
                TextMeshProUGUI textComponent = slot.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    checkTitle.Append(textComponent.text.ToUpper());
                }
                else
                {
                    Debug.Log("ERROR: no se encontró el texto en una letra.");
                    return;
                }
            }

            
        }

        if (checkTitle.ToString() == title.ToUpper())
        {
            Debug.Log("✅ Title check: IS CORRECT");
            MovieGuess_Controller.MovieGuess_instance.IsCorrectTitle(true);
            return;
        }
        else
        {
            Debug.Log("❌ Title check: IS WRONG");
            MovieGuess_Controller.MovieGuess_instance.IsIncorrectTitle(true);
            return;
        }
    }

    public void DisableEmptyLetterSquares()
    {
        foreach (Transform child in Grid_Empty_Letter_Squares.transform)
        {
            Button btn = child.GetComponentInChildren<Button>();
            if (btn != null)
            {
                btn.interactable = false;
            }
        }

        Debug.Log("🛑 Letras del grid de respuesta desactivadas");
    }

    #region tnt
    public void EliminarLetrasFalsas()
    {
        foreach (GameObject fake in fakeLetters)
        {
            if (fake != null)
            {
                Destroy(fake);
            }
        }

        fakeLetters.Clear();

        tnt = true;

        mgc._api.UseHelpBomb(mgc.pic.currentLevel, mgc.pic.currentMovie,
            onSuccess: response =>
            {
                Debug.Log("Post TNT Movie SUCCESS: " + response);
            },
                onError: error =>
                {
                    Debug.LogError("Post TNT Movie fallido: " + error);

                });
    }
    #endregion
}
