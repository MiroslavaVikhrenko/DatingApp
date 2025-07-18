using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class MessageHub(IUnitOfWork uow, IHubContext<PresenceHub> presenceHub) : Hub
{
    public override async Task OnConnectedAsync()
    {
        // getting http conetxt from hub context => this is where the initial nefgotiation takes place
        // it's an http request to set up the signalR connection
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext?.Request?.Query["userId"].ToString() ?? throw new HubException("Othre user not found");

        // create group because need to ensure that messaging is private between the two users that are connected to this hub
        var groupName = GetGroupName(GetUserId(), otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await AddToGroup(groupName); // save to db

        var messages = await uow.MessageRepository.GetMessageThread(GetUserId(), otherUser);

        // notify to the users inside this group and pass back the message thread
        await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var sender = await uow.MemberRepository.GetMemberByIdAsync(GetUserId());
        var recipient = await uow.MemberRepository.GetMemberByIdAsync(createMessageDto.RecipientId);

        if (recipient == null || sender == null || sender.Id == createMessageDto.RecipientId)
            throw new HubException("Cannot send message");

        var message = new Message
        {
            SenderId = sender.Id,
            RecipientId = recipient.Id,
            Content = createMessageDto.Content
        };

        var groupName = GetGroupName(sender.Id, recipient.Id);
        var group = await uow.MessageRepository.GetMessageGroup(groupName);
        var userInGroup = group != null && group.Connections.Any(x => x.UserId == message.RecipientId);

        if (userInGroup)
        {
            // mark as read
            message.DateRead = DateTime.UtcNow;
        }

        uow.MessageRepository.AddMessage(message);

        if (await uow.Complete())
        {
            await Clients.Group(groupName).SendAsync("NewMessage", message.ToDto());
            // for notifications of new messages if user is on different tab
            var connections = await PresenceTracker.GetConnectionsForUser(recipient.Id);
            if (connections != null && connections.Count > 0 && !userInGroup)
            {
                // notify a user that they have a new message => 
                // they might be connected on different divices and we want to notify all connections
                await presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", message.ToDto());
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // manually remove connection from db
        await uow.MessageRepository.RemoveConnection(Context.ConnectionId);
        // when a user disconnects from signalR hub and they are inside the group, then they're automatically removed from the group
        await base.OnDisconnectedAsync(exception);
    }

    private async Task<bool> AddToGroup(string groupName)
    {
        var group = await uow.MessageRepository.GetMessageGroup(groupName);
        var connection = new Connection(Context.ConnectionId, GetUserId());

        if (group == null)
        {
            group = new Group(groupName);
            uow.MessageRepository.AddGroup(group);
        }

        group.Connections.Add(connection);

        return await uow.Complete();
    }

    private static string GetGroupName(string? caller, string? other)
    {
        // create a group name based on the user IDs
        // always return the same regardless of which order they connected to the hub =>
        // so need to order into alphabetical order
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        // if < 0 => string A < string B
        // if 0 => string A = string B
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

    private string GetUserId()
    {
        return Context.User?.GetMemberId() ?? throw new HubException("Cannot get member id");
    }
}
