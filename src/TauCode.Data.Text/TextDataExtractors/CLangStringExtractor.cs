using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TauCode.Data.Text.TextDataExtractors
{
    public class CLangStringExtractor : TextDataExtractorBase<string>
    {
        #region Static

        private static readonly string[] ReplacementStrings =
        {
            "\"\"",
            "\\\\",
            "0\0",
            "a\a",
            "b\b",
            "f\f",
            "n\n",
            "r\r",
            "t\t",
            "v\v",
        };

        private static readonly Dictionary<char, char> Replacements;

        private static char? GetReplacement(char escape)
        {
            if (Replacements.TryGetValue(escape, out var replacement))
            {
                return replacement;
            }

            return null;
        }

        static CLangStringExtractor()
        {
            Replacements = ReplacementStrings
                .ToDictionary(
                    x => x.First(),
                    x => x.Skip(1).Single());
        }

        #endregion

        public CLangStringExtractor(TerminatingDelegate terminator = null)
            : base(
                null,
                terminator)
        {
        }

        protected override TextDataExtractionResult TryExtractImpl(
            ReadOnlySpan<char> input,
            out string value)
        {
            value = default;
            var pos = 0;

            var sb = new StringBuilder();

            while (true)
            {
                if (pos == input.Length)
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnclosedString);
                }

                var c = input[pos];

                if (c == '"')
                {
                    #region double quote (")

                    if (pos > 0)
                    {
                        if (pos == input.Length - 1)
                        {
                            pos++;
                            if (this.IsOutOfCapacity(pos))
                            {
                                return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InputIsTooLong);
                            }

                            break;
                        }

                        // pos < input.Length - 1
                        if (this.IsTermination(input, pos + 1))
                        {
                            pos++;
                            if (this.IsOutOfCapacity(pos))
                            {
                                return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InputIsTooLong);
                            }
                            break;
                        }

                        return new TextDataExtractionResult(
                            pos + 1,
                            TextDataExtractionErrorCodes.UnexpectedCharacter);
                    }

                    #endregion
                }
                else if (c == '\\')
                {
                    #region escape (\)

                    if (pos + 1 == input.Length)
                    {
                        return new TextDataExtractionResult(pos + 1, TextDataExtractionErrorCodes.UnexpectedEnd);
                    }

                    var nextChar = input[pos + 1];
                    if (nextChar == 'u')
                    {
                        var remaining = input.Length - (pos + 1);
                        if (remaining < 5)
                        {
                            return new TextDataExtractionResult(pos + 1, TextDataExtractionErrorCodes.BadEscape);
                        }

                        var hexNumString = input.Slice(pos + 2, 4);
                        var codeParsed = int.TryParse(
                            hexNumString,
                            NumberStyles.HexNumber,
                            CultureInfo.InvariantCulture,
                            out var code);

                        if (!codeParsed)
                        {
                            return new TextDataExtractionResult(pos + 1, TextDataExtractionErrorCodes.BadEscape);
                        }

                        var unescapedChar = (char)code;
                        sb.Append(unescapedChar);

                        pos += 6; // skip "\", 'u' and 'hhhh'
                        continue;
                    }
                    else
                    {
                        var replacement = GetReplacement(nextChar);
                        if (replacement.HasValue)
                        {
                            sb.Append(replacement);
                            pos += 2;
                            continue;
                        }
                        else
                        {
                            return new TextDataExtractionResult(pos + 1, TextDataExtractionErrorCodes.BadEscape);
                        }
                    }

                    #endregion
                }
                else if (c.IsCaretControl())
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.NewLineInString);
                }
                else
                {
                    if (pos == 0)
                    {
                        // only '"' can be 0th char.
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                    }

                    sb.Append(c);
                }

                pos++;
                if (this.IsOutOfCapacity(pos))
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InputIsTooLong);
                }
            }

            value = sb.ToString();
            return new TextDataExtractionResult(pos, null);
        }
    }
}
