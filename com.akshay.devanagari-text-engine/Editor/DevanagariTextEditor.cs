// Devanagari Text Engine
// Copyright (c) 2026 Akshay Kadam. MIT License.

using UnityEngine;
using UnityEditor;
using DevanagariText.Components;

namespace DevanagariText.Editor
{
    /// <summary>
    /// Custom editor for DevanagariTextMeshProUGUI component.
    /// </summary>
    [CustomEditor(typeof(DevanagariTextMeshProUGUI))]
    [CanEditMultipleObjects]
    public class DevanagariTextMeshProUGUIEditor : UnityEditor.Editor
    {
        private SerializedProperty _language;
        private SerializedProperty _enableNormalization;
        private SerializedProperty _legacyMode;
        private SerializedProperty _numeralMode;
        private SerializedProperty _debugMode;
        
        private string _previewText = "नमस्ते दुनिया - Hello World";
        private bool _showPreview = true;
        
        private void OnEnable()
        {
            _language = serializedObject.FindProperty("_language");
            _enableNormalization = serializedObject.FindProperty("_enableNormalization");
            _legacyMode = serializedObject.FindProperty("_legacyMode");
            _numeralMode = serializedObject.FindProperty("_numeralMode");
            _debugMode = serializedObject.FindProperty("_debugMode");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // Mode Section
            EditorGUILayout.LabelField("Mode", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_legacyMode);
            
            // Language Section (Only show if NOT in Legacy Mode)
            if (!_legacyMode.boolValue)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Language", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_language);
            }
            
            EditorGUILayout.Space();
            
            // Settings Section
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            if (!_legacyMode.boolValue)
            {
                EditorGUILayout.PropertyField(_enableNormalization);
            }
            EditorGUILayout.PropertyField(_numeralMode);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_debugMode);
            
            EditorGUILayout.Space();
            
