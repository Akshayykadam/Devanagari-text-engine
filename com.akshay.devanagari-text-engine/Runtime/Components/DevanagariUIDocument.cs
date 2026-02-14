// Devanagari Text Engine
// Copyright (c) 2026 Akshay Kadam. MIT License.

using UnityEngine;
using UnityEngine.UIElements;
using DevanagariText.Core;
using DevanagariText.Shaping;
using DevanagariText.TextProcessing;

namespace DevanagariText.Components
{
    /// <summary>
    /// Devanagari text support for UI Toolkit (Unity 6+).
    /// 
    /// UI Toolkit uses Unity's Advanced Text Generator (ATG) which natively supports
    /// Devanagari complex text layout — Shirorekha connects, conjuncts form correctly,
    /// and matras position properly. No workarounds needed!
    /// 
    /// SETUP:
    /// 1. Enable ATG: Edit > Project Settings > UI Toolkit > Enable Advanced Text Generator
    /// 2. Create Font Asset: Right-click NotoSansDevanagari-Regular.ttf > Create > UI Toolkit > Font Asset
    /// 3. Assign the font in your USS stylesheet: -unity-font-definition: url("path/to/font.asset");
    /// 4. Add this component to a GameObject with a UIDocument component
    /// 
    /// This component applies NFC normalization and numeral conversion to all
    /// Label/TextElement children in the UIDocument.
    /// </summary>
    [AddComponentMenu("Devanagari Text/Devanagari UI Document (UI Toolkit)")]
    [RequireComponent(typeof(UIDocument))]
    [DisallowMultipleComponent]
    public class DevanagariUIDocument : MonoBehaviour
    {
        [Header("Language")]
        [SerializeField]
        [Tooltip("Select language for language-specific normalization")]
        private DevanagariLanguage _language = DevanagariLanguage.Hindi;
        
        [Header("Settings")]
        [SerializeField]
        [Tooltip("Enable Unicode NFC normalization for proper conjunct rendering")]
        private bool _enableNormalization = true;
        
        [SerializeField]
        [Tooltip("Numeral display mode")]
        private NumeralMode _numeralMode = NumeralMode.None;
        
        [Header("Auto Processing")]
        [SerializeField]
        [Tooltip("Automatically process all Label/TextElement children")]
        private bool _autoProcessAll = true;
        
        [SerializeField]
        [Tooltip("CSS class to mark elements for Devanagari processing (if autoProcessAll is false)")]
        private string _devanagariClass = "devanagari-text";
        
        [Header("Debug")]
        [SerializeField]
        private bool _debugMode = false;
        
        private UIDocument _uiDocument;
        
        public DevanagariLanguage Language
        {
            get => _language;
            set { if (_language != value) { _language = value; ProcessAllText(); } }
        }
        
        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }
        
        private void OnEnable()
        {
            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument != null && _uiDocument.rootVisualElement != null)
                ProcessAllText();
            else
                StartCoroutine(ProcessNextFrame());
        }
        
        private System.Collections.IEnumerator ProcessNextFrame()
        {
            yield return null;
            if (_uiDocument != null && _uiDocument.rootVisualElement != null)
                ProcessAllText();
        }
        
        /// <summary>
        /// Process all text elements in the UIDocument.
        /// </summary>
        public void ProcessAllText()
        {
            if (_uiDocument == null || _uiDocument.rootVisualElement == null) return;
            
            var root = _uiDocument.rootVisualElement;
            if (_autoProcessAll)
                root.Query<TextElement>().ForEach(ProcessTextElement);
            else
                root.Query<TextElement>(className: _devanagariClass).ForEach(ProcessTextElement);
        }
        
        /// <summary>
        /// Process a single text element.
        /// </summary>
        public void ProcessTextElement(TextElement element)
        {
            if (element == null) return;
            string original = element.text;
            if (string.IsNullOrEmpty(original)) return;
            
            string processed = ProcessText(original);
            if (processed != original)
            {
                element.text = processed;
                if (_debugMode)
                    Debug.Log($"[DevanagariUIDocument] Processed '{element.name}': '{original}' → '{processed}'");
            }
        }
        
        /// <summary>
        /// Set text on a named element.
        /// </summary>
        public void SetText(string elementName, string text)
        {
            if (_uiDocument == null || _uiDocument.rootVisualElement == null) return;
            var element = _uiDocument.rootVisualElement.Q<TextElement>(elementName);
            if (element != null) element.text = ProcessText(text);
        }
        
        /// <summary>
        /// Set text on a Label by name.
        /// </summary>
        public void SetLabelText(string labelName, string text)
        {
            if (_uiDocument == null || _uiDocument.rootVisualElement == null) return;
            var label = _uiDocument.rootVisualElement.Q<Label>(labelName);
            if (label != null) label.text = ProcessText(text);
        }
        
        private string ProcessText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            string result = text;
            if (_enableNormalization && DevanagariTextProcessor.NeedsProcessing(result))
                result = GlobalProcessedTextCache.Process(result);
            
            switch (_numeralMode)
            {
                case NumeralMode.Devanagari: result = NumeralConverter.ToDevanagariNumerals(result); break;
                case NumeralMode.Western: result = NumeralConverter.ToWesternNumerals(result); break;
            }
            return result;
        }
        
        /// <summary>Forces a refresh of all text processing.</summary>
        public void Refresh() { ProcessAllText(); }
    }
}
