// Devanagari Text Engine
// Copyright (c) 2026 Akshay Kadam. MIT License.

using System;
using System.Collections.Generic;
using DevanagariText.Core;

namespace DevanagariText.TextProcessing
{
    /// <summary>
    /// Represents a text match result.
    /// </summary>
    public readonly struct TextMatch
    {
        /// <summary>The starting index of the match in the original text.</summary>
        public readonly int StartIndex;
        /// <summary>The length of the match in the original text.</summary>
        public readonly int Length;
        /// <summary>The matched text.</summary>
        public readonly string MatchedText;
        
        public TextMatch(int startIndex, int length, string matchedText)
        {
            StartIndex = startIndex;
            Length = length;
            MatchedText = matchedText;
        }
    }
    
    /// <summary>
    /// Provides Devanagari-aware text search and comparison utilities.
    /// </summary>
    public static class TextSearchUtility
    {
        /// <summary>
        /// Prepares a string for use as a search key by normalizing it.
        /// </summary>
        public static string PrepareSearchKey(string input, DevanagariLanguage language = DevanagariLanguage.Hindi)
        {
            return DevanagariNormalizer.Normalize(input, language);
        }
        
        /// <summary>
        /// Performs smart comparison of two strings, handling Devanagari variants.
        /// </summary>
        public static bool SmartCompare(string a, string b, DevanagariLanguage language = DevanagariLanguage.Hindi)
        {
            return DevanagariNormalizer.NormalizedEquals(a, b, language);
        }
        
        /// <summary>
        /// Checks if text contains the query using normalized comparison.
        /// Handles nukta variants and chandrabindu/anusvara equivalence.
        /// For Marathi, also handles Eyelash Ra equivalence.
        /// </summary>
        public static bool Contains(string text, string query, DevanagariLanguage language = DevanagariLanguage.Hindi)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(query))
                return false;
            
            string normalizedText = DevanagariNormalizer.Normalize(text, language);
            string normalizedQuery = DevanagariNormalizer.Normalize(query, language);
            
            return normalizedText.Contains(normalizedQuery);
        }
        
        /// <summary>
        /// Checks if text starts with the query using normalized comparison.
        /// </summary>
        public static bool StartsWith(string text, string query, DevanagariLanguage language = DevanagariLanguage.Hindi)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(query))
                return false;
            
            string normalizedText = DevanagariNormalizer.Normalize(text, language);
            string normalizedQuery = DevanagariNormalizer.Normalize(query, language);
            
            return normalizedText.StartsWith(normalizedQuery, StringComparison.Ordinal);
        }
        
        /// <summary>
        /// Checks if text ends with the query using normalized comparison.
        /// </summary>
        public static bool EndsWith(string text, string query, DevanagariLanguage language = DevanagariLanguage.Hindi)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(query))
                return false;
            
            string normalizedText = DevanagariNormalizer.Normalize(text, language);
            string normalizedQuery = DevanagariNormalizer.Normalize(query, language);
            
            return normalizedText.EndsWith(normalizedQuery, StringComparison.Ordinal);
        }
        
        /// <summary>
        /// Finds the first occurrence of the query in the text.
        /// </summary>
        public static bool TryFind(string text, string query, out TextMatch match, DevanagariLanguage language = DevanagariLanguage.Hindi)
        {
            match = default;
            
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(query))
                return false;
            
            string normalizedText = DevanagariNormalizer.Normalize(text, language);
            string normalizedQuery = DevanagariNormalizer.Normalize(query, language);
            
            int normalizedIndex = normalizedText.IndexOf(normalizedQuery, StringComparison.Ordinal);
            if (normalizedIndex < 0)
                return false;
            
            int originalStart = MapNormalizedIndexToOriginal(text, normalizedIndex);
            int originalEnd = MapNormalizedIndexToOriginal(text, normalizedIndex + normalizedQuery.Length);
            int originalLength = originalEnd - originalStart;
            
            if (originalStart + originalLength > text.Length)
                originalLength = text.Length - originalStart;
            
            match = new TextMatch(originalStart, originalLength, text.Substring(originalStart, originalLength));
            return true;
        }
        
        /// <summary>
        /// Finds all occurrences of the query in the text.
        /// </summary>
        public static List<TextMatch> FindAll(string text, string query, DevanagariLanguage language = DevanagariLanguage.Hindi)
        {
            var matches = new List<TextMatch>();
            
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(query))
                return matches;
            
            string normalizedText = DevanagariNormalizer.Normalize(text, language);
            string normalizedQuery = DevanagariNormalizer.Normalize(query, language);
            
            int searchStart = 0;
            while (searchStart < normalizedText.Length)
            {
                int normalizedIndex = normalizedText.IndexOf(normalizedQuery, searchStart, StringComparison.Ordinal);
                if (normalizedIndex < 0) break;
                
                int originalStart = MapNormalizedIndexToOriginal(text, normalizedIndex);
                int originalEnd = MapNormalizedIndexToOriginal(text, normalizedIndex + normalizedQuery.Length);
                int originalLength = originalEnd - originalStart;
                
                if (originalStart + originalLength > text.Length)
                    originalLength = text.Length - originalStart;
                
                matches.Add(new TextMatch(originalStart, originalLength, text.Substring(originalStart, originalLength)));
                searchStart = normalizedIndex + normalizedQuery.Length;
            }
            
            return matches;
        }
        
        /// <summary>
        /// Filters a list of items based on a search query.
        /// </summary>
        public static List<T> Filter<T>(IEnumerable<T> items, string query, Func<T, string> textSelector, DevanagariLanguage language = DevanagariLanguage.Hindi)
        {
            var results = new List<T>();
            
            if (items == null)
                return results;
            
            if (string.IsNullOrEmpty(query))
            {
                foreach (var item in items)
                    results.Add(item);
                return results;
            }
            
            string normalizedQuery = DevanagariNormalizer.Normalize(query, language);
            
            foreach (var item in items)
            {
                string text = textSelector(item);
                if (string.IsNullOrEmpty(text)) continue;
                
                string normalizedText = DevanagariNormalizer.Normalize(text, language);
                if (normalizedText.Contains(normalizedQuery))
                    results.Add(item);
            }
            
            return results;
        }
        
        /// <summary>
        /// Compares two strings for sorting, handling Devanagari normalization.
        /// </summary>
        public static int Compare(string a, string b, DevanagariLanguage language = DevanagariLanguage.Hindi)
        {
            if (a == b) return 0;
            if (a == null) return -1;
            if (b == null) return 1;
            
            string normalizedA = DevanagariNormalizer.Normalize(a, language);
            string normalizedB = DevanagariNormalizer.Normalize(b, language);
            
            return string.Compare(normalizedA, normalizedB, StringComparison.Ordinal);
        }
        
        /// <summary>
        /// Maps an index in normalized text to the corresponding index in original text.
        /// </summary>
        private static int MapNormalizedIndexToOriginal(string original, int normalizedIndex)
        {
            if (normalizedIndex <= 0)
                return 0;
            
            int normalizedPos = 0;
            int originalPos = 0;
            
            while (originalPos < original.Length && normalizedPos < normalizedIndex)
            {
                char c = original[originalPos];
                if (c != DevanagariUnicodeRanges.Nukta)
                    normalizedPos++;
                originalPos++;
            }
            
            return originalPos;
        }
    }
}
