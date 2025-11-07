using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TodoList.Api.Controllers;
using TodoList.Api.Data.Interfaces;
using TodoList.Api.Dtos;
using TodoList.Api.Models;

namespace TodoList.Api.Tests.Controllers;

public class TasksControllerXUnitTests
{
    private readonly Mock<ITaskRepository> _mockRepository;
    private readonly Mock<ILogger<TasksController>> _mockLogger;
    private readonly Mock<IValidator<TaskItemCreateDto>> _mockCreateValidator;
    private readonly Mock<IValidator<TaskItemUpdateDto>> _mockUpdateValidator;
    private readonly TasksController _controller;

    public TasksControllerXUnitTests()
    {
        _mockRepository = new Mock<ITaskRepository>();
        _mockLogger = new Mock<ILogger<TasksController>>();
        _mockCreateValidator = new Mock<IValidator<TaskItemCreateDto>>();
        _mockUpdateValidator = new Mock<IValidator<TaskItemUpdateDto>>();

        _controller = new TasksController(
            _mockRepository.Object,
            _mockLogger.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object);

        // Setup HttpContext for RequestAborted token
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    #region GetAllTasks Tests

    [Fact]
    public async Task GetAllTasks_ReturnsOk_WithPaginatedResponse()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new() { Id = 1, Title = "Task 1", Description = "Desc 1", CreateDate = DateTime.Now, IsCompleted = false },
            new() { Id = 2, Title = "Task 2", Description = "Desc 2", CreateDate = DateTime.Now, IsCompleted = true }
        };

        _mockRepository.Setup(repo => repo.GetAllTasksAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((tasks, 2));

        // Act
        var result = await _controller.GetAllTasks(1, 20);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PaginatedTaskResponse>(okResult.Value);
        Assert.Equal(2, response.TotalCount);
        Assert.Equal(2, response.Tasks.Count);
        Assert.Equal(1, response.PageNumber);
        Assert.Equal(20, response.PageSize);
    }

