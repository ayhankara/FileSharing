namespace SecureFileStorage.Models
{
    public class Folder
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? OwnerId { get; set; }
        public int? ParentFolderId { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? IsActive { get; set; }
        public int? IsDelete { get; set; } 
        public Folder ParentFolder { get; set; } 
        public User Owner { get; set; }
        public virtual ICollection<File> Files { get; set; }
        public virtual ICollection<Folder> Subfolders { get; set; }
    }


}
