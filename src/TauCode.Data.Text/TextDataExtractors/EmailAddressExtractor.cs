using System;
using System.Collections.Generic;
using TauCode.Data.Text.EmailAddressSupport;
using TauCode.Data.Text.EmailAddressSupport.SegmentExtractors;
using TauCode.Data.Text.Exceptions;

namespace TauCode.Data.Text.TextDataExtractors
{
    public class EmailAddressExtractor : TextDataExtractorBase<EmailAddress>
    {
        #region Invariants

        private static readonly SegmentExtractor[] InitialSegmentExtractors =
        {
            new LocalPartWordExtractor(),
            new LocalPartQuotedStringExtractor(),
            new CommentExtractor(true),
            new LocalPartWhiteSpaceExtractor(),
        };

        private static readonly SegmentExtractor[] AfterMeaninglessSegmentExtractors =
        {
            new LocalPartWordExtractor(),
            new LocalPartQuotedStringExtractor(),
            new CommentExtractor(true),
            new LocalPartWhiteSpaceExtractor(),
            new LocalPartFoldingWhiteSpaceExtractor(),
        };

        private static readonly SegmentExtractor[] AfterLocalPartSegmentExtractors =
        {
            new AtSignExtractor(),
            new PeriodExtractor(),
            new CommentExtractor(false),
            new LocalPartWhiteSpaceExtractor(),
            new LocalPartFoldingWhiteSpaceExtractor(),
        };

        private static readonly SegmentExtractor[] AfterLocalPartPeriodSegmentExtractors =
        {
            new LocalPartWordExtractor(),
            new LocalPartQuotedStringExtractor(),
            new CommentExtractor(true),
            new LocalPartWhiteSpaceExtractor(),
            new LocalPartFoldingWhiteSpaceExtractor(),
        };

        private static readonly SegmentExtractor[] AfterAtSymbolSegmentExtractors =
        {
            new DomainLabelExtractor(),
            new DomainIPAddressExtractor(),
            new CommentExtractor(false),
        };

        private static readonly SegmentExtractor[] AfterDomainPeriodSegmentExtractors =
        {
            new DomainLabelExtractor(),
            new CommentExtractor(false),
        };

        internal static readonly HashSet<char> AllowedSymbols;
        internal static readonly HashSet<char> AllowedSymbolsInQuotedString;
        internal static readonly HashSet<char> AllowedSymbolsInComment;

        #endregion

        #region Static ctor

        static EmailAddressExtractor()
        {
            var allowedSymbolsInLocalPartWord = new List<char>
            {
                '-',
                '+',
                '=',
                '!',
                '?',
                '%',
                '~',
                '$',
                '&',
                '/',
                '|',
                '{',
                '}',
                '#',
                '*',
                '^',
                '_',
                '`',
                '\''
            };

            AllowedSymbols = new HashSet<char>(allowedSymbolsInLocalPartWord);

            var allowedSymbolsInQuotedStringList = new List<char>(AllowedSymbols);
            allowedSymbolsInQuotedStringList.AddRange(new[]
            {
                ']',
                '[',
                ',',
                '.',
                ')',
                '(',
                '@',
                ' ',
                ':',
                (char)160,
            });

            AllowedSymbolsInQuotedString = new HashSet<char>(allowedSymbolsInQuotedStringList);

            var allowedSymbolsInCommentList = new List<char>(AllowedSymbols);
            allowedSymbolsInCommentList.AddRange(new[]
            {
                ']',
                '[',
                ',',
                '.',
                '@',
                ' ',
                ':',
                (char)160,
            });
            AllowedSymbolsInComment = new HashSet<char>(allowedSymbolsInCommentList);
        }

        #endregion

        #region Fields

        private readonly SegmentExtractor[] _afterDomainLabelSegmentExtractors;
        private readonly SegmentExtractor[] _afterDomainIPAddressSegmentExtractors;

        #endregion

        public EmailAddressExtractor(TerminatingDelegate terminator = null)
            : base(
                Helper.Constants.EmailAddress.DefaultMaxConsumption,
                terminator)
        {
            var terminatingSegmentExtractor = new TerminatingSegmentExtractor(this);

            _afterDomainLabelSegmentExtractors = new SegmentExtractor[]
            {
                new PeriodExtractor(),
                terminatingSegmentExtractor,
                new CommentExtractor(false),
            };

            _afterDomainIPAddressSegmentExtractors = new SegmentExtractor[]
            {
                terminatingSegmentExtractor,
                new CommentExtractor(false),
            };

            this.HostNameExtractor = new HostNameExtractor(terminator);
        }

