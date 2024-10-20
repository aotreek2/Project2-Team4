using UnityEngine;

public class SystemClickHandler : MonoBehaviour
{
    void Update()
    {
        // Detect right-click
        if (Input.GetMouseButtonDown(1)) // 1 is the right mouse button
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                CubeInteraction cube = hit.collider.GetComponent<CubeInteraction>();
                if (cube != null)
                {
                    Debug.Log($"SystemClickHandler: Right-clicked on {cube.systemType}.");
                    cube.OnSystemClicked();
                }
                else
                {
                    Debug.Log($"SystemClickHandler: Right-clicked on object {hit.collider.gameObject.name} without CubeInteraction.");
                }
            }
        }

        // Optionally, detect left-click for other interactions
        // if (Input.GetMouseButtonDown(0)) { ... }
    }
}
