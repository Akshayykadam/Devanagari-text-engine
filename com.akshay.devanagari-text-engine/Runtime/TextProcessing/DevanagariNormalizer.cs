// Devanagari Text Engine
// Copyright (c) 2026 Akshay Kadam. MIT License.

using System;
using System.Text;
using DevanagariText.Core;

namespace DevanagariText.TextProcessing
{
    /// <summary>
    /// Provides Devanagari text normalization for search and comparison.
    /// Handles nukta variants, chandrabindu/anusvara normalization,
    /// matra removal, and Marathi-specific Eyelash Ra normalization.
    /// </summary>
    public static class DevanagariNormalizer
    {
        // Nukta-bearing consonants and their base forms
        // क़ (U+0958) → क (U+0915)
        // ख़ (U+0959) → ख (U+0916)
        // ग़ (U+095A) → ग (U+0917)
        // ज़ (U+095B) → ज (U+091C)
        // ड़ (U+095C) → ड (U+0921)
        // ढ़ (U+095D) → ढ (U+0922)
        // फ़ (U+095E) → फ (U+092B)
        // य़ (U+095F) → य (U+092F)
        
        [ThreadStatic]
        private static StringBuilder _sharedBuilder;
        
        private static StringBuilder GetBuilder(int capacity)
        {
            var builder = _sharedBuilder;
            if (builder == null)
            {
                _sharedBuilder = new StringBuilder(capacity);
                return _sharedBuilder;
            }
            builder.Clear();
            if (builder.Capacity < capacity)
                builder.Capacity = capacity;
            return builder;
        }
        
        /// <summary>
        /// Removes all dependent vowel signs (matras) from Devanagari text.
        /// Useful for consonant-skeleton matching.
        /// </summary>
        public static string RemoveMatras(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            
            bool hasMatras = false;
            foreach (char c in input)
            {
                if (DevanagariUnicodeRanges.IsMatra(c))
                {
                    hasMatras = true;
                    break;
                }
            }
            
            if (!hasMatras)
                return input;
            
            var builder = GetBuilder(input.Length);
            foreach (char c in input)
            {
                if (!DevanagariUnicodeRanges.IsMatra(c))
                    builder.Append(c);
            }
            
            return builder.ToString();
        }
        
        /// <summary>
        /// Normalizes nukta (dot below) variants to their base consonant forms.
        /// For example: क़ → क, ख़ → ख, ग़ → ग, ज़ → ज, ड़ → ड, ढ़ → ढ, फ़ → फ
        /// Handles both atomic nukta forms (U+0958-095F) and decomposed (consonant + U+093C).
        /// </summary>
        public static string NormalizeNukta(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            
            var builder = GetBuilder(input.Length);
            foreach (char c in input)
            {
                char normalized = NormalizeNuktaChar(c);
                if (normalized != '\0')
                    builder.Append(normalized);
            }
            
            return builder.ToString();
        }
        
        /// <summary>
        /// Normalizes anusvara and chandrabindu marks.
        /// Chandrabindu (ँ U+0901) is normalized to anusvara (ं U+0902)
        /// for consistent matching.
        /// </summary>
        public static string NormalizeAnusvara(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            
            bool hasChandrabindu = false;
            foreach (char c in input)
            {
                if (c == DevanagariUnicodeRanges.Chandrabindu)
                {
                    hasChandrabindu = true;
                    break;
                }
            }
            
            if (!hasChandrabindu)
                return input;
            
            var builder = GetBuilder(input.Length);
            foreach (char c in input)
            {
                if (c == DevanagariUnicodeRanges.Chandrabindu)
                    builder.Append(DevanagariUnicodeRanges.Anusvara);
                else
                    builder.Append(c);
            }
            
            return builder.ToString();
        }
        
        /// <summary>
        /// Normalizes Eyelash Ra (ऱ, U+0931) to standard Ra (र, U+0930).
        /// Eyelash Ra is used in Marathi for the "r" sound in certain contexts.
        /// For search normalization, they should be treated as equivalent.
        /// </summary>
        public static string NormalizeEyelashRa(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            
            bool hasEyelashRa = false;
            foreach (char c in input)
            {
                if (c == DevanagariUnicodeRanges.EyelashRa)
                {
                    hasEyelashRa = true;
                    break;
                }
            }
            
            if (!hasEyelashRa)
                return input;
            
            var builder = GetBuilder(input.Length);
            foreach (char c in input)
            {
                if (c == DevanagariUnicodeRanges.EyelashRa)
                    builder.Append('\u0930'); // Standard Ra
                else
                    builder.Append(c);
            }
            
            return builder.ToString();
        }
        
