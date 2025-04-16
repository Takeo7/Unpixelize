using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;

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


    #region SetLetters

    public void SetEmptySquares(int length)
    {
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
    }

    public void SetLetterSquares(int correctLetterCount, string title, int fakeLetterCount)
    {
        originalLetters.Clear();
        fakeLetters.Clear();
        allLetters.Clear();

        // Crear letras correctas
        for (int i = 0; i < correctLetterCount; i++)
        {
            GameObject g = Instantiate(letter_square);
            string letter = title[i].ToString().ToUpper();
            g.GetComponentInChildren<Sqr_letter_script>().letter = letter;
            g.GetComponentInChildren<TextMeshProUGUI>().SetText(letter);

            originalLetters.Add(g);
        }

        // Crear letras falsas
        for (int i = 0; i < fakeLetterCount; i++)
        {
            GameObject g = Instantiate(letter_square);
            string randomLetter = GetRandomLetter();
            g.GetComponentInChildren<Sqr_letter_script>().letter = randomLetter;
            g.GetComponentInChildren<TextMeshProUGUI>().SetText(randomLetter);

            fakeLetters.Add(g);
        }

        // Unir y mezclar todas
        allLetters.AddRange(originalLetters);
        allLetters.AddRange(fakeLetters);
        ShuffleList(allLetters);

        // Asignar al grid
        foreach (GameObject g in allLetters)
        {
            g.transform.SetParent(Grid_Letter_Squares.transform, false);
            g.SetActive(true);
        }
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
        // Recorremos de derecha a izquierda para simular volver a la última posición libre
        /*for (int i = Grid_Letter_Squares.transform.childCount - 1; i >= 0; i--)
        {
            Transform slot = Grid_Letter_Squares.transform.GetChild(i);
            if (slot.childCount == 0)
                return slot;
        }
        return null;*/

        return Grid_Letter_Squares.transform;
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
    }
}
