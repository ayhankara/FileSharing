namespace SecureFileStorage.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public int? FileId { get; set; }
        public int? FolderId { get; set; }
        public int? UserId { get; set; }
        public int? AccessLevel { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? IsActive { get; set; }
        public int? IsDelete { get; set; }
    }
}
