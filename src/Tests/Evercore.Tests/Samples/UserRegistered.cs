using Evercore.Data;
using Evercore.StrongTypes;

namespace Evercore.Tests.Samples;

public record UserRegistered(string FirstName, string LastName, string Email): IEvent
{
    public static EventType EventType => (EventType) "user.created";
}