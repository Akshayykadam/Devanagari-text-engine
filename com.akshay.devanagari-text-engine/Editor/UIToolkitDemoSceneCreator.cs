// Devanagari Text Engine
// Copyright (c) 2026 Akshay Kadam. MIT License.

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.IO;

namespace DevanagariText.Editor
{
    /// <summary>
    /// Creates a demo scene using UI Toolkit for perfect Devanagari rendering.
    /// Uses Unity 6's Advanced Text Generator (ATG) which properly handles
    /// Shirorekha, conjuncts, and matras.
    /// </summary>
    public static class UIToolkitDemoSceneCreator
    {
        private const string FONTS_PATH = "Packages/com.akshay.devanagari-text-engine/Runtime/Fonts";
        
        [MenuItem("Tools/Devanagari Text/Create UI Toolkit Demo (Recommended)", false, 10)]
        public static void CreateUIToolkitDemo()
        {
            EnsureDirectory("Assets/DevanagariDemo");
            
            string ussPath = "Assets/DevanagariDemo/DevanagariDemo.uss";
            CreateUSSFile(ussPath);
            
            string uxmlPath = "Assets/DevanagariDemo/DevanagariDemo.uxml";
            CreateUXMLFile(uxmlPath, ussPath);
            
            string panelSettingsPath = "Assets/DevanagariDemo/DevanagariPanelSettings.asset";
            CreatePanelSettings(panelSettingsPath);
            
            string scriptPath = "Assets/DevanagariDemo/DevanagariDemoSetup.cs";
            CreateSetupScript(scriptPath);
            
            AssetDatabase.Refresh();
            SetupScene(uxmlPath, panelSettingsPath);
            
            EditorUtility.DisplayDialog(
                "Devanagari UI Toolkit Demo Created!",
                "✅ Demo created using UI Toolkit!\n\n" +
                "IMPORTANT — Enable ATG for proper rendering:\n\n" +
                "Edit > Project Settings > UI Toolkit\n" +
                "✓ Enable Advanced Text Generator\n\n" +
                "Supports both Hindi and Marathi text.\n" +
                "Shirorekha connects, conjuncts form correctly!",
                "Got it!");
        }
        
        private static void CreateUSSFile(string path)
        {
            string content = @"/* Devanagari Demo - UI Toolkit Stylesheet */
/* Uses Unity 6 ATG for proper Devanagari rendering */

:root {
    --deva-bg: rgb(25, 25, 35);
    --deva-text: rgb(240, 240, 240);
    --deva-accent: rgb(255, 153, 0);
    --deva-secondary: rgb(100, 200, 255);
    --deva-success: rgb(76, 217, 100);
    --deva-card-bg: rgba(255, 255, 255, 0.08);
    --deva-border: rgba(255, 255, 255, 0.15);
}

.root-container {
    flex-grow: 1;
    background-color: var(--deva-bg);
    padding: 40px;
    -unity-font: resource('Fonts/NotoSansDevanagari-Regular');
}

.title {
    font-size: 42px;
    color: var(--deva-accent);
    -unity-text-align: middle-center;
    margin-bottom: 10px;
    -unity-font-style: bold;
}

.subtitle {
    font-size: 18px;
    color: var(--deva-secondary);
    -unity-text-align: middle-center;
    margin-bottom: 30px;
}

.card {
    background-color: var(--deva-card-bg);
    border-radius: 12px;
    border-width: 1px;
    border-color: var(--deva-border);
    padding: 20px;
    margin-bottom: 15px;
}

.card-title {
    font-size: 20px;
    color: var(--deva-accent);
    margin-bottom: 10px;
    -unity-font-style: bold;
}

.card-text {
    font-size: 24px;
    color: var(--deva-text);
    white-space: normal;
    margin-bottom: 8px;
}

.card-text-large {
    font-size: 32px;
    color: var(--deva-text);
    -unity-text-align: middle-center;
    margin: 15px 0;
}

.conjunct-text {
    font-size: 36px;
    color: var(--deva-success);
    -unity-text-align: middle-center;
    letter-spacing: 2px;
}

.matra-text {
    font-size: 28px;
    color: var(--deva-secondary);
    -unity-text-align: middle-center;
}

.note-text {
    font-size: 14px;
    color: rgba(255, 255, 255, 0.5);
    -unity-text-align: middle-center;
    margin-top: 20px;
    -unity-font-style: italic;
}

.two-column {
    flex-direction: row;
    justify-content: space-between;
}

.column {
    flex-grow: 1;
    margin: 0 5px;
}
";
            File.WriteAllText(path, content);
        }
        