        internal HostNameExtractor HostNameExtractor { get; }

        public override TerminatingDelegate Terminator
        {
            get => this.HostNameExtractor.Terminator;
            set => this.HostNameExtractor.Terminator = value;
        }

        protected override TextDataExtractionResult TryExtractImpl(
            ReadOnlySpan<char> input,
            out EmailAddress emailAddress)
        {
            var context = new EmailAddressExtractionContext(this);

            while (true)
            {
                if (context.Position == input.Length)
                {
                    break;
                }

                var segmentExtractors = GetAppropriateSegmentExtractors(context);

                SegmentExtractor selectedExtractor = null;
                foreach (var extractor in segmentExtractors)
                {
                    if (extractor.Accepts(input, context))
                    {
                        selectedExtractor = extractor;
                        break;
                    }
                }

                if (selectedExtractor == null)
                {
                    emailAddress = default;

                    return new TextDataExtractionResult(
                        context.Position,
                        TextDataExtractionErrorCodes.UnexpectedCharacter);
                }

                var result = selectedExtractor.Extract(input, context, out var segment);

                if (result.ErrorCode.HasValue)
                {
                    emailAddress = default;
                    var charsConsumed = context.Position + result.CharsConsumed;

                    // out of input => ignore error segment extractor returned
                    if (this.IsOutOfCapacity(charsConsumed))
                    {
                        emailAddress = default;
                        return new TextDataExtractionResult(
                            this.MaxConsumption.Value + 1,
                            TextDataExtractionErrorCodes.InputIsTooLong);
                    }

                    return new TextDataExtractionResult(charsConsumed, result.ErrorCode);
                }

                if (this.IsOutOfCapacity(context.Position + result.CharsConsumed))
                {
                    emailAddress = default;
                    return new TextDataExtractionResult(
                        this.MaxConsumption.Value + 1,
                        TextDataExtractionErrorCodes.InputIsTooLong);
                }

                if (!segment.HasValue)
                {
                    // should never happen
                    emailAddress = default;
                    return new TextDataExtractionResult(context.Position, TextDataExtractionErrorCodes.InternalError);
                }

                var segmentValue = segment.Value;

                var oldLocalPartLength = context.LocalPartLength;
                var oldPosition = context.Position;

                if (segmentValue.IsMeaningful())
                {
                    context.AddSegment(segmentValue);
                }

                if (context.AtSymbolIndex == null)
                {
                    // still working with local part
                    var newLocalPartLength = context.LocalPartLength;

                    if (newLocalPartLength > Helper.Constants.EmailAddress.MaxLocalPartLength)
                    {
                        var charsToBeCut = newLocalPartLength - Helper.Constants.EmailAddress.MaxLocalPartLength;
                        var lastSegmentCharsToRemain = segmentValue.Length - charsToBeCut;
                        var consumedBeforeItBecameTooMuch = oldPosition + lastSegmentCharsToRemain;

                        emailAddress = null;
                        return new TextDataExtractionResult(
                            consumedBeforeItBecameTooMuch + 1,
                            TextDataExtractionErrorCodes.LocalPartIsTooLong);
                    }
                }

                if (segmentValue.Type == SegmentType.Termination)
                {
                    break;
                }

                context.Position += segmentValue.Length;
            }

            #region deal with incomplete email address

            if (context.AtSymbolIndex == null || context.DomainLength == 0)
            {
                emailAddress = default;
                return new TextDataExtractionResult(context.Position, TextDataExtractionErrorCodes.UnexpectedEnd);
            }

            #endregion

            var localPartArray = new char[context.LocalPartLength];
            var pos = 0;

            foreach (var segment in context.LocalPartSegments)
            {
                var span = input.Slice(segment.Start, segment.Length);
                var destination = new Span<char>(localPartArray, pos, span.Length);
                span.CopyTo(destination);
                pos += span.Length;
            }

            var localPart = new string(localPartArray);
            var hostNameNullable = BuildDomain(input, context, out var domainResult);
            if (hostNameNullable == null)
            {
                emailAddress = default;
                return domainResult;
            }

            var length = EmailAddress.CalculateLength(localPart.Length, hostNameNullable.Value);
            if (length > Helper.Constants.EmailAddress.MaxCleanEmailAddressLength)
            {
                emailAddress = default;
                return new TextDataExtractionResult(context.Position, TextDataExtractionErrorCodes.EmailAddressIsTooLong);
            }

            emailAddress = new EmailAddress(localPart, hostNameNullable.Value);
            return new TextDataExtractionResult(context.Position, null);
        }

