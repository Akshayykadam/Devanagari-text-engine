// Devanagari Text Engine
// Copyright (c) 2026 Akshay Kadam. MIT License.

using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

namespace DevanagariText.Editor
{
    /// <summary>
    /// TMP Font Asset generator for Devanagari text (Hindi/Marathi).
    /// </summary>
    public class DevanagariFontAssetGenerator : EditorWindow
    {
        private const string FONTS_PATH = "Packages/com.akshay.devanagari-text-engine/Runtime/Fonts";
        private const string OUTPUT_PATH = "Assets/Fonts";
        
        private Font _sourceFont;
        private int _atlasResolution = 2048;
        private int _samplingPointSize = 64;
        private int _padding = 5;
        
        private bool _includeVowels = true;
        private bool _includeConsonants = true;
        private bool _includeMatras = true;
        private bool _includeNuktaConsonants = true;
        private bool _includeDevanagariDigits = true;
        private bool _includeLatinBasic = true;
        private bool _includeWesternDigits = true;
        private bool _includePunctuation = true;
        
        private static readonly string DEVANAGARI_RANGES = "0900-097F,0964-0965,A8E0-A8FF";
        private static readonly string LATIN_RANGES = "0020-007E,00A0-00FF";
        
        [MenuItem("Tools/Devanagari Text/Font Asset Generator")]
        public static void ShowWindow()
        {
            var window = GetWindow<DevanagariFontAssetGenerator>("Devanagari Font Generator");
            window.minSize = new Vector2(450, 550);
        }
        
        private void OnEnable()
        {
            if (_sourceFont == null)
                _sourceFont = AssetDatabase.LoadAssetAtPath<Font>($"{FONTS_PATH}/NotoSansDevanagari-Regular.ttf");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Devanagari Font Asset Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Generates a TextMeshPro Font Asset with the required Devanagari Unicode character ranges " +
                "for proper Hindi and Marathi text rendering.", MessageType.Info);
            
            EditorGUILayout.Space();
            _sourceFont = (Font)EditorGUILayout.ObjectField("Source Font (TTF)", _sourceFont, typeof(Font), false);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Atlas Settings", EditorStyles.boldLabel);
            _atlasResolution = EditorGUILayout.IntPopup("Atlas Resolution", _atlasResolution,
                new string[] { "512", "1024", "2048", "4096" }, new int[] { 512, 1024, 2048, 4096 });
            _samplingPointSize = EditorGUILayout.IntSlider("Sampling Point Size", _samplingPointSize, 16, 128);
            _padding = EditorGUILayout.IntSlider("Padding", _padding, 1, 10);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Character Sets", EditorStyles.boldLabel);
            _includeVowels = EditorGUILayout.Toggle("Devanagari Vowels (अ-औ)", _includeVowels);
            _includeConsonants = EditorGUILayout.Toggle("Devanagari Consonants (क-ह)", _includeConsonants);
            _includeMatras = EditorGUILayout.Toggle("Matras & Marks", _includeMatras);
            _includeNuktaConsonants = EditorGUILayout.Toggle("Nukta Consonants (क़-य़)", _includeNuktaConsonants);
            _includeDevanagariDigits = EditorGUILayout.Toggle("Devanagari Digits (०-९)", _includeDevanagariDigits);
            _includeLatinBasic = EditorGUILayout.Toggle("Latin (A-Z, a-z)", _includeLatinBasic);
            _includeWesternDigits = EditorGUILayout.Toggle("Western Digits (0-9)", _includeWesternDigits);
            _includePunctuation = EditorGUILayout.Toggle("Punctuation & Symbols", _includePunctuation);
            
            EditorGUILayout.Space(15);
            
            EditorGUI.BeginDisabledGroup(_sourceFont == null);
            var origColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f, 1f);
            if (GUILayout.Button("✦  Generate Font Asset  ✦", GUILayout.Height(40)))
                GenerateFontAsset();
            GUI.backgroundColor = origColor;
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space(5);
            if (GUILayout.Button("Copy Unicode Ranges to Clipboard"))
            {
                string ranges = _includeLatinBasic ? $"{DEVANAGARI_RANGES},{LATIN_RANGES}" : DEVANAGARI_RANGES;
                EditorGUIUtility.systemCopyBuffer = ranges;
                Debug.Log($"[Devanagari Font Generator] Unicode ranges copied: {ranges}");
                EditorUtility.DisplayDialog("Copied", "Unicode ranges copied to clipboard.\n\nPaste in: Window > TextMeshPro > Font Asset Creator\nSet Character Set to 'Unicode Range (Hex)'", "OK");
            }
            
            if (GUILayout.Button("Open TMP Font Asset Creator"))
                EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Font Asset Creator");
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox($"Output: {OUTPUT_PATH}/NotoSansDevanagari SDF.asset", MessageType.None);
        }
        
        private void GenerateFontAsset()
        {
            if (_sourceFont == null) { EditorUtility.DisplayDialog("Error", "No source font assigned!", "OK"); return; }
            EnsureOutputFolder();
            
            string ranges = _includeLatinBasic ? $"{DEVANAGARI_RANGES},{LATIN_RANGES}" : DEVANAGARI_RANGES;
            Selection.activeObject = _sourceFont;
            EditorGUIUtility.PingObject(_sourceFont);
            EditorGUIUtility.systemCopyBuffer = ranges;
            
            string suggestedName = _sourceFont.name.Replace("-Regular", "").Replace("-Bold", " Bold");
            bool openCreator = EditorUtility.DisplayDialog(
                "Generate Devanagari Font Asset",
                $"Font: {_sourceFont.name}\n\n" +
                $"Unicode ranges copied to clipboard:\n{ranges}\n\n" +
                "Steps in the Font Asset Creator:\n" +
                $"1. Source Font File → {_sourceFont.name}\n" +
                $"2. Sampling Point Size → {_samplingPointSize}\n" +
                $"3. Padding → {_padding}\n" +
                $"4. Atlas Resolution → {_atlasResolution}x{_atlasResolution}\n" +
                "5. Character Set → Unicode Range (Hex)\n" +
                "6. Paste the ranges (already in clipboard)\n" +
                "7. Click 'Generate Font Atlas'\n" +
                $"8. Save as '{suggestedName} SDF' in {OUTPUT_PATH}/",
                "Open Font Asset Creator", "Cancel");
            
            if (openCreator)
                EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Font Asset Creator");
        }
        
        private void EnsureOutputFolder()
        {
            if (!AssetDatabase.IsValidFolder(OUTPUT_PATH))
            {
                string[] parts = OUTPUT_PATH.Split('/');
                string currentPath = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string newPath = currentPath + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                        AssetDatabase.CreateFolder(currentPath, parts[i]);
                    currentPath = newPath;
                }
            }
        }
        
        public static TMP_FontAsset FindGeneratedFontAsset()
        {
            string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { "Assets/Fonts", "Assets" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("Devanagari") || path.Contains("NotoSans"))
                {
                    var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                    if (font != null) return font;
                }
            }
            return null;
        }
    }
}
