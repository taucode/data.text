using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TauCode.Data.Text.Exceptions;
using TauCode.Extensions;

namespace TauCode.Data.Text
{
    public delegate bool TerminatingDelegate(ReadOnlySpan<char> input, int position);

    public static class TextDataExtractor
    {
        private static readonly int Int32MaxLength = int.MinValue.ToString().Length;
        private static readonly int Int64MaxLength = long.MinValue.ToString().Length;
        private static readonly int DoubleMaxLength = "-1.7976931348623157E+308".Length;
        private static readonly int DecimalMaxLength = "79228162514264337593543950335".Length;
        private static readonly int TimeSpanMaxLength = "-10675199.02:48:05.4775808".Length;
        private static readonly int DateTimeOffsetMaxLength = "9999-12-31T23:59:59.9999999+00:00".Length;
        private const int UriMaxLength = 2000;
        private const int FilePathMaxLength = 256;
        private const int KeyMaxLength = 200;
        private const int TermMaxLength = 200;

        private static readonly HashSet<char> ProhibitedFilePathChars = new HashSet<char>
        {
            '<',
            '>',
            ':',
            '"',
            '/',
            '\\',
            '|',
            '?',
            '*',
        };

        private static readonly string[] EscapeReplacementStrings =
        {
            "\"\"",
            "''",
            "\\\\",
            "n\n",
            "r\r",
            "t\t",
        };

        private static readonly Dictionary<char, char> EscapeReplacements;

        private static readonly HashSet<string> BooleanItems = new HashSet<string>(new[] { "false", "true" });

        private static readonly HashSet<char> DoubleChars;
        private static readonly HashSet<char> DecimalChars;
        private static readonly HashSet<char> TimeSpanChars;
        private static readonly HashSet<char> DateTimeOffsetChars;

        static TextDataExtractor()
        {
            DoubleChars = new HashSet<char>("+-eE0123456789.");
            DecimalChars = new HashSet<char>("+-0123456789.");
            TimeSpanChars = new HashSet<char>("+-0123456789.:");
            DateTimeOffsetChars = new HashSet<char>("+-0123456789.:TZ");

            EscapeReplacements = EscapeReplacementStrings
                .ToDictionary(
                    x => x.First(),
                    x => x.Skip(1).Single());

        }

        public static int TryExtractInt32(
            ReadOnlySpan<char> input,
            out int v,
            TerminatingDelegate terminatingPredicate = null)
        {
            terminatingPredicate ??= Helper.DefaultTerminatingPredicate;

            if (input.Length == 0)
            {
                v = default;
                return 0;
            }

            var pos = 0;

            while (true)
            {
                if (pos > Int32MaxLength)
                {
                    v = default;
                    return 0;
                }

                if (pos == input.Length)
                {
                    break;
                }

                var c = input[pos];
                if (c == '-' || c == '+')
                {
                    if (pos == 0)
                    {
                        // ok
                    }
                    else if (terminatingPredicate(input, pos))
                    {
                        break;
                    }
                    else
                    {
                        v = default;
                        return 0;
                    }
                }
                else if (c.IsDecimalDigit())
                {
                    // ok
                }
                else if (terminatingPredicate(input, pos))
                {
                    break;
                }
                else
                {
                    v = default;
                    return 0;
                }

                pos++;
            }

            if (pos == 0)
            {
                v = default;
                return 0;
            }

            input = input[..pos];
            var parsed = int.TryParse(input, out v);
            return parsed ? pos : 0;
        }

        public static int TryExtractInt64(
            ReadOnlySpan<char> input,
            out long v,
            TerminatingDelegate terminatingPredicate = null)
        {
            terminatingPredicate ??= Helper.DefaultTerminatingPredicate;

            if (input.Length == 0)
            {
                v = default;
                return 0;
            }

            var pos = 0;

            while (true)
            {
                if (pos > Int64MaxLength)
                {
                    v = default;
                    return 0;
                }

                if (pos == input.Length)
                {
                    break;
                }

                var c = input[pos];
                if (c == '-' || c == '+')
                {
                    if (pos == 0)
                    {
                        // ok
                    }
                    else if (terminatingPredicate(input, pos))
                    {
                        break;
                    }
                    else
                    {
                        v = default;
                        return 0;
                    }
                }
                else if (c.IsDecimalDigit())
                {
                    // ok
                }
                else if (terminatingPredicate(input, pos))
                {
                    break;
                }
                else
                {
                    v = default;
                    return 0;
                }

                pos++;
            }

            if (pos == 0)
            {
                v = default;
                return 0;
            }

            input = input[..pos];
            var parsed = long.TryParse(input, out v);
            return parsed ? pos : 0;
        }

