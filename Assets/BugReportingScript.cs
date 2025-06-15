using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BugReportingScript : MonoBehaviour
{
    public static BugReportingScript bugInstance;
    private void Awake()
    {
        if (bugInstance == null)
        {
            bugInstance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetCamera()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
    }


    public TMP_InputField reportInput;

    public void SendReport()
    {
        string reportText = reportInput.text + "---" + SceneManager.GetActiveScene().name;

        ApiClient.Instance.SendReport(
            reportText,
            onSuccess: response =>
            {
                Debug.Log("✅ Reporte enviado correctamente: " + response);
            },
            onError: error =>
            {
                Debug.LogError("❌ Fallo al enviar reporte: " + error);
            });

        reportInput.text = "";

    }
}
