# Changelog

## [1.0.0] - 2026-02-14

### Added
- Unified Devanagari text engine supporting Hindi and Marathi
- Zero-config `DevanagariTextFixer` component (auto-detects TMP and legacy Text)
- `DevanagariTextMeshProUGUI` and `DevanagariTextMeshPro` extended components
- `DevanagariLegacyText` for legacy UI.Text support
- `DevanagariTMPInputField` and `DevanagariLegacyInputField` Devanagari-aware input fields
- `DevanagariUIDocument` for UI Toolkit with ATG support
- Unicode NFC normalization for proper conjunct rendering
- Numeral conversion (Devanagari ↔ Western)
- Devanagari-aware text search with language-specific normalization
- Marathi-specific: Eyelash Ra (ऱ) normalization, ळ (LLA) support
- Editor tools: Font Validator, Font Asset Generator, Demo Scene creators
- Bundled Noto Sans Devanagari fonts (Regular & Bold)
