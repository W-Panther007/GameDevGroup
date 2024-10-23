using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    private GameObject CurrentPlacingTower;
    private PlayerMovement playerMovementScript;  // Reference to the PlayerMovement script

    void Start () 
    {
        // Get the PlayerMovement script from the player GameObject
        playerMovementScript = GetComponent<PlayerMovement>();
    }

    void Update ()
    {
        if (CurrentPlacingTower != null)
        {
            // Use the camera from the PlayerMovement script
            Camera playerCamera = playerMovementScript.PlayerCamera.GetComponent<Camera>();

            Ray camray = playerCamera.ScreenPointToRay(Input.mousePosition);

            // Raycast to determine the placement position
            if (Physics.Raycast(camray, out RaycastHit hitInfo, 100f))
            {
                // Calculate the bounds of the tower
                Bounds towerBounds = CurrentPlacingTower.GetComponent<Renderer>().bounds;

                // Adjust the Y position to ensure the tower is placed on the ground
                Vector3 newPosition = hitInfo.point;
                newPosition.y += towerBounds.extents.y; // Move it upwards by half the tower's height

                // Place the tower at the adjusted position
                CurrentPlacingTower.transform.position = newPosition;
            }

            // Place the tower if left mouse button is clicked
            if (Input.GetMouseButtonDown(0))
            {
                CurrentPlacingTower = null;
            }
        }
    }

    public void SetTowerToPlace(GameObject tower)
    {
        // Instantiate a new tower at the initial position
        CurrentPlacingTower = Instantiate(tower, Vector3.zero, Quaternion.identity);
    }
}
