using System.Text;

// todo ut this extractor
namespace TauCode.Data.Text.TextDataExtractors
{
    public class SqlStringExtractor : TextDataExtractorBase<string>
    {
        public SqlStringExtractor(TerminatingDelegate? terminator = null)
            : base(null, terminator)
        {
        }

        protected override TextDataExtractionResult TryExtractImpl(ReadOnlySpan<char> input, out string? value)
        {
            var pos = 0;
            value = default;

            if (input.Length == 0)
            {
                return new TextDataExtractionResult(0, TextDataExtractionErrorCodes.UnexpectedEnd);
            }

            var c = input[0];
            if (c != '\'')
            {
                return new TextDataExtractionResult(0, TextDataExtractionErrorCodes.UnexpectedCharacter);
            }

            pos++;

            var sb = new StringBuilder();

            while (true)
            {
                if (pos == input.Length)
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnclosedString);
                }

                c = input[pos];

                if (c == '\'')
                {
                    // consume '\''
                    pos++;

                    if (pos == input.Length)
                    {
                        // input ends with closing '\''
                        break;
                    }
                    else
                    {
                        // input goes on
                        c = input[pos];
                        if (c == '\'')
                        {
                            // two quotes in row => one quote in result
                            sb.Append('\'');
                        }
                        else if (this.IsTermination(input, pos))
                        {
                            // char going after closing '\'' gotta be a valid one, i.e. terminator
                            break;
                        }
                        else
                        {
                            return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                        }
                    }
                }
                else if (c.IsCaretControl())
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.NewLineInString);
                }
                else
                {
                    sb.Append(c);
                }

                pos++;
            }

            value = sb.ToString();
            return new TextDataExtractionResult(pos, null);
        }
    }
}