        public static int TryExtractDouble(
            ReadOnlySpan<char> input,
            out double v,
            TerminatingDelegate terminatingPredicate = null)
        {
            terminatingPredicate ??= Helper.DefaultTerminatingPredicate;

            if (input.Length == 0)
            {
                v = default;
                return 0;
            }

            var pos = 0;

            while (true)
            {
                if (pos > DoubleMaxLength)
                {
                    v = default;
                    return 0;
                }

                if (pos == input.Length)
                {
                    break;
                }

                var c = input[pos];
                if (DoubleChars.Contains(c))
                {
                    // ok
                }
                else if (terminatingPredicate(input, pos))
                {
                    break;
                }
                else
                {
                    v = default;
                    return 0;
                }

                pos++;
            }

            if (pos == 0)
            {
                v = default;
                return 0;
            }

            input = input[..pos];
            var parsed = double.TryParse(input, out v);
            return parsed ? pos : 0;
        }

        public static int TryExtractDecimal(
            ReadOnlySpan<char> input,
            out decimal v,
            TerminatingDelegate terminatingPredicate = null)
        {
            terminatingPredicate ??= Helper.DefaultTerminatingPredicate;

            if (input.Length == 0)
            {
                v = default;
                return 0;
            }

            var pos = 0;

            while (true)
            {
                if (pos > DecimalMaxLength)
                {
                    v = default;
                    return 0;
                }

                if (pos == input.Length)
                {
                    break;
                }

                var c = input[pos];
                if (DecimalChars.Contains(c))
                {
                    // ok
                }
                else if (terminatingPredicate(input, pos))
                {
                    break;
                }
                else
                {
                    v = default;
                    return 0;
                }

                pos++;
            }

            if (pos == 0)
            {
                v = default;
                return 0;
            }

            input = input[..pos];
            var parsed = decimal.TryParse(input, out v);
            return parsed ? pos : 0;
        }

        public static int TryExtractItem(
            ReadOnlySpan<char> input,
            HashSet<string> items,
            out string v,
            TerminatingDelegate terminatingPredicate = null)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (input.Length == 0)
            {
                v = default;
                return 0;
            }

            terminatingPredicate ??= Helper.DefaultTerminatingPredicate;

            foreach (var item in items)
            {
                if (input.StartsWith(item, StringComparison.Ordinal))
                {
                    if (input.Length == item.Length)
                    {
                        v = item;
                        return item.Length;
                    }
                    else
                    {
                        if (terminatingPredicate(input, item.Length))
                        {
                            v = item;
                            return item.Length;
                        }

                        v = default;
                        return 0;
                    }
                }
            }

            v = default;
            return 0;
        }

        public static int TryExtractBoolean(
            ReadOnlySpan<char> input,
            out bool v,
            TerminatingDelegate terminatingPredicate = null)
        {
            var result = TryExtractItem(input, BooleanItems, out var item, terminatingPredicate);
            if (result == 0)
            {
                v = default;
                return result;
            }

            v = item == "true";
            return item.Length;
        }

        public static int TryExtractTimeSpan(
            ReadOnlySpan<char> input,
            out TimeSpan v,
            TerminatingDelegate terminatingPredicate = null)
        {
            if (input.Length == 0)
            {
                v = default;
                return 0;
            }

            terminatingPredicate ??= Helper.DefaultTerminatingPredicate;

            var pos = 0;
            while (true)
            {
                if (pos == input.Length)
                {
                    break;
                }

                var c = input[pos];
                if (TimeSpanChars.Contains(c))
                {
                    // ok
                }
                else if (terminatingPredicate(input, pos))
                {
                    break;
                }
                else
                {
                    v = default;
                    return 0;
                }

                if (pos == TimeSpanMaxLength)
                {
                    v = default;
                    return 0;
                }

                pos++;
            }

            if (pos == 0)
            {
                v = default;
                return 0;
            }

            var parseInput = input[..pos];
            var parsed = TimeSpan.TryParse(parseInput, CultureInfo.InvariantCulture, out v);
            return parsed ? pos : 0;
        }

