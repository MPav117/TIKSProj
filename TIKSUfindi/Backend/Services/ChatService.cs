using Models;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public class ChatService
    {
        private readonly ZanatstvoContext _context;

        public ChatService(ZanatstvoContext context)
        {
            _context = context;
        }

        public async Task SaveMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ChatMessage>> GetMessagesAsync(string chatRoom)
        {
            return await _context.ChatMessages
                .Where(m => m.ChatRoom == chatRoom)
                .Include(m => m.Sender)
                .ThenInclude(s => s.Identitet)
                .Include(m => m.Receiver)
                .ThenInclude(k => k.Identitet)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }
    }
}