        /// <summary>
        /// Removes all diacritic marks (matras, anusvara, chandrabindu, visarga,
        /// nukta, virama) from Devanagari text.
        /// </summary>
        public static string RemoveDiacritics(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            
            bool hasDiacritics = false;
            foreach (char c in input)
            {
                if (DevanagariUnicodeRanges.IsDiacriticMark(c))
                {
                    hasDiacritics = true;
                    break;
                }
            }
            
            if (!hasDiacritics)
                return input;
            
            var builder = GetBuilder(input.Length);
            foreach (char c in input)
            {
                if (!DevanagariUnicodeRanges.IsDiacriticMark(c))
                    builder.Append(c);
            }
            
            return builder.ToString();
        }
        
        /// <summary>
        /// Performs full Devanagari text normalization:
        /// - Normalizes nukta variants to base consonants
        /// - Normalizes chandrabindu to anusvara
        /// - Removes nukta marks
        /// - Converts to lowercase (for Latin letters)
        /// - For Marathi: also normalizes Eyelash Ra (ऱ→र)
        /// </summary>
        /// <param name="input">The text to normalize.</param>
        /// <param name="language">The target language for language-specific rules.</param>
        /// <returns>Fully normalized text.</returns>
        public static string Normalize(string input, DevanagariLanguage language = DevanagariLanguage.Hindi)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            
            var builder = GetBuilder(input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                
                // Skip nukta combining mark
                if (c == DevanagariUnicodeRanges.Nukta)
                    continue;
                
                // Normalize atomic nukta forms
                char normalized = NormalizeNuktaChar(c);
                
                // Normalize chandrabindu → anusvara
                if (normalized == DevanagariUnicodeRanges.Chandrabindu)
                    normalized = DevanagariUnicodeRanges.Anusvara;
                
                // Marathi-specific: Normalize Eyelash Ra → standard Ra
                if (language == DevanagariLanguage.Marathi && normalized == DevanagariUnicodeRanges.EyelashRa)
                    normalized = '\u0930';
                
                // Lowercase Latin letters
                if (normalized >= 'A' && normalized <= 'Z')
                    normalized = (char)(normalized + 32);
                
                builder.Append(normalized);
            }
            
            return builder.ToString();
        }
        
        /// <summary>
        /// Normalizes text for search indexing and matching.
        /// </summary>
        public static string PrepareSearchKey(string input, DevanagariLanguage language = DevanagariLanguage.Hindi)
        {
            return Normalize(input, language);
        }
        
        /// <summary>
        /// Checks if two strings are equal after normalization.
        /// </summary>
        public static bool NormalizedEquals(string a, string b, DevanagariLanguage language = DevanagariLanguage.Hindi)
        {
            if (a == b) return true;
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
                return string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b);
            
            return string.Equals(Normalize(a, language), Normalize(b, language), StringComparison.Ordinal);
        }
        
        /// <summary>
        /// Normalizes a single character — converts atomic nukta consonants
        /// to their base forms.
        /// </summary>
        private static char NormalizeNuktaChar(char c)
        {
            switch (c)
            {
                case '\u0958': return '\u0915'; // क़ → क (Qa → Ka)
                case '\u0959': return '\u0916'; // ख़ → ख (Khha → Kha)
                case '\u095A': return '\u0917'; // ग़ → ग (Ghha → Ga)
                case '\u095B': return '\u091C'; // ज़ → ज (Za → Ja)
                case '\u095C': return '\u0921'; // ड़ → ड (Dddha → Dda)
                case '\u095D': return '\u0922'; // ढ़ → ढ (Rha → Ddha)
                case '\u095E': return '\u092B'; // फ़ → फ (Fa → Pha)
                case '\u095F': return '\u092F'; // य़ → य (Yya → Ya)
                default: return c;
            }
        }
    }
}