        public static int TryExtractDateTimeOffset(
            ReadOnlySpan<char> input,
            out DateTimeOffset v,
            TerminatingDelegate terminatingPredicate = null)
        {
            if (input.Length == 0)
            {
                v = default;
                return 0;
            }

            terminatingPredicate ??= Helper.DefaultTerminatingPredicate;

            var pos = 0;
            while (true)
            {
                if (pos == input.Length)
                {
                    break;
                }

                var c = input[pos];
                if (DateTimeOffsetChars.Contains(c))
                {
                    // ok
                }
                else if (terminatingPredicate(input, pos))
                {
                    break;
                }
                else
                {
                    v = default;
                    return 0;
                }

                if (pos == DateTimeOffsetMaxLength)
                {
                    v = default;
                    return 0;
                }

                pos++;
            }

            if (pos == 0)
            {
                v = default;
                return 0;
            }

            var parseInput = input[..pos];
            var parsed = DateTimeOffset.TryParse(parseInput, out v);
            return parsed ? pos : 0;
        }

        public static int TryExtractUri(
            ReadOnlySpan<char> input,
            out Uri v,
            TerminatingDelegate terminatingPredicate = null)
        {
            if (input.Length == 0)
            {
                v = default;
                return 0;
            }

            terminatingPredicate ??= Helper.DefaultTerminatingPredicate;

            var pos = 0;

            while (true)
            {
                if (pos == input.Length)
                {
                    break;
                }

                var c = input[pos];

                if (c.IsInlineWhiteSpaceOrCaretControl())
                {
                    if (terminatingPredicate(input, pos))
                    {
                        break;
                    }
                    else
                    {
                        v = default;
                        return 0;
                    }
                }

                if (pos == UriMaxLength)
                {
                    v = default;
                    return 0;
                }

                pos++;
            }

            if (pos == 0)
            {
                v = default;
                return 0;
            }

            try
            {
                var uriString = input[..pos].ToString();
                v = new Uri(uriString);
                return pos;
            }
            catch
            {
                v = default;
                return 0;
            }
        }

        public static int TryExtractFilePath(
            ReadOnlySpan<char> input,
            out string v,
            TerminatingDelegate terminatingPredicate = null)
        {
            if (input.Length == 0)
            {
                v = default;
                return 0;
            }

            terminatingPredicate ??= Helper.DefaultTerminatingPredicate;

            var pos = 0;

            while (true)
            {
                if (pos == input.Length)
                {
                    break;
                }

                var c = input[pos];

                if (c == '/' || c == '\\')
                {
                    // go on...
                }
                else if (c == ':')
                {
                    if (pos == 1 && input[0].IsLatinLetterInternal())
                    {
                        // something like 'c:', ok
                    }
                    else
                    {
                        v = default;
                        return 0;
                    }
                }
                else if (c.IsInlineWhiteSpaceOrCaretControl() || ProhibitedFilePathChars.Contains(c))
                {
                    if (terminatingPredicate(input, pos))
                    {
                        break;
                    }
                    else
                    {
                        v = default;
                        return 0;
                    }
                }

                if (pos == FilePathMaxLength)
                {
                    v = default;
                    return 0;
                }

                pos++;
            }

            if (pos == 0)
            {
                v = default;
                return 0;
            }

            var filePathSpan = input[..pos];
            if (IsValidFilePath(filePathSpan))
            {
                v = filePathSpan.ToString();
                return pos;
            }

            v = default;
            return 0;
        }

        public static int TryExtractJsonString(
            ReadOnlySpan<char> input,
            out string v,
            out TextDataExtractionException exception,
            bool returnException = false,
            TerminatingDelegate terminatingPredicate = null)
        {
            if (input.Length == 0)
            {
                v = default;
                exception = default;

                if (returnException)
                {
                    exception = Helper.CreateException(ExtractionErrorTag.EmptyInput, null);
                }

                return 0;
            }

            terminatingPredicate ??= Helper.DefaultTerminatingPredicate;

            var pos = 0;

            var c = input[0];

            char delimiter;

            if (c == '"' || c == '\'')
            {
                delimiter = c;
            }
            else
            {
                v = default;
                exception = default;

                if (returnException)
                {
                    exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, pos);
                }

                return 0;
            }

