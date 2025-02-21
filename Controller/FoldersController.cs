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
    public class FoldersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AzureBlobStorageService _blobStorageService; // Blob storage servisi

        public FoldersController(ApplicationDbContext context, AzureBlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }

        // Klasör Oluşturma
        [HttpPost("create")]
        public async Task<IActionResult> CreateFolder([FromBody] FolderCreateDto folderCreateDto)
        {
            if (folderCreateDto == null || string.IsNullOrWhiteSpace(folderCreateDto.Name))
            {
                return BadRequest("Klasör adı gereklidir.");
            }

            try
            {
                var newFolder = new Folder
                {
                    Name = folderCreateDto.Name,
                    ParentFolderId = folderCreateDto.ParentFolderId,
                    OwnerId = GetCurrentUserId() // Kullanıcı ID'sini al
                };

                _context.Folders.Add(newFolder);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetFolder), new { id = newFolder.Id }, new { message = "Klasör başarıyla oluşturuldu.", folderId = newFolder.Id });
            }
            catch (Exception ex)
            {
                // Loglama yapılabilir
                return StatusCode(500, "Klasör oluşturulurken bir hata oluştu.");
            }
        }

        // Klasör İçeriğini Listeleme
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFolder(int id)
        {
            var folder = await _context.Folders.FindAsync(id);

            if (folder == null)
            {
                return NotFound("Klasör bulunamadı.");
            }

            try
            {
                // Klasördeki dosyaları ve alt klasörleri getir
                var files = await _context.Files.Where(f => f.FolderId == id).ToListAsync();
                var subfolders = await _context.Folders.Where(f => f.ParentFolderId == id).ToListAsync();

                return Ok(new
                {
                    files = files.Select(f => new { f.Id, f.Name, f.Type, f.Size }),
                    folders = subfolders.Select(f => new { f.Id, f.Name })
                });
            }
            catch (Exception ex)
            {
                // Loglama yapılabilir
                return StatusCode(500, "Klasör içeriği alınırken bir hata oluştu.");
            }
        }

        // Klasör Silme
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFolder(int id)
        {
            var folder = await _context.Folders.FindAsync(id);

            if (folder == null)
            {
                return NotFound("Klasör bulunamadı.");
            }

            try
            {
                // Klasörü silmeden önce içindeki dosyaları ve alt klasörleri de silmek isteyebilirsiniz.
                var filesInFolder = await _context.Files.Where(f => f.FolderId == id).ToListAsync();
                foreach (var file in filesInFolder)
                {
                    // Dosyaları sil (Azure Blob Storage'daki dosyaları da sil)
                    try
                    {
                        await _blobStorageService.DeleteFileAsync(file.BlobId);
                    }
                    catch (Exception blobEx)
                    {
                        // Loglama yapılabilir: Blob silme hatasını ayrı logla
                        // Örneğin: _logger.LogError($"Blob silinirken hata oluştu: {blobEx.Message}");
                    }
                    _context.Files.Remove(file);
                }

                var subfolders = await _context.Folders.Where(f => f.ParentFolderId == id).ToListAsync();
                foreach (var subfolder in subfolders)
                {
                    // Alt klasörleri de sil (Recursive silme işlemi gerekebilir)
                    // Burada alt klasörleri silme işlemi için de aynı mantığı uygulayabilirsiniz.
                    // Ancak dikkatli olun, çok derin bir klasör yapısı varsa performans sorunları yaşanabilir.
                    _context.Folders.Remove(subfolder);
                }

                _context.Folders.Remove(folder);
                await _context.SaveChangesAsync();

                return NoContent(); // 204 No Content
            }
            catch (Exception ex)
            {
                // Loglama yapılabilir
                return StatusCode(500, "Klasör silinirken bir hata oluştu.");
            }
        }

        // Helper metot (Mevcut kullanıcının ID'sini almak için)
        private int GetCurrentUserId()
        {
            // Bu metot, mevcut kullanıcının ID'sini almalıdır.
            // Örnek olarak, kimlik doğrulama bilgilerinden (claims) kullanıcı ID'sini alabilirsiniz.
            // Bu kısım, uygulamanızdaki kimlik doğrulama sistemine göre değişecektir.
            // Örneğin:
            // var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            // if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            // {
            //     return userId;
            // }
            // Geçici olarak sabit bir ID döndürüyorum:
            return 1;
        }
    }

 
}