            _showPreview = EditorGUILayout.Foldout(_showPreview, "Preview", true);
            if (_showPreview)
            {
                EditorGUI.indentLevel++;
                _previewText = EditorGUILayout.TextField("Preview Text", _previewText);
                
                if (GUILayout.Button("Apply Preview Text"))
                {
                    foreach (var t in targets)
                    {
                        var comp = t as DevanagariTextMeshProUGUI;
                        if (comp != null)
                        {
                            Undo.RecordObject(comp, "Apply Preview Text");
                            comp.OriginalText = _previewText;
                            EditorUtility.SetDirty(comp);
                        }
                    }
                }
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Quick Tests", EditorStyles.miniLabel);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Hindi", EditorStyles.miniButton)) SetPreviewText("नमस्ते दुनिया");
                if (GUILayout.Button("Marathi", EditorStyles.miniButton)) SetPreviewText("नमस्कार जग");
                if (GUILayout.Button("English", EditorStyles.miniButton)) SetPreviewText("Hello World");
                if (GUILayout.Button("Mixed", EditorStyles.miniButton)) SetPreviewText("नमस्ते Hello दुनिया World");
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Numerals", EditorStyles.miniButton)) SetPreviewText("कीमत: 123.45 रुपये");
                if (GUILayout.Button("Conjuncts", EditorStyles.miniButton)) SetPreviewText("क्षत्रिय ज्ञान त्रिकोण श्रीमान");
                if (GUILayout.Button("Matras", EditorStyles.miniButton)) SetPreviewText("कि की कु कू के कै को कौ");
                if (GUILayout.Button("Marathi ळ", EditorStyles.miniButton)) SetPreviewText("माळ कोळसा बाळ");
                EditorGUILayout.EndHorizontal();

                if (_legacyMode.boolValue)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("Legacy Mode enabled: Text is being converted to Kruti Dev encoding. Ensure the font is set to Kruti Dev 010.", MessageType.Info);
                }
                
                EditorGUI.indentLevel--;
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void SetPreviewText(string text)
        {
            _previewText = text;
            foreach (var t in targets)
            {
                var comp = t as DevanagariTextMeshProUGUI;
                if (comp != null)
                {
                    Undo.RecordObject(comp, "Set Preview Text");
                    comp.OriginalText = text;
                    EditorUtility.SetDirty(comp);
                }
            }
        }
    }
    
    /// <summary>
    /// Custom editor for DevanagariTextMeshPro component.
    /// </summary>
    [CustomEditor(typeof(DevanagariTextMeshPro))]
    [CanEditMultipleObjects]
    public class DevanagariTextMeshProEditor : UnityEditor.Editor
    {
        private SerializedProperty _language;
        private SerializedProperty _enableNormalization;
        private SerializedProperty _legacyMode;
        private SerializedProperty _numeralMode;
        private SerializedProperty _debugMode;
        
        private string _previewText = "नमस्ते दुनिया";
        
        private void OnEnable()
        {
            _language = serializedObject.FindProperty("_language");
            _enableNormalization = serializedObject.FindProperty("_enableNormalization");
            _legacyMode = serializedObject.FindProperty("_legacyMode");
            _numeralMode = serializedObject.FindProperty("_numeralMode");
            _debugMode = serializedObject.FindProperty("_debugMode");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.LabelField("Mode", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_legacyMode);
            
            if (!_legacyMode.boolValue)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Language", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_language);
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            if (!_legacyMode.boolValue)
            {
                EditorGUILayout.PropertyField(_enableNormalization);
            }
            EditorGUILayout.PropertyField(_numeralMode);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_debugMode);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Quick Apply", EditorStyles.boldLabel);
            _previewText = EditorGUILayout.TextField("Text", _previewText);
            
            if (GUILayout.Button("Apply Text"))
            {
                foreach (var t in targets)
                {
                    var comp = t as DevanagariTextMeshPro;
                    if (comp != null)
                    {
                        Undo.RecordObject(comp, "Apply Text");
                        comp.OriginalText = _previewText;
                        EditorUtility.SetDirty(comp);
                    }
                }
            }
            
            if (_legacyMode.boolValue)
            {
                EditorGUILayout.HelpBox("Legacy Mode enabled: Text converted to Kruti Dev encoding.", MessageType.Info);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
    
    /// <summary>
    /// Custom editor for DevanagariLegacyText component.
    /// </summary>
    [CustomEditor(typeof(DevanagariLegacyText))]
    [CanEditMultipleObjects]
    public class DevanagariLegacyTextEditor : UnityEditor.Editor
    {
        private SerializedProperty _language;
        private SerializedProperty _enableNormalization;
        private SerializedProperty _legacyMode;
        private SerializedProperty _numeralMode;
        private SerializedProperty _processInEditMode;
        private SerializedProperty _checkInterval;
        private SerializedProperty _debugMode;
        
        private string _previewText = "नमस्ते दुनिया";
        private bool _showPreview = true;
        
        private void OnEnable()
        {
            _language = serializedObject.FindProperty("_language");
            _enableNormalization = serializedObject.FindProperty("_enableNormalization");
            _legacyMode = serializedObject.FindProperty("_legacyMode");
            _numeralMode = serializedObject.FindProperty("_numeralMode");
            _processInEditMode = serializedObject.FindProperty("_processInEditMode");
            _checkInterval = serializedObject.FindProperty("_checkInterval");
            _debugMode = serializedObject.FindProperty("_debugMode");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Always show Legacy Mode at the top for easy access
            EditorGUILayout.LabelField("Mode", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_legacyMode);
            
            if (!_legacyMode.boolValue)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Language", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_language);
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            if (!_legacyMode.boolValue)
            {
                EditorGUILayout.PropertyField(_enableNormalization);
            }
            EditorGUILayout.PropertyField(_numeralMode);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_processInEditMode);
            EditorGUILayout.PropertyField(_checkInterval);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_debugMode);
            
            EditorGUILayout.Space();
            
            _showPreview = EditorGUILayout.Foldout(_showPreview, "Quick Apply", true);
            if (_showPreview)
            {
                EditorGUI.indentLevel++;
                _previewText = EditorGUILayout.TextField("Text", _previewText);
                
                if (GUILayout.Button("Apply Text"))
                {
                    foreach (var t in targets)
                    {
                        var comp = t as DevanagariLegacyText;
                        if (comp != null)
                        {
                            Undo.RecordObject(comp, "Apply Text");
                            comp.OriginalText = _previewText;
                            EditorUtility.SetDirty(comp);
                        }
                    }
                }
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Quick Tests", EditorStyles.miniLabel);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Hindi", EditorStyles.miniButton)) SetPreviewText("नमस्ते दुनिया");
                if (GUILayout.Button("Marathi", EditorStyles.miniButton)) SetPreviewText("नमस्कार जग");
                if (GUILayout.Button("Mixed", EditorStyles.miniButton)) SetPreviewText("नमस्ते Hello दुनिया World");
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Numerals", EditorStyles.miniButton)) SetPreviewText("कीमत: 123.45 रुपये");
                if (GUILayout.Button("Conjuncts", EditorStyles.miniButton)) SetPreviewText("क्षत्रिय ज्ञान त्रिकोण");
                EditorGUILayout.EndHorizontal();
                
                if (_legacyMode.boolValue)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("Legacy Mode enabled: Text converted to Kruti Dev encoding.", MessageType.Info);
                }
                
                EditorGUI.indentLevel--;
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void SetPreviewText(string text)
        {
            _previewText = text;
            foreach (var t in targets)
            {
                var comp = t as DevanagariLegacyText;
                if (comp != null)
                {
                    Undo.RecordObject(comp, "Set Preview Text");
                    comp.OriginalText = text;
                    EditorUtility.SetDirty(comp);
                }
            }
        }
    }
    
    /// <summary>
    /// Custom editor for DevanagariTMPInputField component.
    /// </summary>
    [CustomEditor(typeof(DevanagariTMPInputField))]
    [CanEditMultipleObjects]
    public class DevanagariTMPInputFieldEditor : UnityEditor.Editor
    {
        private SerializedProperty _language;
        private SerializedProperty _enableNormalization;
        private SerializedProperty _legacyMode;
        private SerializedProperty _numeralMode;
        private SerializedProperty _debugMode;

        private void OnEnable()
        {
            _language = serializedObject.FindProperty("_language");
            _enableNormalization = serializedObject.FindProperty("_enableNormalization");
            _legacyMode = serializedObject.FindProperty("_legacyMode");
            _numeralMode = serializedObject.FindProperty("_numeralMode");
            _debugMode = serializedObject.FindProperty("_debugMode");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Mode", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_legacyMode);

            if (!_legacyMode.boolValue)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Language", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_language);
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            if (!_legacyMode.boolValue)
            {
                EditorGUILayout.PropertyField(_enableNormalization);
            }
            EditorGUILayout.PropertyField(_numeralMode);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_debugMode);

            if (_legacyMode.boolValue)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Legacy Mode enabled: Input text is auto-converted to Kruti Dev encoding.", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
    
    /// <summary>
    /// Custom editor for DevanagariLegacyInputField component.
    /// </summary>
    [CustomEditor(typeof(DevanagariLegacyInputField))]
    [CanEditMultipleObjects]
    public class DevanagariLegacyInputFieldEditor : UnityEditor.Editor
    {
        private SerializedProperty _language;
        private SerializedProperty _enableNormalization;
        private SerializedProperty _legacyMode;
        private SerializedProperty _numeralMode;
        private SerializedProperty _debugMode;

        private void OnEnable()
        {
            _language = serializedObject.FindProperty("_language");
            _enableNormalization = serializedObject.FindProperty("_enableNormalization");
            _legacyMode = serializedObject.FindProperty("_legacyMode");
            _numeralMode = serializedObject.FindProperty("_numeralMode");
            _debugMode = serializedObject.FindProperty("_debugMode");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Mode", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_legacyMode);

            if (!_legacyMode.boolValue)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Language", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_language);
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            if (!_legacyMode.boolValue)
            {
                EditorGUILayout.PropertyField(_enableNormalization);
            }
            EditorGUILayout.PropertyField(_numeralMode);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_debugMode);

            if (_legacyMode.boolValue)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Legacy Mode enabled: Input text is auto-converted to Kruti Dev encoding.", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

}
