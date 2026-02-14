# Devanagari Text Engine for Unity

**Unified Hindi, Marathi & Kruti Dev text rendering for Unity.**

Solves broken conjuncts (क्ष, त्र), detached matras, and missing ligatures in Unity's text components. Supports **Unicode** and **Legacy (Kruti Dev)** workflows with full mixed-language text support.

## Features

- ✅ **Unicode NFC Normalization** — fixes broken conjuncts for Hindi & Marathi
- ✅ **Legacy Mode (Kruti Dev)** — auto-converts Unicode to Kruti Dev encoding on the fly
- ✅ **Mixed-Language Safe** — English, numbers, and symbols (₹, etc.) pass through unchanged in Legacy Mode
- ✅ **Marathi Support** — proper ळ (LLA) and ऱ (Eyelash Ra) handling
- ✅ **Input Fields** — real-time conversion while typing (TMP & Legacy)
- ✅ **Devanagari Numerals** — toggle between Western (0-9) and Devanagari (०-९)
- ✅ **TextMeshPro & Legacy Text** — works with both UI systems
- ✅ **Zero-Config** — attach component, assign font, done
- ✅ **Editor Tools** — font validator, demo scenes, easy testing
- ✅ **Bundled Assets** — includes `KrutiDev010.ttf` and pre-built TMP SDF asset

## Quick Start

### Legacy Mode (Recommended — Kruti Dev Font)

1. Add `Devanagari Text Fixer` (or `Devanagari Text Mesh Pro`) to your GameObject.
2. Assign the included **`KrutiDev010_SDF`** font asset to your TMP component.
3. **Done!** Legacy Mode is on by default — Hindi/Marathi text renders correctly out of the box.

> **Mixed text works too:** A string like `"Hello नमस्ते ₹999 बाळ"` will convert only the Devanagari portions, leaving English and symbols untouched.

### Unicode Mode (Noto Sans / Google Fonts)

1. Uncheck **Legacy Mode** in the Inspector.
2. Select **Hindi** or **Marathi** (important for `ळ` / `ऱ` handling).
3. Assign a Unicode-compliant Devanagari font.

## Input Fields

1. Create a **TMP Input Field**.
2. Add the `Devanagari TMP Input Field` component.
3. Assign **KrutiDev010_SDF** to the Input Field's Text Component.
4. **Play** — text converts and shapes in real-time as you type. Placeholder text is supported.

## Components

| Component | Description |
|-----------|-------------|
| `DevanagariTextFixer` | Universal fixer. Attach to any Text/TMP object. |
| `DevanagariTextMeshProUGUI` | Standalone TMP UGUI with built-in fixing. |
| `DevanagariTextMeshPro` | Standalone 3D TMP component. |
| `DevanagariLegacyText` | For Unity's legacy `Text` UI. |
| `DevanagariTMPInputField` | Auto-fixes input text and placeholders while typing. |
| `DevanagariLegacyInputField` | Same as above, for legacy Input Fields. |

## Programmatic Usage

```csharp
using DevanagariText.Components;

// Set text — auto-converts if Legacy Mode is on
var fixer = GetComponent<DevanagariTextFixer>();
fixer.Text = "Hello नमस्ते 999";

// Manual conversion
using DevanagariText.TextProcessing;
string krutiText = KrutidevConverter.Convert("नमस्ते");
```

## Folder Structure
```
com.akshay.devanagari-text-engine/
├── Runtime/
│   ├── Fonts/          # Kruti Dev & Noto Sans assets
│   ├── Components/     # All script components
│   ├── TextProcessing/  # Converter, normalizer, search
│   ├── Core/           # Unicode ranges, language enum
│   └── Shaping/        # Text processor, cache
└── Editor/             # Custom Inspectors & Tools
```

## Requirements
- Unity 2021.3+
- TextMeshPro (com.unity.textmeshpro)

## License
MIT License — Copyright (c) 2026 Akshay Kadam
