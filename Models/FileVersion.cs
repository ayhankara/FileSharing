namespace SecureFileStorage.Models
{
    public class FileVersion
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public File File { get; set; }
        public int VersionNumber { get; set; }
        public string BlobId { get; set; }
        public DateTime UploadDate { get; set; }
    }


}
