using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TodoList.Api.Controllers;
using TodoList.Api.Data.Interfaces;
using TodoList.Api.Dtos;
using TodoList.Api.Models;

namespace TodoList.Api.Tests.Controllers;

public class TasksControllerTests
{
    private readonly Mock<ITaskRepository> _mockRepository;
    private readonly Mock<ILogger<TasksController>> _mockLogger;
    private readonly Mock<IValidator<TaskItemCreateDto>> _mockCreateValidator;
    private readonly Mock<IValidator<TaskItemUpdateDto>> _mockUpdateValidator;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _mockRepository = new Mock<ITaskRepository>();
        _mockLogger = new Mock<ILogger<TasksController>>();
        _mockCreateValidator = new Mock<IValidator<TaskItemCreateDto>>();
        _mockUpdateValidator = new Mock<IValidator<TaskItemUpdateDto>>();

        _controller = new TasksController(
            _mockRepository.Object,
            _mockLogger.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object)
        {
            // Setup HttpContext for RequestAborted token
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task GetAllTasks_ReturnsOk_WithPaginatedResponse()
    {
        // This test verifies that GetAllTasks returns an Ok result with the correct paginated response
        var tasks = new List<TaskItem>
        {
            new() { Id = 1, Title = "Task 1", Description = "Desc 1", CreateDate = DateTime.Now, IsCompleted = false },
            new() { Id = 2, Title = "Task 2", Description = "Desc 2", CreateDate = DateTime.Now, IsCompleted = true }
        };

        // The repository mock returns a tuple of the task list and total count
        _mockRepository.Setup(repo => repo.GetAllTasksAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((tasks, 2));

        // This test calls the GetAllTasks method
        var result = await _controller.GetAllTasks(1, 20);

        // This test asserts that the result is an OkObjectResult containing the expected paginated response
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        //This test asserts that the response has the correct structure and values
        var response = Assert.IsType<PaginatedTaskResponse>(okResult.Value);
        //This test asserts that the response contains the expected data
        Assert.Equal(2, response.TotalCount);
        Assert.Equal(2, response.Tasks.Count);
        Assert.Equal(1, response.PageNumber);
        Assert.Equal(20, response.PageSize);
    }
}