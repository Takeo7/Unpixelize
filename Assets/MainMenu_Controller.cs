using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu_Controller : MonoBehaviour
{
    
    public Scene_Controller sc;

    [Space]
    public TMP_InputField email;
    public string email_t;
    public TMP_InputField password;
    public string password_t;

    [Space]
    public TextMeshProUGUI errorText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //sc.PrepareScene(Scene_Controller.Scenes.LevelSelector);
        StartLogin();
    }


    public void StartLogin()
    {
        Debug.Log("Start Login");
        if (ApiClient.Instance.Testing)
        {
            email_t = "test@example.com";
            password_t = "password";
        }else if (email.text == null)
        {
            email_t = "test@example.com";
            password_t = "password";
        }
        else
        {
            email_t = email.text;
            password_t = password.text;
        }
        ApiClient.Instance.Login(email_t, password_t,
        onSuccess: response =>
        {
            string token = ExtractTokenFromResponse(response);
            PlayerInfoController.Player_Instance.playerData.authToken = token;
            ApiClient.Instance.authToken = token;

            errorText.gameObject.SetActive(true);
            errorText.color = Color.green;
            errorText.text = "Login exitoso, cargando info";

            /*
            ApiClient.Instance.GetLevels(
                levelsResponse => 
                { 
                    Debug.Log("Niveles recibidos: " + levelsResponse);
                    NextScene();
                },
                error => Debug.LogError("Error cargando niveles: " + error));
            */

            NextScene();
        },
        onError: error =>
        {
            Debug.LogError("Login fallido: " + error);

            errorText.gameObject.SetActive(true);
            errorText.color = Color.red;
            errorText.text = "Login fallido: " + error;

        });
    }

    

    

    [System.Serializable]
    public class LoginResponse
    {
        public string token;
    }

    string ExtractTokenFromResponse(string json)
    {
        LoginResponse data = JsonUtility.FromJson<LoginResponse>(json);
        return data.token;
    }



    public void NextScene()
    {
        sc.ChangeScene(Scene_Controller.Scenes.LevelSelector);
    }
}