    [Fact]
    public async Task GetAllTasks_ReturnsBadRequest_WhenPageNumberIsZero()
    {
        // Act
        var result = await _controller.GetAllTasks(0, 20);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task GetAllTasks_ReturnsBadRequest_WhenPageSizeIsInvalid()
    {
        // Act - Test too small
        var resultSmall = await _controller.GetAllTasks(1, 0);
        Assert.IsType<BadRequestObjectResult>(resultSmall.Result);

        // Act - Test too large
        var resultLarge = await _controller.GetAllTasks(1, 101);
        Assert.IsType<BadRequestObjectResult>(resultLarge.Result);
    }

    [Fact]
    public async Task GetAllTasks_ReturnsInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.GetAllTasksAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAllTasks(1, 20);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetAllTasks_PassesCompletedFilter_ToRepository()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.GetAllTasksAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<TaskItem>(), 0));

        // Act
        await _controller.GetAllTasks(1, 20, true);

        // Assert
        _mockRepository.Verify(repo => repo.GetAllTasksAsync(
            1, 20, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetTaskById Tests

    [Fact]
    public async Task GetTaskById_ReturnsOk_WhenTaskExists()
    {
        // Arrange
        var task = new TaskItem
        {
            Id = 1,
            Title = "Test Task",
            Description = "Test Description",
            CreateDate = DateTime.Now,
            IsCompleted = false
        };

        _mockRepository.Setup(repo => repo.GetTaskByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        // Act
        var result = await _controller.GetTaskById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTask = Assert.IsType<TaskItem>(okResult.Value);
        Assert.Equal(1, returnedTask.Id);
        Assert.Equal("Test Task", returnedTask.Title);
    }

    [Fact]
    public async Task GetTaskById_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.GetTaskByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _controller.GetTaskById(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetTaskById_ReturnsInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.GetTaskByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetTaskById(1);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    #endregion

    #region CreateTask Tests

    [Fact]
    public async Task CreateTask_ReturnsCreatedAtAction_WhenValidRequest()
    {
        // Arrange
        var request = new TaskItemCreateDto
        {
            Title = "New Task",
            Description = "New Description"
        };

        var createdTask = new TaskItem
        {
            Id = 1,
            Title = "New Task",
            Description = "New Description",
            CreateDate = DateTime.Now,
            IsCompleted = false
        };

        _mockCreateValidator.Setup(v => v.ValidateAsync(It.IsAny<TaskItemCreateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository.Setup(repo => repo.CreateTaskAsync(
            It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdTask);

        // Act
        var result = await _controller.CreateTask(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<TaskResponse>(createdResult.Value);
        Assert.Equal("Task created successfully", response.Message);
        Assert.NotNull(response.Task);
        Assert.Equal(1, response.Task.Id);
    }

    [Fact]
    public async Task CreateTask_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var request = new TaskItemCreateDto { Title = "ab" }; // Too short

        var validationFailures = new List<ValidationFailure>
        {
            new("Title", "Title must be between 3 and 100 characters")
        };

        _mockCreateValidator.Setup(v => v.ValidateAsync(It.IsAny<TaskItemCreateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var result = await _controller.CreateTask(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateTask_ReturnsBadRequest_WhenRepositoryThrowsArgumentException()
    {
        // Arrange
        var request = new TaskItemCreateDto
        {
            Title = "Valid Title",
            Description = "Description"
        };

        _mockCreateValidator.Setup(v => v.ValidateAsync(It.IsAny<TaskItemCreateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository.Setup(repo => repo.CreateTaskAsync(
            It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Validation error from database"));

        // Act
        var result = await _controller.CreateTask(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateTask_TrimsWhitespace_FromTitleAndDescription()
    {
        // Arrange
        var request = new TaskItemCreateDto
        {
            Title = "  Trimmed Title  ",
            Description = "  Trimmed Description  "
        };

        var createdTask = new TaskItem
        {
            Id = 1,
            Title = "Trimmed Title",
            Description = "Trimmed Description",
            CreateDate = DateTime.Now,
            IsCompleted = false
        };

        _mockCreateValidator.Setup(v => v.ValidateAsync(It.IsAny<TaskItemCreateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository.Setup(repo => repo.CreateTaskAsync(
            "Trimmed Title", "Trimmed Description", It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdTask);

        // Act
        var result = await _controller.CreateTask(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        _mockRepository.Verify(repo => repo.CreateTaskAsync(
            "Trimmed Title", "Trimmed Description", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region UpdateTask Tests

    [Fact]
    public async Task UpdateTask_ReturnsOk_WhenTaskUpdatedSuccessfully()
    {
        // Arrange
        var request = new TaskItemUpdateDto
        {
            Title = "Updated Task",
            Description = "Updated Description",
            IsCompleted = true
        };

        var updatedTask = new TaskItem
        {
            Id = 1,
            Title = "Updated Task",
            Description = "Updated Description",
            CreateDate = DateTime.Now,
            IsCompleted = true
        };

        _mockUpdateValidator.Setup(v => v.ValidateAsync(It.IsAny<TaskItemUpdateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository.Setup(repo => repo.UpdateTaskAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedTask);

        // Act
        var result = await _controller.UpdateTask(1, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<MessageResponse>(okResult.Value);
        Assert.Equal("Task updated successfully", response.Message);
    }

    [Fact]
    public async Task UpdateTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var request = new TaskItemUpdateDto
        {
            Title = "Updated Task",
            IsCompleted = false
        };

        _mockUpdateValidator.Setup(v => v.ValidateAsync(It.IsAny<TaskItemUpdateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository.Setup(repo => repo.UpdateTaskAsync(
            999, It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _controller.UpdateTask(999, request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateTask_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var request = new TaskItemUpdateDto { Title = "X", IsCompleted = false };

        var validationFailures = new List<ValidationFailure>
        {
            new("Title", "Title must be between 3 and 100 characters")
        };

        _mockUpdateValidator.Setup(v => v.ValidateAsync(It.IsAny<TaskItemUpdateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var result = await _controller.UpdateTask(1, request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    #endregion

    #region DeleteTask Tests

    [Fact]
    public async Task DeleteTask_ReturnsOk_WhenTaskDeletedSuccessfully()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.DeleteTaskAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteTask(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<MessageResponse>(okResult.Value);
        Assert.Equal("Task removed successfully", response.Message);
    }

    [Fact]
    public async Task DeleteTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.DeleteTaskAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteTask(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task DeleteTask_ReturnsInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.DeleteTaskAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.DeleteTask(1);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    #endregion

    #region CancellationToken Tests

    [Fact]
    public async Task GetAllTasks_PassesCancellationToken_ToRepository()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        _controller.ControllerContext.HttpContext = new DefaultHttpContext
        {
            RequestAborted = cts.Token
        };

        _mockRepository.Setup(repo => repo.GetAllTasksAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<TaskItem>(), 0));

        // Act
        await _controller.GetAllTasks(1, 20);

        // Assert
        _mockRepository.Verify(repo => repo.GetAllTasksAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(), cts.Token), Times.Once);
    }

    [Fact]
    public async Task CreateTask_PassesCancellationToken_ToRepository()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        _controller.ControllerContext.HttpContext = new DefaultHttpContext
        {
            RequestAborted = cts.Token
        };

        var request = new TaskItemCreateDto { Title = "Test", Description = "Test" };
        var createdTask = new TaskItem { Id = 1, Title = "Test", CreateDate = DateTime.Now };

        _mockCreateValidator.Setup(v => v.ValidateAsync(It.IsAny<TaskItemCreateDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockRepository.Setup(repo => repo.CreateTaskAsync(
            It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdTask);

        // Act
        await _controller.CreateTask(request);

        // Assert
        _mockRepository.Verify(repo => repo.CreateTaskAsync(
            It.IsAny<string>(), It.IsAny<string?>(), cts.Token), Times.Once);
    }

    #endregion
}