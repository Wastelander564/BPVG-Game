using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraSwitcher : MonoBehaviour
{
    public Camera camera1;
    public Camera camera2;

    private bool switchToCamera2 = false;

    void Start()
    {
        // Ensure both cameras are initially enabled
        camera1.enabled = true;
        camera2.enabled = false;
    }

    void Update()
    {
        // Check for input to toggle between cameras
        if (Input.GetKeyDown(KeyCode.X))
        {
            switchToCamera2 = !switchToCamera2; // Toggle the switch state

            // Enable the appropriate camera based on the switch state
            camera1.enabled = !switchToCamera2;
            camera2.enabled = switchToCamera2;
        }
    }
}
