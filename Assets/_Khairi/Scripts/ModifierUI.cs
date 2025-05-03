// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class ModifierUI : MonoBehaviour
// {
//     [SerializeField] private Image modifierIcon;
//     [SerializeField] private TextMeshProUGUI modifierText;
//     [SerializeField] private Image backgroundImage;
    
//     [Header("Colors")]
//     [SerializeField] private Color positiveColor = new Color(0.2f, 0.8f, 0.2f);
//     [SerializeField] private Color negativeColor = new Color(0.8f, 0.2f, 0.2f);
    
//     public void SetModifier(ModifierData modifier, bool isGlitched, bool isPositive, Color glitchColor, string glitchChars)
//     {
//         // Set the icon
//         if (modifierIcon != null && modifier.icon != null)
//         {
//             modifierIcon.sprite = modifier.icon;
//             modifierIcon.color = isGlitched ? glitchColor : Color.white;
//         }
        
//         // Set the text
//         if (modifierText != null)
//         {
//             string displayText = isGlitched ? ApplyGlitchToText(modifier.description, glitchChars) : modifier.description;
//             modifierText.text = displayText;
//             modifierText.color = isGlitched ? glitchColor : Color.white;
//         }
        
//         // Set background color based on positive/negative
//         if (backgroundImage != null)
//         {
//             Color baseColor = isPositive ? positiveColor : negativeColor;
            
//             // If glitched, adjust the color
//             if (isGlitched)
//             {
//                 baseColor = Color.Lerp(baseColor, glitchColor, 0.5f);
//             }
            
//             backgroundImage.color = baseColor;
//         }
//     }
    
//     private string ApplyGlitchToText(string text, string glitchChars)
//     {
//         string glitchedText = text;
        
//         // Replace some characters with glitch characters
//         int glitchCount = Random.Range(1, Mathf.Max(2, text.Length / 3));
        
//         for (int i = 0; i < glitchCount; i++)
//         {
//             int charIndex = Random.Range(0, text.Length);
//             int glitchCharIndex = Random.Range(0, glitchChars.Length);
            
//             if (charIndex < glitchedText.Length && glitchCharIndex < glitchChars.Length)
//             {
//                 char glitchChar = glitchChars[glitchCharIndex];
                
//                 if (charIndex == 0)
//                 {
//                     glitchedText = glitchChar + glitchedText.Substring(1);
//                 }
//                 else if (charIndex == glitchedText.Length - 1)
//                 {
//                     glitchedText = glitchedText.Substring(0, charIndex) + glitchChar;
//                 }
//                 else
//                 {
//                     glitchedText = glitchedText.Substring(0, charIndex) + glitchChar + glitchedText.Substring(charIndex + 1);
//                 }
//             }
//         }
        
//         return glitchedText;
//     }
// }