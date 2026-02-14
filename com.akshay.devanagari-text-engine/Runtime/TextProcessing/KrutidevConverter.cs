// Devanagari Text Engine
// Copyright (c) 2026 Akshay Kadam. MIT License.

using System.Text;
using System.Collections.Generic;

namespace DevanagariText.TextProcessing
{
    /// <summary>
    /// Converts Unicode Devanagari text to Kruti Dev 010 encoding.
    /// Non-Devanagari text (English, numbers, symbols like ₹) passes through unchanged,
    /// making it safe for mixed-language strings.
    /// </summary>
    public static class KrutidevConverter
    {
        // ── Mapping table ─────────────────────────────────────────────
        // Order: longest match first — ligatures before halant forms before base chars.
        
        private static readonly Dictionary<string, string> _unicodeToKruti = new Dictionary<string, string>
        {
            // ── Specific ligatures (3-char sequences stored as 2 Unicode chars) ──
            { "क्ष", "{k" }, { "त्र", "=" }, { "ज्ञ", "K" }, { "श्र", "J" },
            { "द्व", "}" }, { "द्य", "| " }, { "क्र", "Ø" }, { "ट्र", "Vª" },
            { "ड्र", "Mª" }, { "त्त", "Ùk" }, { "द्ध", "˜" }, { "द्द", "»" },
            { "हृ", "â" }, { "ह्म", "ã" }, { "ह्य", "á" }, { "ह्र", "º" },

            // ── Conjuncts / Halant forms (consonant + halant) ──
            { "क्", "D" }, { "ख्", "K" }, { "ग्", "X" }, { "घ्", "?" }, 
            { "च्", "P" }, { "छ्", "N~" }, { "ज्", "T" }, { "झ्", "Ö~" }, 
            { "ञ्", "¥~" }, { "ट्", "V~" }, { "ठ्", "B~" }, { "ड्", "M~" },
            { "ढ्", "<~" }, { "ण्", "." }, { "त्", "R" }, { "थ्", "F" },
            { "द्", "n~" }, { "ध्", "/" }, { "न्", "U" }, { "प्", "I" },
            { "फ्", "Q" }, { "ब्", "C" }, { "भ्", "H" }, { "म्", "E" },
            { "य्", "¸" }, { "ल्", "Y" }, { "व्", "O" }, { "श्", "'" },
            { "ष्", "Ô" }, { "स्", "L" }, { "ह्", "g~" }, { "ळ्", "G~" },

            // ── Vowels (multi-char first) ──
            { "अं", "v" }, { "अः", "v%" },
            { "आ", "vk" }, { "ऐ", ",s" }, { "ओ", "vks" }, { "औ", "vkS" },
            { "अ", "v" }, { "इ", "b" }, { "ई", "bZ" },
            { "उ", "m" }, { "ऊ", "Å" }, { "ऋ", "Í" }, { "ॠ", "Î" },
            { "ए", "," },

            // ── Consonants ──
            { "क", "d" }, { "ख", "[k" }, { "ग", "x" }, { "घ", "?k" }, { "ङ", "Ä" },
            { "च", "p" }, { "छ", "N" }, { "ज", "t" }, { "झ", "Ö" }, { "ञ", "¥" },
            { "ट", "V" }, { "ठ", "B" }, { "ड", "M" }, { "ढ", "<" }, { "ण", ".k" },
            { "त", "r" }, { "थ", "Fk" }, { "द", "n" }, { "ध", "/k" }, { "न", "u" },
            { "प", "i" }, { "फ", "Q" }, { "ब", "c" }, { "भ", "Hk" }, { "म", "e" },
            { "य", "k" }, { "र", "j" }, { "ल", "y" }, { "व", "o" },
            { "श", "'k" }, { "ष", "\"k" }, { "स", "l" }, { "ह", "g" },
            // Marathi-specific
            { "ळ", "G" }, { "ऱ", "j" },

            // ── Matras & combining marks ──
            { "ो", "ks" }, { "ौ", "kS" },   // multi-char matras first
            { "ा", "k" }, { "ि", "f" }, { "ी", "h" }, { "ु", "q" }, { "ू", "w" },
            { "ृ", "`" }, { "े", "s" }, { "ै", "S" },
            { "ं", "a" }, { "ः", "%" }, { "ँ", "¡" }, { "़", "+" }, { "्", "~" },

            // ── Devanagari punctuation ──
            { "।", "A" },
        };

