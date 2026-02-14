// Devanagari Text Engine
// Copyright (c) 2026 Akshay Kadam. MIT License.

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;

namespace DevanagariText.Editor
{
    /// <summary>
    /// Font validation tool for Devanagari text support (Hindi/Marathi).
    /// Checks if TMP Font Assets contain the required Devanagari character ranges.
    /// </summary>
    public class DevanagariTextFontValidator : EditorWindow
    {
        private TMP_FontAsset _fontToValidate;
        private Vector2 _scrollPosition;
        private List<ValidationResult> _results = new List<ValidationResult>();
        private bool _hasValidated = false;
        
        private struct ValidationResult
        {
            public string Category;
            public string Range;
            public int TotalChars;
            public int FoundChars;
            public float Coverage;
            public bool IsRequired;
        }
        
        [MenuItem("Tools/Devanagari Text/Font Validator")]
        public static void ShowWindow()
        {
            var window = GetWindow<DevanagariTextFontValidator>("Devanagari Font Validator");
            window.minSize = new Vector2(450, 400);
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Devanagari Font Validator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Validates that a TMP Font Asset contains the required Devanagari Unicode characters " +
                "for proper Hindi and Marathi text rendering.", MessageType.Info);
            
            EditorGUILayout.Space();
            
            _fontToValidate = (TMP_FontAsset)EditorGUILayout.ObjectField(
                "Font Asset", _fontToValidate, typeof(TMP_FontAsset), false);
            
            EditorGUILayout.Space();
            
            EditorGUI.BeginDisabledGroup(_fontToValidate == null);
            if (GUILayout.Button("Validate Font", GUILayout.Height(30)))
                ValidateFont();
            EditorGUI.EndDisabledGroup();
            
            if (_hasValidated)
            {
                EditorGUILayout.Space();
                DrawResults();
            }
        }
        
        private void ValidateFont()
        {
            _results.Clear();
            _hasValidated = true;
            if (_fontToValidate == null) return;
            
            var availableChars = new HashSet<uint>();
            foreach (var pair in _fontToValidate.characterTable)
                availableChars.Add(pair.unicode);
            
            CheckRange("Devanagari Vowels", 0x0904, 0x0914, true, availableChars);
            CheckRange("Devanagari Consonants", 0x0915, 0x0939, true, availableChars);
            CheckRange("Devanagari Matras (Vowel Signs)", 0x093E, 0x094C, true, availableChars);
            CheckRange("Virama + Nukta + Anusvara", 0x0901, 0x0903, true, availableChars);
            CheckSingleChars("Essential Marks", new uint[] { 0x094D, 0x093C, 0x093D }, true, availableChars);
            CheckSingleChars("Marathi-specific (ळ, ऱ)", new uint[] { 0x0933, 0x0931 }, false, availableChars);
            CheckRange("Devanagari Digits", 0x0966, 0x096F, false, availableChars);
            CheckRange("Devanagari Punctuation", 0x0964, 0x0965, false, availableChars);
            CheckRange("Nukta Consonants", 0x0958, 0x095F, false, availableChars);
            CheckRange("Basic Latin (A-Z)", 0x0041, 0x005A, false, availableChars);
            CheckRange("Basic Latin (a-z)", 0x0061, 0x007A, false, availableChars);
            CheckRange("Western Digits (0-9)", 0x0030, 0x0039, false, availableChars);
        }
        
        private void CheckRange(string category, uint start, uint end, bool required, HashSet<uint> available)
        {
            int total = (int)(end - start + 1), found = 0;
            for (uint c = start; c <= end; c++)
                if (available.Contains(c)) found++;
            _results.Add(new ValidationResult { Category = category, Range = $"U+{start:X4}-U+{end:X4}", TotalChars = total, FoundChars = found, Coverage = total > 0 ? (float)found / total : 0f, IsRequired = required });
        }
        
        private void CheckSingleChars(string category, uint[] chars, bool required, HashSet<uint> available)
        {
            int total = chars.Length, found = 0;
            foreach (uint c in chars) if (available.Contains(c)) found++;
            _results.Add(new ValidationResult { Category = category, Range = "Various", TotalChars = total, FoundChars = found, Coverage = total > 0 ? (float)found / total : 0f, IsRequired = required });
        }
        
        private void DrawResults()
        {
            EditorGUILayout.LabelField("Validation Results", EditorStyles.boldLabel);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            bool allRequiredPassed = true;
            foreach (var result in _results)
            {
                EditorGUILayout.BeginHorizontal("box");
                if (result.Coverage >= 1f) EditorGUILayout.LabelField("✅", GUILayout.Width(25));
                else if (result.Coverage > 0.5f) EditorGUILayout.LabelField("⚠️", GUILayout.Width(25));
                else { EditorGUILayout.LabelField("❌", GUILayout.Width(25)); if (result.IsRequired) allRequiredPassed = false; }
                
                string label = result.IsRequired ? $"{result.Category} (Required)" : result.Category;
                EditorGUILayout.LabelField(label, GUILayout.MinWidth(200));
                EditorGUILayout.LabelField($"{result.FoundChars}/{result.TotalChars}", GUILayout.Width(60));
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(GUILayout.Width(100), GUILayout.Height(16)), result.Coverage, $"{result.Coverage * 100:F0}%");
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
            
            if (allRequiredPassed)
                EditorGUILayout.HelpBox("✅ Font passes all required checks for Devanagari text.", MessageType.Info);
            else
                EditorGUILayout.HelpBox("❌ Font is missing required Devanagari characters. Consider using Noto Sans Devanagari.", MessageType.Warning);
        }
    }
}
