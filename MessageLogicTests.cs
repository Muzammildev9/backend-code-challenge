using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace CodeChallenge.Tests.Logic;

public class MessageLogicTests
{
    private readonly Mock<IMessageRepository> _repoMock;
    private readonly MessageLogic _logic;
    private readonly Guid _orgId = Guid.NewGuid();

    public MessageLogicTests()
    {
        _repoMock = new Mock<IMessageRepository>();
        _logic = new MessageLogic(_repoMock.Object);
    }

    [Fact]
    public async Task CreateMessage_ShouldReturnCreated_WhenValid()
    {
        // Arrange
        var req = new CreateMessageRequest
        {
            Title = "Valid Title",
            Content = new string('x', 20)
        };

        _repoMock.Setup(r => r.GetByTitleAsync(_orgId, req.Title))
                 .ReturnsAsync((Message?)null);

        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Message>()))
                 .ReturnsAsync((Message m) => m);

        // Act
        var result = await _logic.CreateMessageAsync(_orgId, req);

        // Assert
        result.Should().BeOfType<Created<Message>>();
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<Message>()), Times.Once);
    }

    [Fact]
    public async Task CreateMessage_ShouldReturnConflict_WhenTitleExists()
    {
        // Arrange
        var req = new CreateMessageRequest { Title = "Dup", Content = new string('x', 20) };

        _repoMock.Setup(r => r.GetByTitleAsync(_orgId, req.Title))
                 .ReturnsAsync(new Message { Title = req.Title });

        // Act
        var result = await _logic.CreateMessageAsync(_orgId, req);

        // Assert
        result.Should().BeOfType<Conflict>();
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<Message>()), Times.Never);
    }

    [Fact]
    public async Task CreateMessage_ShouldReturnValidationError_WhenContentTooShort()
    {
        // Arrange
        var req = new CreateMessageRequest { Title = "OK Title", Content = "short" };

        // Act
        var result = await _logic.CreateMessageAsync(_orgId, req);

        // Assert
        result.Should().BeOfType<ValidationError>();
    }

    [Fact]
    public async Task GetMessage_ShouldReturnMessage_WhenExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var msg = new Message { Id = id, OrganizationId = _orgId, Title = "Hello", Content = "Content", IsActive = true };

        _repoMock.Setup(r => r.GetByIdAsync(_orgId, id))
                 .ReturnsAsync(msg);

        // Act
        var result = await _logic.GetMessageAsync(_orgId, id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetAllMessages_ShouldReturnList()
    {
        // Arrange
        var list = new List<Message>
        {
            new Message { Id = Guid.NewGuid(), OrganizationId = _orgId, Title = "A", Content = "C1" },
            new Message { Id = Guid.NewGuid(), OrganizationId = _orgId, Title = "B", Content = "C2" }
        };

        _repoMock.Setup(r => r.GetAllByOrganizationAsync(_orgId))
                 .ReturnsAsync(list);

        // Act
        var result = await _logic.GetAllMessagesAsync(_orgId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateMessage_ShouldReturnNotFound_WhenMissing()
    {
        // Arrange
        var id = Guid.NewGuid();
        var req = new UpdateMessageRequest { Title = "New", Content = new string('x', 20), IsActive = true };

        _repoMock.Setup(r => r.GetByIdAsync(_orgId, id))
                 .ReturnsAsync((Message?)null);

        // Act
        var result = await _logic.UpdateMessageAsync(_orgId, id, req);

        // Assert
        result.Should().BeOfType<NotFound>();
    }

    [Fact]
    public async Task UpdateMessage_ShouldReturnConflict_WhenInactive()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = new Message { Id = id, OrganizationId = _orgId, Title = "Old", Content = "Old", IsActive = false };

        _repoMock.Setup(r => r.GetByIdAsync(_orgId, id))
                 .ReturnsAsync(existing);

        var req = new UpdateMessageRequest { Title = "New", Content = new string('x', 20), IsActive = true };

        // Act
        var result = await _logic.UpdateMessageAsync(_orgId, id, req);

        // Assert
        result.Should().BeOfType<Conflict>();
    }

    [Fact]
    public async Task UpdateMessage_ShouldReturnUpdated_WhenSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = new Message { Id = id, OrganizationId = _orgId, Title = "Old", Content = "Old", IsActive = true };

        _repoMock.Setup(r => r.GetByIdAsync(_orgId, id)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.GetByTitleAsync(_orgId, It.IsAny<string>())).ReturnsAsync((Message?)null);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Message>())).ReturnsAsync((Message m) => m);

        var req = new UpdateMessageRequest { Title = "New Title", Content = new string('x', 20), IsActive = true };

        // Act
        var result = await _logic.UpdateMessageAsync(_orgId, id, req);

        // Assert
        result.Should().BeOfType<Updated>();
        _repoMock.Verify(r => r.UpdateAsync(It.Is<Message>(m => m.Id == id && m.Title == req.Title)), Times.Once);
    }

    [Fact]
    public async Task DeleteMessage_ShouldReturnNotFound_WhenMissing()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(_orgId, id)).ReturnsAsync((Message?)null);

        // Act
        var result = await _logic.DeleteMessageAsync(_orgId, id);

        // Assert
        result.Should().BeOfType<NotFound>();
    }

    [Fact]
    public async Task DeleteMessage_ShouldReturnConflict_WhenInactive()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = new Message { Id = id, OrganizationId = _orgId, IsActive = false };

        _repoMock.Setup(r => r.GetByIdAsync(_orgId, id)).ReturnsAsync(existing);

        // Act
        var result = await _logic.DeleteMessageAsync(_orgId, id);

        // Assert
        result.Should().BeOfType<Conflict>();
    }

    [Fact]
    public async Task DeleteMessage_ShouldReturnDeleted_WhenSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = new Message { Id = id, OrganizationId = _orgId, IsActive = true };

        _repoMock.Setup(r => r.GetByIdAsync(_orgId, id)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.DeleteAsync(_orgId, id)).ReturnsAsync(true);

        // Act
        var result = await _logic.DeleteMessageAsync(_orgId, id);

        // Assert
        result.Should().BeOfType<Deleted>();
        _repoMock.Verify(r => r.DeleteAsync(_orgId, id), Times.Once);
    }
}
