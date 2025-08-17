namespace Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public int SenderId { get; set; }
        public required Korisnik Sender { get; set; }

        public int ReceiverId { get; set; }
        public required Korisnik Receiver { get; set; }

        public string ChatRoom { get; set; } = string.Empty;

        public string SenderUsername => Sender.Identitet.Username;
        public string ReceiverUsername => Receiver.Identitet.Username;
    }
}