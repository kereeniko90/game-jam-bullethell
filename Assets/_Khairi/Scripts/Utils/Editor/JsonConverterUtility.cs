using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
public class JsonConverterUtility : EditorWindow
{
    private TextAsset originalJsonFile;
    private string outputPath = "Assets/Resources/BulletUpgradesConverted.json";
    private bool addWrapping = true;
    
    [MenuItem("Tools/Bullet Upgrades/Convert JSON Format")]
    public static void ShowWindow()
    {
        GetWindow<JsonConverterUtility>("JSON Converter");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("JSON Format Converter", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        originalJsonFile = EditorGUILayout.ObjectField("Original JSON File", originalJsonFile, typeof(TextAsset), false) as TextAsset;
        
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Output JSON Path");
        outputPath = EditorGUILayout.TextField(outputPath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string path = EditorUtility.SaveFilePanel("Save Converted JSON", "Assets", "BulletUpgradesConverted.json", "json");
            if (!string.IsNullOrEmpty(path))
            {
                // Convert to project-relative path
                if (path.StartsWith(Application.dataPath))
                {
                    outputPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    Debug.LogWarning("Please select a location inside the Assets directory.");
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        addWrapping = EditorGUILayout.Toggle("Add Array Wrapper", addWrapping);
        
        EditorGUILayout.Space();
        
        GUI.enabled = originalJsonFile != null;
        if (GUILayout.Button("Convert JSON Format"))
        {
            ConvertJsonFormat();
        }
        GUI.enabled = true;
    }
    
    private void ConvertJsonFormat()
    {
        if (originalJsonFile == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a JSON file to convert.", "OK");
            return;
        }
        
        try
        {
            string json = originalJsonFile.text;
            
            // Clean up the JSON to ensure it's compatible with Unity's JsonUtility
            json = CleanUpJson(json);
            
            // Add wrapper if requested (for top-level arrays)
            if (addWrapping && json.Trim().StartsWith("[") && json.Trim().EndsWith("]"))
            {
                json = "{\"items\":" + json + "}";
            }
            
            // Validate JSON syntax by trying to parse it
            try
            {
                // We're not using the result, just checking if it parses
                JsonUtility.FromJson<object>(json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error parsing converted JSON: {ex.Message}");
                EditorUtility.DisplayDialog("Error", $"The converted JSON is not valid: {ex.Message}", "OK");
                return;
            }
            
            // Ensure directory exists
            string directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Write the converted JSON to file
            File.WriteAllText(outputPath, json);
            
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Conversion Complete", 
                "JSON has been converted successfully and saved to " + outputPath, "OK");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error converting JSON: {ex.Message}\n{ex.StackTrace}");
            EditorUtility.DisplayDialog("Conversion Error", 
                $"Error converting JSON: {ex.Message}", "OK");
        }
    }
    
    private string CleanUpJson(string json)
    {
        // Remove any BOM or other non-standard characters at the start
        if (json.StartsWith("\uFEFF") || json.StartsWith("\u00EF\u00BB\u00BF"))
        {
            json = json.Substring(1);
        }
        
        // Remove comments (both // and /* */)
        json = Regex.Replace(json, @"//.*?$", "", RegexOptions.Multiline);
        json = Regex.Replace(json, @"/\*.*?\*/", "", RegexOptions.Singleline);
        
        // Ensure property names are in quotes (handle 'property: value' to '"property": value')
        json = Regex.Replace(json, @"(\s*)(\w+)(\s*):", "$1\"$2\"$3:");
        
        // Fix trailing commas in arrays and objects
        json = Regex.Replace(json, @",(\s*[\]}])", "$1");
        
        return json;
    }
}
#endif