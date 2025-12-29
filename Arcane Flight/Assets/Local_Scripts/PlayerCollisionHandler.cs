using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCollisionHandler : MonoBehaviour
{
    [Header("References")]
    public PlaneMovementController plane;
    public GameObject gameOverPanel;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collided object is an obstacle
        if (!other.CompareTag("Obstacle"))
            return;

        // Stop plane movement
        if (plane != null)
            plane.StopGame();

        // Show Game Over UI
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Debug.Log("GAME OVER - Obstacle Hit");
    }

    // 🔥 Called by Restart Button
    public void RestartGame()
    {
        // Ensure time is running
        Time.timeScale = 1f;

        // Reload the current scene
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}
