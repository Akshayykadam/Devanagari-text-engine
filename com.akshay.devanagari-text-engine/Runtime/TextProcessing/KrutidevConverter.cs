// Devanagari Text Engine
// Copyright (c) 2026 Akshay Kadam. MIT License.

using System.Text;
using System.Collections.Generic;

namespace DevanagariText.TextProcessing
{
    /// <summary>
    /// Converts Unicode Devanagari text to Kruti Dev 010 (legacy font) encoding.
    /// This is useful for older systems or specific styling requirements where
    /// Unicode Rendering might not be perfect.
    /// </summary>
    public static class KrutidevConverter
    {
        private static readonly Dictionary<string, string> _unicodeToKruti = new Dictionary<string, string>
        {
            // Special combinations first
            { "‘", "^" }, { "’", "*" }, { "“", "ß" }, { "”", "Þ" },
            { "(", "¼" }, { ")", "½" }, { "{", "¿" }, { "}", "À" },
            { "=", "¾" }, { "।", "A" }, { "?", "\\" }, { "-", "&" },
            { "µ", "µ" }, { "॰", "॰" }, { ",", "]" }, { ".", "-" },
            
            // Vowels
            { "अ", "v" }, { "आ", "vk" }, { "इ", "b" }, { "ई", "bZ" },
            { "उ", "m" }, { "ऊ", "Å" }, { "ऋ", "Í" }, { "ॠ", "Î" },
            { "ए", "," }, { "ऐ", ",s" }, { "ओ", "vks" }, { "औ", "vkS" },
            { "अं", "v" }, { "अः", "v%" },
            
            // Consonants
            { "क", "d" }, { "ख", "[k" }, { "ग", "x" }, { "घ", "?k" }, { "ङ", "Ä" },
            { "च", "p" }, { "छ", "N" }, { "ज", "t" }, { "झ", "Ö" }, { "ञ", "¥" },
            { "ट", "V" }, { "ठ", "B" }, { "ड", "M" }, { "ढ", "<" }, { "ण", ".k" },
            { "त", "r" }, { "थ", "Fk" }, { "द", "n" }, { "ध", "/k" }, { "न", "u" },
            { "प", "i" }, { "फ", "Q" }, { "ब", "c" }, { "भ", "Hk" }, { "म", "e" },
            { "य", "k" }, { "र", "j" }, { "ल", "y" }, { "व", "o" },
            { "श", "'k" }, { "ष", "\"k" }, { "स", "l" }, { "ह", "g" },
            // Marathi-specific consonants
            { "ळ", "G" }, { "ऱ", "j" },
            
            // Matras
            { "ा", "k" }, { "ि", "f" }, { "ी", "h" }, { "ु", "q" }, { "ू", "w" },
            { "ृ", "`" }, { "े", "s" }, { "ै", "S" }, { "ो", "ks" }, { "ौ", "kS" },
            { "ं", "a" }, { "ः", "%" }, { "ँ", "¡" }, { "़", "+" }, { "्", "~" },
            
            // Conjuncts/Halant forms
            { "क्", "D" }, { "ख्", "K" }, { "ग्", "X" }, { "घ्", "?" }, 
            { "च्", "P" }, { "छ्", "N~" }, { "ज्", "T" }, { "झ्", "Ö~" }, 
            { "ञ्", "¥~" }, { "ट्", "V~" }, { "ठ्", "B~" }, { "ड्", "M~" },
            { "ढ्", "<~" }, { "ण्", "." }, { "त्", "R" }, { "थ्", "F" },
            { "द्", "n~" }, { "ध्", "/" }, { "न्", "U" }, { "प्", "I" },
            { "फ्", "Q" }, { "ब्", "C" }, { "भ्", "H" }, { "म्", "E" },
            { "य्", "¸" }, // partial ya
            { "ल्", "Y" }, { "व्", "O" }, { "श्", "‘" }, { "ष्", "Ô" },
            { "स्", "L" }, { "ह्", "g~" },
            // Marathi-specific halant forms
            { "ळ्", "G~" },
            
            // Specific ligatures
            { "क्ष", "{k" }, { "त्र", "=" }, { "ज्ञ", "K" }, { "श्र", "J" },
            { "द्व", "}" }, { "द्य", "| " }, { "क्र", "Ø" }, { "ट्र", "Vª" },
            { "ड्र", "Mª" }, { "त्त", "Ùk" }, { "द्ध", "˜" }, { "द्द", "»" },
            { "हृ", "â" }, { "ह्म", "ã" }, { "ह्य", "á" }, { "ह्र", "º" }
        };

        public static string Convert(string unicodeText)
        {
            if (string.IsNullOrEmpty(unicodeText)) return unicodeText;

            // 1. Pre-processing: Move 'i' matra (chhoti ee) before the consonant
            string processed = PreProcessMatras(unicodeText);
            
            // 2. Tokenize and replace
            StringBuilder result = new StringBuilder();
            int i = 0;
            while (i < processed.Length)
            {
                // Check 2-char sequences first (for ligatures/special chars)
                bool matched = false;
                if (i < processed.Length - 1)
                {
                    string substr2 = processed.Substring(i, 2);
                    if (_unicodeToKruti.ContainsKey(substr2))
                    {
                        result.Append(_unicodeToKruti[substr2]);
                        i += 2;
                        matched = true;
                    }
                }
                
                if (!matched)
                {
                    string charStr = processed[i].ToString();
                    if (_unicodeToKruti.ContainsKey(charStr))
                    {
                        result.Append(_unicodeToKruti[charStr]);
                    }
                    else
                    {
                        // Pass through unknown chars (often numbers or spaces)
                        result.Append(charStr);
                    }
                    i++;
                }
            }
            
            return result.ToString();
        }

        // Logic to handle the Chhoti Ee matra (ि) which appears AFTER consonant in Unicode 
        // but needs to be rendered BEFORE it (or strictly speaking, mapped to 'f' which Kruti places correctly)
        private static string PreProcessMatras(string text)
        {
            const char matraI = 'ि'; // 093F
            const char halant = '्'; // 094D
            
            // We need to identify [ConsonantSequence] + [MatraI] and swap them.
            // ConsonantSequence can be: Consonant, or Consonant + Halant + Consonant...
            
            StringBuilder sb = new StringBuilder(text);
            
            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] == matraI)
                {
                    // Trace back to find the start of the syllable
                    int start = i - 1;
                    while (start >= 0)
                    {
                        // If we hit a halant, go back further to include the previous consonant
                        if (sb[start] == halant)
                        {
                            start--;
                            if (start < 0) break; // Invalid halant at start
                        }
                        else if (IsDevanagariConsonant(sb[start]))
                        {
                            // Found a consonant. 
                            // Check if there is a halant before it? 
                            if (start > 0 && sb[start-1] == halant)
                            {
                                start--; // Move to halant
                                // Loop continues
                            }
                            else
                            {
                                // This is the start consonant
                                break;
                            }
                        }
                        else
                        {
                            // Not a consonant or halant (maybe another matra or space)
                            // Stop here, the syllable started after this char
                            start++; 
                            break;
                        }
                    }
                    
                    if (start < 0) start = 0;
                    if (start < i)
                    {
                        // Swap: Move Matra I from 'i' to 'start'
                        sb.Remove(i, 1);
                        sb.Insert(start, matraI);
                    }
                }
            }
            
            return sb.ToString();
        }

        private static bool IsDevanagariConsonant(char c)
        {
            return (c >= 0x0915 && c <= 0x0939) || (c >= 0x0958 && c <= 0x095F);
        }
    }
}
