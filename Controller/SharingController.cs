using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SecureFileStorage.Models;
using SecureFileStorage.Services;
using SecureFileStorage.DTOs;
namespace SecureFileStorage.Controller
{


    [Authorize] // Kimlik doğrulama gerektir
    [ApiController]
    [Route("api/[controller]")]
    public class SharingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AzureBlobStorageService _blobStorageService; // Blob storage servisi

        public SharingController(ApplicationDbContext context, AzureBlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }

        // Dosya/Klasör Paylaşma
        [HttpPost("share")]
        public async Task<IActionResult> ShareFile([FromBody] ShareDto shareDto)
        {
            if (shareDto == null || shareDto.FileId <= 0 || shareDto.SharedWithUserId <= 0 || string.IsNullOrWhiteSpace(shareDto.PermissionLevel))
            {
                return BadRequest("Geçersiz paylaşım bilgileri.");
            }

            var file = await _context.Files.FindAsync(shareDto.FileId);
            var sharedWithUser = await _context.Users.FindAsync(shareDto.SharedWithUserId);

            if (file == null || sharedWithUser == null)
            {
                return NotFound("Dosya veya kullanıcı bulunamadı.");
            }

            try
            {
                var newShare = new SharedFile
                {
                    FileId = shareDto.FileId,
                    SharedWithUserId = shareDto.SharedWithUserId,
                    PermissionLevel = shareDto.PermissionLevel,
                    ShareLink = GenerateShareLink() // Paylaşım linki oluştur
                };

                _context.SharedFiles.Add(newShare);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetSharedFiles), new { userId = shareDto.SharedWithUserId }, new { message = "Dosya başarıyla paylaşıldı.", shareId = newShare.Id });
            }
            catch (Exception ex)
            {
                // Loglama yapılabilir
                return StatusCode(500, "Dosya paylaşılırken bir hata oluştu.");
            }
        }

        // Benimle Paylaşılanları Listeleme
        [HttpGet("sharedWithMe/{userId}")]
        public async Task<IActionResult> GetSharedFiles(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Geçersiz kullanıcı ID.");
            }

            try
            {
                var sharedFiles = await _context.SharedFiles
                                                .Where(sf => sf.SharedWithUserId == userId)
                                                .Include(sf => sf.File) // Dosya bilgilerini de getir
                                                .ToListAsync();

                var result = sharedFiles.Select(sf => new
                {
                    sf.Id,
                    sf.FileId,
                    FileName = sf.File.Name,
                    sf.PermissionLevel
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Loglama yapılabilir
                return StatusCode(500, "Paylaşılan dosyalar alınırken bir hata oluştu.");
            }
        }

        // Paylaşımı Kaldırma (Sharing'i Silme)
        [HttpDelete("share/{id}")]
        public async Task<IActionResult> DeleteShare(int id)
        {
            var share = await _context.SharedFiles.FindAsync(id);

            if (share == null)
            {
                return NotFound("Paylaşım bilgisi bulunamadı.");
            }

            try
            {
                // Paylaşımı sil
                _context.SharedFiles.Remove(share);
                await _context.SaveChangesAsync();

                return NoContent(); // 204 No Content
            }
            catch (Exception ex)
            {
                // Loglama yapılabilir
                return StatusCode(500, "Paylaşım silinirken bir hata oluştu.");
            }
        }


        //Dosyayı silme
        [HttpDelete("file/{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var file = await _context.Files.FindAsync(id);

            if (file == null)
            {
                return NotFound("Dosya bulunamadı.");
            }
            try
            {
                await _blobStorageService.DeleteFileAsync(file.BlobId);

                _context.Files.Remove(file);
                await _context.SaveChangesAsync();
                return NoContent(); // 204 No Content
            }
            catch (Exception ex)
            {
                // Loglama yapılabilir
                return StatusCode(500, "Dosya silinirken bir hata oluştu.");
            }
        }

        // Helper metotlar
        private string GenerateShareLink()
        {
            // Güvenli ve benzersiz bir paylaşım linki oluştur
            // Örnek olarak: Guid.NewGuid().ToString() kullanılabilir
            return Guid.NewGuid().ToString();
        }
    }


}
