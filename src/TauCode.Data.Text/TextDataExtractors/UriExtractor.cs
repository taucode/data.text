namespace TauCode.Data.Text.TextDataExtractors;

public class UriExtractor : TextDataExtractorBase<Uri>
{
    private static readonly string[] ValidUriBeginnings =
    {
        Uri.UriSchemeHttps + ":",
        Uri.UriSchemeHttp + ":",

        Uri.UriSchemeFile + ":",
        Uri.UriSchemeFtp + ":",
        Uri.UriSchemeGopher + ":",
        Uri.UriSchemeMailto + ":",
        Uri.UriSchemeNetPipe + ":",
        Uri.UriSchemeNetTcp + ":",
        Uri.UriSchemeNews + ":",
        Uri.UriSchemeNntp + ":",
    };

    public UriExtractor(TerminatingDelegate? terminator = null)
        : base(
            Helper.Constants.Uri.DefaultMaxConsumption,
            terminator)
    {
    }


    protected override TextDataExtractionResult TryExtractImpl(
        ReadOnlySpan<char> input,
        out Uri? value)
    {
        value = default;

        #region detect if input starts with proper beginning

        var isValidBeginning = false;

        foreach (var validUriBeginning in ValidUriBeginnings)
        {
            if (input.StartsWith(validUriBeginning, StringComparison.Ordinal))
            {
                isValidBeginning = true;
                break;
            }
        }

        if (!isValidBeginning)
        {
            return new TextDataExtractionResult(0, TextDataExtractionErrorCodes.FailedToExtractUri);
        }

        #endregion

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
                if (this.IsTermination(input, pos))
                {
                    break;
                }
                else
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                }
            }

            if (pos == Helper.Constants.Uri.MaxLength)
            {
                return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UriIsTooLong);
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

        try
        {
            var uriString = input[..pos].ToString();
            value = new Uri(uriString);
            return new TextDataExtractionResult(pos, null);
        }
        catch
        {
            // dismiss
        }

        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.FailedToExtractUri);
    }
}