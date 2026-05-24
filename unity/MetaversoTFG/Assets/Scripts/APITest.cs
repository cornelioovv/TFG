using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class APITest : MonoBehaviour
{
    private const string API_URL = "http://localhost:3000/api/obras";

    [Header("Visual")]
    public Material materialNormal;
    public Material materialActivado;

    private MeshRenderer meshRenderer;
    private bool puedeActivar = true;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Este método se llama cuando el player hace click
    void OnMouseDown()
    {
        if (puedeActivar)
        {
            ActivarBoton();
        }
    }

    void ActivarBoton()
    {
        Debug.Log("Botón presionado!");

        // Cambiar color temporalmente
        if (materialActivado != null)
        {
            meshRenderer.material = materialActivado;
        }

        // Llamar API
        StartCoroutine(ObtenerObras());
    }

    IEnumerator ObtenerObras()
    {
        puedeActivar = false;

        Debug.Log("Llamando a la API...");

        UnityWebRequest request = UnityWebRequest.Get(API_URL);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("API respondió correctamente!");
            Debug.Log("Datos recibidos: " + request.downloadHandler.text);

            // Contar obras
            string json = request.downloadHandler.text;
            int numeroObras = json.Split(new string[] { "\"id\":" }, System.StringSplitOptions.None).Length - 1;
            Debug.Log($"Hay {numeroObras} obras en la base de datos");
        }
        else
        {
            Debug.LogError("Error: " + request.error);
            Debug.LogError("¿Está el servidor corriendo?");
        }

        // Volver a color normal
        yield return new WaitForSeconds(0.5f);
        if (materialNormal != null)
        {
            meshRenderer.material = materialNormal;
        }

        puedeActivar = true;
    }
}