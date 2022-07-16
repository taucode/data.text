using System;
using TauCode.Data.Text.Exceptions;

namespace TauCode.Data.Text
{
    public abstract class TextDataExtractorBase<T> : ITextDataExtractor<T>
    {
        #region Fields

        private int? _maxConsumption;
        private TerminatingDelegate _terminator;

        #endregion

        #region ctor

        protected TextDataExtractorBase(
            int? maxConsumption,
            TerminatingDelegate terminator)
        {
            _maxConsumption = CheckMaxConsumption(maxConsumption);
            _terminator = terminator;
        }

        #endregion

        #region Abstract

        protected abstract TextDataExtractionResult TryExtractImpl(
            ReadOnlySpan<char> input,
            out T value);

        #endregion

        #region Protected

        protected void CheckConsumption(int pos)
        {
            if (this.MaxConsumption.HasValue)
            {
                if (pos > this.MaxConsumption.Value)
                {
                    throw new TextDataExtractionException(
                        Helper.GetErrorMessage(TextDataExtractionErrorCodes.InputTooLong),
                        TextDataExtractionErrorCodes.InputTooLong,
                        pos);
                }
            }
        }

        #endregion

        #region Private

        private static int? CheckMaxConsumption(int? maxConsumption)
        {
            if (maxConsumption.HasValue)
            {
                if (maxConsumption.Value < 1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
            }

            return maxConsumption;
        }

        #endregion

        #region ITextDataExtractor<T> Members

        public virtual TerminatingDelegate Terminator
        {
            get
            {
                if (_terminator == null)
                {
                    _terminator = Helper.DefaultTerminatingPredicate;
                }

                return _terminator;
            }
            set => _terminator = value;
        }

        public virtual int? MaxConsumption
        {
            get => _maxConsumption;
            set => _maxConsumption = CheckMaxConsumption(value);
        }

        public TextDataExtractionResult TryExtract(
            ReadOnlySpan<char> input,
            out T value)
        {
            if (input.Length == 0)
            {
                value = default;
                return new TextDataExtractionResult(0, TextDataExtractionErrorCodes.UnexpectedEnd);
            }

            try
            {
                var result = this.TryExtractImpl(input, out value);
                return result;
            }
            catch (TextDataExtractionException ex)
            {
                if (ex.ErrorCode == TextDataExtractionErrorCodes.InputTooLong)
                {
                    value = default;
                    return new TextDataExtractionResult(
                        ex.CharsConsumed,
                        ex.ErrorCode);
                }

                throw;
            }
        }

        public virtual int Extract(ReadOnlySpan<char> input, out T value)
        {
            var result = this.TryExtract(input, out value);
            if (result.ErrorCode == null)
            {
                return result.CharsConsumed;
            }

            var message = this.GetErrorMessage(result.ErrorCode.Value);

            throw new TextDataExtractionException(message, result.ErrorCode.Value, result.CharsConsumed);
        }

        public virtual bool TryParse(ReadOnlySpan<char> input, out T value)
        {
            var savedTerminator = this.Terminator;

            try
            {
                this.Terminator = (inputArg, indexArg) => false;
                var result = this.TryExtract(input, out value);
                return result.ErrorCode == null;
            }
            finally
            {
                // todo_deferred: ut this
                this.Terminator = savedTerminator;
            }
        }

        public virtual T Parse(ReadOnlySpan<char> input)
        {
            var savedTerminator = this.Terminator;

            try
            {
                this.Terminator = (inputArg, indexArg) => false;
                var result = this.TryExtract(input, out var value);

                if (result.ErrorCode == null)
                {
                    return value;
                }

                var message = Helper.GetErrorMessage(result.ErrorCode.Value);

                throw new TextDataExtractionException(message, result.ErrorCode.Value, result.CharsConsumed);
            }
            finally
            {
                this.Terminator = savedTerminator; // todo_deferred ut this
            }

        }

        public virtual string GetErrorMessage(int errorCode)
        {
            return Helper.GetErrorMessage(errorCode);
        }


        #endregion
    }
}
