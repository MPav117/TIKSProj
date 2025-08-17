using Microsoft.AspNetCore.SignalR;
using Models;
using Services;

namespace Hubs;
public class ChatHub : Hub
{
    private readonly ChatService _chatService;
    private readonly ZanatstvoContext _context;

    public ChatHub(ChatService chatService, ZanatstvoContext context)
    {
        _chatService = chatService;
        _context = context;
    }
    public async Task JoinChat()//string chatRoom
    {
        await Clients.All
            .SendAsync("ReceiveMessage", "admin", "you have joined ");
    }

    public async Task JoinSpecificChatRoom(string chatRoom)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatRoom);

        //_shared.connections[Context.ConnectionId] = conn;

        await Clients.Group(chatRoom)
            .SendAsync("JoinSpecificChatRoom", "admin", $"you have joined {chatRoom}");
    }

    public async Task SendMessage(string chatRoom, string message, int senderId, int receiverId)
    {

        Console.WriteLine(chatRoom);

        var sender = await _context.Korisnici
            .Include(k => k.Identitet)
            .FirstOrDefaultAsync(k => k.ID == senderId);

        var receiver = await _context.Korisnici
            .Include(k => k.Identitet)
            .FirstOrDefaultAsync(k => k.ID == receiverId);

        if (sender != null && receiver != null)
        {
            var chatMessage = new ChatMessage
            {
                SenderId = sender.ID,
                Sender = sender,
                ReceiverId = receiver.ID,
                Receiver = receiver,
                ChatRoom = chatRoom,
                Message = message
            };

            await _chatService.SaveMessageAsync(chatMessage);
            await Clients.Group(chatRoom).SendAsync("ReceiveMessage", sender.Identitet.Username, message);
        }
    }



    public async Task LoadMessages(string chatRoom)
    {
        try
        {
            var messages = await _chatService.GetMessagesAsync(chatRoom);
            var messageDtos = messages.Select(m => new
            {
                SenderUsername = m.Sender?.Identitet?.Username ?? "Unknown",
                Message = m.Message
            }).ToList();

            foreach (var msg in messageDtos)
            {
                Console.WriteLine($"Loaded message from {msg.SenderUsername}: {msg.Message}");
            }

            await Clients.Caller.SendAsync("LoadMessages", messageDtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Greška pri učitavanju poruka za chatRoom {chatRoom}: {ex}");
            await Clients.Caller.SendAsync("Error", "Došlo je do greške prilikom učitavanja poruka.");
        }
    }

}