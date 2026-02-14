// Devanagari Text Engine
// Copyright (c) 2026 Akshay Kadam. MIT License.

using System.Text;
using System.Collections.Generic;

namespace DevanagariText.TextProcessing
{
    /// <summary>
    /// Converts Unicode Devanagari text to Kruti Dev 010 encoding.
    /// Non-Devanagari text (English, numbers, ₹) is wrapped in TMP font tags
    /// so it renders with a standard font instead of KrutiDev's remapped glyphs.
    /// </summary>
    public static class KrutidevConverter
    {
        // ── Mapping table ─────────────────────────────────────────────

        private static readonly Dictionary<string, string> _unicodeToKruti = new Dictionary<string, string>
        {
            // ── Ligatures (2 Unicode chars) ──
            { "क्ष", "{k" }, { "त्र", "=" }, { "ज्ञ", "K" }, { "श्र", "J" },
            { "द्व", "}" }, { "द्य", "| " }, { "क्र", "Ø" }, { "ट्र", "Vª" },
            { "ड्र", "Mª" }, { "त्त", "Ùk" }, { "द्ध", "˜" }, { "द्द", "»" },
            { "हृ", "â" }, { "ह्म", "ã" }, { "ह्य", "á" }, { "ह्र", "º" },

            // ── Halant forms (consonant + halant) ──
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
            { "ळ", "G" }, { "ऱ", "j" },

            // ── Matras & marks ──
            { "ो", "ks" }, { "ौ", "kS" },
            { "ा", "k" }, { "ि", "f" }, { "ी", "h" }, { "ु", "q" }, { "ू", "w" },
            { "ृ", "`" }, { "े", "s" }, { "ै", "S" },
            { "ं", "a" }, { "ः", "%" }, { "ँ", "¡" }, { "़", "+" }, { "्", "~" },

            // ── Devanagari punctuation ──
            { "।", "A" },
        };

        // ── Helpers ───────────────────────────────────────────────────

        private static bool IsDevanagariOrMark(char c)
        {
            return (c >= 0x0900 && c <= 0x097F)
                || (c >= 0xA8E0 && c <= 0xA8FF);
        }

        private static bool IsDevanagariConsonant(char c)
        {
            return (c >= 0x0915 && c <= 0x0939) || (c >= 0x0958 && c <= 0x095F);
        }

        // ── Public API ────────────────────────────────────────────────

        /// <summary>
        /// Converts Unicode Devanagari text to Kruti Dev 010 encoding.
        /// Non-Devanagari characters are wrapped in TMP font tags using the
        /// specified fallback font so they render correctly alongside KrutiDev.
        /// </summary>
        /// <param name="unicodeText">Input text (may contain mixed scripts).</param>
        /// <param name="fallbackFontName">
        /// TMP font asset name for non-Devanagari text (e.g. "LiberationSans SDF").
        /// If null or empty, non-Devanagari text passes through without font tags.
        /// </param>
        public static string Convert(string unicodeText, string fallbackFontName = null)
        {
            if (string.IsNullOrEmpty(unicodeText)) return unicodeText;

            bool useFontTags = !string.IsNullOrEmpty(fallbackFontName);
            var result = new StringBuilder(unicodeText.Length);
            int len = unicodeText.Length;
            int i = 0;

            while (i < len)
            {
                // ── Non-Devanagari run ──
                if (!IsDevanagariOrMark(unicodeText[i]))
                {
                    int runStart = i;
                    while (i < len && !IsDevanagariOrMark(unicodeText[i]))
                        i++;

                    if (useFontTags)
                    {
                        result.Append("<font=\"");
                        result.Append(fallbackFontName);
                        result.Append("\">");
                        result.Append(unicodeText, runStart, i - runStart);
                        result.Append("</font>");
                    }
                    else
                    {
                        result.Append(unicodeText, runStart, i - runStart);
                    }
                    continue;
                }

                // ── Devanagari run ──
                int segStart = i;
                while (i < len && IsDevanagariOrMark(unicodeText[i]))
                    i++;

                ConvertSegment(unicodeText, segStart, i - segStart, result);
            }

            return result.ToString();
        }

        // ── Internal ──────────────────────────────────────────────────

        private static void ConvertSegment(string source, int offset, int length, StringBuilder output)
        {
            string processed = PreProcessMatras(source, offset, length);
            int pLen = processed.Length;
            int i = 0;

            while (i < pLen)
            {
                bool matched = false;

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
                        output.Append(key1);
                    i++;
                }
            }
        }

        private static string PreProcessMatras(string text, int offset, int length)
        {
            const char matraI = '\u093F';
            const char halant = '\u094D';

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
