namespace TauCode.Data.Text.TextDataExtractors;

public class EnumExtractor<T> : ItemExtractor<T> where T : struct
{
    public EnumExtractor(
        bool ignoreCase = true,
        TerminatingDelegate? terminator = null)
        : base(
            Enum
                .GetValues(typeof(T))
                .Cast<T>()
                .Select(x => x.ToString())
                .ToHashSet()!,
            ignoreCase,
            Enum.Parse<T>,
            terminator)
    {
    }
}