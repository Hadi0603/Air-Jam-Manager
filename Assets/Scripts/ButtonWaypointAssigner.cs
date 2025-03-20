using UnityEngine;
using UnityEngine.UI;

public class ButtonWaypointAssigner : MonoBehaviour
{
    public PlaneController planeController;
    public Canvas[] canvases;
    public Button[] buttons;
    public int[] stopWaypointIndices;
    public int[] targetWaypointIndices;

    void Start()
    {
        if (buttons.Length != stopWaypointIndices.Length || buttons.Length != targetWaypointIndices.Length)
        {
            Debug.LogError("Mismatch: Ensure buttons, stopWaypointIndices, and targetWaypointIndices arrays have the same length.");
            return;
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            int stopIndex = stopWaypointIndices[i];
            int targetIndex = targetWaypointIndices[i];
            buttons[i].onClick.AddListener(() =>
            {
                planeController.ContinueMovement(stopIndex, targetIndex);
                planeController.UpdateButtonStates();
            });

            canvases[i].enabled = false;
        }
    }
}