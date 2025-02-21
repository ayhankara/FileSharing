// RefreshToken.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecureFileStorage.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public string JwtId { get; set; }

        public bool IsUsed { get; set; }

        public bool IsRevoked { get; set; }

        public DateTime IssuedAt { get; set; }

        public DateTime ExpiryDate { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}