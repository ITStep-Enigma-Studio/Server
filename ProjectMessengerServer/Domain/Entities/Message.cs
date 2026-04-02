namespace ProjectMessengerServer.Domain.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public Chat Chat { get; set; } = null!;
        public int MessageInChatId { get; set; }
        public int SenderId { get; set; }
        public User Sender { get; set; } = null!;
        public Guid? FileId { get; set; }
        public FileEntity? File { get; set; }
        public string? Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