        // ── Pre-computed consonant range check ────────────────────────
        
        private static bool IsDevanagariOrMark(char c)
        {
            return (c >= 0x0900 && c <= 0x097F)   // Main Devanagari block
                || (c >= 0xA8E0 && c <= 0xA8FF);  // Devanagari Extended
        }

        private static bool IsDevanagariConsonant(char c)
        {
            return (c >= 0x0915 && c <= 0x0939) || (c >= 0x0958 && c <= 0x095F);
        }

        // ── Public API ────────────────────────────────────────────────

        /// <summary>
        /// Converts Unicode Devanagari text to Kruti Dev 010 encoding.
        /// Non-Devanagari characters (Latin, digits, ₹, etc.) pass through unchanged.
        /// </summary>
        public static string Convert(string unicodeText)
        {
            if (string.IsNullOrEmpty(unicodeText)) return unicodeText;

            var result = new StringBuilder(unicodeText.Length);
            int len = unicodeText.Length;
            int i = 0;

            while (i < len)
            {
                // Fast path: non-Devanagari chars pass through as-is
                if (!IsDevanagariOrMark(unicodeText[i]))
                {
                    result.Append(unicodeText[i++]);
                    continue;
                }

                // Collect contiguous Devanagari segment
                int segStart = i;
                while (i < len && IsDevanagariOrMark(unicodeText[i]))
                    i++;

                ConvertSegment(unicodeText, segStart, i - segStart, result);
            }

            return result.ToString();
        }

        // ── Internal conversion ───────────────────────────────────────

        private static void ConvertSegment(string source, int offset, int length, StringBuilder output)
        {
            // Pre-process matras into a working buffer
            string processed = PreProcessMatras(source, offset, length);

            int i = 0;
            int pLen = processed.Length;

            while (i < pLen)
            {
                bool matched = false;

                // Try 2-char key first (covers halant forms, multi-char matras, ligatures)
                if (i < pLen - 1)
                {
                    string key2 = processed.Substring(i, 2);
                    if (_unicodeToKruti.TryGetValue(key2, out string val2))
                    {
                        output.Append(val2);
                        i += 2;
                        matched = true;
                    }
                }

                if (!matched)
                {
                    string key1 = processed.Substring(i, 1);
                    if (_unicodeToKruti.TryGetValue(key1, out string val1))
                        output.Append(val1);
                    else
                        output.Append(key1); // unmapped — pass through
                    i++;
                }
            }
        }

        // Moves ि (chhoti ee, U+093F) before its consonant cluster,
        // since KrutiDev expects the visual order (left of consonant).
        private static string PreProcessMatras(string text, int offset, int length)
        {
            const char matraI = '\u093F'; // ि
            const char halant = '\u094D'; // ्

            var sb = new StringBuilder(text, offset, length, length + 4);

            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] != matraI) continue;

                int start = i - 1;
                while (start >= 0)
                {
                    if (sb[start] == halant)
                    {
                        start--;
                        if (start < 0) break;
                    }
                    else if (IsDevanagariConsonant(sb[start]))
                    {
                        if (start > 0 && sb[start - 1] == halant)
                            start--;
                        else
                            break;
                    }
                    else
                    {
                        start++;
                        break;
                    }
                }

                if (start < 0) start = 0;
                if (start < i)
                {
                    sb.Remove(i, 1);
                    sb.Insert(start, matraI);
                }
            }

            return sb.ToString();
        }
    }
}
