using UnityEngine;

public class CamaraNave : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Vector3 offset = new Vector3(0, 1, -5);
    [SerializeField] private float suavizado = 10f;

    private Vector3 velocidad = Vector3.zero;
    private Vector3 rotacionObjetivo;

    private void LateUpdate()
    {
        Transform padre = transform.parent;
        if (padre == null) return;

        // Calcular posición objetivo (relativa al padre)
        Vector3 posicionObjetivo = padre.position +
                                  padre.forward * offset.z +
                                  padre.up * offset.y +
                                  padre.right * offset.x;

        // Suavizado de posición
        transform.position = Vector3.SmoothDamp(
            transform.position,
            posicionObjetivo,
            ref velocidad,
            suavizado * Time.deltaTime
        );

        // Mirar hacia donde mira la nave (ligeramente adelante)
        Vector3 lookTarget = padre.position + padre.forward * 10f;
        transform.LookAt(lookTarget);
    }
}