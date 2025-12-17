using Chattt.Data;
using Chattt.DTOs;
using Chattt.Entities;
using Chattt.Hubs;
using Chattt.Services;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Chattt.Tests;

public class MessageServiceTests
{
    private static AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static Mock<IHubContext<ChatHub>> GetMockHub()
    {
        var mockHub = new Mock<IHubContext<ChatHub>>();
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockHub.Setup(h => h.Clients).Returns(mockClients.Object);
        mockClients.Setup(c => c.User(It.IsAny<string>())).Returns(mockClientProxy.Object);

        return mockHub;
    }

    [Fact]
    public async Task SendMessage_Should_SaveToDb_And_NotifyReceiver()
    {
        /*
         * Arrange.
         */
        var db = GetInMemoryDbContext();
        var mockHub = GetMockHub();
        var service = new MessageService(db, mockHub.Object);

        var sender = new User { Username = "sender", PasswordHash = "hash" };
        var receiver = new User { Username = "receiver", PasswordHash = "hash" };
        db.Users.AddRange(sender, receiver);
        await db.SaveChangesAsync();

        var dto = new SendMessageDto("receiver", "Hello World");

        /*
         * Act.
         */
        var result = await service.SendMessageAsync(sender.Id, dto);

        /*
         * Assert.
         */
        result.IsSuccess.Should().BeTrue();

        var msg = await db.Messages.FirstOrDefaultAsync();
        msg.Should().NotBeNull();
        msg!.Content.Should().Be("Hello World");
        msg.SenderId.Should().Be(sender.Id);
        msg.ReceiverId.Should().Be(receiver.Id);

        mockHub.Verify(h => h.Clients.User(receiver.Id.ToString())
            .SendCoreAsync("ReceiveMessage", It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendMessage_Should_Fail_When_ReceiverNotFound()
    {
        /*
         * Arrange.
         */
        var db = GetInMemoryDbContext();
        var mockHub = GetMockHub();
        var service = new MessageService(db, mockHub.Object);

        var sender = new User { Username = "sender", PasswordHash = "hash" };
        db.Users.Add(sender);
        await db.SaveChangesAsync();

        var dto = new SendMessageDto("unknown_user", "Hello");

        /*
         * Act.
         */
        var result = await service.SendMessageAsync(sender.Id, dto);

        /*
         * Assert.
         */
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found");

        (await db.Messages.AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task GetConversation_Should_ReturnSortedHistory()
    {
        /*
         * Arrange.
         */
        var db = GetInMemoryDbContext();
        var service = new MessageService(db, GetMockHub().Object);

        var me = new User { Username = "me", PasswordHash = "hash" };
        var friend = new User { Username = "friend", PasswordHash = "hash" };
        db.Users.AddRange(me, friend);
        await db.SaveChangesAsync();

        var msg1 = new Message { Content = "Hi", SenderId = me.Id, ReceiverId = friend.Id, SentAt = DateTime.UtcNow.AddMinutes(-10) };
        var msg2 = new Message { Content = "Hello", SenderId = friend.Id, ReceiverId = me.Id, SentAt = DateTime.UtcNow.AddMinutes(-5) };
        var msg3 = new Message { Content = "How are you?", SenderId = me.Id, ReceiverId = friend.Id, SentAt = DateTime.UtcNow }; // Самое свежее

        db.Messages.AddRange(msg1, msg3, msg2);
        await db.SaveChangesAsync();

        /*
         * Act.
         */
        var history = await service.GetConversationAsync(me.Id, "friend");

        /*
         * Assert.
         */
        history.Should().HaveCount(3);

        var list = history.ToList();
        list[0].Content.Should().Be("Hi");
        list[1].Content.Should().Be("Hello");
        list[2].Content.Should().Be("How are you?");

        list[0].IsMe.Should().BeTrue();
        list[1].IsMe.Should().BeFalse();
    }
}