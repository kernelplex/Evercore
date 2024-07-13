using Evercore.Storage;
using Evercore.Tests.Samples;
using FluentAssertions;

namespace Evercore.Tests;

public class ContextTests
{
    private EventStore _eventStore;
    private TransientMemoryStorageEngine _transientMemoryStorageEngine;
    
    const string FirstName = "Robert";
    const string LastName = "Chavez";
    const string Email = "rchavez@example.com";
    
    public ContextTests()
    {
        _transientMemoryStorageEngine = new TransientMemoryStorageEngine();
        _eventStore = new EventStore(_transientMemoryStorageEngine);
        _eventStore.RegisterEventType<UserRegistered>();
    }

    [Fact]
    public async Task WithContext_ReturnsValue()
    {
        var state = await _eventStore.WithContext(async (ctx, stoppingToken) =>
        {
            var result = await ctx.Create<User>((id) => User.Initialize(id), cancellationToken: stoppingToken);
            var user = result.Unwrap();
            return user.State;
        }, cancellationToken: CancellationToken.None);

        state.Should().NotBeNull();
    }
    
    [Fact]
    public async Task WithContext_DoesNotReturnValue()
    {
        var action = async () =>
        {
            await _eventStore.WithContext(async (ctx, stoppingToken) =>
            {
                var result = await ctx.Create<User>((id) => User.Initialize(id), cancellationToken: stoppingToken);
                result.Unwrap();
            }, cancellationToken: CancellationToken.None);
        };
        await action.Should().NotThrowAsync();
    }
    
    [Fact]
    public async Task WithContext_EventsAreApplied()
    {
        var state = await _eventStore.WithContext(async (ctx, stoppingToken) =>
        {
            var result = await ctx.Create<User>((id) => User.Initialize(id), cancellationToken: stoppingToken);
            var user = result.Unwrap();
            user.RegisterUser(ctx, FirstName,  LastName, Email);
            return user.State;
        }, cancellationToken: CancellationToken.None);

        state.FirstName.Should().Be(FirstName);
        state.LastName.Should().Be(LastName);
        state.Email.Should().Be(Email);
    }
    
    [Fact]
    public async Task WithReadonlyContext_ReturnsValue()
    {
        // Initialize User
        var id = await _eventStore.WithContext(async (ctx, stoppingToken) =>
        {
            var result = await ctx.Create<User>((id) => User.Initialize(id), cancellationToken: stoppingToken);
            var user = result.Unwrap();
            user.RegisterUser(ctx, FirstName,  LastName, Email);
            return user.Id;
        }, cancellationToken: CancellationToken.None);
        
        // New context to retrieve user.
        var state = await _eventStore.WithReadonlyContext(async (ctx, stoppingToken) =>
        {
            var result = await ctx.Load(User.Initialize, id, cancellationToken: stoppingToken);
            return result.ValueOrThrow;
        }, cancellationToken: CancellationToken.None);

        state.FirstName.Should().Be(FirstName);
        state.LastName.Should().Be(LastName);
        state.Email.Should().Be(Email);
    }
    
    [Fact]
    public async Task WithReadonlyContext_WithoutReturnValue()
    {
        // Initialize User
        var id = await _eventStore.WithContext(async (ctx, stoppingToken) =>
        {
            var result = await ctx.Create<User>((id) => User.Initialize(id), cancellationToken: stoppingToken);
            var user = result.Unwrap();
            user.RegisterUser(ctx, FirstName,  LastName, Email);
            return user.Id;
        }, cancellationToken: CancellationToken.None);
        
        // New context to retrieve user.
        await _eventStore.WithReadonlyContext(async (ctx, stoppingToken) =>
        {
            var result = await ctx.Load(User.Initialize, id, cancellationToken: stoppingToken);
            var state = result.ValueOrThrow.State;
            state.FirstName.Should().Be(FirstName);
            state.LastName.Should().Be(LastName);
            state.Email.Should().Be(Email);
        }, cancellationToken: CancellationToken.None);

    }
}