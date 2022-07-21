using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TauCode.Data.Text.TextDataExtractors
{
    public class SemanticVersionExtractor : TextDataExtractorBase<SemanticVersion>
    {
        #region Static

        private const string Pattern = @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<preRelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildMetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$";


        private static readonly HashSet<char> AcceptableChars;

        static SemanticVersionExtractor()
        {
            var chars = new List<char> { '+', '-', '.' };

            for (var c = 'a'; c <= 'z'; c++)
            {
                chars.Add(c);
            }

            for (var c = 'A'; c <= 'Z'; c++)
            {
                chars.Add(c);
            }

            for (var c = '0'; c <= '9'; c++)
            {
                chars.Add(c);
            }

            AcceptableChars = new HashSet<char>(chars);
        }

        #endregion

        public SemanticVersionExtractor(TerminatingDelegate terminator = null)
            : base(
                Helper.Constants.SemanticVersion.DefaultMaxConsumption,
                terminator)
        {
        }

        protected override TextDataExtractionResult TryExtractImpl(ReadOnlySpan<char> input, out SemanticVersion value)
        {
            int pos;
            value = default;

            for (pos = 0; pos < input.Length; pos++)
            {
                if (this.IsOutOfCapacity(pos))
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InputIsTooLong);
                }

                var c = input[pos];

                if (AcceptableChars.Contains(c))
                {
                    continue;
                }

                if (this.IsTermination(input, pos))
                {
                    break;
                }

                return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
            }

            if (this.IsOutOfCapacity(pos))
            {
                return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InputIsTooLong);
            }

            if (pos == 0)
            {
                return new TextDataExtractionResult(0, TextDataExtractionErrorCodes.UnexpectedEnd);
            }

            var stringInput = input[..pos].ToString(); // bad (performance), but let it be.

            var match = Regex.Match(stringInput, Pattern);
            if (!match.Success)
            {
                value = default;
                return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.FailedToExtractSemanticVersion);
            }

            int major;
            int minor;
            int patch;

            try
            {
                // not great performance
                var majorString = match.Groups["major"].Value;
                major = int.Parse(majorString);

                var minorString = match.Groups["minor"].Value;
                minor = int.Parse(minorString);

                var patchString = match.Groups["patch"].Value;
                patch = int.Parse(patchString);
            }
            catch
            {
                value = null;
                return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.FailedToExtractSemanticVersion);
            }

            var preRelease = match.Groups["preRelease"].Value;
            if (preRelease == string.Empty)
            {
                preRelease = null;
            }

            var buildMetadata = match.Groups["buildMetadata"].Value;
            if (buildMetadata == string.Empty)
            {
                buildMetadata = null;
            }

            value = new SemanticVersion(major, minor, patch, preRelease, buildMetadata);
            return new TextDataExtractionResult(match.Length, null);
        }
    }
}
