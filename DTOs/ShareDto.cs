namespace SecureFileStorage.DTOs
{
    // Paylaşım DTO
    public class ShareDto
    {
        public int FileId { get; set; }
        public int SharedWithUserId { get; set; }
        public string PermissionLevel { get; set; }
    }

}
