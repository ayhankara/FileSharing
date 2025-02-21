namespace SecureFileStorage.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int? FileId { get; set; }
        public File File { get; set; }
        public string ActivityType { get; set; } // Örn: Upload, Download, Share
        public DateTime ActivityDate { get; set; }
    }


}
