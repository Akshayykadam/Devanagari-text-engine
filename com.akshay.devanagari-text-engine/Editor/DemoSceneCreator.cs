// Devanagari Text Engine
// Copyright (c) 2026 Akshay Kadam. MIT License.

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace DevanagariText.Editor
{
    /// <summary>
    /// Creates a demo scene showcasing Devanagari text features (Hindi/Marathi).
    /// </summary>
    public class DemoSceneCreator : EditorWindow
    {
        [MenuItem("Tools/Devanagari Text/Create Demo Scene")]
        public static void CreateDemoScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Canvas
            var canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Title
            CreateTextElement(canvasObj, "Title", "देवनागरी टेक्स्ट इंजन - Demo", 
                new Vector2(0, 400), 48, TextAlignmentOptions.Center);
            
            // Hindi examples
            CreateTextElement(canvasObj, "HindiDemo", "नमस्ते दुनिया!", 
                new Vector2(0, 320), 36, TextAlignmentOptions.Center);
            
            // Marathi examples  
            CreateTextElement(canvasObj, "MarathiDemo", "नमस्कार जग! माळ कोळसा बाळ", 
                new Vector2(0, 260), 36, TextAlignmentOptions.Center);
            
            CreateTextElement(canvasObj, "ConjunctDemo", "क्षत्रिय ज्ञान त्रिकोण श्रीमान", 
                new Vector2(0, 200), 32, TextAlignmentOptions.Center);
            
            CreateTextElement(canvasObj, "MatraDemo", "कि की कु कू के कै को कौ कं कः", 
                new Vector2(0, 140), 32, TextAlignmentOptions.Center);
            
            CreateTextElement(canvasObj, "MixedText", "Hello नमस्ते World दुनिया 123 ४५६", 
                new Vector2(0, 80), 28, TextAlignmentOptions.Center);
            
            CreateTextElement(canvasObj, "ParagraphDemo", 
                "भारत एक विविधता से भरा देश है। यहाँ की संस्कृति, " +
                "परंपराएँ और भाषाएँ अनेक हैं। हिंदी और मराठी भारत की प्रमुख भाषाओं में से हैं।", 
                new Vector2(0, 0), 24, TextAlignmentOptions.Center);
            
            CreateTextElement(canvasObj, "NumeralDemo", "कीमत: ₹999.99 | मात्रा: 150", 
                new Vector2(0, -80), 28, TextAlignmentOptions.Center);
            
            // EventSystem
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            // Input Field
            CreateInputField(canvasObj, "DevanagariInput", "देवनागरी में टाइप करें...", 
                new Vector2(0, -160), new Vector2(600, 50));
            
            // Instructions
            CreateTextElement(canvasObj, "Instructions", 
                "Add DevanagariTextFixer to any TMP text for auto processing.\n" +
                "Use DevanagariTMPInputField for Devanagari-capable input fields.",
                new Vector2(0, -260), 20, TextAlignmentOptions.Center);
            
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[Devanagari Text] Demo scene created! Add DevanagariTextFixer to text objects.");
        }
        
        private static void CreateTextElement(GameObject parent, string name, string text, 
            Vector2 position, int fontSize, TextAlignmentOptions alignment)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            var rectTransform = obj.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = new Vector2(1600, 80);
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.enableWordWrapping = true;
            obj.AddComponent<DevanagariText.Components.DevanagariTextFixer>();
        }
        
        private static void CreateInputField(GameObject parent, string name, string placeholder, 
            Vector2 position, Vector2 size)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            var rectTransform = obj.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
            var image = obj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            var textArea = new GameObject("Text Area");
            textArea.transform.SetParent(obj.transform, false);
            var textAreaRect = textArea.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(10, 5);
            textAreaRect.offsetMax = new Vector2(-10, -5);
            textArea.AddComponent<RectMask2D>();
            
            var placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(textArea.transform, false);
            var placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;
            var placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = placeholder;
            placeholderText.fontSize = 24;
            placeholderText.fontStyle = FontStyles.Italic;
            placeholderText.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
            placeholderText.alignment = TextAlignmentOptions.Left;
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(textArea.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var inputText = textObj.AddComponent<TextMeshProUGUI>();
            inputText.fontSize = 24;
            inputText.color = Color.white;
            inputText.alignment = TextAlignmentOptions.Left;
            
            var inputField = obj.AddComponent<TMP_InputField>();
            inputField.textViewport = textAreaRect;
            inputField.textComponent = inputText;
            inputField.placeholder = placeholderText;
            obj.AddComponent<DevanagariText.Components.DevanagariTMPInputField>();
        }
    }
}
