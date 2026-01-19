using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VNVTStore.API.Controllers.v1;
using VNVTStore.Application.Common;
using VNVTStore.Application.Interfaces;
using Xunit;

namespace VNVTStore.Tests.Controllers.v1;

public class UploadControllerTests
{
    private readonly Mock<IImageUploadService> _mockUploadService;
    private readonly UploadController _controller;

    public UploadControllerTests()
    {
        _mockUploadService = new Mock<IImageUploadService>();
        _controller = new UploadController(_mockUploadService.Object);
    }

    [Fact]
    public async Task Upload_ReturnsBadRequest_WhenFileIsNull()
    {
        // Act
        var result = await _controller.Upload(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No file uploaded.", badRequestResult.Value);
    }

    [Fact]
    public async Task Upload_ReturnsBadRequest_WhenFileIsEmpty()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(0);

        // Act
        var result = await _controller.Upload(mockFile.Object);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No file uploaded.", badRequestResult.Value);
    }

    [Fact]
    public async Task Upload_ReturnsOk_WhenServiceSucceeds()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var content = "fake image content";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;

        mockFile.Setup(f => f.Length).Returns(stream.Length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        mockFile.Setup(f => f.FileName).Returns("test.jpg");

        _mockUploadService.Setup(s => s.UploadImageAsync(It.IsAny<Stream>(), "test.jpg", It.IsAny<string>()))
            .ReturnsAsync(Result.Success("uploads/test.jpg"));

        // Act
        var result = await _controller.Upload(mockFile.Object);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value;
        Assert.NotNull(value);
        
        var urlProperty = value!.GetType().GetProperty("url");
        Assert.NotNull(urlProperty);
        var urlValue = urlProperty!.GetValue(value) as string;
        Assert.Equal("uploads/test.jpg", urlValue);
    }

    [Fact]
    public async Task Upload_ReturnsBadRequest_WhenServiceFails()
    {
         // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(100);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
        mockFile.Setup(f => f.FileName).Returns("test.jpg");

        _mockUploadService.Setup(s => s.UploadImageAsync(It.IsAny<Stream>(), "test.jpg", It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>(Error.Validation("Upload.Failed", "Failed to upload")));

        // Act
        var result = await _controller.Upload(mockFile.Object);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var error = badRequestResult.Value as Error;
        Assert.NotNull(error);
        Assert.Equal("Validation", error.Code);
    }
}
