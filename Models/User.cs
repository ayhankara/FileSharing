using System.ComponentModel.DataAnnotations;

namespace SecureFileStorage.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string PasswordSalt { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        public int IsActive { get; set; } = 1;

        public int IsDelete { get; set; } = 0;

        // İlişkiler (dosya ve klasörler için - önceki kodunuzdan)
        public virtual ICollection<File> Files { get; set; }
        public virtual ICollection<Folder> Folders { get; set; }
    }
}
