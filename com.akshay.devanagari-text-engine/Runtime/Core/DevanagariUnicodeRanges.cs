// Devanagari Text Engine
// Copyright (c) 2026 Akshay Kadam. MIT License.

namespace DevanagariText.Core
{
    /// <summary>
    /// Unicode ranges for Devanagari characters.
    /// Superset covering both Hindi and Marathi character sets.
    /// </summary>
    public static class DevanagariUnicodeRanges
    {
        // Devanagari block: U+0900 to U+097F
        public const char DevanagariStart = '\u0900';
        public const char DevanagariEnd = '\u097F';
        
        // Devanagari Extended: U+A8E0 to U+A8FF
        public const char DevanagariExtendedStart = '\uA8E0';
        public const char DevanagariExtendedEnd = '\uA8FF';
        
        // Vedic Extensions: U+1CD0 to U+1CFF
        public const char VedicExtensionsStart = '\u1CD0';
        public const char VedicExtensionsEnd = '\u1CFF';
        
        // Vowels range
        public const char VowelStart = '\u0904'; // Short A
        public const char VowelEnd = '\u0914';   // AU
        
        // Consonants range
        public const char ConsonantStart = '\u0915'; // Ka
        public const char ConsonantEnd = '\u0939';   // Ha
        
        // Dependent vowel signs (Matras) range
        public const char MatraStart = '\u093E'; // Aa matra
        public const char MatraEnd = '\u094C';   // AU matra
        
        // Virama (Halant) - used for conjuncts
        public const char Virama = '\u094D';
        
        // Nukta - dot below for borrowed sounds
        public const char Nukta = '\u093C';
        
        // Anusvara and Chandrabindu (nasal marks)
        public const char Chandrabindu = '\u0901';
        public const char Anusvara = '\u0902';
        public const char Visarga = '\u0903';
        
        // Devanagari digits
        public const char DigitZero = '\u0966';
        public const char DigitNine = '\u096F';
        
        // Avagraha
        public const char Avagraha = '\u093D';
        
        // Om
        public const char Om = '\u0950';
        
        // Devanagari-specific punctuation
        public const char Danda = '\u0964';
        public const char DoubleDanda = '\u0965';
        
        // === Marathi-specific characters ===
        
        /// <summary>
        /// ळ (LLA) — Used extensively in Marathi but rarely in Hindi.
        /// U+0933
        /// </summary>
        public const char Lla = '\u0933';
        
        /// <summary>
        /// ऱ (Eyelash Ra) — Used in Marathi for the "r" sound in certain positions.
        /// U+0931
        /// </summary>
        public const char EyelashRa = '\u0931';
        
        /// <summary>
        /// Checks if a character is in the Devanagari Unicode block.
        /// </summary>
        public static bool IsDevanagari(char c)
        {
            return (c >= DevanagariStart && c <= DevanagariEnd) ||
                   (c >= DevanagariExtendedStart && c <= DevanagariExtendedEnd) ||
                   (c >= VedicExtensionsStart && c <= VedicExtensionsEnd);
        }
        
        /// <summary>
        /// Checks if a character is a Devanagari letter (vowel or consonant).
        /// </summary>
        public static bool IsDevanagariLetter(char c)
        {
            return (c >= VowelStart && c <= VowelEnd) ||
                   (c >= ConsonantStart && c <= ConsonantEnd);
        }
        
        /// <summary>
        /// Checks if a character is a Devanagari consonant.
        /// </summary>
        public static bool IsConsonant(char c)
        {
            return c >= ConsonantStart && c <= ConsonantEnd;
        }
        
        /// <summary>
        /// Checks if a character is a Devanagari vowel (independent form).
        /// </summary>
        public static bool IsVowel(char c)
        {
            return c >= VowelStart && c <= VowelEnd;
        }
        
        /// <summary>
        /// Checks if a character is a Devanagari dependent vowel sign (matra).
        /// </summary>
        public static bool IsMatra(char c)
        {
            return (c >= MatraStart && c <= MatraEnd) ||
                   c == '\u0962' || c == '\u0963'; // Vocalic L/LL matras
        }
        
        /// <summary>
        /// Checks if a character is the Virama (Halant).
        /// Used to form conjuncts between consonants.
        /// </summary>
        public static bool IsVirama(char c)
        {
            return c == Virama;
        }
        
        /// <summary>
        /// Checks if a character is the Nukta (dot below).
        /// Used for borrowed sounds like क़ ख़ ग़.
        /// </summary>
        public static bool IsNukta(char c)
        {
            return c == Nukta;
        }
        
        /// <summary>
        /// Checks if a character is a Devanagari diacritic mark
        /// (matra, anusvara, chandrabindu, visarga, nukta, virama).
        /// </summary>
        public static bool IsDiacriticMark(char c)
        {
            return IsMatra(c) ||
                   c == Anusvara ||
                   c == Chandrabindu ||
                   c == Visarga ||
                   c == Nukta ||
                   c == Virama ||
                   (c >= '\u0951' && c <= '\u0957'); // Stress/accent marks
        }
        
        /// <summary>
        /// Checks if a character is a Devanagari digit (०-९).
        /// </summary>
        public static bool IsDevanagariDigit(char c)
        {
            return c >= DigitZero && c <= DigitNine;
        }
        
        /// <summary>
        /// Checks if a character is a Latin letter.
        /// </summary>
        public static bool IsLatin(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        }
        
        /// <summary>
        /// Checks if a character is a digit (Devanagari or Western).
        /// </summary>
        public static bool IsDigit(char c)
        {
            if (c >= '0' && c <= '9') return true;
            if (c >= DigitZero && c <= DigitNine) return true;
            return false;
        }
        
        /// <summary>
        /// Checks if a character is the Marathi-specific ळ (LLA).
        /// </summary>
        public static bool IsLla(char c)
        {
            return c == Lla;
        }
        
        /// <summary>
        /// Checks if a character is the Marathi-specific ऱ (Eyelash Ra).
        /// </summary>
        public static bool IsEyelashRa(char c)
        {
            return c == EyelashRa;
        }
        
        /// <summary>
        /// Converts a Devanagari digit to its integer value.
        /// Returns -1 if not a Devanagari digit.
        /// </summary>
        public static int DevanagariDigitToInt(char c)
        {
            if (c >= DigitZero && c <= DigitNine)
            {
                return c - DigitZero;
            }
            return -1;
        }
    }
}
