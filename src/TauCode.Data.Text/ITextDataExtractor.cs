namespace TauCode.Data.Text;

public interface ITextDataExtractor<T>
{
    TerminatingDelegate? Terminator { get; set; }

    /// <summary>
    /// Max number of chars the extractor can consume.
    /// If null, then there is no limit.
    /// </summary>
    int? MaxConsumption { get; set; }

    string GetErrorMessage(int errorCode);

    TextDataExtractionResult TryExtract(ReadOnlySpan<char> input, out T? value);

    int Extract(ReadOnlySpan<char> input, out T? value);

    bool TryParse(ReadOnlySpan<char> input, out T? value);

    T? Parse(ReadOnlySpan<char> input);
}