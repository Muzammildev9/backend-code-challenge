using System.Collections.Concurrent;
using CodeChallenge.Api.Models;

public class InMemoryMessageRepository : IMessageRepository
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<Guid, Message> _store = new();

    public Task<Message?> GetByIdAsync(Guid organizationId, Guid id)
    {
        _lock.EnterReadLock();
        try
        {
            return Task.FromResult(
                _store.TryGetValue(id, out var msg) && msg.OrganizationId == organizationId
                    ? msg
                    : null
            );
        }
        finally { _lock.ExitReadLock(); }
    }

    public Task<IEnumerable<Message>> GetAllByOrganizationAsync(Guid organizationId)
    {
        _lock.EnterReadLock();
        try
        {
            return Task.FromResult(
                _store.Values
                    .Where(x => x.OrganizationId == organizationId)
                    .OrderByDescending(x => x.CreatedAt)
                    .AsEnumerable()
            );
        }
        finally { _lock.ExitReadLock(); }
    }

    public Task<Message?> GetByTitleAsync(Guid organizationId, string title)
    {
        _lock.EnterReadLock();
        try
        {
            return Task.FromResult(
                _store.Values.FirstOrDefault(x =>
                    x.OrganizationId == organizationId &&
                    x.Title.Equals(title, StringComparison.OrdinalIgnoreCase))
            );
        }
        finally { _lock.ExitReadLock(); }
    }

    public Task<Message> CreateAsync(Message message)
    {
        _lock.EnterWriteLock();
        try
        {
            message.Id = Guid.NewGuid();
            _store[message.Id] = message;
            return Task.FromResult(message);
        }
        finally { _lock.ExitWriteLock(); }
    }

    public Task<Message?> UpdateAsync(Message message)
    {
        _lock.EnterWriteLock();
        try
        {
            if (!_store.ContainsKey(message.Id)) return Task.FromResult<Message?>(null);

            _store[message.Id] = message;
            return Task.FromResult<Message?>(message);
        }
        finally { _lock.ExitWriteLock(); }
    }

    public Task<bool> DeleteAsync(Guid organizationId, Guid id)
    {
        _lock.EnterWriteLock();
        try
        {
            if (_store.TryGetValue(id, out var msg) && msg.OrganizationId == organizationId)
                return Task.FromResult(_store.Remove(id));

            return Task.FromResult(false);
        }
        finally { _lock.ExitWriteLock(); }
    }
}
