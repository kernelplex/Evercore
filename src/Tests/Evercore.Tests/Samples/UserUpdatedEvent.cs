using Evercore.Data;
using Evercore.StrongTypes;

namespace Evercore.Tests.Samples;

public record UserUpdatedEvent(string? FirstName = null, string? LastName = null, string? Email = null): IEvent
{
    public static EventType EventType => (EventType) "user.updated";
}