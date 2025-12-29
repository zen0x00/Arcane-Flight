using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class WifiUIController : MonoBehaviour
{
    [Header("System")]
    public LinuxWifiScanner scanner;
    public LinuxWifiConnector connector;

    [Header("UI References")]
    public Transform listParent;
    public GameObject buttonPrefab;
    public TMP_InputField passwordField;
    public TextMeshProUGUI statusText;
    public Button connectButton;

    [Header("Colors")]
    public Color selectedColor = new Color(0.2f, 0.6f, 1f, 1f);

    [Header("Scene Navigation")]
    [Tooltip("Scene to load after successful Wi-Fi connection")]
    public string sceneToLoadAfterConnect;

    // Internal state
    private string selectedSSID = null;
    private Button currentSelectedButton = null;
    private Color normalColor;

    // --------------------------------------------------
    // Scan Wi-Fi Networks
    // --------------------------------------------------
    public void Scan()
    {
        foreach (Transform t in listParent)
            Destroy(t.gameObject);

        selectedSSID = null;
        currentSelectedButton = null;

        passwordField.text = "";
        passwordField.interactable = false;
        connectButton.interactable = false;

        statusText.text = "Scanning Wi-Fi...";

        normalColor = buttonPrefab.GetComponent<Image>().color;

        string json = scanner.Scan();
        WifiData data = JsonUtility.FromJson<WifiData>(json);

        if (data == null || data.networks == null)
        {
            statusText.text = "No Wi-Fi networks found";
            return;
        }

        foreach (WifiNetwork net in data.networks)
        {
            if (string.IsNullOrEmpty(net.ssid))
                continue;

            GameObject btnObj = Instantiate(buttonPrefab, listParent);
            Button btn = btnObj.GetComponent<Button>();
            Image img = btnObj.GetComponent<Image>();
            TMP_Text label = btnObj.GetComponentInChildren<TMP_Text>();

            label.text = $"{net.ssid} ({net.signal}%)";

            btn.onClick.AddListener(() =>
            {
                HandleSelection(btn, net);
            });

            // Auto-select currently connected network
            if (net.connected)
            {
                HandleSelection(btn, net);
            }
        }

        statusText.text = "Select a Wi-Fi network";
    }

    // --------------------------------------------------
    // Handle Button Selection (Toggle)
    // --------------------------------------------------
    private void HandleSelection(Button btn, WifiNetwork net)
    {
        // Toggle OFF
        if (currentSelectedButton == btn)
        {
            btn.GetComponent<Image>().color = normalColor;
            currentSelectedButton = null;
            selectedSSID = null;

            passwordField.text = "";
            passwordField.interactable = false;
            connectButton.interactable = false;

            statusText.text = "Selection cleared";
            return;
        }

        // Reset previous
        if (currentSelectedButton != null)
            currentSelectedButton.GetComponent<Image>().color = normalColor;

        // Select new
        currentSelectedButton = btn;
        currentSelectedButton.GetComponent<Image>().color = selectedColor;
        selectedSSID = net.ssid;

        passwordField.interactable = net.security != "--";
        if (net.security == "--")
            passwordField.text = "";

        connectButton.interactable = true;
        statusText.text = "Selected: " + selectedSSID;
    }

    // --------------------------------------------------
    // Connect and Load Scene
    // --------------------------------------------------
    public void Connect()
    {
        if (string.IsNullOrEmpty(selectedSSID))
        {
            statusText.text = "Select a network first";
            return;
        }

        statusText.text = "Connecting...";
        connectButton.interactable = false;

        bool success = connector.Connect(
            selectedSSID,
            passwordField.interactable ? passwordField.text : ""
        );

        if (success)
        {
            statusText.text = "Connected. Loading...";
            passwordField.text = "";

            // Delay to ensure network stability
            Invoke(nameof(LoadNextScene), 1.5f);
        }
        else
        {
            statusText.text = "Connection failed";
            connectButton.interactable = true;
        }
    }

    // --------------------------------------------------
    // Scene Load
    // --------------------------------------------------
    private void LoadNextScene()
    {
        if (string.IsNullOrEmpty(sceneToLoadAfterConnect))
        {
            Debug.LogError("Scene name not set in WifiUIController");
            return;
        }

        SceneManager.LoadScene(sceneToLoadAfterConnect);
    }
}

#region Data Models

[System.Serializable]
public class WifiData
{
    public List<WifiNetwork> networks;
}

[System.Serializable]
public class WifiNetwork
{
    public string ssid;
    public int signal;
    public string security;
    public bool connected;
}

#endregion
