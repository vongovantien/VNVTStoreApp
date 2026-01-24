using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VNVTStore.API.Controllers.v1;
using VNVTStore.Application.Common;
using VNVTStore.Application.Interfaces;
using VNVTStore.Application.DTOs;
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
        var result = await _controller.Upload(null!);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Upload_ReturnsOk_WhenServiceSucceeds()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(100);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
        mockFile.Setup(f => f.FileName).Returns("test.jpg");

        _mockUploadService.Setup(s => s.UploadImageAsync(It.IsAny<Stream>(), "test.jpg", It.IsAny<string>()))
            .ReturnsAsync(Result.Success(new FileDto { Path = "url", Url = "url" }));

        var result = await _controller.Upload(mockFile.Object);
        Assert.IsType<OkObjectResult>(result);
    }
}
