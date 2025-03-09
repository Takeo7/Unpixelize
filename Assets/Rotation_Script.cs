using UnityEngine;

public class Rotation_Script : MonoBehaviour
{
    public float velocidadRotacion = 100f; // Grados por segundo

    void Update()
    {
        transform.Rotate(0, 0, -velocidadRotacion * Time.deltaTime);
    }
}
