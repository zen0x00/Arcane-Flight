using UnityEngine;

public class QuitGame : MonoBehaviour
{
    // Called by UI Button
    public void QuitApp()
    {
        Debug.Log("Quit button pressed");

        // Closes the Linux application
        Application.Quit();

#if UNITY_EDITOR
        // Stop Play Mode in Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
