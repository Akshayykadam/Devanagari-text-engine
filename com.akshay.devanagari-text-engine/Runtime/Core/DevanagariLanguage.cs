// Devanagari Text Engine
// Copyright (c) 2026 Akshay Kadam. MIT License.

namespace DevanagariText.Core
{
    /// <summary>
    /// Supported Devanagari languages.
    /// Controls language-specific normalization and search behavior.
    /// </summary>
    public enum DevanagariLanguage
    {
        /// <summary>Hindi — standard Devanagari normalization.</summary>
        Hindi,
        /// <summary>Marathi — adds Eyelash Ra (ऱ→र) normalization and ळ handling.</summary>
        Marathi
    }
}
