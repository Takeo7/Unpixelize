using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UnityEngine.UI;

public class MovieGuess_LettersController : MonoBehaviour
{
    #region Singleton
    public static MovieGuess_LettersController MovieGuessLetters_instance { get; private set; }

    private void Awake()
    {
        if (MovieGuessLetters_instance == null)
        {
            MovieGuessLetters_instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    public MovieGuess_Controller mgc;

    public GameObject Grid_Empty_Letter_Squares;
    public GameObject empty_letter_square;
    public List<Transform> emptySlots = new List<Transform>();
    public List<int> answerKeyToTitleIndex = new List<int>();

    public GameObject Grid_Letter_Squares;
    public GameObject letter_square;

    public List<GameObject> originalLetters = new List<GameObject>();
    public List<GameObject> fakeLetters = new List<GameObject>();
    public List<GameObject> allLetters = new List<GameObject>();

    bool tnt;

    public Dictionary<int, string> buyedLetters_es = new Dictionary<int, string>();
    public Dictionary<int, string> buyedLetters_en = new Dictionary<int, string>();

    public List<Transform> visibleLetterSlots = new List<Transform>();
    public string cleanedTitle;


    public void SetAllLetters(int length, int correctLetterCount, string title, int fakeLetterCount)
    {
        SetEmptySquares(length, correctLetterCount, title, fakeLetterCount);
    }

    public void SetEmptySquares(int length, int correctLetterCount, string title, int fakeLetterCount)
    {
        StartCoroutine(LayoutGridAndLetters(length, correctLetterCount, title, fakeLetterCount));
    }

    private IEnumerator LayoutGridAndLetters(int length, int correctLetterCount, string title, int fakeLetterCount)
    {
        foreach (Transform child in Grid_Empty_Letter_Squares.transform)
            Destroy(child.gameObject);

        emptySlots.Clear();
        visibleLetterSlots.Clear();
        answerKeyToTitleIndex.Clear();
        cleanedTitle = title.Replace(" ", "");

        yield return null;

        int maxPerRow = 8;
        string[] allWords = title.Split(' ');

        List<List<string>> plannedRows = new List<List<string>>();
        List<string> currentRowWords = new List<string>();
        int currentWidth = 0;

        foreach (string word in allWords)
        {
            if (string.IsNullOrEmpty(word)) continue;

            if (currentWidth > 0 && currentWidth + word.Length + 1 > maxPerRow)
            {
                plannedRows.Add(new List<string>(currentRowWords));
                currentRowWords.Clear();
                currentWidth = 0;
            }

            currentRowWords.Add(word);
            currentWidth += word.Length + 1;
        }

        if (currentRowWords.Count > 0)
            plannedRows.Add(new List<string>(currentRowWords));

        int titleIndex = 0;

        foreach (List<string> rowOfWords in plannedRows)
        {
            int contentWidth = 0;
            foreach (string w in rowOfWords)
                contentWidth += w.Length;
            contentWidth += rowOfWords.Count - 1;

            int emptySpace = maxPerRow - contentWidth;
            int leftPadding = Mathf.FloorToInt(emptySpace / 2f);

            for (int i = 0; i < leftPadding; i++)
            {
                GameObject filler = Instantiate(empty_letter_square, Grid_Empty_Letter_Squares.transform);
                filler.GetComponent<Image>().enabled = false;
            }

            for (int i = 0; i < rowOfWords.Count; i++)
            {
                string word = rowOfWords[i];

                foreach (char c in word)
                {
                    GameObject slot = Instantiate(empty_letter_square, Grid_Empty_Letter_Squares.transform);
                    slot.GetComponent<Image>().enabled = true;

                    emptySlots.Add(slot.transform);
                    visibleLetterSlots.Add(slot.transform);
                    answerKeyToTitleIndex.Add(titleIndex);
                    titleIndex++;
                }

                if (i < rowOfWords.Count - 1)
                {
                    GameObject spaceSlot = Instantiate(empty_letter_square, Grid_Empty_Letter_Squares.transform);
                    spaceSlot.GetComponent<Image>().enabled = false;
                    emptySlots.Add(spaceSlot.transform);
                    titleIndex++;
                }
            }

            int rightPadding = emptySpace - leftPadding;
            for (int i = 0; i < rightPadding; i++)
            {
                GameObject filler = Instantiate(empty_letter_square, Grid_Empty_Letter_Squares.transform);
                filler.GetComponent<Image>().enabled = false;
            }
        }

        SetLetterSquares(correctLetterCount, title, fakeLetterCount);
    }






    public void SetLetterSquares(int correctLetterCount, string title, int fakeLetterCount)
    {
        Debug.Log("Set Letters");

        originalLetters.Clear();
        fakeLetters.Clear();
        allLetters.Clear();

        foreach (Transform child in Grid_Letter_Squares.transform)
            Destroy(child.gameObject);

        // Letras correctas (solo las visibles)
        for (int i = 0; i < cleanedTitle.Length; i++)
        {
            string letter = cleanedTitle[i].ToString().ToUpper();

            GameObject g = Instantiate(letter_square);
            g.GetComponentInChildren<Sqr_letter_script>().letter = letter;
            g.GetComponentInChildren<TextMeshProUGUI>().SetText(letter);
            originalLetters.Add(g);
        }

        allLetters.AddRange(originalLetters);

        // Letras falsas
        if (!tnt)
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

        ShuffleList(allLetters);

        foreach (GameObject g in allLetters)
        {
            g.transform.SetParent(Grid_Letter_Squares.transform, false);
            g.SetActive(true);
        }

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
    public void AutoPlaceNextCorrectLetter(string title)
    {
        List<int> libres = new List<int>();

        for (int i = 0; i < visibleLetterSlots.Count; i++)
        {
            if (visibleLetterSlots[i].childCount == 0)
                libres.Add(i);
        }

        if (libres.Count == 0)
        {
            Debug.Log("[AUTO] No hay huecos libres.");
            return;
        }

        int idx = libres[Random.Range(0, libres.Count)];
        string letra = cleanedTitle[idx].ToString().ToUpper();

        int indexEnTituloOriginal = GetIndexInOriginalTitle(title, idx);

        mgc.PostAddNewLetter_API(indexEnTituloOriginal, letra, mgc.tit_lang);
        PlaceBuyedLetter(letra, idx, mgc.tit_lang);
    }


    private int GetIndexInOriginalTitle(string fullTitle, int indexSinEspacios)
    {
        int count = 0;
        for (int i = 0; i < fullTitle.Length; i++)
        {
            if (fullTitle[i] != ' ')
            {
                if (count == indexSinEspacios)
                    return i;
                count++;
            }
        }
        return -1;
    }




    public IEnumerator MoverLetraConDelay(string letter, int indexInCleanedTitle, MovieGuess_Controller.TitleLanguage lang)
    {
        yield return null;

        Transform slot = visibleLetterSlots[indexInCleanedTitle];

        if (slot.childCount > 0)
        {
            Debug.LogWarning($"[MOVER] Slot #{indexInCleanedTitle} ya estaba ocupado.");
            yield break;
        }

        Transform letterGrid = Grid_Letter_Squares.transform;

        for (int j = letterGrid.childCount - 1; j >= 0; j--)
        {
            Transform letterSlot = letterGrid.GetChild(j);
            var script = letterSlot.GetComponent<Sqr_letter_script>();

            if (script != null && script.letter == letter)
            {
                letterSlot.SetParent(slot, false);
                letterSlot.localPosition = Vector3.zero;

                Button btn = letterSlot.GetComponentInChildren<Button>();
                if (btn != null) btn.interactable = false;

                Dictionary<int, string> targetDict = (lang == MovieGuess_Controller.TitleLanguage.es)
                    ? buyedLetters_es
                    : buyedLetters_en;

                if (!targetDict.ContainsKey(indexInCleanedTitle))
                    targetDict.Add(indexInCleanedTitle, letter);

                yield break;
            }
        }

        Debug.LogWarning($"[MOVER] Letra '{letter}' no encontrada en grid inferior.");
    }


    public void RestoreBuyedLettersFromServer()
    {
        string title = MovieGuess_Controller.MovieGuess_instance.title;
        string titleSinEspacios = cleanedTitle;
        var lang = MovieGuess_Controller.MovieGuess_instance.tit_lang;
        PlayerInfoController pic = PlayerInfoController.Player_Instance;

        var sublevel = pic.playerData.levelsProgress[pic.currentLevel - 1]
            .subLevels[(pic.currentMovie - 1) - (9 * (pic.currentLevel - 1))];

        HelpLetters helpLetters = lang == MovieGuess_Controller.TitleLanguage.es
            ? sublevel.help.helpLetters_es
            : sublevel.help.helpLetters_en;

        Dictionary<int, string> targetDict = lang == MovieGuess_Controller.TitleLanguage.es
            ? buyedLetters_es
            : buyedLetters_en;

        // Limpia solo los visibles
        foreach (Transform slot in visibleLetterSlots)
        {
            if (slot.childCount > 0)
                Destroy(slot.GetChild(0).gameObject);
        }

        Debug.Log("🧹 Limpieza completada de letras compradas en los slots visibles");

        if (helpLetters != null && helpLetters.letters > 0)
        {
            Debug.Log($"♻️ Restaurando {helpLetters.letters} letras compradas del servidor...");

            if (targetDict.Count == 0)
            {
                List<int> posiblesIndices = new List<int>();
                for (int i = 0; i < titleSinEspacios.Length; i++)
                    posiblesIndices.Add(i);

                for (int i = 0; i < posiblesIndices.Count; i++)
                {
                    int randomIndex = Random.Range(i, posiblesIndices.Count);
                    (posiblesIndices[i], posiblesIndices[randomIndex]) = (posiblesIndices[randomIndex], posiblesIndices[i]);
                }

                int placed = 0;
                foreach (int index in posiblesIndices)
                {
                    if (placed >= helpLetters.letters) break;
                    string letter = titleSinEspacios[index].ToString().ToUpper();
                    targetDict.Add(index, letter);
                    placed++;
                }
            }

            foreach (var kvp in targetDict)
            {
                StartCoroutine(MoverLetraConDelay(kvp.Value, kvp.Key, lang));
            }
        }
        else
        {
            Debug.Log("✅ No hay letras compradas para este idioma.");
        }

        mgc.HideSoftLoadingScreen();
    }


    public void PlaceBuyedLetter(string letter, int indexInCleanedTitle, MovieGuess_Controller.TitleLanguage lang)
    {
        if (indexInCleanedTitle < 0 || indexInCleanedTitle >= visibleLetterSlots.Count)
        {
            Debug.LogError($"❌ Índice inválido {indexInCleanedTitle} en PlaceBuyedLetter");
            return;
        }

        StartCoroutine(MoverLetraConDelay(letter, indexInCleanedTitle, lang));
        CheckTitle(MovieGuess_Controller.MovieGuess_instance.title);
    }


  




    public void CheckTitle(string title)
    {
        if (visibleLetterSlots.Count != cleanedTitle.Length)
        {
            Debug.LogError("❌ Error: cantidad de slots visibles no coincide con título sin espacios.");
            return;
        }

        StringBuilder constructed = new StringBuilder();

        for (int i = 0; i < visibleLetterSlots.Count; i++)
        {
            Transform slot = visibleLetterSlots[i];

            if (slot.childCount > 0)
            {
                constructed.Append(slot.GetComponentInChildren<TextMeshProUGUI>().text.ToUpper());
            }
            else
            {
                MovieGuess_Controller.MovieGuess_instance.IsIncorrectTitle(false);
                return;
            }
        }

        if (constructed.ToString() == cleanedTitle.ToUpper())
        {
            Debug.Log("✅ Title check: IS CORRECT");
            MovieGuess_Controller.MovieGuess_instance.IsCorrectTitle(true);
        }
        else
        {
            Debug.Log("❌ Title check: IS WRONG");
            MovieGuess_Controller.MovieGuess_instance.IsIncorrectTitle(true);
        }
    }


    public void AutoPlaceAllCorrectLetters(string title)
    {
        if (answerKeyToTitleIndex.Count != visibleLetterSlots.Count)
        {
            Debug.LogWarning($"[AUTO ALL] Mismatch: answerKeyToTitleIndex ({answerKeyToTitleIndex.Count}) vs visibleLetterSlots ({visibleLetterSlots.Count})");
            return;
        }

        string cleaned = title.Replace(" ", "");

        for (int i = 0; i < visibleLetterSlots.Count; i++)
        {
            if (visibleLetterSlots[i].childCount > 0) continue;

            if (i >= cleaned.Length)
            {
                Debug.LogWarning($"[AUTO ALL] Slot #{i} fuera de rango para cleanedTitle (len={cleaned.Length})");
                continue;
            }

            string letter = cleaned[i].ToString().ToUpper();
            int titleIndex = answerKeyToTitleIndex[i];

            Debug.Log($"[AUTO ALL] Colocando letra '{letter}' en slot #{i} → título[{titleIndex}]");
            mgc.PostAddNewLetter_API(titleIndex, letter, mgc.tit_lang);
            PlaceBuyedLetter(letter, i, mgc.tit_lang);
        }
    }



    public void DisableEmptyLetterSquares()
    {
        foreach (Transform child in Grid_Empty_Letter_Squares.transform)
        {
            Button btn = child.GetComponentInChildren<Button>();
            if (btn != null) btn.interactable = false;
        }
    }

    public void EliminarLetrasFalsas()
    {
        foreach (GameObject fake in fakeLetters)
        {
            if (fake != null) Destroy(fake);
        }
        fakeLetters.Clear();
        tnt = true;
    }

    public Transform GetAvailableGridSlot()
    {
        return Grid_Letter_Squares.transform;
    }

    public Transform GetNextEmptySlot()
    {
        foreach (Transform slot in emptySlots)
        {
            Image slotImage = slot.GetComponent<Image>();
            if (slotImage != null && slotImage.enabled && slot.childCount == 0)
            {
                return slot;
            }
        }
        return null;
    }
}
