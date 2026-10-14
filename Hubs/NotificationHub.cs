using DeluxeNET.Security;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeluxeNET.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly jwt _jwt;
        private const bool SKIP_AUTH = false;

        private static readonly ConcurrentDictionary<long, HashSet<string>> _connections = new();

        public NotificationHub(jwt __jwt)
        {
            _jwt = __jwt;
        }

        public override async Task OnConnectedAsync()
        {
            long id;

            if (SKIP_AUTH)
            {
                // Dev mode: assign a fake user ID
                id = -1;
            }
            else
            {
                var httpContext = Context.GetHttpContext();
                var authHeader = httpContext?.Request.Headers["Authorization"].ToString();

                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    Context.Abort();
                    return;
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();

                long? playerId;

                try
                {
                    playerId = _jwt.VerifyToken(token);
                }
                catch
                {
                    Context.Abort();
                    return;
                }

                if (playerId == null)
                {
                    Context.Abort();
                    return;
                }

                id = playerId.Value;
            }

            _connections.AddOrUpdate(
                id,
                _ => new HashSet<string> { Context.ConnectionId },
                (_, set) =>
                {
                    lock (set)
                    {
                        set.Add(Context.ConnectionId);
                    }
                    return set;
                });

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            foreach (var entry in _connections)
            {
                lock (entry.Value)
                {
                    entry.Value.Remove(Context.ConnectionId);

                    if (entry.Value.Count == 0)
                    {
                        _connections.TryRemove(entry.Key, out _);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendOne(long playerId, object payload)
        {
            if (_connections.TryGetValue(playerId, out var conns))
            {
                List<Task> tasks;

                lock (conns)
                {
                    tasks = conns
                        .Select(id => Clients.Client(id).SendAsync("Notification", payload))
                        .ToList();
                }

                await Task.WhenAll(tasks);
            }
        }

        public async Task SendMany(List<long> playerIds, object payload)
        {
            foreach (var id in playerIds)
            {
                await SendOne(id, payload);
            }
        }

        public async Task SendAll(object payload)
        {
            await Clients.All.SendAsync("Notification", payload);
        }
    }
}