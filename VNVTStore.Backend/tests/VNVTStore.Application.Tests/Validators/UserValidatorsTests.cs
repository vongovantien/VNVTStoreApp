using FluentAssertions;
using FluentValidation.TestHelper;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Users.Validators;

namespace VNVTStore.Application.Tests.Validators;

public class UserValidatorsTests
{
    private readonly CreateUserDtoValidator _createValidator;

    public UserValidatorsTests()
    {
        _createValidator = new CreateUserDtoValidator();
    }

    [Fact]
    public void CreateUser_ShouldFail_WhenUsernameIsEmpty()
    {
        // Arrange
        var dto = new CreateUserDto 
        { 
            Username = "", 
            Email = "test@test.com",
            Password = "password123"
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void CreateUser_ShouldFail_WhenEmailIsInvalid()
    {
        // Arrange
        var dto = new CreateUserDto 
        { 
            Username = "testuser", 
            Email = "invalid-email",
            Password = "password123"
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void CreateUser_ShouldFail_WhenPasswordTooShort()
    {
        // Arrange
        var dto = new CreateUserDto 
        { 
            Username = "testuser", 
            Email = "test@test.com",
            Password = "12345"
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void CreateUser_ShouldPass_WhenValid()
    {
        // Arrange
        var dto = new CreateUserDto 
        { 
            Username = "testuser", 
            Email = "test@test.com",
            Password = "password123"
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateUser_ShouldFail_WhenUsernameContainsSpecialChars()
    {
        // Arrange
        var dto = new CreateUserDto 
        { 
            Username = "test@user!", 
            Email = "test@test.com",
            Password = "password123"
        };

        // Act
        var result = _createValidator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }
}
