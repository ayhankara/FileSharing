namespace SecureFileStorage.DTOs
{
    // Klasör Oluşturma DTO
    public class FolderCreateDto
    {
        public string Name { get; set; }
        public int? ParentFolderId { get; set; }
    }
}
