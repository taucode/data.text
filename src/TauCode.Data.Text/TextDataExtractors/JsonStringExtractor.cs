using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TauCode.Data.Text.TextDataExtractors
{
    public class JsonStringExtractor : TextDataExtractorBase<string>
    {
        private static readonly Dictionary<char, char> JsonEscapeReplacements;
        private static readonly string[] JsonEscapeReplacementStrings =
        {
            "\"\"",
            "''",
            "\\\\",
            "n\n",
            "r\r",
            "t\t",
        };

        static JsonStringExtractor()
        {
            JsonEscapeReplacements = JsonEscapeReplacementStrings
                .ToDictionary(
                    x => x.First(),
                    x => x.Skip(1).Single());
        }

        public JsonStringExtractor(TerminatingDelegate terminator = null)
            : base(
                null,
                terminator) // todo_deferred ut MaxConsumption
        {
        }

        protected override TextDataExtractionResult TryExtractImpl(
            ReadOnlySpan<char> input,
            out string value)
        {
            // todo_deferred: ut unclosed string, newline in string.

            value = default;

            var pos = 0;
            var c = input[0];
            char delimiter;

            if (c == '"' || c == '\'')
            {
                delimiter = c;
            }
            else
            {
                return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
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
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnclosedString);
                }

                c = input[pos];

                if (c.IsCaretControl())
                {
                    // newline in string
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.NewLineInString);
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
                        if (this.Terminator(input, pos + 1))
                        {
                            pos++;
                            break;
                        }
                        else
                        {
                            return new TextDataExtractionResult(
                                pos + 1,
                                TextDataExtractionErrorCodes.UnexpectedCharacter);
                        }
                    }
                }
                else if (c == '\\')
                {
                    #region escaping

                    if (pos + 1 == input.Length)
                    {
                        return new TextDataExtractionResult(pos + 1, TextDataExtractionErrorCodes.UnclosedString);
                    }

                    var nextChar = input[pos + 1];
                    if (nextChar == 'u')
                    {
                        var remaining = input.Length - (pos + 1);
                        if (remaining < 5)
                        {
                            return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.BadEscape);
                        }

                        var hexNumString = input.Slice(pos + 2, 4);
                        var codeParsed = int.TryParse(
                            hexNumString,
                            NumberStyles.HexNumber,
                            CultureInfo.InvariantCulture,
                            out var code);

                        if (!codeParsed)
                        {
                            return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.BadEscape);
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
                            // opposite delimiter doesn't need to be escaped, e.g. strings 'escaped double quote \" inside' and "escaped quote \' inside" are wrong.
                            return new TextDataExtractionResult(pos + 1, TextDataExtractionErrorCodes.BadEscape);
                        }

                        var replacement = GetJsonEscapeReplacement(nextChar);
                        if (replacement.HasValue)
                        {
                            sb.Append(replacement);
                            pos += 2;
                            continue;
                        }
                        else
                        {
                            return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.BadEscape);
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

            value = sb.ToString();
            return new TextDataExtractionResult(pos, null);
        }


        private static char? GetJsonEscapeReplacement(char escape)
        {
            if (JsonEscapeReplacements.TryGetValue(escape, out var replacement))
            {
                return replacement;
            }

            return null;
        }
    }
}
