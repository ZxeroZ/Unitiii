using UnityEngine;

public class PlayerController : MonoBehaviour
{

    // Velocidad de movimiento, fuwerza de salto, sensibilidad del mouse, velocidad de caída, etc.
    [Header("Configuración de Movimiento")]
    public float moveSpeed = 6f;

    [Header("Configuración de Salto")]
    public float jumpForce = 7f;
    public float fallMultiplier = 2.5f; 
    public float trampolineForce = 20f; 

    // Configuración de la camara 

    [Header("Configuración de Cámara")]
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;
    public Transform cameraFollowTarget; // Objeto vacío que sigue la cámara en 3ra persona.
    public float mouseSensitivity = 100f;

    [Header("Detección de Suelo")]
    public Transform groundCheck;       // Objeto en los pies para saber si tocamos el suelo.
    public float groundCheckRadius = 0.4f; // Radio de detección del suelo.
    public LayerMask groundLayer;       // Define qué capas son consideradas "suelo".

    // --- VARIABLES PRIVADAS (para uso interno del script) ---
    private Rigidbody rb;
    private bool isGrounded;
    private float xRotation = 0f;
    private Vector3 respawnPoint;

    // Se ejecuta UNA SOLA VEZ al empezar el juego.
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked; // Bloquea el cursor en el centro de la pantalla.
        SwitchToThirdPerson(); // Empezamos en tercera persona.
        respawnPoint = transform.position; // El punto de inicio es el primer checkpoint.
    }

    void Update()
    {
        // detección de suelo con una esfera invisible.

        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        // --- MOVIMIENTO CON WASD ---
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        rb.linearVelocity = new Vector3(move.x * moveSpeed, rb.linearVelocity.y, move.z * moveSpeed);

        // --- SALTO ---
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // Reseteamos la velocidad vertical para un salto consistente.
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange); // VelocityChange ignora la masa para un salto más arcade.
        }

        // --- MEJORA DE SALTO: CAÍDA RÁPIDA ---
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }

        // --- CONTROL DE CÁMARA CON MOUSE ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f); // Limita la rotación vertical.
        firstPersonCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        cameraFollowTarget.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // --- CAMBIO DE CÁMARA ---
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (firstPersonCamera.gameObject.activeSelf) SwitchToThirdPerson();
            else SwitchToFirstPerson();
        }
    }

    // Se ejecuta cuando el jugador ENTRA en un colisionador marcado como "Trigger".
    private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("DeathZone"))
    {
        GameManager.Instance.RespawnPlayer(gameObject);
    }
    else if (other.CompareTag("Checkpoint"))
    {
        Vector3 checkpointPos = other.transform.position + new Vector3(0, 2f, 0);
        GameManager.Instance.UpdateCheckpoint(checkpointPos);
        UIManager.Instance.ShowMessage("Checkpoint alcanzado");
        other.gameObject.SetActive(false);
    }
    else if (other.CompareTag("Finish"))
    {
        GameManager.Instance.WinGame();
    }
}
    // Se ejecuta cuando el jugador CHOCA FÍSICAMENTE con otro colisionador.
    private void OnCollisionEnter(Collision collision)
    {
        // Si chocamos con una plataforma de salto
        if (collision.gameObject.CompareTag("Trampoline"))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // Reseteamos velocidad para un impulso limpio.
            rb.AddForce(Vector3.up * trampolineForce, ForceMode.Impulse); // Impulse da un "golpe" de fuerza instantáneo.
        }
    }

    // --- FUNCIONES PARA CAMBIAR CÁMARAS ---
    void SwitchToFirstPerson()
    {
        firstPersonCamera.gameObject.SetActive(true);
        thirdPersonCamera.gameObject.SetActive(false);
    }

    void SwitchToThirdPerson()
    {
        firstPersonCamera.gameObject.SetActive(false);
        thirdPersonCamera.gameObject.SetActive(true);
    }
}
