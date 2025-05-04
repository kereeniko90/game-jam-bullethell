using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
public class BulletUpgradeImporter : EditorWindow
{
    private string jsonPath = "";
    private string outputFolder = "Assets/ScriptableObjects/BulletUpgrades";
    private TextAsset jsonFile;
    
    [MenuItem("Tools/Bullet Upgrades/Import From JSON")]
    public static void ShowWindow()
    {
        GetWindow<BulletUpgradeImporter>("Bullet Upgrade Importer");
    }

// JSON helper class to wrap arrays for Unity's JsonUtility (which doesn't handle top-level arrays)
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        // Check if JSON is already wrapped
        if (json.StartsWith("[") && json.EndsWith("]"))
        {
            // Wrap the existing JSON array
            string newJson = "{\"items\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.items;
        }
        
        // Try to parse directly if it's already wrapped
        try
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.items;
        }
        catch
        {
            Debug.LogError("Failed to parse JSON array. Make sure it's a valid JSON array of objects.");
            return null;
        }
    }
    
    [Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }
}

// JSON data structures matching the expected JSON format
[Serializable]
public class BulletUpgradeJson
{
    public string upgradeName;
    public string icon;
    public string bulletType;
    public string description;
    public BaseBulletModifierJson[] baseModifiers;
    public SpecificBulletModifierJson[] specificModifiers;
    public PlayerModifierJson[] playerModifiers;
    public bool canGlitchName;
    public bool canGlitchIcon;
    public bool canGlitchModifiers;
}

[Serializable]
public class BaseBulletModifierJson
{
    public string type;
    public float value;
    public bool isPositive;
    public string description;
    public string icon;
    public bool canGlitch;
}

[Serializable]
public class SpecificBulletModifierJson
{
    public string targetScript;
    public string propertyName;
    public float value;
    public bool isMultiplier;
    public bool isPositive;
    public string description;
    public string icon;
    public bool canGlitch;
}

