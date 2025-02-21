namespace SecureFileStorage.DTOs
{
    public class FileUpdateDto
    {
        public string Name { get; set; }
        public int? FolderId { get; set; } // Nullable int, güncellenmeyebilir
    }
}
