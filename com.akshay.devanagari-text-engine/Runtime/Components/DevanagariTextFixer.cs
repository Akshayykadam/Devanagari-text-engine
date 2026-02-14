// Devanagari Text Engine
// Copyright (c) 2026 Akshay Kadam. MIT License.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DevanagariText.Core;
using DevanagariText.Shaping;
using DevanagariText.TextProcessing;

namespace DevanagariText.Components
{
    /// <summary>
    /// Text direction enum for Devanagari text (primarily LTR, but supports auto-detect).
    /// </summary>
    public enum TextDirection
    {
        Auto,
        LTR,
        RTL
    }
    
    /// <summary>
    /// Numeral display mode for Devanagari text.
    /// </summary>
    public enum NumeralMode
    {
        /// <summary>Keep numerals as-is.</summary>
        None,
        /// <summary>Convert Western digits to Devanagari (0-9 → ०-९).</summary>
        Devanagari,
        /// <summary>Convert Devanagari digits to Western (०-९ → 0-9).</summary>
        Western
    }
    
    /// <summary>
    /// Zero-config Devanagari text fixer supporting Hindi and Marathi.
    /// Attach this component to any GameObject with a TMP_Text or legacy Text component
    /// and it will automatically handle Devanagari text processing.
    /// 
    /// Usage:
    /// 1. Add this component to any GameObject with a text component
    /// 2. Select language (Hindi or Marathi)
    /// 3. That's it! The text will be automatically processed.
    /// 
    /// Features:
    /// - Auto-detects Devanagari text
    /// - Applies Unicode NFC normalization for proper conjunct rendering
    /// - Optional Devanagari numeral conversion
    /// - Works with TextMeshPro (UI and 3D) and legacy Text
    /// </summary>
    [AddComponentMenu("Devanagari Text/Devanagari Text Fixer (Auto)")]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class DevanagariTextFixer : MonoBehaviour
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
        [Tooltip("Legacy Mode: Converts text to Kruti Dev 010 encoding. Use this if you are using the Kruti Dev font.")]
        private bool _legacyMode = true;
        
        [SerializeField]
        [Tooltip("TMP Font Asset name for English/Latin text when using Legacy Mode (e.g. 'LiberationSans SDF'). Leave empty to skip font-switching.")]
        private string _fallbackFontName = "LiberationSans SDF";
        
        [SerializeField]
        [Tooltip("Numeral display mode: None (keep as-is), Devanagari (0→०), or Western (०→0)")]
        private NumeralMode _numeralMode = NumeralMode.None;
        
        [Header("Advanced")]
        [SerializeField]
        [Tooltip("Process text even in Edit mode (useful for previewing)")]
        private bool _processInEditMode = true;
        
        [SerializeField]
        [Tooltip("Check interval in seconds (0 = every frame, higher = better performance)")]
        [Range(0f, 1f)]
        private float _checkInterval = 0f;
        
        [Header("Debug")]
        [SerializeField]
        [Tooltip("Show debug information in console")]
        private bool _debugMode = false;
        
        // Cached components
        private TMP_Text _tmpText;
        private Text _legacyText;
        private bool _isLegacyText;
        
        // State tracking
        private string _lastRawText = "";
        private string _lastProcessedText = "";
        private string _originalLogicalText = "";
        
        private float _lastCheckTime;
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
                    UpdateText(true);
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
                    UpdateText(true);
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the original logical (unprocessed) text.
        /// Use this property to set text programmatically.
        /// </summary>
        public string Text
        {
            get => _originalLogicalText;
            set
            {
                if (_originalLogicalText != value)
                {
                    _originalLogicalText = value ?? "";
                    UpdateText(true);
                }
            }
        }
        
        /// <summary>
        /// Gets the underlying TMP_Text component. Returns null if using legacy Text.
        /// </summary>
        public TMP_Text TextComponent
        {
            get
            {
                EnsureTextComponent();
                return _tmpText;
            }
        }
        
        /// <summary>
        /// Gets the underlying legacy Text component. Returns null if using TMP.
        /// </summary>
        public Text LegacyTextComponent
        {
            get
            {
                EnsureTextComponent();
                return _legacyText;
            }
        }
        
        /// <summary>
        /// Gets or sets the numeral display mode.
        /// </summary>
        public NumeralMode NumeralDisplayMode
        {
            get => _numeralMode;
            set
            {
                if (_numeralMode != value)
                {
                    _numeralMode = value;
                    UpdateText(true);
                }
            }
        }
        
        private void Awake()
        {
            EnsureTextComponent();
        }
        
        private void OnEnable()
        {
            EnsureTextComponent();
            
            if (_tmpText != null)
            {
                TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTMPTextChanged);
                
                if (string.IsNullOrEmpty(_originalLogicalText) && !string.IsNullOrEmpty(_tmpText.text))
                {
                    _originalLogicalText = _tmpText.text;
                    _lastRawText = _tmpText.text;
                }
            }
            
            if (_legacyText != null)
            {
                if (string.IsNullOrEmpty(_originalLogicalText) && !string.IsNullOrEmpty(_legacyText.text))
                {
                    _originalLogicalText = _legacyText.text;
                    _lastRawText = _legacyText.text;
                }
            }
            
