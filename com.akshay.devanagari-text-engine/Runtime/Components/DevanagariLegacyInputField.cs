// Devanagari Text Engine
// Copyright (c) 2026 Akshay Kadam. MIT License.

using UnityEngine;
using UnityEngine.UI;
using DevanagariText.Core;
using DevanagariText.Shaping;
using DevanagariText.TextProcessing;

namespace DevanagariText.Components
{
    /// <summary>
    /// Devanagari-aware legacy InputField component (Hindi/Marathi).
    /// </summary>
    [AddComponentMenu("Devanagari Text/Devanagari Legacy Input Field")]
    [RequireComponent(typeof(InputField))]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class DevanagariLegacyInputField : MonoBehaviour
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
        
        private InputField _inputField;
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
        
        public string LogicalText => _inputField != null ? _inputField.text : "";
        
        public InputField InputFieldComponent
        {
            get { if (_inputField == null) _inputField = GetComponent<InputField>(); return _inputField; }
        }
        
        private void Awake() { _inputField = GetComponent<InputField>(); }
        
        private void OnEnable()
        {
            _inputField = GetComponent<InputField>();
            if (_inputField != null)
            {
                _inputField.onValueChanged.AddListener(OnValueChanged);
                
                // Handle Placeholder if it exists and we are in Legacy Mode
                if (_legacyMode && _inputField.placeholder != null)
                {
                    var placeholderText = _inputField.placeholder.GetComponent<Text>();
                    if (placeholderText != null)
                    {
                        string original = placeholderText.text;
                        if (DevanagariTextProcessor.NeedsProcessing(original))
                        {
                            placeholderText.text = KrutidevConverter.Convert(original);
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
                    int originalLength = newValue.Length;
                    int newLength = processed.Length;
                    int caretPos = _inputField.caretPosition;

                    _inputField.text = processed;
                    
                    int diff = newLength - originalLength;
                    _inputField.caretPosition = Mathf.Clamp(caretPos + diff, 0, newLength);
                }
                if (_debugMode)
                    Debug.Log($"[DevanagariLegacyInputField] '{gameObject.name}' ({_language}) | Input: '{newValue}' | Processed: '{processed}'");
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
        
        public string GetSearchKey()
        {
            return DevanagariNormalizer.PrepareSearchKey(LogicalText, _language);
        }
    }
}
