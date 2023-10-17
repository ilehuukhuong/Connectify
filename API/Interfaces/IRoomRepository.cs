using API.Entities;

namespace API.Interfaces
{
    public interface IRoomRepository
    {
        Task<Room> GetRoomForConnection(string connectionId);
        void RemoveConnection(Connection connection);
        Task<Room> GetRoom(string roomName);
        void AddRoom(Room room);
        void AddCall(Call call);
        void RemoveRoom(Room room);
        Task<Call> FindCall(string caller, string recipient);
        void UpdateCall(Call call);
        bool CheckCall(string caller, string recipient);
        Task<bool> CheckUserInCall(string username);
    }
}