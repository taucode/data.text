using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TauCode.Data.Text.TextDataExtractors;
using TauCode.Extensions;

namespace TauCode.Data.Text.Tests.TextDataExtractor.StringItem;

[TestFixture]
public class StringItemExtractorTests
{
    [Test]
    [TestCaseSource(nameof(GetBadHashSets))]
    public void Ctor_HashSetIsNull_ThrowsArgumentException(HashSet<string> items)
    {
        // Arrange

        // Act
        var ex = Assert.Throws<ArgumentNullException>(() => new StringItemExtractor(null, false));

        // Assert
        Assert.That(ex.ParamName, Is.EqualTo("items"));
    }

    [Test]
    [TestCaseSource(nameof(GetBadHashSets))]
    public void Ctor_BadHashSet_ThrowsArgumentException(HashSet<string> items)
    {
        // Arrange

        // Act
        var ex = Assert.Throws<ArgumentException>(() => new StringItemExtractor(items, false));

        // Assert
        Assert.That(ex, Has.Message.StartWith("'items' cannot be empty or contain empty items."));
        Assert.That(ex.ParamName, Is.EqualTo("items"));
    }

    [Test]
    [TestCaseSource(nameof(GetTestDtos))]
    public void TryExtract_SomeArgument_ReturnsExpectedResult(StringItemExtractorTestDto testDto)
    {
        // Arrange
        var input = testDto.TestInput;
        TerminatingDelegate terminatingPredicate =
            testDto.TestTerminatingChars != null ?
                (span, position) => span[position].IsIn(testDto.TestTerminatingChars.ToArray())
                :
                null;

        var extractor = new StringItemExtractor(
            testDto.TestItems.ToHashSet(),
            testDto.TestIgnoreCase,
            terminator: terminatingPredicate);

        // Act
        var result = extractor.TryExtract(input, out var value);
        string errorMessage = null;
        if (result.ErrorCode.HasValue)
        {
            errorMessage = extractor.GetErrorMessage(result.ErrorCode.Value);
        }

        // Assert
        if (result.ErrorCode.HasValue)
        {
            // check test dto
            Assert.That(testDto.ExpectedStringItem, Is.EqualTo(default(string)));
            Assert.That(testDto.ExpectedErrorMessage, Is.Not.Null);
            Assert.That(testDto.ExpectedErrorCode, Is.Not.Null);

            // check test itself
            Assert.That(result.CharsConsumed, Is.EqualTo(testDto.ExpectedConsumed));
            Assert.That(value, Is.EqualTo(default(string)));
            Assert.That(result.ErrorCode.Value, Is.EqualTo(testDto.ExpectedErrorCode));
            Assert.That(errorMessage, Is.EqualTo(testDto.ExpectedErrorMessage));
        }
        else
        {
            // check test dto
            Assert.That(testDto.ExpectedErrorMessage, Is.Null);
            Assert.That(testDto.ExpectedErrorCode, Is.Null);

            Assert.That(result.CharsConsumed, Is.EqualTo(testDto.ExpectedConsumed));
            Assert.That(value, Is.EqualTo(testDto.ExpectedStringItem));
        }
    }

    public static IList<StringItemExtractorTestDto> GetTestDtos()
    {
        var json = typeof(StringItemExtractorTests).Assembly.GetResourceText(
            $".{nameof(StringItemExtractorTests)}.json",
            true);

        var dtos = JsonConvert.DeserializeObject<IList<StringItemExtractorTestDto>>(json);

        foreach (var dto in dtos)
        {
            dto.TestInput = TestHelper.TransformTestString(dto.TestInput);
            if (dto.ExpectedStringItem != null)
            {
                dto.ExpectedStringItem = TestHelper.TransformTestString(dto.ExpectedStringItem);
            }
        }

        return dtos;
    }

    public static IList<HashSet<string>> GetBadHashSets()
    {
        return new List<HashSet<string>>
        {
            new(),
            new(new []{ "one", "" }),
            new(new []{ "one", null }),
        };
    }
}
