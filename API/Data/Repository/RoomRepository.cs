
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repository
{
    public class RoomRepository : IRoomRepository
    {
        private readonly DataContext _context;
        public RoomRepository(DataContext context)
        {
            _context = context;
        }
        public void AddCall(Call call)
        {
            _context.Calls.Add(call);
        }

        public void AddRoom(Room room)
        {
            _context.Rooms.Add(room);
        }

        public void UpdateCall(Call call)
        {
            _context.Calls.Update(call);
        }

        public bool CheckCall(string caller, string recipient)
        {
            if (_context.Calls.Where(x => ((x.CallerUsername == caller && x.RecipientUsername == recipient) || (x.CallerUsername == recipient && x.RecipientUsername == caller)) && x.EndTime == null).FirstOrDefault() != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<Call> FindCall(string caller, string recipient)
        {
            return await _context.Calls.Where(x => ((x.CallerUsername == caller && x.RecipientUsername == recipient) || (x.CallerUsername == recipient && x.RecipientUsername == caller)) && x.EndTime == null).FirstOrDefaultAsync();
        }

        public async Task<Room> GetRoom(string roomName)
        {
            return await _context.Rooms.Include(x => x.Connections).FirstOrDefaultAsync(x => x.Name == roomName);
        }

        public async Task<Room> GetRoomForConnection(string connectionId)
        {
            return await _context.Rooms.Include(x => x.Connections)
                .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        public void RemoveRoom(Room room)
        {
            _context.Rooms.Remove(room);
        }

        public async Task<bool> CheckUserInCall(string username)
        {
            if (await _context.Calls.Where(x => (x.CallerUsername == username || x.RecipientUsername == username) && x.EndTime == null).FirstOrDefaultAsync() != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}