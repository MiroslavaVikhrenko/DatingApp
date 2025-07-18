using System;
using System.Collections.Concurrent;

namespace API.SignalR;

// hold in memory the users and their connections to presence hub (not scalable (load balancer if we have multiple instances of running API)
public class PresenceTracker
{
    // potentially each user might have multiple connections to app 
    // string in outer dictionary => userId
    // string in inner dictionary => connectionId
    // byte => value
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> OnlineUsers = new();

    public Task UserConnected(string userId, string connectionId)
    {
        var connections = OnlineUsers.GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>());

        connections.TryAdd(connectionId, 0); // 0 for byte
        return Task.CompletedTask;
    }

    public Task UserDisconnected(string userId, string connectionId)
    {
        if (OnlineUsers.TryGetValue(userId, out var connections)) // gives all connection that match this user id
        {
            connections.TryRemove(connectionId, out _); // out _ => don't need to use what we're passing out here

            if (connections.IsEmpty)
            {
                // if connections empty remove user id from outer dictionary
                OnlineUsers.TryRemove(userId, out _);
            }
        }

        return Task.CompletedTask;
    }

    public Task<string[]> GetOnlineUsers()
    {
        return Task.FromResult(OnlineUsers.Keys.OrderBy(k => k).ToArray());
    }

    public static Task<List<string>> GetConnectionsForUser(string userId)
    {
        if (OnlineUsers.TryGetValue(userId, out var connections))
        {
            return Task.FromResult(connections.Keys.ToList());
        }

        return Task.FromResult(new List<string>());
    }
}
