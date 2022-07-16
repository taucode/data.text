using System;
using System.Linq;

namespace TauCode.Data.Text.TextDataExtractors
{
    public class EnumExtractor<T> : ItemExtractor<T> where T : struct
    {
        public EnumExtractor(
            bool ignoreCase,
            TerminatingDelegate terminator = null)
            : base(
                Enum
                    .GetValues(typeof(T))
                    .Cast<T>()
                    .Select(x => x.ToString())
                    .ToHashSet(),
                ignoreCase,
                x => Enum.Parse<T>(x, ignoreCase),
                terminator)
        {
        }
    }
}