            var oppositeDelimiter = '\'';
            if (delimiter == '\'')
            {
                oppositeDelimiter = '"';
            }

            pos++; // skip opening delimiter

            var sb = new StringBuilder();

            while (true)
            {
                if (pos == input.Length)
                {
                    // unclosed string
                    exception = default;

                    if (returnException)
                    {
                        exception = Helper.CreateException(ExtractionErrorTag.UnclosedString, pos);
                    }

                    v = default;
                    return 0;
                }

                c = input[pos];

                if (c.IsCaretControl())
                {
                    exception = default;

                    if (returnException)
                    {
                        exception = Helper.CreateException(ExtractionErrorTag.NewLineInString, pos);
                    }

                    v = default;
                    return 0;
                }

                if (c == delimiter)
                {
                    if (pos == input.Length - 1)
                    {
                        pos++;
                        break;
                    }
                    else
                    {
                        if (terminatingPredicate(input, pos + 1))
                        {
                            pos++;
                            break;
                        }
                        else
                        {
                            exception = default;

                            if (returnException)
                            {
                                exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, pos + 1);
                            }

                            v = default;
                            return 0;
                        }
                    }
                }
                else if (c == '\\')
                {
                    #region escaping

                    if (pos + 1 == input.Length)
                    {
                        exception = default;
                        if (returnException)
                        {
                            exception = Helper.CreateException(ExtractionErrorTag.UnclosedString, pos + 1);
                        }

                        v = default;
                        return 0;
                    }

                    var nextChar = input[pos + 1];
                    if (nextChar == 'u')
                    {
                        var remaining = input.Length - (pos + 1);
                        if (remaining < 5)
                        {
                            exception = default;

                            if (returnException)
                            {
                                exception = Helper.CreateException(ExtractionErrorTag.BadEscape, pos);
                            }

                            v = default;
                            return 0;
                        }

                        var hexNumString = input.Slice(pos + 2, 4);
                        var codeParsed = int.TryParse(
                            hexNumString,
                            NumberStyles.HexNumber,
                            CultureInfo.InvariantCulture,
                            out var code);

                        if (!codeParsed)
                        {
                            exception = default;

                            if (returnException)
                            {
                                exception = Helper.CreateException(ExtractionErrorTag.BadEscape, pos);
                            }

                            v = default;
                            return 0;
                        }

                        var unescapedChar = (char)code;
                        sb.Append(unescapedChar);

                        pos += 6;
                        continue;
                    }
                    else
                    {
                        if (nextChar == oppositeDelimiter)
                        {
                            // opposite delimiter doesn't need to be escaped, e.g. strings 'escaped double quote \" inside' and "escaped quote \' inside" are wrong. todo: am i right?

                            v = default;
                            exception = default;
                            if (returnException)
                            {
                                exception = Helper.CreateException(ExtractionErrorTag.BadEscape, pos);
                            }

                            return 0;
                        }

                        var replacement = GetEscapeReplacement(nextChar);
                        if (replacement.HasValue)
                        {
                            sb.Append(replacement);
                            pos += 2;
                            continue;
                        }
                        else
                        {
                            v = default;
                            exception = default;

                            if (returnException)
                            {
                                exception = Helper.CreateException(ExtractionErrorTag.BadEscape, pos);
                            }

                            return 0;
                        }
                    }


                    #endregion
                }
                else
                {
                    // go on
                }

