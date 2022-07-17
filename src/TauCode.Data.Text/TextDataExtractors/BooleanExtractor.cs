using System.Collections.Generic;

namespace TauCode.Data.Text.TextDataExtractors
{
    public class BooleanExtractor : ItemExtractor<bool>
    {
        private static readonly HashSet<string> BooleanItems = new HashSet<string>(new[] { "false", "true" });

        public BooleanExtractor(TerminatingDelegate terminator)
            : base(
                BooleanItems,
                true,
                (s, ignoreCase) => bool.Parse(s),
                terminator)
        {
        }
    }
}
