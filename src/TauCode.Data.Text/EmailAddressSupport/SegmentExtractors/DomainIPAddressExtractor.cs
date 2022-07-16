using System;
using TauCode.Data.Text.TextDataExtractors;

namespace TauCode.Data.Text.EmailAddressSupport.SegmentExtractors
{
    internal class DomainIPAddressExtractor : SegmentExtractor
    {
        private static readonly HostNameExtractor IPHostNameExtractor = new HostNameExtractor((span, position) => span[position] == ']');

        internal override bool Accepts(ReadOnlySpan<char> input, EmailAddressExtractionContext context)
        {
            var c = input[context.Position];
            return c == '[';
        }

        protected override TextDataExtractionResult ExtractImpl(
            ReadOnlySpan<char> input, EmailAddressExtractionContext context,
            out Segment? segment)
        {
            if (context.Position + 1 == input.Length)
            {
                segment = null;
                return new TextDataExtractionResult(1, TextDataExtractionErrorCodes.UnexpectedEnd);
            }

            var ipHostNameSpan = input[(context.Position + 1)..];
            var c = ipHostNameSpan[0];

            if (c.IsDecimalDigit())
            {
                var pos = context.Position + 1; // skip '['
                var ipv4Span = input[pos..];
                var ipv4HostNameResult = IPHostNameExtractor.TryExtract(ipv4Span, out var ipv4HostName);

                if (ipv4HostNameResult.ErrorCode.HasValue)
                {
                    segment = null;
                    var consumed = pos + ipv4HostNameResult.CharsConsumed - context.Position;
                    return new TextDataExtractionResult(
                        consumed,
                        ipv4HostNameResult.ErrorCode);
                }
                else
                {
                    // ipv4 extracted successfully.
                    if (input.Length == pos + ipv4HostNameResult.CharsConsumed)
                    {
                        // we are missing closing ']'
                        segment = null;
                        return new TextDataExtractionResult(
                            input.Length - context.Position,
                            TextDataExtractionErrorCodes.UnexpectedEnd);
                    }
                    else if (input[pos + ipv4HostNameResult.CharsConsumed] == ']')
                    {
                        // great.
                        pos += ipv4HostNameResult.CharsConsumed;
                        var consumed = pos - context.Position;
                        segment = new Segment(
                            SegmentType.DomainIPAddress,
                            context.Position,
                            consumed + 1, // skip ']'
                            ipv4HostName);

                        return new TextDataExtractionResult(consumed, null);
                    }
                    else
                    {
                        // should never happen because terminating char was ']'
                        segment = null;
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InternalError);
                    }
                }
            }
            else if (c == 'I')
            {
                const string ipv6Tag = "IPv6:";

                var pos = context.Position + 1; // skip '['
                for (var i = 1; i < ipv6Tag.Length; i++)
                {
                    var eq = input[pos + i] == ipv6Tag[i];
                    if (!eq)
                    {
                        var consumed = pos - context.Position;
                        segment = null;
                        return new TextDataExtractionResult(consumed, TextDataExtractionErrorCodes.InvalidIPv6Prefix);
                    }
                }

                pos += ipv6Tag.Length;

                var ipv6Span = input[pos..];
                var ipv6HostNameResult = IPHostNameExtractor.TryExtract(ipv6Span, out var ipv6HostName);

                if (ipv6HostNameResult.ErrorCode.HasValue)
                {
                    segment = null;
                    var consumed = pos + ipv6HostNameResult.CharsConsumed - context.Position;
                    return new TextDataExtractionResult(
                        consumed,
                        ipv6HostNameResult.ErrorCode);
                }
                else
                {
                    // ipv6 extracted successfully.
                    if (input.Length == pos + ipv6HostNameResult.CharsConsumed)
                    {
                        // we are missing closing ']'
                        segment = null;
                        return new TextDataExtractionResult(
                            input.Length - context.Position,
                            TextDataExtractionErrorCodes.UnexpectedEnd);
                    }
                    else if (input[pos + ipv6HostNameResult.CharsConsumed] == ']')
                    {
                        // great.
                        pos += ipv6HostNameResult.CharsConsumed;
                        var consumed = pos - context.Position;
                        segment = new Segment(
                            SegmentType.DomainIPAddress,
                            context.Position,
                            consumed + 1, // skip ']'
                            ipv6HostName);

                        return new TextDataExtractionResult(consumed, null);
                    }
                    else
                    {
                        // should never happen because terminating char was ']'
                        segment = null;
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.InternalError);
                    }
                }
            }
            else
            {
                segment = null;
                return new TextDataExtractionResult(1, TextDataExtractionErrorCodes.UnexpectedCharacter);
            }
        }
    }
}
