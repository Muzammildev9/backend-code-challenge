using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeChallenge.Api.Controllers;
using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CodeChallenge.Tests.Controllers;

public class MessagesControllerTests
{
    private readonly Mock<IMessageLogic> _logicMock;
    private readonly MessagesController _controller;
    private readonly Guid _orgId = Guid.NewGuid();

    public MessagesControllerTests()
    {
        _logicMock = new Mock<IMessageLogic>();
        _controller = new MessagesController(_logicMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithMessages()
    {
        var list = new List<Message> { new Message { Id = Guid.NewGuid(), OrganizationId = _orgId } };

        _logicMock.Setup(l => l.GetAllMessagesAsync(_orgId)).ReturnsAsync(list);

        var result = await _controller.GetAll(_orgId);

        result.Should().BeOfType<OkObjectResult>();
        var ok = result as OkObjectResult;
        ok!.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        var id = Guid.NewGuid();
        _logicMock.Setup(l => l.GetMessageAsync(_orgId, id)).ReturnsAsync((Message?)null);

        var result = await _controller.GetById(_orgId, id);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenFound()
    {
        var id = Guid.NewGuid();
        var msg = new Message { Id = id, OrganizationId = _orgId, Title = "T", Content = "C" };

        _logicMock.Setup(l => l.GetMessageAsync(_orgId, id)).ReturnsAsync(msg);

        var result = await _controller.GetById(_orgId, id);

        result.Should().BeOfType<OkObjectResult>();
        var ok = result as OkObjectResult;
        ok!.Value.Should().BeEquivalentTo(msg);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenSuccess()
    {
        var req = new CreateMessageRequest { Title = "T", Content = new string('x', 20) };
        var created = new Message { Id = Guid.NewGuid(), OrganizationId = _orgId, Title = req.Title };

        _logicMock.Setup(l => l.CreateMessageAsync(_orgId, req))
                  .ReturnsAsync(new Created<Message>(created));

        var result = await _controller.Create(_orgId, req);

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenUpdated()
    {
        var id = Guid.NewGuid();
        var req = new UpdateMessageRequest { Title = "New", Content = new string('x', 20), IsActive = true };

        _logicMock.Setup(l => l.UpdateMessageAsync(_orgId, id, req))
                  .ReturnsAsync(new Updated());

        var result = await _controller.Update(_orgId, id, req);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleted()
    {
        var id = Guid.NewGuid();

        _logicMock.Setup(l => l.DeleteMessageAsync(_orgId, id))
                  .ReturnsAsync(new Deleted());

        var result = await _controller.Delete(_orgId, id);

        result.Should().BeOfType<NoContentResult>();
    }
}
