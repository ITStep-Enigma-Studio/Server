namespace ProjectMessengerServer.Domain.Entities
{
    public class FileEntity
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = null!;
        public string Type { get; set; } = null!; 
        public string FileName { get; set; } = null!;
        public long Size { get; set; }
        public FileAccessType AccessType { get; set; }
        public enum FileAccessType
        {
            Public,
            Chat,
            Private
        }

        public FilePurpose Purpose { get; set; }
        public enum FilePurpose
        {
            ChatImage,
            ChatVideo,
            ChatFile,
            AvatarUser,
            AvatarChat,
            Background
        }


        public int UploadedId { get; set; }
        public User Uploaded { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
