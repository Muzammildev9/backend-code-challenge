using CodeChallenge.Models;
using CodeChallenge.Repositories;
using CodeChallenge.Requests;
using CodeChallenge.Results;

namespace CodeChallenge.Logic
{
    public class MessageLogic : IMessageLogic
    {
        private readonly IMessageRepository _repository;

        public MessageLogic(IMessageRepository repository)
        {
            _repository = repository;
        }

        // -------------------------------------------------------
        // CREATE
        // -------------------------------------------------------
        public async Task<Result> CreateMessageAsync(Guid organizationId, CreateMessageRequest request)
        {
            if (organizationId == Guid.Empty)
                return Result.Fail("OrganizationId is required.");

            if (request == null)
                return Result.Fail("Request is required.");

            // Centralized validation
            var validation = ValidateRequest(request.Title, request.Content);
            if (validation.Any())
                return new ValidationError(validation);

            // Check duplicate title in the same organization
            var existing = await _repository.GetByTitleAsync(organizationId, request.Title.Trim());
            if (existing != null)
                return Result.Conflict("A message with the same title already exists.");

            var message = new Message
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                Title = request.Title.Trim(),
                Content = request.Content.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _repository.AddAsync(message);
            return Result.Success(message);
        }

        // -------------------------------------------------------
        // UPDATE
        // -------------------------------------------------------
        public async Task<Result> UpdateMessageAsync(Guid organizationId, Guid id, UpdateMessageRequest request)
        {
            if (organizationId == Guid.Empty)
                return Result.Fail("OrganizationId is required.");

            if (id == Guid.Empty)
                return Result.Fail("MessageId is required.");

            if (request == null)
                return Result.Fail("Request is required.");

            // Validate fields
            var validation = ValidateRequest(request.Title, request.Content);
            if (validation.Any())
                return new ValidationError(validation);

            var message = await _repository.GetAsync(organizationId, id);
            if (message == null)
                return Result.NotFound("Message not found.");

            if (!message.IsActive)
                return new ValidationError(new Dictionary<string, string[]>
                {
                    { "Message", new[]{ "Inactive messages cannot be updated." } }
                });

            message.Title = request.Title.Trim();
            message.Content = request.Content.Trim();
            message.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(message);
            return Result.Success(message);
        }

        // -------------------------------------------------------
        // DELETE
        // -------------------------------------------------------
        public async Task<Result> DeleteMessageAsync(Guid organizationId, Guid id)
        {
            if (organizationId == Guid.Empty)
                return Result.Fail("OrganizationId is required.");

            if (id == Guid.Empty)
                return Result.Fail("MessageId is required.");

            var message = await _repository.GetAsync(organizationId, id);
            if (message == null)
                return Result.NotFound("Message not found.");

            await _repository.DeleteAsync(message);
            return Result.Success();
        }

        // -------------------------------------------------------
        // GET (Single)
        // -------------------------------------------------------
        public async Task<Message?> GetMessageAsync(Guid organizationId, Guid id)
        {
            return await _repository.GetAsync(organizationId, id);
        }

        // -------------------------------------------------------
        // GET ALL
        // -------------------------------------------------------
        public async Task<IEnumerable<Message>> GetAllMessagesAsync(Guid organizationId)
        {
            return await _repository.GetAllAsync(organizationId);
        }

        // -------------------------------------------------------
        // CENTRALIZED VALIDATION
        // -------------------------------------------------------
        private Dictionary<string, string[]> ValidateRequest(string title, string content)
        {
            var errors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(title) || title.Length < 3 || title.Length > 200)
                errors["Title"] = new[] { "Title must be between 3 and 200 characters." };

            if (string.IsNullOrWhiteSpace(content) || content.Length < 10 || content.Length > 1000)
                errors["Content"] = new[] { "Content must be between 10 and 1000 characters." };

            return errors;
        }
    }
}
