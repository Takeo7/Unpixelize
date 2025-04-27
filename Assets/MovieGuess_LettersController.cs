using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;
using System.Collections;

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

        SetLetterSquares(correctLetterCount, title, fakeLetterCount);
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
            g.GetComponentInChildren<Sqr_letter_script>().letter = letter;
            g.GetComponentInChildren<TextMeshProUGUI>().SetText(letter);

            originalLetters.Add(g);
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

        PlaceBuyedLetters();
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
        foreach (Transform slot in emptySlots)
        {
            if (slot.childCount == 0)
                return slot;
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

        PlaceBuyedLetter(targetLetter, randomIndex, MovieGuess_Controller.MovieGuess_instance.tit_lang);
        
    }

    public void PlaceBuyedLetter(string letter, int place, MovieGuess_Controller.TitleLanguage lang)
    {
        StartCoroutine(MoverLetraConDelay(letter, place, lang));
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

    public IEnumerator MoverLetraConDelay(string letter, int place, MovieGuess_Controller.TitleLanguage lang)
    {
        yield return null; // Esperar un frame para asegurarnos de que todo esté instanciado

        Debug.Log($"[MoverLetraConDelay] Letra: {letter} --- Place: {place} --- Lang: {lang}");

        Transform emptyGrid = Grid_Empty_Letter_Squares.transform;
        Transform letterGrid = Grid_Letter_Squares.transform;

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

                yield break; // Salimos tras colocar una letra
            }
        }

        Debug.LogWarning("⚠️ No se encontró ninguna letra disponible para colocar.");
    }



    #endregion

    public bool CheckTitle(string title)
    {
        Transform grid = Grid_Empty_Letter_Squares.transform;

        if (grid.childCount != title.Length)
        {
            Debug.LogError("ERROR: cantidad de casillas no coincide con la longitud del título.");
            return false;
        }

        StringBuilder checkTitle = new StringBuilder();

        for (int i = 0; i < title.Length; i++)
        {
            Transform slot = grid.GetChild(i);

            if (slot.childCount == 0)
            {
                Debug.Log("ERROR: hay casillas vacías.");
                return false;
            }

            TextMeshProUGUI textComponent = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                checkTitle.Append(textComponent.text.ToUpper());
            }
            else
            {
                Debug.Log("ERROR: no se encontró el texto en una letra.");
                return false;
            }
        }

        if (checkTitle.ToString() == title.ToUpper())
        {
            Debug.Log("✅ Title check: IS CORRECT");
            return true;
        }
        else
        {
            Debug.Log("❌ Title check: IS WRONG");
            return false;
        }
    }

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
    }
}