[Serializable]
public class PlayerModifierJson
{
    public string type;
    public float value;
    public bool isPositive;
    public string description;
    public string icon;
    public bool canGlitch;
}
#endif
    
    private void OnGUI()
    {
        GUILayout.Label("Bullet Upgrade JSON Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        jsonFile = EditorGUILayout.ObjectField("JSON File", jsonFile, typeof(TextAsset), false) as TextAsset;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Output Folder");
        outputFolder = EditorGUILayout.TextField(outputFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                // Convert to project-relative path
                if (path.StartsWith(Application.dataPath))
                {
                    outputFolder = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    Debug.LogWarning("Please select a folder inside the Assets directory.");
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        GUI.enabled = jsonFile != null;
        if (GUILayout.Button("Import Bullet Upgrades"))
        {
            ImportBulletUpgrades();
        }
        GUI.enabled = true;
    }
    
    private void ImportBulletUpgrades()
    {
        if (jsonFile == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a JSON file.", "OK");
            return;
        }
        
        // Ensure output directory exists
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }
        
        string json = jsonFile.text;
        BulletUpgradeJson[] upgrades = JsonHelper.FromJson<BulletUpgradeJson>(json);
        
        if (upgrades == null || upgrades.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "Failed to parse JSON or no upgrades found.", "OK");
            return;
        }
        
        int successCount = 0;
        
        // Show progress bar
        EditorUtility.DisplayProgressBar("Importing Bullet Upgrades", "Starting import...", 0f);
        
        try
        {
            // Process each upgrade
            for (int i = 0; i < upgrades.Length; i++)
            {
                float progress = (float)i / upgrades.Length;
                EditorUtility.DisplayProgressBar("Importing Bullet Upgrades", 
                    $"Importing {upgrades[i].upgradeName} ({i+1}/{upgrades.Length})", progress);
                
                // Create the scriptable object
                BulletUpgrade upgrade = CreateInstance<BulletUpgrade>();
                
                // Apply basic properties
                upgrade.upgradeName = upgrades[i].upgradeName;
                upgrade.description = upgrades[i].description;
                upgrade.bulletType = (BulletManager.BulletType)Enum.Parse(typeof(BulletManager.BulletType), upgrades[i].bulletType);
                upgrade.canGlitchName = upgrades[i].canGlitchName;
                upgrade.canGlitchIcon = upgrades[i].canGlitchIcon;
                upgrade.canGlitchModifiers = upgrades[i].canGlitchModifiers;
                
                // Load icon if it exists
                if (!string.IsNullOrEmpty(upgrades[i].icon))
                {
                    upgrade.icon = FindSprite(upgrades[i].icon);
                }
                
                // Process base modifiers
                foreach (var baseModJson in upgrades[i].baseModifiers)
                {
                    BaseBulletModifier baseModifier = new BaseBulletModifier();
                    baseModifier.type = (BaseBulletModifier.ModifierType)Enum.Parse(typeof(BaseBulletModifier.ModifierType), baseModJson.type);
                    baseModifier.value = baseModJson.value;
                    baseModifier.isPositive = baseModJson.isPositive;
                    baseModifier.description = baseModJson.description;
                    baseModifier.canGlitch = baseModJson.canGlitch;
                    
                    // Load icon if it exists
                    if (!string.IsNullOrEmpty(baseModJson.icon))
                    {
                        baseModifier.icon = FindSprite(baseModJson.icon);
                    }
                    
                    upgrade.baseModifiers.Add(baseModifier);
                }
                
                // Process specific modifiers
                foreach (var specificModJson in upgrades[i].specificModifiers)
                {
                    SpecificBulletModifier specificModifier = new SpecificBulletModifier();
                    specificModifier.targetScript = (SpecificBulletModifier.BulletScript)Enum.Parse(typeof(SpecificBulletModifier.BulletScript), specificModJson.targetScript);
                    specificModifier.propertyName = specificModJson.propertyName;
                    specificModifier.value = specificModJson.value;
                    specificModifier.isMultiplier = specificModJson.isMultiplier;
                    specificModifier.isPositive = specificModJson.isPositive;
                    specificModifier.description = specificModJson.description;
                    specificModifier.canGlitch = specificModJson.canGlitch;
                    
                    // Load icon if it exists
                    if (!string.IsNullOrEmpty(specificModJson.icon))
                    {
                        specificModifier.icon = FindSprite(specificModJson.icon);
                    }
                    
                    upgrade.specificModifiers.Add(specificModifier);
                }
                
                // Process player modifiers
                foreach (var playerModJson in upgrades[i].playerModifiers)
                {
                    PlayerModifier playerModifier = new PlayerModifier();
                    playerModifier.type = (PlayerModifier.ModifierType)Enum.Parse(typeof(PlayerModifier.ModifierType), playerModJson.type);
                    playerModifier.value = playerModJson.value;
                    playerModifier.isPositive = playerModJson.isPositive;
                    playerModifier.description = playerModJson.description;
                    playerModifier.canGlitch = playerModJson.canGlitch;
                    
                    // Load icon if it exists
                    if (!string.IsNullOrEmpty(playerModJson.icon))
                    {
                        playerModifier.icon = FindSprite(playerModJson.icon);
                    }
                    
                    upgrade.playerModifiers.Add(playerModifier);
                }
                
                // Save the scriptable object to disk
                string assetPath = Path.Combine(outputFolder, SanitizeFileName(upgrades[i].upgradeName) + ".asset");
                AssetDatabase.CreateAsset(upgrade, assetPath);
                
                successCount++;
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Import Complete", 
                $"Successfully imported {successCount} of {upgrades.Length} bullet upgrades.", "OK");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error importing bullet upgrades: {ex.Message}\n{ex.StackTrace}");
            EditorUtility.DisplayDialog("Import Error", 
                $"Error importing bullet upgrades: {ex.Message}", "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
    
    private Sprite FindSprite(string spriteName)
    {
        // Search for sprites with the given name
        string[] guids = AssetDatabase.FindAssets("t:Sprite " + spriteName);
        
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        
        return null;
    }
    
    private string SanitizeFileName(string fileName)
    {
        // Replace invalid file name characters
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }
        
        return fileName;
    }
}