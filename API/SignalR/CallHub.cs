using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class CallHub : Hub
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IHubContext<MessageHub> _messageHub;
        private readonly IHubContext<PresenceHub> _presenceHub;

        public CallHub(IUnitOfWork uow, IMapper mapper, IHubContext<MessageHub> messageHub, IHubContext<PresenceHub> presenceHub)
        {
            _presenceHub = presenceHub;
            _messageHub = messageHub;
            _uow = uow;
            _mapper = mapper;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"];
            var roomName = GetRoomName(Context.User.GetUsername(), otherUser);

            var roomCheck = await _uow.RoomRepository.GetRoom(roomName);
            if (roomCheck != null)
            {
                if (roomCheck.Connections.Any(x => x.Username == Context.User.GetUsername()))
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("YouAreAlreadyInThisCall");
                    throw new HubException("You Are Already In This Call");
                }

                if (await _uow.RoomRepository.CheckUserInCall(Context.User.GetUsername()) == true)
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("YouAreAlreadyInOtherCall");
                    throw new HubException("You Are Already In Other Call");
                }
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
            var room = await AddToRoom(roomName);

            await Clients.Group(roomName).SendAsync("UpdatedRoom", room);

            if (room.Connections.Count() == 1)
            {
                var connections = await PresenceTracker.GetConnectionsForUser(otherUser);
                if (connections != null)
                {
                    if (await _uow.RoomRepository.CheckUserInCall(otherUser) == true)
                    {
                        var message = new Message
                        {
                            Sender = await _uow.UserRepository.GetUserByUsernameAsync(Context.User.GetUsername()),
                            Recipient = await _uow.UserRepository.GetUserByUsernameAsync(otherUser),
                            SenderUsername = Context.User.GetUsername(),
                            RecipientUsername = otherUser,
                            Content = "Missed Call",
                            MessageType = "MissCall"
                        };

                        _uow.MessageRepository.AddMessage(message);

                        if (await _uow.Complete())
                        {
                            await _messageHub.Clients.Group(roomName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
                        }

                        await Clients.Client(Context.ConnectionId).SendAsync("UserBusy");
                    }
                    else
                    {
                        var caller = await _uow.UserRepository.GetUserByUsernameAsync(Context.User.GetUsername());
                        await _presenceHub.Clients.Clients(connections).SendAsync("IncomingCall", new { username = caller.UserName, knownAs = caller.KnownAs });
                        await Clients.Client(Context.ConnectionId).SendAsync("StartCall");
                    }
                }
                else
                {
                    var message = new Message
                    {
                        Sender = await _uow.UserRepository.GetUserByUsernameAsync(Context.User.GetUsername()),
                        Recipient = await _uow.UserRepository.GetUserByUsernameAsync(otherUser),
                        SenderUsername = Context.User.GetUsername(),
                        RecipientUsername = otherUser,
                        Content = "Missed Call",
                        MessageType = "MissCall"
                    };

                    _uow.MessageRepository.AddMessage(message);

                    if (await _uow.Complete())
                    {
                        await _messageHub.Clients.Group(roomName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
                    }

                    await Clients.Group(roomName).SendAsync("UserOffline");
                }
            }
            else
            {
                await Clients.Client(Context.ConnectionId).SendAsync("IncomingCall");
            }
        }

        public async Task AcceptCall(CreateCallDto createCallDto)
        {
            createCallDto.RecipientUsername = Context.User.GetUsername();
            var roomName = GetRoomName(createCallDto.CallerUsername, createCallDto.RecipientUsername);
            var room = await _uow.RoomRepository.GetRoom(roomName);

            if (room != null)
            {
                var connections = room.Connections.Select(c => c.ConnectionId).ToList();

                if (connections.Count() <= 1) throw new HubException("Failed to Accept Call");

                foreach (var connectionId in connections)
                {
                    var connection = room.Connections.FirstOrDefault(x => x.ConnectionId == connectionId);
                    await Clients.Client(connectionId).SendAsync("AcceptCall");
                }

                var caller = await _uow.UserRepository.GetUserByUsernameAsync(createCallDto.CallerUsername);
                var receiver = await _uow.UserRepository.GetUserByUsernameAsync(createCallDto.RecipientUsername);

                var call = new Call
                {
                    Caller = caller,
                    Receiver = receiver,
                    CallerId = caller.Id,
                    ReceiverId = receiver.Id,
                    CallerUsername = createCallDto.CallerUsername,
                    ReceiverUsername = createCallDto.RecipientUsername
                };

                _uow.RoomRepository.AddCall(call);

                await _uow.Complete();
            }
            else throw new HubException("Failed to Accept Call");
        }

        public async Task Decline(string callerUserName)
        {
            var roomName = GetRoomName(callerUserName, Context.User.GetUsername());
            var message = new Message
            {
                Sender = await _uow.UserRepository.GetUserByUsernameAsync(callerUserName),
                Recipient = await _uow.UserRepository.GetUserByUsernameAsync(Context.User.GetUsername()),
                SenderUsername = callerUserName,
                RecipientUsername = Context.User.GetUsername(),
                Content = "Miss Call",
                MessageType = "MissCall"
            };

            _uow.MessageRepository.AddMessage(message);

            if (await _uow.Complete())
            {
                await _messageHub.Clients.Group(roomName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }

            await Clients.Group(roomName).SendAsync("UserBusy");
        }

        public async Task MissCall(string recipientUsername)
        {
            var roomName = GetRoomName(Context.User.GetUsername(), recipientUsername);
            var message = new Message
            {
                Sender = await _uow.UserRepository.GetUserByUsernameAsync(Context.User.GetUsername()),
                Recipient = await _uow.UserRepository.GetUserByUsernameAsync(recipientUsername),
                SenderUsername = Context.User.GetUsername(),
                RecipientUsername = recipientUsername,
                Content = "Missed Call",
                MessageType = "MissCall"
            };

            _uow.MessageRepository.AddMessage(message);

            if (await _uow.Complete())
            {
                await _messageHub.Clients.Group(roomName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }

            await Clients.Group(roomName).SendAsync("UserBusy");
        }

        public async Task Micro(CreateCallDto createCallDto)
        {
            var roomName = GetRoomName(createCallDto.CallerUsername, createCallDto.RecipientUsername);
            var room = await _uow.RoomRepository.GetRoom(roomName);
            if (room != null)
            {
                await Clients.Group(roomName).SendAsync("MicroUpdate", new { username = Context.User.GetUsername(), micro = createCallDto.IsVoiceEnabled });
            }
            else
            {
                throw new HubException("Failed to Update Microphone");
            }
        }

        public async Task Camera(CreateCallDto createCallDto)
        {
            var roomName = GetRoomName(createCallDto.CallerUsername, createCallDto.RecipientUsername);
            var room = await _uow.RoomRepository.GetRoom(roomName);
            if (room != null)
            {
                await Clients.Group(roomName).SendAsync("CameraUpdate", new { username = Context.User.GetUsername(), camera = createCallDto.IsVideoEnabled });
            }
            else
            {
                throw new HubException("Failed to Update Camera");
            }
        }

        public async Task Screen(CreateCallDto createCallDto)
        {
            var roomName = GetRoomName(createCallDto.CallerUsername, createCallDto.RecipientUsername);
            var room = await _uow.RoomRepository.GetRoom(roomName);
            if (room != null)
            {
                await Clients.Group(roomName).SendAsync("ScreenUpdate", new { username = Context.User.GetUsername(), screen = createCallDto.IsScreenSharingEnabled });
            }
            else
            {
                throw new HubException("Failed to Update Screen");
            }
        }

        public async Task EndCall(string otherUserName)
        {
            var roomName = GetRoomName(Context.User.GetUsername(), otherUserName);
            var room = await _uow.RoomRepository.GetRoom(roomName);

            if (room != null)
            {
                var connections = room.Connections.Select(c => c.ConnectionId).ToList();

                foreach (var connectionId in connections)
                {
                    var connection = room.Connections.FirstOrDefault(x => x.ConnectionId == connectionId);
                    await Clients.Client(connectionId).SendAsync("EndCall");
                }

                if (_uow.RoomRepository.CheckCall(Context.User.GetUsername(), otherUserName))
                {
                    var call = await _uow.RoomRepository.FindCall(Context.User.GetUsername(), otherUserName);

                    call.EndTime = DateTime.UtcNow;

                    _uow.RoomRepository.UpdateCall(call);

                    TimeSpan duration = call.EndTime.Value - call.StartTime;

                    var message = new Message
                    {
                        SenderId = call.CallerId,
                        RecipientId = call.ReceiverId,
                        Sender = call.Caller,
                        Recipient = call.Receiver,
                        SenderUsername = call.CallerUsername,
                        RecipientUsername = call.ReceiverUsername,
                        Content = duration.ToString(@"hh\:mm\:ss"),
                        MessageType = "Call"
                    };

                    var group = await _uow.MessageRepository.GetMessageGroup(room.Name);

                    if (group.Connections.Any(x => x.Username == call.ReceiverUsername || x.Username == call.CallerUsername))
                    {
                        message.DateRead = DateTime.UtcNow;
                    }
                    else
                    {
                        var connectionsPresenceTracker = await PresenceTracker.GetConnectionsForUser(call.ReceiverUsername);
                        if (connectionsPresenceTracker != null)
                        {
                            await _presenceHub.Clients.Clients(connectionsPresenceTracker).SendAsync("NewMessageReceived", new { username = call.CallerUsername, knownAs = call.Caller.KnownAs });
                        }

                        connectionsPresenceTracker = await PresenceTracker.GetConnectionsForUser(call.CallerUsername);
                        if (connectionsPresenceTracker != null)
                        {
                            await _presenceHub.Clients.Clients(connectionsPresenceTracker).SendAsync("NewMessageReceived", new { username = call.CallerUsername, knownAs = call.Caller.KnownAs });
                        }
                    }

                    _uow.MessageRepository.AddMessage(message);

                    if (await _uow.Complete())
                    {
                        await _messageHub.Clients.Group(room.Name).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
                    }
                }
                else throw new HubException("Failed to find call");
            }
            else throw new HubException("Failed to find room");
        }

        private (string caller, string other) GetCallerAndOtherFromRoomName(string roomName)
        {
            string[] parts = roomName.Split('-');

            string callerOrOther1 = parts[0];
            string callerOrOther2 = parts[1];

            return (callerOrOther1, callerOrOther2);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var room = await RemoveFromRoom();
            await Clients.Group(room.Name).SendAsync("UpdatedGroup");
            (string caller, string other) = GetCallerAndOtherFromRoomName(room.Name);

            if (_uow.RoomRepository.CheckCall(caller, other))
            {
                var connections = room.Connections.Select(c => c.ConnectionId).ToList();

                foreach (var connectionId in connections)
                {
                    var connection = room.Connections.FirstOrDefault(x => x.ConnectionId == connectionId);
                    await Clients.Client(connectionId).SendAsync("EndCall");
                }

                var call = await _uow.RoomRepository.FindCall(caller, other);

                call.EndTime = DateTime.UtcNow;

                _uow.RoomRepository.UpdateCall(call);

                TimeSpan duration = call.EndTime.Value - call.StartTime;

                var message = new Message
                {
                    SenderId = call.CallerId,
                    RecipientId = call.ReceiverId,
                    Sender = call.Caller,
                    Recipient = call.Receiver,
                    SenderUsername = call.CallerUsername,
                    RecipientUsername = call.ReceiverUsername,
                    Content = duration.ToString(@"hh\:mm\:ss"),
                    MessageType = "Call"
                };

                var group = await _uow.MessageRepository.GetMessageGroup(room.Name);

                if (group.Connections.Any(x => x.Username == call.ReceiverUsername || x.Username == call.CallerUsername))
                {
                    message.DateRead = DateTime.UtcNow;
                }
                else
                {
                    var connectionsPresenceTracker = await PresenceTracker.GetConnectionsForUser(call.ReceiverUsername);
                    if (connectionsPresenceTracker != null)
                    {
                        await _presenceHub.Clients.Clients(connectionsPresenceTracker).SendAsync("NewMessageReceived", new { username = call.CallerUsername, knownAs = call.Caller.KnownAs });
                    }

                    connectionsPresenceTracker = await PresenceTracker.GetConnectionsForUser(call.CallerUsername);
                    if (connectionsPresenceTracker != null)
                    {
                        await _presenceHub.Clients.Clients(connectionsPresenceTracker).SendAsync("NewMessageReceived", new { username = call.CallerUsername, knownAs = call.Caller.KnownAs });
                    }
                }

                _uow.MessageRepository.AddMessage(message);

                if (await _uow.Complete())
                {
                    await _messageHub.Clients.Group(room.Name).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        private async Task<Room> RemoveFromRoom()
        {
            var room = await _uow.RoomRepository.GetRoomForConnection(Context.ConnectionId);
            var connection = room.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            _uow.RoomRepository.RemoveConnection(connection);

            if (await _uow.Complete()) return room;

            throw new HubException("Failed to remove from room");
        }

        private string GetRoomName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }

        private async Task<Room> AddToRoom(string roomName)
        {
            var room = await _uow.RoomRepository.GetRoom(roomName);

            if (room == null)
            {
                room = new Room(roomName);
                _uow.RoomRepository.AddRoom(room);
            }

            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            room.Connections.Add(connection);

            if (await _uow.Complete()) return room;

            throw new HubException("Failed to add to room");
        }
    }
}