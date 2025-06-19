using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;


public class MainMenu_Controller : MonoBehaviour
{
    
    public Scene_Controller sc;

    [Space]
    public TMP_InputField email;
    public string email_t;
    public TMP_InputField password;
    public string password_t;

    [Space]
    public GameObject errorScreen;
    public TextMeshProUGUI errorText;
    public Color errorColor;

    [Space]
    public bool deleteData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (deleteData)
        {
            PlayerPrefs.DeleteAll();
        }

        StartCoroutine(CheckInternetConnection(isConnected =>
        {
            if (isConnected)
            {
                if (PlayerPrefs.HasKey("REGISTERED_EMAIL"))
                {
                    StartLogin();
                }
                else
                {
                    StartRegister();
                }
            }
            else
            {
                Debug.LogWarning("Sin conexión a internet.");
                errorText.text = "NO INTERNET";
            }
        }));



        BugReportingScript.bugInstance.ResetCamera();



    }

    public IEnumerator CheckInternetConnection(System.Action<bool> callback)
    {
        UnityWebRequest request = new UnityWebRequest("https://www.google.com");
        request.method = UnityWebRequest.kHttpVerbHEAD; // Más rápido, sin descargar datos
        request.timeout = 5; // segundos

        yield return request.SendWebRequest();

        bool isConnected = !request.result.Equals(UnityWebRequest.Result.ConnectionError) &&
                           !request.result.Equals(UnityWebRequest.Result.ProtocolError);

        Debug.Log($"🌐 Conexión a internet: {(isConnected ? "Disponible" : "No disponible")}");
        callback?.Invoke(isConnected);
    }

    public void StartLogin()
    {
        Debug.Log("Start Login");
        if (ApiClient.Instance.Testing)
        {
            email_t = "test@example.com";
            password_t = "password";
        }
        else
        {
            email_t = PlayerPrefs.GetString("REGISTERED_EMAIL");
            Debug.Log("USER: " + email_t);
            password_t = PlayerPrefs.GetString("password");
            Debug.Log("PASSWORD: " + password_t);
        }

        ApiClient.Instance.Login(email_t, password_t,
        onSuccess: response =>
        {
            string token = ApiClient.Instance.authToken;
            PlayerInfoController.Player_Instance.playerData.authToken = token;
            ApiClient.Instance.authToken = token;

            //errorText.gameObject.SetActive(true);
            //errorText.color = Color.green;
            //errorText.text = "Login exitoso, cargando info";

            /*
            ApiClient.Instance.GetLevels(
                levelsResponse => 
                { 
                    Debug.Log("Niveles recibidos: " + levelsResponse);
                    NextScene();
                },
                error => Debug.LogError("Error cargando niveles: " + error));
            */

            ApiClient.Instance.GetPlayerInfo(
            onSuccess: info => {
                Debug.Log("✅ Amount actualizado correctamente."+ info.amount);
                PlayerInfoController.Player_Instance.SetPopcorns(info.amount);
                NextScene();
            },
             onError: err => {
                Debug.LogWarning("⚠️ No se pudo obtener amount: " + err);
                });
            
        },
        onError: error =>
        {
            Debug.LogError("Login fallido: " + error);

            errorScreen.SetActive(true);
            errorText.color = errorColor;
            errorText.text = "Sorry,  something didnt go as expected.";

        });
    }

    
    public void StartRegister()
    {
        ApiClient.Instance.Register(
    onSuccess: res => {
        Debug.Log("Registro exitoso: " + res);
        ApiClient.Instance.GetPlayerInfo(
            onSuccess: info => {
                Debug.Log("✅ Amount actualizado correctamente." + info.amount);
                PlayerInfoController.Player_Instance.SetPopcorns(info.amount);
                NextScene();
            },
             onError: err => {
                 Debug.LogWarning("⚠️ No se pudo obtener amount: " + err);
             });
    }
    ,
    onError: err => Debug.LogError("❌ Registro fallido: " + err)
);
    }

    
    

  



    public void NextScene()
    {
        sc.ChangeScene(Scene_Controller.Scenes.LevelSelector);
    }
}
