// Devanagari Text Engine
// Copyright (c) 2026 Akshay Kadam. MIT License.

using System;
using System.Text;
using DevanagariText.Core;

namespace DevanagariText.TextProcessing
{
    /// <summary>
    /// Provides conversion between Devanagari numerals (०-९) and Western digits (0-9).
    /// </summary>
    public static class NumeralConverter
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
                builder.Capacity = capacity;
            return builder;
        }
        
        /// <summary>
        /// Converts Western digits (0-9) to Devanagari numerals (०-९).
        /// Non-digit characters are passed through unchanged.
        /// </summary>
        /// <example>
        /// NumeralConverter.ToDevanagariNumerals("123") returns "१२३"
        /// NumeralConverter.ToDevanagariNumerals("Price: 99") returns "Price: ९९"
        /// </example>
        public static string ToDevanagariNumerals(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            
            bool hasWesternDigits = false;
            foreach (char c in input)
            {
                if (c >= '0' && c <= '9')
                {
                    hasWesternDigits = true;
                    break;
                }
            }
            
            if (!hasWesternDigits)
                return input;
            
            var builder = GetBuilder(input.Length);
            foreach (char c in input)
            {
                if (c >= '0' && c <= '9')
                    builder.Append((char)(DevanagariUnicodeRanges.DigitZero + (c - '0')));
                else
                    builder.Append(c);
            }
            
            return builder.ToString();
        }
        
        /// <summary>
        /// Converts Devanagari numerals (०-९) to Western digits (0-9).
        /// Non-digit characters are passed through unchanged.
        /// </summary>
        /// <example>
        /// NumeralConverter.ToWesternNumerals("१२३") returns "123"
        /// NumeralConverter.ToWesternNumerals("कीमत: ९९") returns "कीमत: 99"
        /// </example>
        public static string ToWesternNumerals(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            
            bool hasDevanagariDigits = false;
            foreach (char c in input)
            {
                if (DevanagariUnicodeRanges.IsDevanagariDigit(c))
                {
                    hasDevanagariDigits = true;
                    break;
                }
            }
            
            if (!hasDevanagariDigits)
                return input;
            
            var builder = GetBuilder(input.Length);
            foreach (char c in input)
            {
                if (DevanagariUnicodeRanges.IsDevanagariDigit(c))
                    builder.Append((char)('0' + (c - DevanagariUnicodeRanges.DigitZero)));
                else
                    builder.Append(c);
            }
            
            return builder.ToString();
        }
        
        /// <summary>
        /// Checks if the text contains any Devanagari digits.
        /// </summary>
        public static bool HasDevanagariDigits(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            foreach (char c in input)
                if (DevanagariUnicodeRanges.IsDevanagariDigit(c)) return true;
            return false;
        }
        
        /// <summary>
        /// Checks if the text contains any Western digits.
        /// </summary>
        public static bool HasWesternDigits(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            foreach (char c in input)
                if (c >= '0' && c <= '9') return true;
            return false;
        }
        
        /// <summary>
        /// Formats a number using Devanagari numeral system.
        /// </summary>
        public static string FormatNumber(int number)
        {
            return ToDevanagariNumerals(number.ToString());
        }
        
        /// <summary>
        /// Formats a number using Devanagari numeral system with decimal places.
        /// </summary>
        public static string FormatNumber(float number, int decimalPlaces = 2)
        {
            string format = "F" + decimalPlaces;
            return ToDevanagariNumerals(number.ToString(format));
        }
    }
}
