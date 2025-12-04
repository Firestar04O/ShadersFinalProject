using UnityEngine;
using UnityEngine.InputSystem;

public class NaveMovement : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform modeloNave; // Referencia al modelo visual
    [SerializeField] private Camera camaraNave;

    [Header("Corrección Rotación Modelo")]
    [SerializeField] private Vector3 correccionRotacionModelo = new Vector3(-90, 0, 0);

    [Header("Movimiento Principal")]
    [SerializeField] private float velocidad = 25f;
    [SerializeField] private float velocidadStrafe = 15f;
    [SerializeField] private float maxVelocidad = 40f;

    [Header("Rotación con Mouse")]
    [SerializeField] private float sensibilidadMouse = 2f;
    [SerializeField] private float limitePitch = 80f;
    [SerializeField] private float maxAnguloInclinacion = 30f;

    [Header("Límites del Mundo")]
    [SerializeField] private float alturaMaxima = 50f;
    [SerializeField] private float alturaMinima = 5f;

    [Header("Input")]
    private Vector2 mouseInput;
    private float inputAdelante = 0f;
    private float inputStrafe = 0f;

    // Variables de estado
    private float pitchActual = 0f;
    private float yawActual = 0f;
    private float inclinacionVisual = 0f;
    private Quaternion rotacionBaseModelo; // Guardar rotación base

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (camaraNave == null) camaraNave = Camera.main;

        // Buscar modelo automáticamente si no está asignado
        if (modeloNave == null && transform.childCount > 0)
        {
            modeloNave = transform.GetChild(0);
            Debug.Log($"Modelo encontrado: {modeloNave.name}");
        }

        // GUARDAR la rotación base del modelo
        if (modeloNave != null)
        {
            rotacionBaseModelo = modeloNave.localRotation;
            Debug.Log($"Rotación base del modelo: {rotacionBaseModelo.eulerAngles}");

            // Aplicar corrección inicial
            modeloNave.localRotation = rotacionBaseModelo * Quaternion.Euler(correccionRotacionModelo);
        }
    }

    private void Start()
    {
        ConfigurarFisica();
        ConfigurarCursor();

        // Inicializar rotación de la nave (no del modelo)
        yawActual = transform.eulerAngles.y;
        pitchActual = transform.eulerAngles.x;

        Debug.Log($"Nave rotación inicial: {transform.eulerAngles}");
        if (modeloNave != null)
            Debug.Log($"Modelo rotación inicial: {modeloNave.localEulerAngles}");
    }

    private void ConfigurarFisica()
    {
        rb.useGravity = false;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 2f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;
    }

    private void ConfigurarCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ========== INPUT SYSTEM ==========

    public void OnMouseLook(InputAction.CallbackContext context)
    {
        mouseInput = context.ReadValue<Vector2>();
    }

    public void OnAdelante(InputAction.CallbackContext context)
    {
        inputAdelante = context.ReadValue<float>();
    }

    public void OnStrafe(InputAction.CallbackContext context)
    {
        inputStrafe = context.ReadValue<float>();
    }

    private void FixedUpdate()
    {
        RotarConMouse();
        AplicarMovimiento();
        AplicarLimites();
        ActualizarInclinacionVisual();
    }

    private void RotarConMouse()
    {
        if (mouseInput.magnitude > 0.1f)
        {
            yawActual += mouseInput.x * sensibilidadMouse;
            pitchActual -= mouseInput.y * sensibilidadMouse * 0.5f;
            pitchActual = Mathf.Clamp(pitchActual, -limitePitch, limitePitch);

            // Rotar SOLO el GameObject padre (la nave física)
            transform.rotation = Quaternion.Euler(pitchActual, yawActual, 0);

            inclinacionVisual = Mathf.Lerp(
                inclinacionVisual,
                -mouseInput.x * maxAnguloInclinacion,
                8f * Time.fixedDeltaTime
            );
        }
        else
        {
            inclinacionVisual = Mathf.Lerp(inclinacionVisual, 0f, 5f * Time.fixedDeltaTime);
        }
    }

    private void AplicarMovimiento()
    {
        Vector3 movimiento = Vector3.zero;

        if (inputAdelante > 0.1f)
        {
            movimiento += transform.forward * inputAdelante * velocidad;
        }

        if (Mathf.Abs(inputStrafe) > 0.1f)
        {
            movimiento += transform.right * inputStrafe * velocidadStrafe;
        }

        float movimientoVertical = -Mathf.Sin(pitchActual * Mathf.Deg2Rad) * velocidad;
        movimiento += Vector3.up * movimientoVertical;

        if (movimiento.magnitude > 0.1f)
        {
            rb.AddForce(movimiento, ForceMode.Force);

            if (rb.linearVelocity.magnitude > maxVelocidad)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxVelocidad;
            }
        }
        else
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, 0.5f * Time.fixedDeltaTime);
        }
    }

    private void AplicarLimites()
    {
        Vector3 posicion = transform.position;
        posicion.y = Mathf.Clamp(posicion.y, alturaMinima, alturaMaxima);
        transform.position = posicion;

        if (posicion.y >= alturaMaxima && rb.linearVelocity.y > 0)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (posicion.y <= alturaMinima && rb.linearVelocity.y < 0)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
    }

    private void ActualizarInclinacionVisual()
    {
        if (modeloNave != null)
        {
            // Aplicar: Rotación base + Corrección + Inclinación actual
            Quaternion rotacionFinal = rotacionBaseModelo *
                                     Quaternion.Euler(correccionRotacionModelo) *
                                     Quaternion.Euler(0, 0, inclinacionVisual);

            modeloNave.localRotation = Quaternion.Slerp(
                modeloNave.localRotation,
                rotacionFinal,
                10f * Time.fixedDeltaTime
            );
        }
    }
}