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
    /// Extended TextMeshPro component (3D world space) with Devanagari text support (Hindi/Marathi).
    /// </summary>
    [AddComponentMenu("Devanagari Text/Devanagari TextMeshPro - 3D")]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class DevanagariTextMeshPro : MonoBehaviour
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
        
        private TextMeshPro _tmpText;
        
        [SerializeField]
        [HideInInspector]
        private string _originalText = "";
        private string _lastProcessedText = "";
        private bool _isProcessing = false;
        
        /// <summary>
        /// Gets or sets the language for language-specific processing.
        /// </summary>
        public DevanagariLanguage Language
        {
            get => _language;
            set
            {
                if (_language != value)
                {
                    _language = value;
                    ProcessAndApply();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets whether to use Legacy Mode (Kruti Dev conversion).
        /// </summary>
        public bool LegacyMode
        {
            get => _legacyMode;
            set
            {
                if (_legacyMode != value)
                {
                    _legacyMode = value;
                    ProcessAndApply();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the original text.
        /// </summary>
        public string OriginalText
        {
            get => _originalText;
            set
            {
                if (_originalText != value)
                {
                    _originalText = value ?? "";
                    ProcessAndApply();
                }
            }
        }
        
        public string ProcessedText => _lastProcessedText;
        
        public TextMeshPro TextComponent
        {
            get
            {
                if (_tmpText == null) _tmpText = GetComponent<TextMeshPro>();
                return _tmpText;
            }
        }
        
        private void Awake() { _tmpText = GetComponent<TextMeshPro>(); }
        
        private void OnEnable()
        {
            _tmpText = GetComponent<TextMeshPro>();
            
            if (_tmpText != null)
            {
                TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
                if (string.IsNullOrEmpty(_originalText) && !string.IsNullOrEmpty(_tmpText.text))
                    _originalText = _tmpText.text;
            }
            
            ProcessAndApply();
        }
        
        private void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }
        
        private void OnDestroy()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }
        
        private void OnValidate()
        {
            _tmpText = GetComponent<TextMeshPro>();
            ProcessAndApply();
        }
        
        private void OnTextChanged(UnityEngine.Object obj)
        {
            if (obj == _tmpText && !_isProcessing)
            {
                string currentText = _tmpText.text;
                if (currentText != _lastProcessedText)
                {
                    _originalText = currentText;
                    #if UNITY_EDITOR
                    if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
                    #endif
                    ProcessAndApply();
                }
            }
        }
        
        private void ProcessAndApply()
        {
            if (_tmpText == null)
            {
                _tmpText = GetComponent<TextMeshPro>();
                if (_tmpText == null) return;
            }
            
            _isProcessing = true;
            try
            {
                string result = _originalText;
                
                if (_enableNormalization && DevanagariTextProcessor.NeedsProcessing(result))
                    result = GlobalProcessedTextCache.Process(result);
                
                switch (_numeralMode)
                {
                    case NumeralMode.Devanagari:
                        result = NumeralConverter.ToDevanagariNumerals(result);
                        break;
                    case NumeralMode.Western:
                        result = NumeralConverter.ToWesternNumerals(result);
                        break;
                }
                
                if (_legacyMode)
                {
                    result = KrutidevConverter.Convert(result);
                }
                
                if (_tmpText.text != result)
                    _tmpText.text = result;
                
                _lastProcessedText = result;
                
                if (_debugMode)
                    Debug.Log($"[DevanagariTextMeshPro] '{gameObject.name}' ({_language}) | " +
                              $"Original: '{_originalText}' | Processed: '{result}'");
            }
            finally { _isProcessing = false; }
        }
        
        /// <summary>Forces a refresh.</summary>
        public void Refresh() { ProcessAndApply(); }
    }
}