        private IList<SegmentExtractor> GetAppropriateSegmentExtractors(EmailAddressExtractionContext context)
        {
            if (context.Position == 0)
            {
                return InitialSegmentExtractors;
            }
            else if (context.AtSymbolIndex.HasValue)
            {
                // domain
                var lastDomainSegmentType = context.GetLastDomainSegmentType();
                if (lastDomainSegmentType == null)
                {
                    return AfterAtSymbolSegmentExtractors;
                }
                else
                {
                    var lastDomainSegmentTypeValue = lastDomainSegmentType.Value;

                    if (lastDomainSegmentTypeValue == SegmentType.DomainLabel)
                    {
                        return _afterDomainLabelSegmentExtractors;
                    }
                    else if (lastDomainSegmentTypeValue == SegmentType.Period)
                    {
                        return AfterDomainPeriodSegmentExtractors;
                    }
                    else if (lastDomainSegmentTypeValue == SegmentType.DomainIPAddress)
                    {
                        return _afterDomainIPAddressSegmentExtractors;
                    }
                    else
                    {
                        // should never happen.
                        throw new TextDataExtractionException(
                            "Internal error.",
                            TextDataExtractionErrorCodes.InternalError,
                            context.Position);
                    }
                }
            }
            else
            {
                // still local part
                var lastLocalPartSegmentTypeNullable = context.GetLastLocalPartSegmentType();
                if (lastLocalPartSegmentTypeNullable == null)
                {
                    // context.Position > 0, but no meaningful segments yet => we only consumed comments, whitespaces, and folding whitespaces.

                    return AfterMeaninglessSegmentExtractors;
                }

                var lastLocalPartSegmentType = lastLocalPartSegmentTypeNullable.Value;
                switch (lastLocalPartSegmentType)
                {
                    case SegmentType.Period:
                        return AfterLocalPartPeriodSegmentExtractors;

                    case SegmentType.LocalPartWord:
                    case SegmentType.LocalPartQuotedString:
                        return AfterLocalPartSegmentExtractors;

                    default:
                        // cannot be: above three are the only valid options.
                        throw new TextDataExtractionException(
                            this.GetErrorMessage(
                                TextDataExtractionErrorCodes.InternalError),
                            TextDataExtractionErrorCodes.InternalError,
                            context.Position);
                }
            }
        }

        private static HostName? BuildDomain(
            ReadOnlySpan<char> input,
            EmailAddressExtractionContext context,
            out TextDataExtractionResult result)
        {
            var contextIPHostNameSegment = context.GetIPHostNameSegment();

            if (contextIPHostNameSegment != null)
            {
                result = new TextDataExtractionResult(contextIPHostNameSegment.Value.Length, null);
                return contextIPHostNameSegment.Value.IPHostName;
            }

            // got 'domain name' host
            var domainArray = new char[context.DomainLength];
            var pos = 0;

            foreach (var segment in context.DomainSegments)
            {
                var span = input.Slice(segment.Start, segment.Length);
                var destination = new Span<char>(domainArray, pos, span.Length);
                span.CopyTo(destination);
                pos += span.Length;
            }

            var domainString = new string(domainArray);

            var domainResult = context.EmailAddressExtractor.HostNameExtractor.TryExtract(
                domainString,
                out var domain);

            if (domainResult.ErrorCode.HasValue)
            {
                if (domainResult.ErrorCode.Value == TextDataExtractionErrorCodes.InputIsTooLong)
                {
                    result = new TextDataExtractionResult(
                        context.GetDomainStartIndex() + domainResult.CharsConsumed,
                        TextDataExtractionErrorCodes.HostNameIsTooLong);
                }
                else
                {
                    result = new TextDataExtractionResult(
                        context.GetDomainStartIndex() + domainResult.CharsConsumed,
                        domainResult.ErrorCode);
                }

                return null;
            }

            if (domain.Kind == HostNameKind.IPv4)
            {
                // we've got something like 'john@127.0.0.1', which is an error: IPv4 domains must be enclosed in '[' and ']'.
                result = new TextDataExtractionResult(
                    context.GetDomainStartIndex(),
                    TextDataExtractionErrorCodes.IPv4MustBeEnclosedInBrackets);
                return null;
            }

            result = domainResult;
            return domain;
        }
    }
}
