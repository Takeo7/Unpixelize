using UnityEngine;

public class MovieID_Persistant : MonoBehaviour
{
    public static MovieID_Persistant Movie_ID { get; private set; }

    public int _movieID;

    private void Awake()
    {
        if (Movie_ID == null)
        {
            Movie_ID = this;
            //DontDestroyOnLoad(gameObject); // Opcional: Mantiene la instancia entre escenas
        }
        else
        {
            Destroy(gameObject); // Si ya existe una instancia, destruye la nueva
        }
    }

    public void SetMovieID(int id)
    {
        _movieID = id;
    }


}
