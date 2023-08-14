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
    public class MessageHub : Hub
    {
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly IUnitOfWork _uow;
        private readonly IOneDriveService _oneDriveService;
        public MessageHub(IUnitOfWork uow, IMapper mapper, IHubContext<PresenceHub> presenceHub, IOneDriveService oneDriveService)
        {
            _uow = uow;
            _presenceHub = presenceHub;
            _mapper = mapper;
            _oneDriveService = oneDriveService;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"];
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group = await AddToGroup(groupName);

            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

            var messages = await _uow.MessageRepository
                .GetMessageThread(Context.User.GetUsername(), otherUser);

            if (_uow.HasChanges()) await _uow.Complete();

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task CreateLocationMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower()) throw new HubException("You cannot send messages to yourself");

            var sender = await _uow.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _uow.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null || sender == null) throw new HubException("Not found user");

            if (recipient.IsBlocked || recipient.IsDeleted) throw new HubException("Not found user");

            if (await _uow.LikesRepository.GetUserLike(sender.Id, recipient.Id) == null || await _uow.LikesRepository.GetUserLike(recipient.Id, sender.Id) == null)
                throw new HubException("You cannot send messages to this user");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = sender.Latitude + "," + sender.Longitude,
                MessageType = "Location"
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);

            var group = await _uow.MessageRepository.GetMessageGroup(groupName);

            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null)
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                        new { username = sender.UserName, knownAs = sender.KnownAs });
                }
            }

            _uow.MessageRepository.AddMessage(message);

            if (await _uow.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
        }

        public async Task CreateFileMessage([Microsoft.AspNetCore.Mvc.FromForm] CreateFileMessageDto createFileMessageDto)
        {
            var username = Context.User.GetUsername();

            if (username == createFileMessageDto.RecipientUsername.ToLower()) throw new HubException("You cannot send messages to yourself");

            var sender = await _uow.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _uow.UserRepository.GetUserByUsernameAsync(createFileMessageDto.RecipientUsername);

            if (recipient == null || sender == null) throw new HubException("Not found user");

            if (recipient.IsBlocked || recipient.IsDeleted) throw new HubException("Not found user");

            if (await _uow.LikesRepository.GetUserLike(sender.Id, recipient.Id) == null || await _uow.LikesRepository.GetUserLike(recipient.Id, sender.Id) == null)
                throw new HubException("You cannot send messages to this user");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName
            };

            createFileMessageDto.ConvertFile64ToIFormFile();

            if (createFileMessageDto.File != null)
            {
                if (!FileExtensions.IsValidFileName(createFileMessageDto.File.FileName)) throw new HubException("Invalid file name");
                var mediaType = DetermineMediaType(createFileMessageDto.File);
                if (mediaType == "File" && createFileMessageDto.File.Length > 500 * 1024 * 1024) throw new HubException("File size cannot exceed 500 MB");
                var fileUrl = await _oneDriveService.UploadToOneDriveAsync(createFileMessageDto.File);
                message.Content = fileUrl;
                message.MessageType = mediaType;
            }
            else throw new HubException("You must provide a file to send a file message");

            var groupName = GetGroupName(sender.UserName, recipient.UserName);

            var group = await _uow.MessageRepository.GetMessageGroup(groupName);

            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null)
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                        new { username = sender.UserName, knownAs = sender.KnownAs });
                }
            }

            _uow.MessageRepository.AddMessage(message);

            if (await _uow.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
        }

        private string DetermineMediaType(IFormFile file)
        {
            if (FileExtensions.IsImage(file)) return "Image";

            if (FileExtensions.IsVideo(file)) return "Video";

            return "File";
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower())
                throw new HubException("You cannot send messages to yourself");

            var sender = await _uow.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _uow.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername) ?? throw new HubException("Not found user");

            if (recipient.IsBlocked || recipient.IsDeleted) throw new HubException("Not found user");

            if (await _uow.LikesRepository.GetUserLike(sender.Id, recipient.Id) == null || await _uow.LikesRepository.GetUserLike(recipient.Id, sender.Id) == null)
                throw new HubException("You cannot send messages to this user");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            var groupName = GetGroupName(sender.UserName, recipient.UserName);

            var group = await _uow.MessageRepository.GetMessageGroup(groupName);

            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null)
                {
                    await _presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived",
                        new { username = sender.UserName, knownAs = sender.KnownAs });
                }
            }

            _uow.MessageRepository.AddMessage(message);

            if (await _uow.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
        }

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await _uow.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());

            if (group == null)
            {
                group = new Group(groupName);
                _uow.MessageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if (await _uow.Complete()) return group;

            throw new HubException("Failed to add to group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await _uow.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            _uow.MessageRepository.RemoveConnection(connection);

            if (await _uow.Complete()) return group;

            throw new HubException("Failed to remove from group");
        }
    }
}