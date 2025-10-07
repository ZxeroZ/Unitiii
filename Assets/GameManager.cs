using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton para acceder fácilmente desde otros scripts
    private Vector3 respawnPoint;
    private bool gameEnded = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Al inicio, el punto de respawn es donde empieza el jugador
        respawnPoint = GameObject.FindGameObjectWithTag("Player").transform.position;
    }

    // Actualiza el checkpoint cuando el jugador llega a uno nuevo
    public void UpdateCheckpoint(Vector3 newPoint)
    {
        respawnPoint = newPoint;
        Debug.Log($"Nuevo checkpoint guardado: {newPoint}");
    }

    // Reaparecer jugador cuando cae
    public void RespawnPlayer(GameObject player)
    {
        if (gameEnded) return;
        player.transform.position = respawnPoint;
        Debug.Log("Jugador reapareció en último checkpoint.");
        UIManager.Instance.ShowMessage("Has caído... reapareciendo.");
    }

    // Cuando gana el jugador
    public void WinGame()
    {
        if (gameEnded) return;
        gameEnded = true;
        Debug.Log("¡Ganaste!");
        UIManager.Instance.ShowMessage("¡Has ganado!");
        // Puedes agregar aquí: detener movimiento, activar menú, etc.
    }

    // Cuando pierde (por ejemplo, si caes fuera del mapa)
    public void LoseGame(GameObject player)
    {
        if (gameEnded) return;
        UIManager.Instance.ShowMessage("Has perdido ");
        RespawnPlayer(player);
    }
}