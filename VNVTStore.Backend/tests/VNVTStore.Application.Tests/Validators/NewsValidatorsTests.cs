using FluentAssertions;
using FluentValidation.TestHelper;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.News.Validators;

namespace VNVTStore.Application.Tests.Validators;

public class NewsValidatorsTests
{
    private readonly CreateNewsDtoValidator _createValidator;

    public NewsValidatorsTests()
    {
        _createValidator = new CreateNewsDtoValidator();
    }

    [Fact]
    public void CreateNews_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var dto = new CreateNewsDto { Title = "", Content = "Some content" };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void CreateNews_ShouldFail_WhenContentIsEmpty()
    {
        // Arrange
        var dto = new CreateNewsDto { Title = "Test Title", Content = "" };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void CreateNews_ShouldPass_WhenValid()
    {
        // Arrange
        var dto = new CreateNewsDto 
        { 
            Title = "Test Title", 
            Content = "Test content here"
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateNews_ShouldFail_WhenSlugContainsInvalidCharacters()
    {
        // Arrange
        var dto = new CreateNewsDto 
        { 
            Title = "Test Title", 
            Content = "Content",
            Slug = "Invalid Slug With Spaces"
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void CreateNews_ShouldPass_WhenSlugIsValid()
    {
        // Arrange
        var dto = new CreateNewsDto 
        { 
            Title = "Test Title", 
            Content = "Content",
            Slug = "valid-slug-123"
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Slug);
    }
}