        private static void CreateUXMLFile(string path, string ussPath)
        {
            string content = @"<ui:UXML xmlns:ui=""UnityEngine.UIElements"">
    <Style src=""" + Path.GetFileName(ussPath) + @""" />
    <ui:VisualElement class=""root-container"">
        
        <!-- Title -->
        <ui:Label text=""देवनागरी टेक्स्ट इंजन"" class=""title"" name=""title"" />
        <ui:Label text=""UI Toolkit Demo — Hindi & Marathi Rendering"" class=""subtitle"" />
        
        <!-- Hindi Demo -->
        <ui:VisualElement class=""card"">
            <ui:Label text=""हिंदी (Hindi)"" class=""card-title"" />
            <ui:Label text=""अरे! देर से जवाब देने के लिए माफ़ कीजिए, मैं इन दिनों काफी व्यस्त था। लेकिन आपने जो डेमो बनाए हैं, वे सच में बहुत अच्छे लग रहे हैं!"" class=""card-text"" name=""hindi-demo"" />
        </ui:VisualElement>
        
        <!-- Marathi Demo -->
        <ui:VisualElement class=""card"">
            <ui:Label text=""मराठी (Marathi)"" class=""card-title"" />
            <ui:Label text=""नमस्कार! माळ कोळसा बाळ ळ ऱ — मराठी विशिष्ट अक्षरे"" class=""card-text"" name=""marathi-demo"" />
        </ui:VisualElement>
        
        <!-- Conjuncts Demo -->
        <ui:VisualElement class=""card"">
            <ui:Label text=""संयुक्ताक्षर (Conjuncts)"" class=""card-title"" />
            <ui:Label text=""क्ष  त्र  ज्ञ  श्र  क्र  प्र  द्ध  द्व  न्न  ल्ल"" class=""conjunct-text"" name=""conjuncts"" />
        </ui:VisualElement>
        
        <ui:VisualElement class=""two-column"">
            <!-- Matras Demo -->
            <ui:VisualElement class=""card column"">
                <ui:Label text=""मात्राएँ (Matras)"" class=""card-title"" />
                <ui:Label text=""का  कि  की  कु  कू  के  कै  को  कौ  कं  कः"" class=""matra-text"" name=""matras"" />
            </ui:VisualElement>
            
            <!-- Numerals Demo -->
            <ui:VisualElement class=""card column"">
                <ui:Label text=""अंक (Numerals)"" class=""card-title"" />
                <ui:Label text=""कीमत: ₹999.99 | मात्रा: १५०"" class=""card-text"" name=""numerals"" />
                <ui:Label text=""०  १  २  ३  ४  ५  ६  ७  ८  ९"" class=""matra-text"" />
            </ui:VisualElement>
        </ui:VisualElement>
        
        <!-- Greeting -->
        <ui:VisualElement class=""card"">
            <ui:Label text=""नमस्ते दुनिया! नमस्कार जग!"" class=""card-text-large"" name=""greeting"" />
            <ui:Label text=""भारत एक विविधता से भरा देश है। हिंदी और मराठी दोनों देवनागरी लिपि का उपयोग करती हैं।"" class=""card-text"" name=""paragraph"" />
        </ui:VisualElement>
        
        <ui:Label text=""UI Toolkit + ATG = Perfect Devanagari • Hindi & Marathi supported!"" class=""note-text"" />
        
    </ui:VisualElement>
</ui:UXML>";
            File.WriteAllText(path, content);
        }
        
        private static void CreatePanelSettings(string path)
        {
            if (File.Exists(path)) return;
            var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panelSettings.referenceResolution = new Vector2Int(1920, 1080);
            panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panelSettings.match = 0.5f;
            AssetDatabase.CreateAsset(panelSettings, path);
        }
        
        private static void CreateSetupScript(string path)
        {
            if (File.Exists(path)) return;
            string content = @"// Auto-generated by Devanagari Text Engine
using UnityEngine;
using UnityEngine.UIElements;

public class DevanagariDemoSetup : MonoBehaviour
{
    private void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        if (doc == null || doc.rootVisualElement == null) return;
        Debug.Log(""[Devanagari Demo] UI Toolkit demo loaded!"");
    }
}
";
            File.WriteAllText(path, content);
        }
        
        private static void SetupScene(string uxmlPath, string panelSettingsPath)
        {
            var go = new GameObject("Devanagari UI Toolkit Demo");
            var uiDoc = go.AddComponent<UIDocument>();
            
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            if (visualTree != null) uiDoc.visualTreeAsset = visualTree;
            
            var panelSettings = AssetDatabase.LoadAssetAtPath<PanelSettings>(panelSettingsPath);
            if (panelSettings != null) uiDoc.panelSettings = panelSettings;
            
            go.AddComponent<DevanagariText.Components.DevanagariUIDocument>();
            Selection.activeGameObject = go;
            
            Debug.Log("[Devanagari Text] UI Toolkit demo created! Enable ATG in Project Settings.");
        }
        
        private static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] parts = path.Split('/');
                string current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }
        }
    }
}
#endif
