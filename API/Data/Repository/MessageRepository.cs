using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repository
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups
                .Include(x => x.Connections)
                .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups
                .Include(x => x.Connections)
                .FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                .OrderByDescending(x => x.MessageSent)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username
                    && u.RecipientDeleted == false),
                "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username
                    && u.SenderDeleted == false),
                _ => query.Where(u => u.RecipientUsername == messageParams.Username
                    && u.RecipientDeleted == false && u.DateRead == null)
            };

            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>
                .CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
        {
            var query = _context.Messages
                .Where(
                    m => m.RecipientUsername == currentUserName && m.RecipientDeleted == false &&
                    m.SenderUsername == recipientUserName ||
                    m.RecipientUsername == recipientUserName && m.SenderDeleted == false &&
                    m.SenderUsername == currentUserName
                )
                .OrderBy(m => m.MessageSent)
                .AsQueryable();


            var unreadMessages = query.Where(m => m.DateRead == null
                && m.RecipientUsername == currentUserName).ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
            }

            return await query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        public async Task<IEnumerable<UserMessageInfoDto>> GetUserMessages(int userId)
        {
            var messagesInfoQuery = await _context.Messages
                .Include(m => m.Sender).ThenInclude(u => u.Photos)
                .Include(m => m.Recipient).ThenInclude(u => u.Photos)
                .Where(m => (m.RecipientId == userId && m.RecipientDeleted == false) || (m.SenderId == userId && m.SenderDeleted == false))
                .GroupBy(m => m.RecipientId == userId ? m.SenderId : m.RecipientId)
                .Select(g => new
                {
                    FullName = g.Key == userId ? g.FirstOrDefault().Sender.FullName : g.FirstOrDefault().Recipient.FullName,
                    LastMessage = g.OrderByDescending(m => m.MessageSent).FirstOrDefault(),
                    InteractingUserId = g.Key
                })
                .ToListAsync();

            var messagesInfo = messagesInfoQuery.Select(m => new UserMessageInfoDto
            {
                PhotoUrl = m.LastMessage.RecipientId == userId ? m.LastMessage.Sender.Photos.FirstOrDefault(p => p.IsMain)?.Url : m.LastMessage.Recipient.Photos.FirstOrDefault(p => p.IsMain)?.Url,
                FullName = m.FullName,
                LastMessage = m.LastMessage.MessageType switch
                {
                    "Image" => m.FullName + " sent you a photo.",
                    "Video" => m.FullName + " sent you a video.",
                    "File" => m.FullName + " sent you a file.",
                    "Audio" => m.FullName + "sent you an audio.",
                    "Location" => m.FullName + " shared their location with you.",
                    _ => m.LastMessage.Content
                },
                UnreadCount = m.LastMessage.RecipientId == userId ? _context.Messages.Where(message => message.DateRead == null && message.SenderId == m.InteractingUserId && message.RecipientId == userId).Count() : 0
            }).ToList();
            return messagesInfo;
        }
    }
}