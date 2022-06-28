using System;
using System.Collections.Generic;
using TauCode.Data.Text.EmailAddressSupport;
using TauCode.Data.Text.Exceptions;
using TauCode.Extensions;

namespace TauCode.Data.Text
{
    internal static class Helper
    {
        internal static readonly HashSet<char> EmptyChars = new HashSet<char>();
        /// <summary>
        /// Label is a part of dot-separated ascii domain name, e.g.
        /// in case of domain name 'cor1.2.some-site.com',
        /// labels are: 'cor1', '2', 'some-site', 'com'.
        /// </summary>
        internal const int MaxAsciiLabelLength = 63;

        internal static bool IsLatinLetterInternal(this char c)
        {
            if (c >= 'a' && c <= 'z')
            {
                return true;
            }

            if (c >= 'A' && c <= 'Z')
            {
                return true;
            }

            return false;
        }

        internal static bool IsHexDigit(this char c)
        {
            if (c >= 'a' && c <= 'f')
            {
                return true;
            }

            if (c >= 'A' && c <= 'F')
            {
                return true;
            }

            if (c >= '0' && c <= '9')
            {
                return true;
            }

            return false;
        }

        internal static bool IsDecimalDigit(this char c)
        {
            if (c >= '0' && c <= '9')
            {
                return true;
            }

            return false;
        }

        internal static bool IsUnicodeInternal(this char c) => c >= 256;

        internal static bool IsLocalPartSegment(this SegmentType segmentType)
        {
            return
                segmentType == SegmentType.LocalPartWord ||
                segmentType == SegmentType.Period ||
                segmentType == SegmentType.LocalPartQuotedString ||
                false;
        }

        internal static bool IsDomainSegment(this SegmentType segmentType)
        {
            return
                segmentType == SegmentType.Label ||
                segmentType == SegmentType.Period ||
                segmentType == SegmentType.IPAddress ||
                false;
        }

        // todo: ut all messages
        internal static string GetErrorMessage(ExtractionErrorTag errorTag)
        {
            return errorTag switch
            {
                // Common
                ExtractionErrorTag.EmptyInput => "Empty input.",
                ExtractionErrorTag.InputTooLong => "Input is too long.",
                ExtractionErrorTag.UnexpectedChar => "Unexpected character.",
                ExtractionErrorTag.UnexpectedEnd => "Unexpected end.",
                ExtractionErrorTag.BadEscape => "Bad escape.",
                ExtractionErrorTag.InternalError => "Internal error.",

                // HostName
                ExtractionErrorTag.HostNameTooLong => "Host name is too long.",
                ExtractionErrorTag.DomainLabelTooLong => "Domain label is too long.",
                ExtractionErrorTag.InvalidHostName => "Invalid host name.",
                ExtractionErrorTag.InvalidIPv4Address => "Invalid IPv4 address.",
                ExtractionErrorTag.InvalidIPv6Address => "Invalid IPv6 address specification.",

                // EmailAddress
                ExtractionErrorTag.EmptyLocalPart => "Empty local part.",
                ExtractionErrorTag.LocalPartTooLong => "Local part is too long.",
                ExtractionErrorTag.EmailAddressTooLong => "Email address is too long.",
                ExtractionErrorTag.InvalidDomain => "Invalid domain.",
                ExtractionErrorTag.UnescapedSpecialCharacter => "Unescaped special character.",
                ExtractionErrorTag.UnclosedQuotedString => "Unclosed quoted string.",
                ExtractionErrorTag.EmptyQuotedString => "Empty quoted string.",
                ExtractionErrorTag.IPv4MustBeEnclosedInBrackets =>
                    "IPv4 address must be enclosed in '[' and ']'.",

                // Emoji
                ExtractionErrorTag.NonEmojiChar => "Non-emoji character.",
                ExtractionErrorTag.IncompleteEmoji => "Incomplete emoji.",
                ExtractionErrorTag.BadEmoji => "Bad emoji.",

                // SemanticVersion
                ExtractionErrorTag.InvalidSemanticVersion => "Invalid semantic version.",

                _ => "Unknown error",
            };
        }

        internal static TextDataExtractionException CreateException(ExtractionErrorTag errorTag, int? errorPosition)
        {
            // todo: ut all usages
            var message = Helper.GetErrorMessage(errorTag);
            var ex = new TextDataExtractionException(message, errorPosition)
            {
                ExtractionError = errorTag
            };
            return ex;
        }

        internal static bool DefaultTerminatingPredicate(ReadOnlySpan<char> input, int position)
        {
            // todo check args

            var c = input[position];
            return c.IsIn(' ', '\r', '\n', '\t');
        }

        internal static bool IsInlineWhiteSpaceOrCaretControl(this char c) => IsInlineWhiteSpace(c) || IsCaretControl(c);

        internal static bool IsCaretControl(this char c) => c.IsIn('\r', '\n');

        internal static bool IsInlineWhiteSpace(this char c) => c.IsIn(' ', '\t');
    }
}
