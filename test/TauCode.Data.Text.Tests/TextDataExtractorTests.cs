using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using TauCode.Data.Text.Tests.Dto.TextDataExtractor.TryExtract;
using TauCode.Extensions;

namespace TauCode.Data.Text.Tests;

[TestFixture]
public class TextDataExtractorTests
{
    #region TryExtractInt32

    [Test]
    [TestCaseSource(nameof(GetInt32TestDtos))]
    public void TryExtractInt32_SomeArgument_ReturnsExpectedResult(Int32TestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminatingPredicate =
            testDto.TestTerminatingChars != null ? 
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        // Act
        var consumed = TextDataExtractor.TryExtractInt32(input, out var v, terminatingPredicate);

        // Assert
        if (testDto.ExpectedResult == 0)
        {
            Assert.That(consumed, Is.Zero);
            Assert.That(v, Is.EqualTo(default(int)));
        }
        else
        {
            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedInt32));
        }
    }

    public static IList<Int32TestDto> GetInt32TestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".Int32Tests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<Int32TestDto>>(json);

        for (var i = 0; i < dtos.Count; i++)
        {
            dtos[i].Index = i;
        }

        return dtos;
    }


    #endregion

    #region TryExtractInt64

    [Test]
    [TestCaseSource(nameof(GetInt64TestDtos))]
    public void TryExtractInt64_SomeArgument_ReturnsExpectedResult(Int64TestDto testDto)
    {
        // Arrange

        var input = testDto.TestInput;
        TerminatingDelegate terminatingPredicate =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        // Act
        var consumed = TextDataExtractor.TryExtractInt64(input, out var v, terminatingPredicate);

        // Assert
        if (testDto.ExpectedResult == 0)
        {
            Assert.That(consumed, Is.Zero);
            Assert.That(v, Is.EqualTo(default(long)));
        }
        else
        {
            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedInt64));
        }
    }

    public static IList<Int64TestDto> GetInt64TestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".Int64Tests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<Int64TestDto>>(json);

        for (var i = 0; i < dtos.Count; i++)
        {
            dtos[i].Index = i;
        }

        return dtos;
    }

    #endregion

    #region TryExtractDouble

    [Test]
    [TestCaseSource(nameof(GetDoubleTestDtos))]
    public void TryExtractDouble_SomeArgument_ReturnsExpectedResult(DoubleTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminatingPredicate =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        // Act

        var consumed = TextDataExtractor.TryExtractDouble(input, out var v, terminatingPredicate);

        // Assert
        if (testDto.ExpectedResult == 0)
        {
            Assert.That(consumed, Is.Zero);
            Assert.That(v, Is.EqualTo(default(double)));
        }
        else
        {
            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedDouble));
        }
    }

    public static IList<DoubleTestDto> GetDoubleTestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".DoubleTests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<DoubleTestDto>>(json);

        for (var i = 0; i < dtos.Count; i++)
        {
            dtos[i].Index = i;
        }

        return dtos;
    }

    #endregion

    #region TryExtractDecimal

    [Test]
    [TestCaseSource(nameof(GetDecimalTestDtos))]
    public void TryExtractDecimal_SomeArgument_ReturnsExpectedResult(DecimalTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminatingPredicate =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        // Act

        var consumed = TextDataExtractor.TryExtractDecimal(input, out var v, terminatingPredicate);

        // Assert
        if (testDto.ExpectedResult == 0)
        {
            Assert.That(consumed, Is.Zero);
            Assert.That(v, Is.EqualTo(default(decimal)));
        }
        else
        {
            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedDecimal));
        }
    }

    public static IList<DecimalTestDto> GetDecimalTestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".DecimalTests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<DecimalTestDto>>(json);

        for (var i = 0; i < dtos.Count; i++)
        {
            dtos[i].Index = i;
        }

        return dtos;
    }

    #endregion

    #region TryExtractBoolean

    [Test]
    [TestCaseSource(nameof(GetBooleanTestDtos))]
    public void TryExtractBoolean_SomeArgument_ReturnsExpectedResult(BooleanTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminatingPredicate =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        // Act

        var consumed = TextDataExtractor.TryExtractBoolean(input, out var v, terminatingPredicate);

        // Assert
        if (testDto.ExpectedResult == 0)
        {
            Assert.That(consumed, Is.Zero);
            Assert.That(v, Is.EqualTo(default(bool)));
        }
        else
        {
            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedBoolean));
        }
    }

    public static IList<BooleanTestDto> GetBooleanTestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".BooleanTests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<BooleanTestDto>>(json);

        for (var i = 0; i < dtos.Count; i++)
        {
            dtos[i].Index = i;
        }

        return dtos;
    }

    #endregion

    #region TryExtractTimeSpan

    [Test]
    [TestCaseSource(nameof(GetTimeSpanTestDtos))]
    public void TryExtractTimeSpan_SomeArgument_ReturnsExpectedResult(TimeSpanTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminatingPredicate =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        // Act

        var consumed = TextDataExtractor.TryExtractTimeSpan(input, out var v, terminatingPredicate);

        // Assert
        if (testDto.ExpectedResult == 0)
        {
            Assert.That(consumed, Is.Zero);
            Assert.That(v, Is.EqualTo(default(TimeSpan)));
        }
        else
        {
            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedTimeSpan));
        }
    }

    public static IList<TimeSpanTestDto> GetTimeSpanTestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".TimeSpanTests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<TimeSpanTestDto>>(json);

        for (var i = 0; i < dtos.Count; i++)
        {
            dtos[i].Index = i;
        }

        return dtos;
    }

    #endregion

    #region TryExtractDateTimeOffset

    [Test]
    [TestCaseSource(nameof(GetDateTimeOffsetTestDtos))]
    public void TryExtractDateTimeOffset_SomeArgument_ReturnsExpectedResult(DateTimeOffsetTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminatingPredicate =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        // Act

        var consumed = TextDataExtractor.TryExtractDateTimeOffset(input, out var v, terminatingPredicate);

        // Assert
        if (testDto.ExpectedResult == 0)
        {
            Assert.That(consumed, Is.Zero);
            Assert.That(v, Is.EqualTo(default(DateTimeOffset)));
        }
        else
        {
            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedDateTimeOffset));
        }
    }

    public static IList<DateTimeOffsetTestDto> GetDateTimeOffsetTestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".DateTimeOffsetTests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<DateTimeOffsetTestDto>>(json);

        for (var i = 0; i < dtos.Count; i++)
        {
            dtos[i].Index = i;
        }

        return dtos;
    }

    #endregion

    #region TryExtractUri

    [Test]
    [TestCaseSource(nameof(GetUriTestDtos))]
    public void TryExtractUri_SomeArgument_ReturnsExpectedResult(UriTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminatingPredicate =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        // Act

        var consumed = TextDataExtractor.TryExtractUri(input, out var v, terminatingPredicate);

        // Assert
        if (testDto.ExpectedResult == 0)
        {
            Assert.That(consumed, Is.Zero);
            Assert.That(v, Is.EqualTo(default(Uri)));
        }
        else
        {
            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedUri));
        }
    }

    public static IList<UriTestDto> GetUriTestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".UriTests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<UriTestDto>>(json);

        for (var i = 0; i < dtos.Count; i++)
        {
            dtos[i].Index = i;
        }

        return dtos;
    }

    #endregion

    #region TryExtractFilePath

    [Test]
    [TestCaseSource(nameof(GetFilePathTestDtos))]
    public void TryExtractFilePath_SomeArgument_ReturnsExpectedResult(FilePathTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminatingPredicate =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        // Act

        var consumed = TextDataExtractor.TryExtractFilePath(input, out var v, terminatingPredicate);

        // Assert
        if (testDto.ExpectedResult == 0)
        {
            Assert.That(consumed, Is.Zero);
            Assert.That(v, Is.EqualTo(default(string)));
        }
        else
        {
            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedFilePath));
        }
    }

    public static IList<FilePathTestDto> GetFilePathTestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".FilePathTests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<FilePathTestDto>>(json);

        for (var i = 0; i < dtos.Count; i++)
        {
            dtos[i].Index = i;
        }

        return dtos;
    }

    #endregion

    #region TryExtractJsonString

    [Test]
    [TestCaseSource(nameof(GetJsonStringTestDtos))]
    public void TryExtractJsonString_SomeArgument_ReturnsExpectedResult(JsonStringTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminatingPredicate =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        // Act

        var consumed = TextDataExtractor.TryExtractJsonString(
            input,
            out var v,
            out var exception,
            true,
            terminatingPredicate);

        // Assert
        if (testDto.ExpectedResult == 0)
        {
            Assert.That(consumed, Is.Zero);
            Assert.That(v, Is.EqualTo(default(string)));
            
        }
        else
        {
            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedJsonString));
        }
    }

    public static IList<JsonStringTestDto> GetJsonStringTestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".JsonStringTests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<JsonStringTestDto>>(json);

        for (var i = 0; i < dtos.Count; i++)
        {
            dtos[i].Index = i;
        }

        return dtos;
    }

    #endregion
}
