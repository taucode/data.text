using System.Globalization;
using System.Text;

namespace TauCode.Data.Text.TextDataExtractors;

public class JsonStringExtractor : TextDataExtractorBase<string>
{
    #region Static

    private static readonly string[] ReplacementStrings =
    {
        "\"\"",
        "''",
        "\\\\",
        "n\n",
        "r\r",
        "t\t",
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

    static JsonStringExtractor()
    {
        Replacements = ReplacementStrings
            .ToDictionary(
                x => x.First(),
                x => x.Skip(1).Single());
    }

    #endregion

    public JsonStringExtractor(TerminatingDelegate? terminator = null)
        : base(
            null,
            terminator)
    {
    }

    protected override TextDataExtractionResult TryExtractImpl(
        ReadOnlySpan<char> input,
        out string? value)
    {
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
        // min 'MaxConsumption' is 1, 'pos' is 1 here, so don't need to check 'IsOutOfCapacity'.

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
                    if (this.IsOutOfCapacity(pos))
                    {
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InputIsTooLong);
                    }

                    break;
                }
                else
                {
                    if (this.IsTermination(input, pos + 1))
                    {
                        pos++;
                        if (this.IsOutOfCapacity(pos))
                        {
                            return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InputIsTooLong);
                        }


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

                    var replacement = GetReplacement(nextChar);
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
            if (this.IsOutOfCapacity(pos))
            {
                return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InputIsTooLong);
            }
        }

        value = sb.ToString();
        return new TextDataExtractionResult(pos, null);
    }
}