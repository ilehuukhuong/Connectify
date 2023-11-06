using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IMessageRepository
    {
        void AddMessage(Message message);
        void DeleteMessage(Message message);
        Task<Message> GetMessage(int id);
       Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName, int? lastMessageId, int pageSize);
        void AddGroup(Group group);
        void RemoveConnection(Connection connection);
        Task<Connection> GetConnection(string connectionId);
        Task<Group> GetMessageGroup(string groupName);
        Task<Group> GetGroupForConnection(string connectionId);
        Task<PagedList<UserMessageInfoDto>> GetUserMessages(int userId, MessageParams messageParams);
    }
}