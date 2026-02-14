// Devanagari Text Engine
// Copyright (c) 2026 Akshay Kadam. MIT License.

using UnityEngine;
using TMPro;
using DevanagariText.Core;
using DevanagariText.Shaping;
using DevanagariText.TextProcessing;

namespace DevanagariText.Components
{
    /// <summary>
    /// Devanagari-aware TMP InputField component (Hindi/Marathi).
    /// Handles text input with proper NFC normalization and optional numeral conversion.
    /// </summary>
    [AddComponentMenu("Devanagari Text/Devanagari TMP Input Field")]
    [RequireComponent(typeof(TMP_InputField))]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class DevanagariTMPInputField : MonoBehaviour
    {
        [Header("Language")]
        [SerializeField]
        [Tooltip("Select language for language-specific normalization")]
        private DevanagariLanguage _language = DevanagariLanguage.Hindi;
        
        [Header("Settings")]
        [SerializeField]
        [Tooltip("Enable Unicode NFC normalization")]
        private bool _enableNormalization = true;
        
        [SerializeField]
        [Tooltip("Legacy Mode: Converts text to Kruti Dev 010 encoding")]
        private bool _legacyMode = true;
        
        [SerializeField]
        [Tooltip("Numeral display mode")]
        private NumeralMode _numeralMode = NumeralMode.None;
        
        [Header("Debug")]
        [SerializeField]
        [Tooltip("Show debug information in console")]
        private bool _debugMode = false;
        
        private TMP_InputField _inputField;
        private bool _isProcessing = false;
        
        public DevanagariLanguage Language
        {
            get => _language;
            set => _language = value;
        }

        public bool LegacyMode
        {
            get => _legacyMode;
            set => _legacyMode = value;
        }
        
        /// <summary>
        /// Gets the logical (unprocessed) text for search/data operations.
        /// </summary>
        public string LogicalText => _inputField != null ? _inputField.text : "";
        
        public TMP_InputField InputField
        {
            get { if (_inputField == null) _inputField = GetComponent<TMP_InputField>(); return _inputField; }
        }
        
        private void Awake() { _inputField = GetComponent<TMP_InputField>(); }
        
        private void OnEnable()
        {
            _inputField = GetComponent<TMP_InputField>();
            if (_inputField != null)
            {
                _inputField.onValueChanged.AddListener(OnValueChanged);
                
                // Handle Placeholder if it exists and we are in Legacy Mode
                if (_legacyMode && _inputField.placeholder != null)
                {
                    var placeholderDetails = _inputField.placeholder.GetComponent<TMPro.TMP_Text>();
                    if (placeholderDetails != null)
                    {
                        // We only convert if it looks like Devanagari (simple check) 
                        // or just force convert since it's legacy mode. 
                        // But wait, if we convert it once, we shouldn't convert it again if it's already converted code.
                        // For simplicity in this fix, let's just convert it. 
                        // Ideally, users should type english or converted text, but here they typed Unicode.
                        string original = placeholderDetails.text;
                        // Avoid double conversion if possible, but detection is hard.
                        // Let's assume if it has Devanagari chars, Convert.
                        if (DevanagariTextProcessor.NeedsProcessing(original))
                        {
                            placeholderDetails.text = KrutidevConverter.Convert(original);
                        }
                    }
                }
            }
        }
        
        private void OnDisable()
        {
            if (_inputField != null) _inputField.onValueChanged.RemoveListener(OnValueChanged);
        }
        
        private void OnValueChanged(string newValue)
        {
            if (_isProcessing) return;
            _isProcessing = true;
            try
            {
                string processed = ProcessText(newValue);
                if (processed != newValue)
                {
                    // For input fields, we must be careful not to mess up the caret too badly.
                    // When converting to Kruti (Legacy), the length might change (e.g. 2 unicode chars -> 1 kruti char).
                    int originalLength = newValue.Length;
                    int newLength = processed.Length;
                    int caretPos = _inputField.caretPosition;
                    
                    _inputField.text = processed;
                    
                    // Simple caret adjustment attempts to keep relative position
                    // but complex scripts might need more robust handling.
                    // For now, clamping is the safest bet to avoid crashes.
                    int diff = newLength - originalLength;
                    _inputField.caretPosition = Mathf.Clamp(caretPos + diff, 0, newLength);
                }
                
                if (_debugMode)
                    Debug.Log($"[DevanagariTMPInputField] '{gameObject.name}' ({_language}) | Input: '{newValue}' | Processed: '{processed}'");
            }
            finally { _isProcessing = false; }
        }
        
        private string ProcessText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string result = text;
            
            if (_enableNormalization && DevanagariTextProcessor.NeedsProcessing(result))
                result = DevanagariTextProcessor.Process(result);
                
            switch (_numeralMode)
            {
                case NumeralMode.Devanagari: result = NumeralConverter.ToDevanagariNumerals(result); break;
                case NumeralMode.Western: result = NumeralConverter.ToWesternNumerals(result); break;
            }
            
            if (_legacyMode)
            {
                result = KrutidevConverter.Convert(result);
            }
            
            return result;
        }
        
        /// <summary>
        /// Gets the normalized text for search operations.
        /// </summary>
        public string GetSearchKey()
        {
            return DevanagariNormalizer.PrepareSearchKey(LogicalText, _language);
        }
    }
}
