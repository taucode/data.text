using System;

namespace TauCode.Data.Text.TextDataExtractors
{
    public class SqlIdentifierExtractor : TextDataExtractorBase<SqlIdentifier>
    {
        private SqlIdentifierDelimiter _delimiter;

        public SqlIdentifierExtractor(
            TerminatingDelegate terminator = null)
            : base(
                Helper.Constants.SqlIdentifier.DefaultMaxConsumption,
                terminator)
        {
            _delimiter = SqlIdentifierDelimiter.None;
        }

        public SqlIdentifierDelimiter Delimiter
        {
            get => _delimiter;
            set
            {
                var valueIsOk =
                    value != 0 &&
                    ((int)_delimiter & ~0xf) == 0;

                if (!valueIsOk)
                {
                    throw new ArgumentException(
                        "Delimiter cannot be 0 and must be a combination of valid enum values.",
                        nameof(value)); // todo_deferred ut
                }

                _delimiter = value;
            }
        }

        protected override TextDataExtractionResult TryExtractImpl(ReadOnlySpan<char> input, out SqlIdentifier value)
        {
            var pos = 0;
            value = default;

            var actualDelimiter = SqlIdentifierDelimiter.None;
            char? closingDelimiterChar = null;
            var valueStartPos = 0;
            char? prevChar = null;

            while (true)
            {
                if (pos == input.Length)
                {
                    if (closingDelimiterChar.HasValue)
                    {
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedEnd);
                    }
                    else
                    {
                        break;
                    }
                }

                var c = input[pos];

                if (pos == 0)
                {
                    #region pos == 0

                    // 0th char can be opening delimiter or 'ascii identifier' beginning
                    if (c == '"')
                    {
                        if ((this.Delimiter & SqlIdentifierDelimiter.DoubleQuotes) != 0)
                        {
                            actualDelimiter = SqlIdentifierDelimiter.DoubleQuotes;
                            valueStartPos = 1;
                            closingDelimiterChar = '"';
                        }
                        else
                        {
                            return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                        }
                    }
                    else if (c == '[')
                    {
                        if ((this.Delimiter & SqlIdentifierDelimiter.Brackets) != 0)
                        {
                            actualDelimiter = SqlIdentifierDelimiter.Brackets;
                            valueStartPos = 1;
                            closingDelimiterChar = ']';
                        }
                        else
                        {
                            return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                        }
                    }
                    else if (c == '`')
                    {
                        if ((this.Delimiter & SqlIdentifierDelimiter.BackQuotes) != 0)
                        {
                            actualDelimiter = SqlIdentifierDelimiter.BackQuotes;
                            valueStartPos = 1;
                            closingDelimiterChar = '`';
                        }
                        else
                        {
                            return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                        }
                    }
                    else if (c.IsLatinLetterInternal() || c == '_')
                    {
                        // got ascii identifier
                        if ((this.Delimiter & SqlIdentifierDelimiter.None) != 0)
                        {
                            actualDelimiter = SqlIdentifierDelimiter.None;
                        }
                        else
                        {
                            return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                        }
                    }
                    else
                    {
                        return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                    }

                    #endregion
                }
                else if (pos == 1)
                {
                    #region pos == 1

                    if (actualDelimiter == SqlIdentifierDelimiter.None)
                    {
                        // no delimiter
                        if (
                            c.IsLatinLetterInternal() ||
                            c == '_' ||
                            c.IsDecimalDigit() ||
                            false)
                        {
                            // ok
                        }
                        else
                        {
                            return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                        }
                    }
                    else
                    {
                        if (IsEnclosedIdentifierAcceptableStartingChar(c))
                        {
                            // ok.
                        }
                        else
                        {
                            return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                        }
                    }

                    #endregion
                }
                else
                {
                    #region pos > 1

                    if (actualDelimiter == SqlIdentifierDelimiter.None)
                    {
                        #region no delimiter

                        if (
                            c.IsLatinLetterInternal() ||
                            c == '_' ||
                            c.IsDecimalDigit() ||
                            false)
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


                        #endregion

                    }
                    else
                    {
                        #region have delimiter

                        if (this.IsEnclosedIdentifierAcceptableInnerChar(c))
                        {
                            // ok
                        }
                        else if (c == closingDelimiterChar)
                        {
                            // got identifier delimiter

                            // check prev char
                            if (this.IsEnclosedIdentifierAcceptableEndingChar(prevChar))
                            {
                                pos++; // advance position, skip ']' or other closing delimiter
                                if (pos == input.Length)
                                {
                                    break;
                                }
                                else
                                {
                                    // we need to meet a terminator here.
                                    if (this.Terminator(input, pos))
                                    {
                                        // ok, got terminator, let's get out of here
                                        break;
                                    }
                                    else
                                    {
                                        // failed to terminate extraction, the char was incorrect.
                                        return new TextDataExtractionResult(
                                            pos,
                                            TextDataExtractionErrorCodes.UnexpectedCharacter);
                                    }
                                }
                            }
                            else
                            {
                                // got delimiter, but it is not wanted here.
                                return new TextDataExtractionResult(
                                    pos,
                                    TextDataExtractionErrorCodes.UnexpectedCharacter);
                            }
                        }
                        else
                        {
                            return new TextDataExtractionResult(
                                pos,
                                TextDataExtractionErrorCodes.UnexpectedCharacter);
                        }

                        #endregion
                    }

                    #endregion
                }

                prevChar = c;
                pos++;

                this.CheckConsumption(pos); // todo_deferred ut
            }

            string sqlIdentifierValue;
            if (valueStartPos == 0)
            {
                sqlIdentifierValue = input[..pos].ToString();
            }
            else
            {
                if (pos < 3) // [x] is the minimal delimited identifier
                {
                    // should not happen, actually.
                    return new TextDataExtractionResult(pos, TextDataExtractionErrorCodes.UnexpectedCharacter);
                }
                else
                {
                    sqlIdentifierValue = input[1..(pos - 1)].ToString();
                }
            }

            value = new SqlIdentifier(sqlIdentifierValue, actualDelimiter);
            return new TextDataExtractionResult(pos, null);
        }

        protected virtual bool IsEnclosedIdentifierAcceptableStartingChar(char c)
        {
            var result =
                c == '_' ||
                char.IsLetter(c) ||
                false;

            return result;
        }

        protected virtual bool IsEnclosedIdentifierAcceptableInnerChar(char c)
        {
            var result =
                c == '_' ||
                c == ' ' ||
                char.IsLetter(c) ||
                false;

            return result;
        }

        protected virtual bool IsEnclosedIdentifierAcceptableEndingChar(char? c)
        {
            if (c == null)
            {
                return false;
            }

            var result =
                c == '_' ||
                char.IsLetter(c.Value) ||
                c.Value.IsDecimalDigit() ||
                false;

            return result;
        }
    }
}
