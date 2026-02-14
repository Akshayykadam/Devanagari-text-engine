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
    /// Extended legacy Text component with Devanagari text support (Hindi/Marathi).
    /// </summary>
    [AddComponentMenu("Devanagari Text/Devanagari Legacy Text")]
    [RequireComponent(typeof(Text))]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class DevanagariLegacyText : MonoBehaviour
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
        
        [Header("Advanced")]
        [SerializeField]
        [Tooltip("Process text even in Edit mode")]
        private bool _processInEditMode = true;
        
        [SerializeField]
        [Tooltip("Check interval in seconds (0 = every frame)")]
        [Range(0f, 1f)]
        private float _checkInterval = 0f;
        
        [Header("Debug")]
        [SerializeField]
        [Tooltip("Show debug information in console")]
        private bool _debugMode = false;
        
        private Text _legacyText;
        
        [SerializeField]
        [HideInInspector]
        private string _originalText = "";
        private string _lastProcessedText = "";
        private string _lastRawText = "";
        private float _lastCheckTime;
        private bool _isProcessing = false;
        
        public DevanagariLanguage Language
        {
            get => _language;
            set { if (_language != value) { _language = value; ProcessAndApply(); } }
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
        
        public string OriginalText
        {
            get => _originalText;
            set { if (_originalText != value) { _originalText = value ?? ""; ProcessAndApply(); } }
        }
        
        public string ProcessedText => _lastProcessedText;
        
        public Text TextComponent
        {
            get { if (_legacyText == null) _legacyText = GetComponent<Text>(); return _legacyText; }
        }
        
        private void Awake() { _legacyText = GetComponent<Text>(); }
        
        private void OnEnable()
        {
            _legacyText = GetComponent<Text>();
            if (_legacyText != null)
            {
                if (string.IsNullOrEmpty(_originalText) && !string.IsNullOrEmpty(_legacyText.text))
                {
                    _originalText = _legacyText.text;
                    _lastRawText = _legacyText.text;
                }
            }
            ProcessAndApply();
        }
        
        private void Update()
        {
            if (!enabled) return;
            if (!Application.isPlaying && !_processInEditMode) return;
            if (_checkInterval > 0f)
            {
                if (Time.realtimeSinceStartup - _lastCheckTime < _checkInterval) return;
                _lastCheckTime = Time.realtimeSinceStartup;
            }
            CheckForExternalTextChange();
        }
        
        private void OnValidate()
        {
            _legacyText = GetComponent<Text>();
            ProcessAndApply();
        }
        
        private void CheckForExternalTextChange()
        {
            if (_isProcessing || _legacyText == null) return;
            string currentText = _legacyText.text;
            if (currentText != _lastProcessedText && currentText != _lastRawText)
            {
                _originalText = currentText;
                #if UNITY_EDITOR
                if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
                #endif
                ProcessAndApply();
            }
        }
        
        private void ProcessAndApply()
        {
            if (_legacyText == null) { _legacyText = GetComponent<Text>(); if (_legacyText == null) return; }
            _isProcessing = true;
            try
            {
                string result = _originalText;
                if (_enableNormalization && DevanagariTextProcessor.NeedsProcessing(result))
                    result = GlobalProcessedTextCache.Process(result);
                switch (_numeralMode)
                {
                    case NumeralMode.Devanagari: result = NumeralConverter.ToDevanagariNumerals(result); break;
                    case NumeralMode.Western: result = NumeralConverter.ToWesternNumerals(result); break;
                }
                
                if (_legacyMode)
                {
                    result = KrutidevConverter.Convert(result, null);
                }
                
                if (_legacyText.text != result) _legacyText.text = result;
                _lastRawText = _originalText;
                _lastProcessedText = result;
                if (_debugMode)
                    Debug.Log($"[DevanagariLegacyText] '{gameObject.name}' ({_language}) | Original: '{_originalText}' | Processed: '{result}'");
            }
            finally { _isProcessing = false; }
        }
        
        public void Refresh() { ProcessAndApply(); }
    }
}
