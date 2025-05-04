using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineCamera))]
public class CameraBoundsConfiner : MonoBehaviour
{
    [Header("Boundary Settings")]
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = -10f;
    [SerializeField] private float maxY = 10f;
    
    // Optional: Visualize bounds in editor
    [Header("Debug")]
    [SerializeField] private bool showBoundsGizmo = true;
    [SerializeField] private Color boundsGizmoColor = new Color(1, 0, 0, 0.5f);
    
    private CinemachineCamera virtualCamera;
    private Transform cameraTransform;
    [SerializeField] private CinemachinePositionComposer positionComposer;
    
    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineCamera>();
        cameraTransform = virtualCamera.transform;
        
        
        
        
        if (positionComposer == null)
        {
            Debug.LogWarning("No CinemachinePositionComposer found in the pipeline. Camera bounds might not work as expected.");
        }
    }
    
    private void LateUpdate()
    {
        if (virtualCamera.IsValid)
        {
            // Get the current camera position
            Vector3 pos = cameraTransform.position;
            
            // Apply bounds
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            
            // Set the new position
            cameraTransform.position = pos;
            //positionComposer.Scree = Mathf.Clamp01((pos.x - minX) / (maxX - minX));
        }
    }
    
    private void OnDrawGizmos()
    {
        if (showBoundsGizmo)
        {
            Gizmos.color = boundsGizmoColor;
            Vector3 center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0);
            Vector3 size = new Vector3(maxX - minX, maxY - minY, 0.1f);
            Gizmos.DrawCube(center, size);
        }
    }
    
    // Optional: Add a method to set bounds at runtime
    public void SetBounds(float newMinX, float newMaxX, float newMinY, float newMaxY)
    {
        minX = newMinX;
        maxX = newMaxX;
        minY = newMinY;
        maxY = newMaxY;
    }
}