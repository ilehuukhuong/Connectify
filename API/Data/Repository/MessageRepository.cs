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

        public async Task<PagedList<UserMessageInfoDto>> GetUserMessages(int userId, MessageParams messageParams)
        {
            var messagesInfoQuery = _context.Messages
                .Include(m => m.Sender).ThenInclude(u => u.Photos)
                .Include(m => m.Recipient).ThenInclude(u => u.Photos)
                .Where(m => (m.RecipientId == userId && m.RecipientDeleted == false) || (m.SenderId == userId && m.SenderDeleted == false))
                .Where(m => m.Recipient.IsBlocked == false && m.Sender.IsBlocked == false)
                .Where(m => m.Recipient.IsDeleted == false && m.Sender.IsDeleted == false);

            if (!string.IsNullOrEmpty(messageParams.FullName))
            {
                messagesInfoQuery = messagesInfoQuery.Where(m => m.Sender.KnownAs.ToLower().Contains(messageParams.FullName.ToLower()) || m.Recipient.KnownAs.ToLower().Contains(messageParams.FullName.ToLower()) || (m.Recipient.FirstName.ToLower() + " " + m.Recipient.LastName.ToLower()).Contains(messageParams.FullName.ToLower()) || (m.Sender.FirstName.ToLower() + " " + m.Sender.LastName.ToLower()).Contains(messageParams.FullName.ToLower()));
            }

            var finalMessagesInfoQuery = await messagesInfoQuery
                .GroupBy(m => m.RecipientId == userId ? m.SenderId : m.RecipientId)
                .OrderByDescending(g => g.Max(m => m.MessageSent))
                .Select(g => new
                {
                    LastMessage = g.OrderByDescending(m => m.MessageSent).FirstOrDefault(),
                    InteractingUserId = g.Key,
                })
                .ToListAsync();


            var messagesInfo = finalMessagesInfoQuery.Select(m => new UserMessageInfoDto
            {
                PhotoUrl = m.LastMessage.RecipientId == userId ? m.LastMessage.Sender.Photos.FirstOrDefault(p => p.IsMain)?.Url : m.LastMessage.Recipient.Photos.FirstOrDefault(p => p.IsMain)?.Url,
                FullName = m.LastMessage.RecipientId == userId ? m.LastMessage.Sender.FullName : m.LastMessage.Recipient.FullName,
                UserName = m.LastMessage.RecipientId == userId ? m.LastMessage.Sender.UserName : m.LastMessage.Recipient.UserName,
                MessageSent = m.LastMessage.MessageSent,
                LastMessage = m.LastMessage.MessageType switch
                {
                    "Unsent" => m.LastMessage.SenderId == userId ? "You unsent a message." : m.LastMessage.Sender.FirstName + " unsent a message.",
                    "Call" => m.LastMessage.RecipientId == userId ? m.LastMessage.Sender.FirstName + " called you." : m.LastMessage.Recipient.FirstName + " called you.",
                    "MissCall" => m.LastMessage.RecipientId == userId ? "You missed a call from " + m.LastMessage.Sender.FirstName : m.LastMessage.Recipient.FirstName + " missed your call.",
                    "Image" => m.LastMessage.RecipientId == userId ? m.LastMessage.Sender.FirstName + " sent a photo." : "You sent a photo.",
                    "Video" => m.LastMessage.RecipientId == userId ? m.LastMessage.Sender.FirstName + " sent a video." : "You sent a video.",
                    "File" => m.LastMessage.RecipientId == userId ? m.LastMessage.Sender.FirstName + " sent a file." : "You sent a file.",
                    "Audio" => m.LastMessage.RecipientId == userId ? m.LastMessage.Sender.FirstName + "sent an audio." : "You sent an audio.",
                    "Location" => m.LastMessage.RecipientId == userId ? m.LastMessage.Sender.FirstName + " sent a live location." : "You sent a live location.",
                    _ => m.LastMessage.Content
                },
                UnreadCount = m.LastMessage.RecipientId == userId ? _context.Messages.Where(message => message.DateRead == null && message.SenderId == m.InteractingUserId && message.RecipientId == userId).Count() : 0
            })
            .ToList();

            return PagedList<UserMessageInfoDto>.CreateListAsync(messagesInfo, messageParams.PageNumber, messageParams.PageSize);
        }
    }
}