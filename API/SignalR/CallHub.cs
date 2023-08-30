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
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
            var room = await AddToRoom(roomName);

            await Clients.Group(roomName).SendAsync("UpdatedRoom", room);

            if (room.Connections.Count() == 1)
            {
                await Clients.Group(roomName).SendAsync("StartCall");
            }
            else
            {
                await Clients.Group(roomName).SendAsync("IncomingCall");
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

                var call = new Call
                {
                    CallerUsername = createCallDto.CallerUsername,
                    RecipientUsername = createCallDto.RecipientUsername,
                };

                _uow.RoomRepository.AddCall(call);

                await _uow.Complete();
            }
            else throw new HubException("Failed to Accept Call");
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

        public async Task StartCall(CreateCallDto createCallDto)
        {
            createCallDto.CallerUsername = Context.User.GetUsername();

            if (_uow.RoomRepository.CheckCall(createCallDto.CallerUsername, createCallDto.RecipientUsername))
            {
                throw new HubException("Call already exists");
            }
            else
            {
                var otherUserConnections = await PresenceTracker.GetConnectionsForUser(createCallDto.RecipientUsername);
                if (otherUserConnections != null)
                {
                    foreach (var connectionId in otherUserConnections)
                    {
                        await Clients.Client(connectionId).SendAsync("IncomingCall", createCallDto);
                    }
                }
                else
                {
                    var message = new Message
                    {
                        Sender = await _uow.UserRepository.GetUserByUsernameAsync(createCallDto.CallerUsername),
                        Recipient = await _uow.UserRepository.GetUserByUsernameAsync(createCallDto.RecipientUsername),
                        SenderUsername = createCallDto.CallerUsername,
                        RecipientUsername = createCallDto.RecipientUsername,
                        Content = "Missed Call",
                        MessageType = "MissCall"
                    };

                    var groupName = GetGroupName(createCallDto.CallerUsername, createCallDto.RecipientUsername);

                    _uow.MessageRepository.AddMessage(message);

                    if (await _uow.Complete())
                    {
                        await _messageHub.Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
                    }

                    await Clients.Group(GetRoomName(createCallDto.CallerUsername, createCallDto.RecipientUsername)).SendAsync("UserOffline");
                }
            }
        }

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }

        public async Task EndCall(CreateCallDto createCallDto)
        {
            var roomName = GetRoomName(Context.User.GetUsername(), createCallDto.RecipientUsername);
            var room = await _uow.RoomRepository.GetRoom(roomName);

            if (room != null)
            {
                var connections = room.Connections.Select(c => c.ConnectionId).ToList();

                foreach (var connectionId in connections)
                {
                    var connection = room.Connections.FirstOrDefault(x => x.ConnectionId == connectionId);
                    await Clients.Client(connectionId).SendAsync("EndCall");
                }

                if (_uow.RoomRepository.CheckCall(Context.User.GetUsername(), createCallDto.RecipientUsername))
                {
                    var call = await _uow.RoomRepository.FindCall(Context.User.GetUsername(), createCallDto.RecipientUsername);

                    call.EndTime = DateTime.UtcNow;

                    _uow.RoomRepository.UpdateCall(call);

                    TimeSpan duration = call.EndTime.Value - call.StartTime;

                    var message = new Message
                    {
                        Sender = await _uow.UserRepository.GetUserByUsernameAsync(call.CallerUsername),
                        Recipient = await _uow.UserRepository.GetUserByUsernameAsync(call.RecipientUsername),
                        SenderUsername = call.CallerUsername,
                        RecipientUsername = call.RecipientUsername,
                        Content = duration.ToString(@"hh\:mm\:ss"),
                        MessageType = "Call"
                    };

                    var groupName = GetGroupName(call.CallerUsername, call.RecipientUsername);

                    var group = await _uow.MessageRepository.GetMessageGroup(groupName);

                    if (group.Connections.Any(x => x.Username == call.RecipientUsername))
                    {
                        message.DateRead = DateTime.UtcNow;
                    }
                    else
                    {
                        var connectionsMessage = await PresenceTracker.GetConnectionsForUser(call.RecipientUsername);
                        if (connectionsMessage != null)
                        {
                            var sender = await _uow.UserRepository.GetUserByUsernameAsync(call.CallerUsername);
                            await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                                new { username = sender.UserName, knownAs = sender.KnownAs });
                        }
                    }

                    _uow.MessageRepository.AddMessage(message);

                    if (await _uow.Complete())
                    {
                        await _messageHub.Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
                    }
                }
                else throw new HubException("Failed to End Call");
            }
            else throw new HubException("Failed to End Call");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var room = await RemoveFromRoom();
            await Clients.Group(room.Name).SendAsync("UpdatedGroup");
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