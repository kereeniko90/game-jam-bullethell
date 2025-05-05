using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D aimCursor;
    
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero; // Point within cursor that registers clicks
    
    private void Start()
    {
        // Set default cursor
        //SetDefaultCursor();
        SetAimCursor();
    }
    
    public void SetDefaultCursor()
    {
        Cursor.SetCursor(defaultCursor, cursorHotspot, CursorMode.Auto);
    }
    
    public void SetAimCursor()
    {
        Cursor.SetCursor(aimCursor, cursorHotspot, CursorMode.Auto);
    }
    
    // If you want to hide the cursor (e.g., for fullscreen games)
    public void HideCursor()
    {
        Cursor.visible = false;
    }
    
    public void ShowCursor()
    {
        Cursor.visible = true;
    }
}