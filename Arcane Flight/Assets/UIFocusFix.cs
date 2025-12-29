using UnityEngine;
using UnityEngine.EventSystems;

public class UIFocusFix : MonoBehaviour
{
    public GameObject firstButton;

    void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(firstButton);
    }
}