            UpdateText(true);
        }
        
        private void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTMPTextChanged);
        }
        
        private void OnDestroy()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTMPTextChanged);
        }
        
        private void Update()
        {
            if (!enabled) return;
            if (!Application.isPlaying && !_processInEditMode) return;
            
            if (_checkInterval > 0f)
            {
                if (Time.realtimeSinceStartup - _lastCheckTime < _checkInterval)
                    return;
                _lastCheckTime = Time.realtimeSinceStartup;
            }
            
            CheckForExternalTextChange();
        }
        
        private void OnValidate()
        {
            EnsureTextComponent();
            if (_tmpText != null || _legacyText != null)
                UpdateText(true);
        }
        
        private void EnsureTextComponent()
        {
            if (_tmpText == null && _legacyText == null)
            {
                _tmpText = GetComponent<TextMeshProUGUI>();
                if (_tmpText == null)
                    _tmpText = GetComponent<TextMeshPro>();
                
                if (_tmpText != null)
                {
                    _isLegacyText = false;
                }
                else
                {
                    _legacyText = GetComponent<Text>();
                    _isLegacyText = _legacyText != null;
                }
            }
        }
        
        private void OnTMPTextChanged(UnityEngine.Object obj)
        {
            if (obj == _tmpText && !_isProcessing)
                CheckForExternalTextChange();
        }
        
        private void CheckForExternalTextChange()
        {
            if (_isProcessing) return;
            if (_tmpText == null && _legacyText == null) return;
            
            string currentText = _isLegacyText ? _legacyText.text : _tmpText.text;
            
            if (currentText != _lastProcessedText && currentText != _lastRawText)
            {
                _originalLogicalText = currentText;
                
                #if UNITY_EDITOR
                if (!Application.isPlaying)
                    UnityEditor.EditorUtility.SetDirty(this);
                #endif
                
                UpdateText(true);
            }
        }
        
        private void UpdateText(bool force)
        {
            if (_tmpText == null && _legacyText == null)
            {
                EnsureTextComponent();
                if (_tmpText == null && _legacyText == null) return;
            }
            
            if (!force && _originalLogicalText == _lastRawText) return;
            
            _isProcessing = true;
            
            try
            {
                string processedText = ProcessText(_originalLogicalText);
                
                if (_isLegacyText)
                {
                    if (_legacyText.text != processedText)
                        _legacyText.text = processedText;
                }
                else
                {
                    if (_tmpText.text != processedText)
                        _tmpText.text = processedText;
                }
                
                _lastRawText = _originalLogicalText;
                _lastProcessedText = processedText;
                
                if (_debugMode)
                {
                    string componentType = _isLegacyText ? "Legacy Text" : "TMP";
                    string mode = _legacyMode ? "Legacy(Kruti)" : "Unicode";
                    Debug.Log($"[DevanagariTextFixer] '{gameObject.name}' ({componentType}, {_language}, {mode}) | " +
                              $"Original: '{_originalLogicalText}' | Processed: '{processedText}'");
                }
            }
            finally
            {
                _isProcessing = false;
            }
        }
        
        private string ProcessText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            
            string result = text;
            
            // Step 1: Pre-processing (Normalization)
            if (_enableNormalization && DevanagariTextProcessor.NeedsProcessing(result))
            {
                 // Apply standard Unicode normalization first
                 result = GlobalProcessedTextCache.Process(result);
            }
            
            // Step 2: Handle numeral conversion
            switch (_numeralMode)
            {
                case NumeralMode.Devanagari:
                    result = NumeralConverter.ToDevanagariNumerals(result);
                    break;
                case NumeralMode.Western:
                    result = NumeralConverter.ToWesternNumerals(result);
                    break;
            }
            
            // Step 3: Legacy Conversion (Kruti Dev)
            // This happens LAST because the input is usually Unicode.
            if (_legacyMode)
            {
                result = KrutidevConverter.Convert(result, _fallbackFontName);
            }
            
            return result;
        }
        
        /// <summary>
        /// Forces a refresh of the text processing.
        /// </summary>
        public void Refresh()
        {
            UpdateText(true);
        }
        
        /// <summary>
        /// Manually sets the text with processing.
        /// </summary>
        public void SetText(string text)
        {
            Text = text;
        }
        
        /// <summary>
        /// Gets the original logical (unprocessed) text.
        /// </summary>
        public string GetLogicalText()
        {
            return _originalLogicalText;
        }
        
        /// <summary>
        /// Gets the processed (displayed) text.
        /// </summary>
        public string GetDisplayText()
        {
            return _lastProcessedText;
        }
        
        #if UNITY_EDITOR
        private void Reset()
        {
            EnsureTextComponent();
            
            if (_tmpText != null)
            {
                _originalLogicalText = _tmpText.text;
                UnityEditor.EditorUtility.SetDirty(this);
            }
            else if (_legacyText != null)
            {
                _originalLogicalText = _legacyText.text;
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
        #endif
    }
}
