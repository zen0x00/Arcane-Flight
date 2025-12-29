using UnityEngine;
using TMPro;

public class ShoulderMovementTMPUI : MonoBehaviour
{
    public ShoulderRehabController shoulder;

    public TextMeshProUGUI leftText;
    public TextMeshProUGUI rightText;
    public TextMeshProUGUI combinedText;

    void Update()
    {
        leftText.text =
            $"Left: {shoulder.leftMovement}";

        rightText.text =
            $"Right: {shoulder.rightMovement}";

        combinedText.text =
            $"Movement: {shoulder.combinedMovement}";
    }
}
