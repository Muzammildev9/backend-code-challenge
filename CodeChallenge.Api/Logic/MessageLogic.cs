using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;

namespace CodeChallenge.Api.Logic;

public class MessageLogic : IMessageLogic
{
    private readonly IMessageRepository _repository;

    public MessageLogic(IMessageRepository repository)
    {
        _repository = repository;
    }

    // ------------------------------------------------------
    // VALIDATION
    // ------------------------------------------------------

    private Dictionary<string, string[]> ValidateRequest(string title, string content)
{
    var errors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(title) || title.Length < 3 || title.Length > 200)
        errors["Title"] = new[] { "Title must be between 3 and 200 characters." };

    if (string.IsNullOrWhiteSpace(content) || content.Length < 10 || content.Length > 1000)
        errors["Content"] = new[] { "Content must be between 10 and 1000 characters." };

    return errors;
}

//Both Create and Update now use

var validation = ValidateRequest(request.Title, request.Content);
if (validation.Any()) return new ValidationError(validation);

    // ------------------------------------------------------
    // CREATE
    // ------------------------------------------------------

    public async Task<Result> CreateMessageAsync(Guid organizationId, CreateMessageRequest request)
    {
        var validation = ValidateCreate(request);

        if (validation.Any())
            return new ValidationError(validation);

        // Unique title rule
        var existing = await _repository.GetByTitleAsync(organizationId, request.Title);
        if (existing != null)
            return new Conflict("A message with this title already exists.");

        var message = new Message
        {
            OrganizationId = organizationId,
            Title = request.Title,
            Content = request.Content,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(message);
        return new Created<Message>(created);
    }

    // ------------------------------------------------------
    // UPDATE
    // ------------------------------------------------------

    public async Task<Result> UpdateMessageAsync(Guid organizationId, Guid id, UpdateMessageRequest request)
    {
        var validation = ValidateUpdate(request);
        if (validation.Any())
            return new ValidationError(validation);

        var message = await _repository.GetByIdAsync(organizationId, id);

        if (message == null)
            return new NotFound("Message not found.");

        if (!message.IsActive)
            return new Conflict("Inactive messages cannot be updated.");

        // Title unique rule
        if (!message.Title.Equals(request.Title, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _repository.GetByTitleAsync(organizationId, request.Title);
            if (exists != null)
                return new Conflict("A message with this title already exists.");
        }

        message.Title = request.Title;
        message.Content = request.Content;
        message.IsActive = request.IsActive;

        var updated = await _repository.UpdateAsync(message);
        if (updated == null)
            return new NotFound("Unable to update message.");

        return new Updated();
    }

    // ------------------------------------------------------
    // DELETE
    // ------------------------------------------------------

    public async Task<Result> DeleteMessageAsync(Guid organizationId, Guid id)
    {
        var message = await _repository.GetByIdAsync(organizationId, id);

        if (message == null)
            return new NotFound("Message not found.");

        if (!message.IsActive)
            return new Conflict("Inactive messages cannot be deleted.");

        var deleted = await _repository.DeleteAsync(organizationId, id);
        if (!deleted)
            return new NotFound("Unable to delete message.");

        return new Deleted();
    }

    // ------------------------------------------------------
    // GET ONE
    // ------------------------------------------------------

    public Task<Message?> GetMessageAsync(Guid organizationId, Guid id)
        => _repository.GetByIdAsync(organizationId, id);

    // ------------------------------------------------------
    // GET ALL
    // ------------------------------------------------------

    public Task<IEnumerable<Message>> GetAllMessagesAsync(Guid organizationId)
        => _repository.GetAllByOrganizationAsync(organizationId);
}
