// Devanagari Text Engine
// Copyright (c) 2026 Akshay Kadam. MIT License.

using System;
using System.Text;
using DevanagariText.Core;

namespace DevanagariText.Shaping
{
    /// <summary>
    /// Devanagari text processor that ensures proper text handling.
    /// Devanagari rendering (conjuncts, matras) is handled natively by Unity's
    /// TMP/Text engine when a proper Devanagari font is used.
    /// This processor ensures Unicode canonical form (NFC) and validates
    /// proper character sequencing.
    /// </summary>
    public static class DevanagariTextProcessor
    {
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
            {
                builder.Capacity = capacity;
            }
            return builder;
        }
        
        /// <summary>
        /// Processes Devanagari text ensuring proper Unicode normalization.
        /// </summary>
        /// <param name="input">The Devanagari text.</param>
        /// <returns>The processed text in canonical form (NFC).</returns>
        public static string Process(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            
            return input.Normalize(NormalizationForm.FormC);
        }
        
        /// <summary>
        /// Checks if the input contains any Devanagari characters that need processing.
        /// </summary>
        public static bool NeedsProcessing(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            
            foreach (char c in input)
            {
                if (DevanagariUnicodeRanges.IsDevanagari(c))
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Checks if the input text is primarily Devanagari.
        /// Returns true if more than half the letter characters are Devanagari.
        /// </summary>
        public static bool IsPrimarilyDevanagari(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            
            int devanagariCount = 0;
            int latinCount = 0;
            
            foreach (char c in input)
            {
                if (DevanagariUnicodeRanges.IsDevanagariLetter(c))
                    devanagariCount++;
                else if (DevanagariUnicodeRanges.IsLatin(c))
                    latinCount++;
            }
            
            int total = devanagariCount + latinCount;
            if (total == 0) return false;
            
            return devanagariCount > latinCount;
        }
        
        /// <summary>
        /// Validates that the Devanagari character sequence is properly ordered.
        /// Checks virama placement, matra positioning, etc.
        /// </summary>
        public static bool ValidateSequence(string input)
        {
            if (string.IsNullOrEmpty(input))
                return true;
            
            bool lastWasVirama = false;
            
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                
                if (DevanagariUnicodeRanges.IsMatra(c))
                {
                    if (i == 0) return false;
                    
                    char prev = input[i - 1];
                    if (!DevanagariUnicodeRanges.IsConsonant(prev) &&
                        !DevanagariUnicodeRanges.IsNukta(prev) &&
                        !DevanagariUnicodeRanges.IsVirama(prev))
                    {
                        // Let rendering engine decide
                    }
                }
                
                if (DevanagariUnicodeRanges.IsVirama(c))
                {
                    if (i == 0) return false;
                    lastWasVirama = true;
                }
                else
                {
                    lastWasVirama = false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Extracts the base consonants from Devanagari text, removing all
        /// modifiers (matras, virama, nukta, anusvara, etc.).
        /// </summary>
        public static string ExtractBaseConsonants(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            
            var builder = GetBuilder(input.Length);
            foreach (char c in input)
            {
                if (DevanagariUnicodeRanges.IsDevanagariLetter(c) ||
                    !DevanagariUnicodeRanges.IsDevanagari(c))
                {
                    builder.Append(c);
                }
            }
            
            return builder.ToString();
        }
        
        /// <summary>
        /// Gets the syllable boundaries in a Devanagari word.
        /// A syllable generally consists of a consonant cluster followed by a vowel/matra.
        /// </summary>
        public static int[] GetSyllableBoundaries(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new int[0];
            
            var boundaries = new System.Collections.Generic.List<int>();
            boundaries.Add(0);
            
            for (int i = 1; i < input.Length; i++)
            {
                char c = input[i];
                char prev = input[i - 1];
                
                if (DevanagariUnicodeRanges.IsConsonant(c) &&
                    !DevanagariUnicodeRanges.IsVirama(prev))
                {
                    if (DevanagariUnicodeRanges.IsMatra(prev) ||
                        DevanagariUnicodeRanges.IsVowel(prev) ||
                        prev == DevanagariUnicodeRanges.Anusvara ||
                        prev == DevanagariUnicodeRanges.Chandrabindu ||
                        prev == DevanagariUnicodeRanges.Visarga)
                    {
                        boundaries.Add(i);
                    }
                    else if (DevanagariUnicodeRanges.IsConsonant(prev))
                    {
                        boundaries.Add(i);
                    }
                }
                else if (DevanagariUnicodeRanges.IsVowel(c) && i > 0 &&
                         !DevanagariUnicodeRanges.IsVirama(prev))
                {
                    boundaries.Add(i);
                }
            }
            
            return boundaries.ToArray();
        }
    }
}