                sb.Append(c);
                pos++;
            }


            exception = default;
            v = sb.ToString();
            return pos;
        }

        public static int TryExtractKey(
            ReadOnlySpan<char> input,
            out string v,
            TerminatingDelegate terminatingPredicate = null)
        {
            if (input.Length == 0)
            {
                v = default;
                return 0;
            }

            terminatingPredicate ??= Helper.DefaultTerminatingPredicate;

            var pos = 0;
            char? prevChar = null;
            char? prevPrevChar = null;

            while (true)
            {
                if (pos == input.Length)
                {
                    if (prevChar == '-')
                    {
                        v = default;
                        return 0;
                    }

                    break;
                }

                var c = input[pos];

                if (c == '-')
                {
                    if (pos == 0)
                    {
                        // ok
                    }
                    else
                    {
                        if (prevChar == '-')
                        {
                            if (prevPrevChar == '-')
                            {
                                v = default;
                                return 0;
                            }

                            if (pos == 1)
                            {
                                // ok
                            }
                            else
                            {
                                v = default;
                                return 0;
                            }
                        }
                    }
                }
                else
                {
                    if (pos == 0)
                    {
                        v = default;
                        return 0;
                    }

                    if (c.IsLatinLetterInternal() || c.IsDecimalDigit())
                    {
                        // ok
                    }
                    else if (terminatingPredicate(input, pos))
                    {
                        if (prevChar == '-')
                        {
                            v = default;
                            return 0;
                        }

                        break;
                    }
                    else
                    {
                        v = default;
                        return 0;
                    }
                }

                if (pos == KeyMaxLength)
                {
                    v = default;
                    return 0;
                }


                prevPrevChar = prevChar;
                prevChar = c;

                pos++;
            }

            if (pos == 0)
            {
                v = default;
                return 0;
            }

            v = input[..pos].ToString();
            return pos;
        }

        public static int TryExtractTerm(
            ReadOnlySpan<char> input,
            out string v,
            TerminatingDelegate terminatingPredicate = null)
        {
            if (input.Length == 0)
            {
                v = default;
                return 0;
            }

            terminatingPredicate ??= Helper.DefaultTerminatingPredicate;

            var pos = 0;
            char? prevChar = null;

            while (true)
            {
                if (pos == input.Length)
                {
                    if (prevChar == '-')
                    {
                        v = default;
                        return 0;
                    }

                    break;
                }

                var c = input[pos];

                if (c.IsDecimalDigit())
                {
                    if (pos == 0)
                    {
                        v = default;
                        return 0;
                    }

                    // ok
                }
                else if (c.IsLatinLetterInternal())
                {
                    // go on
                }
                else if (c == '-')
                {
                    if (pos == 0 || prevChar == '-')
                    {
                        v = default;
                        return 0;
                    }

                    // ok.
                }
                else if (terminatingPredicate(input, pos))
                {
                    break;
                }
                else
                {
                    v = default;
                    return 0;
                }

                if (pos == TermMaxLength)
                {
                    v = default;
                    return 0;
                }

                prevChar = c;
                pos++;
            }

            if (pos == 0)
            {
                v = default;
                return 0;
            }

            v = input[..pos].ToString();
            return pos;
        }

        #region File Path Support

        private static bool IsValidFilePath(ReadOnlySpan<char> input)
        {
            if (input.IsEmpty)
            {
                return false;
            }

            var parts = input.Split('\\', '/');
            for (var i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                var isValidPart = IsValidFilePathPart(part, i, i == parts.Count - 1);
                if (!isValidPart)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsValidFilePathPart(ReadOnlyMemory<char> part, int index, bool isLast)
        {
            var span = part.Span;

            if (span.Length == 0)
            {
                return index == 0 || isLast;
            }

            for (var i = 0; i < span.Length; i++)
            {
                var c = span[i];
                if (c == ':')
                {
                    var ok =
                        index == 0 &&
                        span.Length == 2 &&
                        i == 1 &&
                        span[0].IsLatinLetterInternal() &&
                        true;

                    if (!ok)
                    {
                        return false;
                    }
                }
                else if (c < 32)
                {
                    return false;
                }
                else if (ProhibitedFilePathChars.Contains(c))
                {
                    return false;
                }
                else if (c == ' ')
                {
                    var isBad = i == 0 || i == span.Length - 1;
                    if (isBad)
                    {
                        return false; // no space at start or end of part
                    }
                }
            }

            return true;
        }

        #endregion

        #region JSON String Support

        private static char? GetEscapeReplacement(char escape)
        {
            if (EscapeReplacements.TryGetValue(escape, out var replacement))
            {
                return replacement;
            }

            return null;
        }


        #endregion
    }
}
