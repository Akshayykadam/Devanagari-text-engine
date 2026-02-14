# Devanagari Text Engine for Unity

**Unified Hindi, Marathi & Kruti Dev text rendering for Unity.**

This package solves the common issues with rendering Devanagari script in Unity, such as broken conjuncts (e.g., क्ष, त्र), detached matras, and missing ligatures. It supports both **Unicode** (standard) and **Legacy** (Kruti Dev) workflows.

## Features

- ✅ **Unicode Support** — Proper NFC normalization for Hindi & Marathi
- ✅ **Legacy Mode (Kruti Dev)** — Auto-converts Unicode text to Kruti Dev encoding on the fly
- ✅ **Input Fields** — Type in Hindi/Marathi and see it render correctly (even in Legacy Mode!)
- ✅ **Unified Package** — Single solution for both Hindi & Marathi languages
- ✅ **TextMeshPro & Legacy Text** — Works with both UI systems
- ✅ **Zero-Config** — "Legacy Mode" is enabled by default for immediate results
- ✅ **Editor Tools** — Font validator, demo scenes, and easy testing
- ✅ **Bundled Assets** — Includes `KrutiDev010.ttf` and TMP Asset

## Quick Start

### 1. Install
Add the package to your project.

### 2. Legacy Mode (Recommended for Kruti Dev)
If you are using the **Kruti Dev** font (standard for many Hindi typing needs):

1.  **Add Component**: Add `Devanagari Text Mesh Pro` (or `Devanagari Text Fixer`) to your GameObject.
2.  **Assign Font**: Ensure your TextMeshPro component uses the included **`KrutiDev010_SDF`** asset.
3.  **Done!**: The **Legacy Mode** toggle is on by default. Any Hindi text you type in the Inspector or set via script will automatically render correctly.

> **Note**: In Legacy Mode, you don't need to select a language. The engine handles the font-specific mapping automatically.

### 3. Unicode Mode (Standard/Google Fonts)
If you are using a Unicode font like **Noto Sans** or **Poppins**:

1.  **Uncheck Legacy Mode**: In the component inspector, disable "Legacy Mode".
2.  **Select Language**: Choose **Hindi** or **Marathi** (crucial for correct rendering of characters like `ळ` and `ऱ`).
3.  **Assign Font**: Use a Unicode-compliant Devanagari font.

## Input Fields

This package fixes the issue where typing in an Input Field shows square boxes or broken characters.

1.  Create a **TMP Input Field**.
2.  Add the `Devanagari TMP Input Field` component.
3.  Assign the **KrutiDev010_SDF** font asset to the Input Field's Text Component.
4.  **Play**: As you type, the text is automatically converted and shaped in real-time. Supports placeholder text too!

## Components

| Component | Description |
|-----------|-------------|
| `DevanagariTextFixer` | Universal fixer. Attach to any object with Text/TMP to fix it. |
| `DevanagariTextMeshProUGUI` | A standalone TMP UGUI component with built-in fixing. |
| `DevanagariTextMeshPro` | A standalone 3D TMP component. |
| `DevanagariLegacyText` | For Unity's legacy `Text` UI. |
| `DevanagariTMPInputField` | **New!** Auto-fixes input text and placeholders while typing. |
| `DevanagariLegacyInputField`| **New!** Same as above, for legacy Input Fields. |

## Programmatic Usage

```csharp
using DevanagariText.Components;

// 1. Basic Text Setting
var fixer = GetComponent<DevanagariTextFixer>();
fixer.Text = "नमस्ते दुनिया"; // Automatically converts if Legacy Mode is on

// 2. Manual Conversion (if needed)
using DevanagariText.TextProcessing;
string krutiText = KrutidevConverter.Convert("नमस्ते"); 
// Returns encoded string for Kruti Dev font
```

## Folder Structure
```
com.akshay.devanagari-text-engine/
├── Runtime/
│   ├── Fonts/          # Includes Kruti Dev & Noto Sans assets
│   ├── Components/     # All script components
│   └── ...
└── Editor/             # Custom Inspectors & Tools
```

## Requirements
- Unity 2021.3+
- TextMeshPro (com.unity.textmeshpro)

## License
MIT License — Copyright (c) 2026 Akshay Kadam
