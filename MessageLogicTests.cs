using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;
using CodeChallenge.Shared;
using FluentAssertions;
using Moq;

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

    // ----------------------- TEST 1 -----------------------
    [Fact]
    public async Task CreateMessage_Should_Create_When_Valid()
    {
        var request = new CreateMessageRequest
        {
            Title = "Hello",
            Content = "Sample content here..."
        };

        _repoMock.Setup(r => r.GetByTitleAsync(_orgId, request.Title))
                 .ReturnsAsync((Message?)null);

        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Message>()))
                 .ReturnsAsync(new Message { Id = Guid.NewGuid(), Title = request.Title });

        var result = await _logic.CreateMessageAsync(_orgId, request);

        result.Should().BeOfType<Created<Message>>();
    }

    // ----------------------- TEST 2 -----------------------
    [Fact]
    public async Task CreateMessage_Should_Return_Conflict_When_Title_Exists()
    {
        var request = new CreateMessageRequest
        {
            Title = "Duplicate",
            Content = "Content..."
        };

        _repoMock.Setup(r => r.GetByTitleAsync(_orgId, request.Title))
                 .ReturnsAsync(new Message());

        var result = await _logic.CreateMessageAsync(_orgId, request);

        result.Should().BeOfType<Conflict>();
    }

    // ----------------------- TEST 3 -----------------------
    [Fact]
    public async Task CreateMessage_Should_Return_ValidationError_When_Invalid_Content()
    {
        var request = new CreateMessageRequest
        {
            Title = "Hi",
            Content = "short"
        };

        var result = await _logic.CreateMessageAsync(_orgId, request);

        result.Should().BeOfType<ValidationError>();
    }

    // ----------------------- TEST 4 -----------------------
    [Fact]
    public async Task UpdateMessage_Should_Return_NotFound_When_Message_Does_Not_Exist()
    {
        var request = new UpdateMessageRequest
        {
            Title = "Updated",
            Content = "Updated content",
            IsActive = true
        };

        _repoMock.Setup(r => r.GetByIdAsync(_orgId, It.IsAny<Guid>()))
                 .ReturnsAsync((Message?)null);

        var result = await _logic.UpdateMessageAsync(_orgId, Guid.NewGuid(), request);

        result.Should().BeOfType<NotFound>();
    }

    // ----------------------- TEST 5 -----------------------
    [Fact]
    public async Task UpdateMessage_Should_Return_Conflict_When_Message_Inactive()
    {
        var existing = new Message
        {
            Id = Guid.NewGuid(),
            Title = "Old",
            Content = "Old content",
            IsActive = false
        };

        var request = new UpdateMessageRequest
        {
            Title = "Updated",
            Content = "Updated content",
            IsActive = true
        };

        _repoMock.Setup(r => r.GetByIdAsync(_orgId, existing.Id))
                 .ReturnsAsync(existing);

        var result = await _logic.UpdateMessageAsync(_orgId, existing.Id, request);

        result.Should().BeOfType<Conflict>();
    }

    // ----------------------- TEST 6 -----------------------
    [Fact]
    public async Task DeleteMessage_Should_Return_NotFound_When_Does_Not_Exist()
    {
        _repoMock.Setup(r => r.GetByIdAsync(_orgId, It.IsAny<Guid>()))
                 .ReturnsAsync((Message?)null);

        var result = await _logic.DeleteMessageAsync(_orgId, Guid.NewGuid());

        result.Should().BeOfType<NotFound>();
    }
}
