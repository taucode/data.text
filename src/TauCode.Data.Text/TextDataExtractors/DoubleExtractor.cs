﻿using System;
using System.Collections.Generic;

namespace TauCode.Data.Text.TextDataExtractors
{
    public class DoubleExtractor : TextDataExtractorBase<double>
    {
        private static readonly HashSet<char> DoubleChars;

        static DoubleExtractor()
        {
            DoubleChars = new HashSet<char>("+-eE0123456789.");
        }

        public DoubleExtractor(TerminatingDelegate terminator)
            : base(
                Helper.Constants.Double.MaxConsumption,
                terminator)
        {
        }

        protected override TextDataExtractionResult TryExtractImpl(
            ReadOnlySpan<char> input,
            out double value)
        {
            var pos = 0;
            value = default;

            while (true)
            {
                if (pos > Helper.Constants.Double.MaxConsumption)
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InputTooLong);
                }

                if (pos == input.Length)
                {
                    break;
                }

                var c = input[pos];
                if (DoubleChars.Contains(c))
                {
                    // ok
                }
                else if (this.Terminator(input, pos))
                {
                    break;
                }
                else
                {
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                }

                pos++;
            }

            if (pos == 0)
            {
                return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedEnd);
            }

            input = input[..pos];
            var parsed = double.TryParse(input, out value);

            if (parsed && double.IsFinite(value))
            {
                return new TextDataExtractionResult(pos, null);
            }

            value = default;
            return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.FailedToExtractDouble);
        }
    }
}