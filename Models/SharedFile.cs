namespace SecureFileStorage.Models
{
    public class SharedFile
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public File File { get; set; }
        public int SharedWithUserId { get; set; }
        public User SharedWithUser { get; set; }
        public string PermissionLevel { get; set; } // Örn: View, Edit, Comment
        public string ShareLink { get; set; }
    }


}
