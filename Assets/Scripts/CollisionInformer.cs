using UnityEngine;

public class CollisionInformer : MonoBehaviour
{
    [SerializeField] private float maxRaycastDistance = 100f;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private GameObject collisionAlert;

    void Update()
    {
        Vector3[] directions = { transform.forward, -transform.forward, -transform.right, transform.right };
        bool anyHit = false; 

        foreach (Vector3 dir in directions)
        {
            Ray ray = new Ray(transform.position, dir);
            if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, layerMask))
            {
                anyHit = true; 
                Debug.DrawRay(transform.position, dir * hit.distance, Color.red, 0.1f);
            }
            else
            {
                Debug.DrawRay(transform.position, dir * maxRaycastDistance, Color.green, 0.1f);
            }
        }

        collisionAlert.SetActive(anyHit); 
    }
}
