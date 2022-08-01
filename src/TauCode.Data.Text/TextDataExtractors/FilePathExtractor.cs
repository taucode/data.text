using TauCode.Extensions;

namespace TauCode.Data.Text.TextDataExtractors
{
    public class FilePathExtractor : TextDataExtractorBase<string>
    {
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

        public FilePathExtractor(TerminatingDelegate? terminator = null)
            : base(
                Helper.Constants.FilePath.DefaultMaxConsumption,
                terminator)
        {
        }

        protected override TextDataExtractionResult TryExtractImpl(
            ReadOnlySpan<char> input,
            out string? value)
        {
            value = default;
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
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                    }
                }
                else if (c.IsInlineWhiteSpaceOrCaretControl() || ProhibitedFilePathChars.Contains(c))
                {
                    if (this.IsTermination(input, pos))
                    {
                        break;
                    }
                    else
                    {
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                    }
                }

                if (pos == Helper.Constants.FilePath.MaxLength)
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.FilePathIsTooLong);
                }

                pos++;

                if (this.IsOutOfCapacity(pos))
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InputIsTooLong);
                }
            }

            if (pos == 0)
            {
                return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedEnd);
            }

            var filePathSpan = input[..pos];
            if (IsValidFilePath(filePathSpan))
            {
                value = filePathSpan.ToString();
                return new TextDataExtractionResult(pos, null);
            }

            return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.FailedToExtractFilePath);
        }

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
    }
}
