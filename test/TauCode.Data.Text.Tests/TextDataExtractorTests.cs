using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TauCode.Data.Text.Tests.Dto;
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
            Assert.That(testDto.ExpectedInt32, Is.Null);

            Assert.That(consumed, Is.Zero);
            Assert.That(v, Is.EqualTo(default(int)));
        }
        else
        {
            Assert.That(testDto.ExpectedInt32, Is.Not.Null);

            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedInt32));
        }
    }

    public static IList<Int32TestDto> GetInt32TestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".Int32Tests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<Int32TestDto>>(json);

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
            Assert.That(testDto.ExpectedInt64, Is.Null);

            Assert.That(consumed, Is.Zero);
            Assert.That(v, Is.EqualTo(default(long)));
        }
        else
        {
            Assert.That(testDto.ExpectedInt64, Is.Not.Null);

            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedInt64));
        }
    }

    public static IList<Int64TestDto> GetInt64TestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".Int64Tests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<Int64TestDto>>(json);

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
            Assert.That(testDto.ExpectedDouble, Is.Null);

            Assert.That(consumed, Is.Zero);
            Assert.That(v, Is.EqualTo(default(double)));
        }
        else
        {
            Assert.That(testDto.ExpectedDouble, Is.Not.Null);

            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedDouble));
        }
    }

    public static IList<DoubleTestDto> GetDoubleTestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".DoubleTests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<DoubleTestDto>>(json);

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
            Assert.That(testDto.ExpectedDecimal, Is.Null);

            Assert.That(consumed, Is.Zero);
            Assert.That(v, Is.EqualTo(default(decimal)));
        }
        else
        {
            Assert.That(testDto.ExpectedDecimal, Is.Not.Null);

            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedDecimal));
        }
    }

    public static IList<DecimalTestDto> GetDecimalTestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".DecimalTests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<DecimalTestDto>>(json);

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
            Assert.That(testDto.ExpectedBoolean, Is.Null);

            Assert.That(consumed, Is.Zero);
            Assert.That(v, Is.EqualTo(default(bool)));
        }
        else
        {
            Assert.That(testDto.ExpectedBoolean, Is.Not.Null);

            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedBoolean));
        }
    }

    public static IList<BooleanTestDto> GetBooleanTestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".BooleanTests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<BooleanTestDto>>(json);

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
            Assert.That(testDto.ExpectedTimeSpan, Is.Null);

            Assert.That(consumed, Is.Zero);
            Assert.That(v, Is.EqualTo(default(TimeSpan)));
        }
        else
        {
            Assert.That(testDto.ExpectedTimeSpan, Is.Not.Null);

            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedTimeSpan));
        }
    }

    public static IList<TimeSpanTestDto> GetTimeSpanTestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".TimeSpanTests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<TimeSpanTestDto>>(json);


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
            Assert.That(testDto.ExpectedDateTimeOffset, Is.Null);

            Assert.That(consumed, Is.Zero);
            Assert.That(v, Is.EqualTo(default(DateTimeOffset)));
        }
        else
        {
            Assert.That(testDto.ExpectedDateTimeOffset, Is.Not.Null);

            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedDateTimeOffset));
        }
    }

    public static IList<DateTimeOffsetTestDto> GetDateTimeOffsetTestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".DateTimeOffsetTests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<DateTimeOffsetTestDto>>(json);

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
            Assert.That(testDto.ExpectedUri, Is.Null);

            Assert.That(consumed, Is.Zero);
            Assert.That(v, Is.Null);
        }
        else
        {
            Assert.That(testDto.ExpectedUri, Is.Not.Null);

            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedUri));
        }
    }

    public static IList<UriTestDto> GetUriTestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".UriTests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<UriTestDto>>(json);

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
            Assert.That(testDto.ExpectedFilePath, Is.Null);

            Assert.That(consumed, Is.Zero);
            Assert.That(v, Is.EqualTo(default(string)));
        }
        else
        {
            Assert.That(testDto.ExpectedFilePath, Is.Not.Null);

            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedFilePath));
        }
    }

    public static IList<FilePathTestDto> GetFilePathTestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".FilePathTests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<FilePathTestDto>>(json);

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
            Assert.That(v, Is.Null);

            Assert.That(testDto.ExpectedJsonString, Is.Null);
            Assert.That(testDto.ExpectedException, Is.Not.Null);

            Assert.That(exception, Is.Not.Null);

            Assert.That(exception.Message, Is.EqualTo(testDto.ExpectedException.Message));
            Assert.That(exception.Index, Is.EqualTo(testDto.ExpectedException.Index));
        }
        else
        {
            Assert.That(testDto.ExpectedJsonString, Is.Not.Null);
            Assert.That(testDto.ExpectedException, Is.Null);

            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedJsonString));

            Assert.That(exception, Is.Null);
        }
    }

    public static IList<JsonStringTestDto> GetJsonStringTestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".JsonStringTests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<JsonStringTestDto>>(json);

        return dtos;
    }

    #endregion

    #region TryExtractKey

    [Test]
    [TestCaseSource(nameof(GetKeyTestDtos))]
    public void TryExtractKey_SomeArgument_ReturnsExpectedResult(KeyTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminatingPredicate =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        // Act

        var consumed = TextDataExtractor.TryExtractKey(input, out var v, terminatingPredicate);

        // Assert
        if (testDto.ExpectedResult == 0)
        {
            Assert.That(testDto.ExpectedKey, Is.Null);

            Assert.That(consumed, Is.Zero);
            Assert.That(v, Is.Null);
        }
        else
        {
            Assert.That(testDto.ExpectedKey, Is.Not.Null);

            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedKey));
        }
    }

    public static IList<KeyTestDto> GetKeyTestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".KeyTests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<KeyTestDto>>(json);

        foreach (var dto in dtos)
        {
            dto.TestInput = TestHelper.TransformTestString(dto.TestInput);
            if (dto.ExpectedKey != null)
            {
                dto.ExpectedKey = TestHelper.TransformTestString(dto.ExpectedKey);
            }
        }

        return dtos;
    }

    #endregion

    #region TryExtractTerm

    [Test]
    [TestCaseSource(nameof(GetTermTestDtos))]
    public void TryExtractTerm_SomeArgument_ReturnsExpectedResult(TermTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminatingPredicate =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        // Act

        var consumed = TextDataExtractor.TryExtractTerm(input, out var v, terminatingPredicate);

        // Assert
        if (testDto.ExpectedResult == 0)
        {
            Assert.That(testDto.ExpectedTerm, Is.Null);

            Assert.That(consumed, Is.Zero);
            Assert.That(v, Is.Null);
        }
        else
        {
            Assert.That(testDto.ExpectedTerm, Is.Not.Null);

            Assert.That(consumed, Is.EqualTo(testDto.ExpectedResult));
            Assert.That(v, Is.EqualTo(testDto.ExpectedTerm));
        }
    }

    public static IList<TermTestDto> GetTermTestDtos()
    {
        var json = typeof(TextDataExtractorTests).Assembly.GetResourceText(".TermTests.json", true);
        var dtos = JsonConvert.DeserializeObject<IList<TermTestDto>>(json);

        foreach (var dto in dtos)
        {
            dto.TestInput = TestHelper.TransformTestString(dto.TestInput);
            if (dto.ExpectedTerm != null)
            {
                dto.ExpectedTerm = TestHelper.TransformTestString(dto.ExpectedTerm);
            }
        }

        return dtos;
    }

    #endregion


    [Test]
    //[Ignore("only for dev")]

    [TestCase("BooleanTests.json", typeof(BooleanTestDto))]
    [TestCase("DateTimeOffsetTests.json", typeof(DateTimeOffsetTestDto))]
    [TestCase("DecimalTests.json", typeof(DecimalTestDto))]
    [TestCase("DoubleTests.json", typeof(DoubleTestDto))]
    [TestCase("FilePathTests.json", typeof(FilePathTestDto))]
    [TestCase("Int32Tests.json", typeof(Int32TestDto))]
    [TestCase("Int64Tests.json", typeof(Int64TestDto))]
    [TestCase("JsonStringTests.json", typeof(JsonStringTestDto))]
    [TestCase("KeyTests.json", typeof(KeyTestDto))]
    [TestCase("TermTests.json", typeof(TermTestDto))]
    [TestCase("TimeSpanTests.json", typeof(TimeSpanTestDto))]
    [TestCase("UriTests.json", typeof(UriTestDto))]

    [TestCase("SemanticVersionTests.Fail.json", typeof(SemanticVersionTestDto))]
    [TestCase("SemanticVersionTests.Success.json", typeof(SemanticVersionTestDto))]

    [TestCase("EmailAddressTests.Fail.ForParsing.json", typeof(EmailAddressTestDto))]
    [TestCase("EmailAddressTests.Success.json", typeof(EmailAddressTestDto))]

    [TestCase("HostNameTests.IPv6.Fail.json", typeof(HostNameTestDto))]
    [TestCase("HostNameTests.IPv6.Success.json", typeof(HostNameTestDto))]
    [TestCase("HostNameTests.json", typeof(HostNameTestDto))]

    public void CreateIndices(string resourceName, Type dtoType)
    {
        var json = this.GetType().Assembly.GetResourceText(resourceName, true);
        var listGeneric = typeof(List<>);
        var dtoListType = listGeneric.MakeGenericType(dtoType);
        var list = (IList)JsonConvert.DeserializeObject(json, dtoListType);

        for (var i = 0; i < list.Count; i++)
        {
            var item = list[i];
            dynamic dynamicItem = item;
            dynamicItem.Index = i;
        }

        var jsonResult = JsonConvert.SerializeObject(list, Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());

        var path = $@"c:\temp\dtos\{resourceName}";
        File.WriteAllText(path, jsonResult, Encoding.UTF8);
    }
}
