using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureFileStorage.DTOs;
using SecureFileStorage.Services;
using SecureFileStorage.Services.Interfaces;
using System.Threading.Tasks;

namespace SecureFileStorage.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly AzureBlobStorageService _blobService;
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        public FilesController(ApplicationDbContext context, IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
            _context = context;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, int folderId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya yüklenemedi.");

            var uniqueFileName = _blobService.GenerateUniqueFileName(file.FileName);

            using (var stream = file.OpenReadStream())
            {
                await _blobService.UploadFileAsync(stream, uniqueFileName);
            }

            var newFile = new Models.File
            {
                Name = file.FileName,
                Type = file.ContentType,
                Size = file.Length,
                UploadDate = DateTime.UtcNow,
                OwnerId = 1, // Örnek kullanıcı ID
                FolderId = folderId,
                BlobId = uniqueFileName
            };

            _context.Files.Add(newFile);
            await _context.SaveChangesAsync();

            return Ok("Dosya başarıyla yüklendi.");
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var file = await _context.Files.FindAsync(id);

            if (file == null)
            {
                return NotFound("Dosya bulunamadı.");
            }

            try
            {
                var stream = await _blobService.DownloadFileAsync(file.BlobId);

                if (stream == null)
                {
                    return NotFound("Dosya Azure Blob Storage'da bulunamadı.");
                }

                return File(stream, file.Type, file.Name);
            }
            catch (Exception ex)
            {
                // Loglama yapılabilir
                return StatusCode(500, "Dosya indirilirken bir hata oluştu.");
            }
        }

        // Diğer dosya işlemleri (silme, güncelleme, versiyon kontrolü)

      


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var file = await _context.Files.FindAsync(id);

            if (file == null)
            {
                return NotFound("Dosya bulunamadı.");
            }

            try
            {
                await _blobService.DeleteFileAsync(file.BlobId);
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


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFile(int id, [FromBody] FileUpdateDto fileUpdateDto)
        {
            if (fileUpdateDto == null || id <= 0)
            {
                return BadRequest("Geçersiz istek.");
            }

            var file = await _context.Files.FindAsync(id);

            if (file == null)
            {
                return NotFound("Dosya bulunamadı.");
            }

            try
            {
                file.Name = fileUpdateDto.Name ?? file.Name; // Name güncellenecekse
                file.FolderId = fileUpdateDto.FolderId ?? file.FolderId; // Klasör ID güncellenecekse

                _context.Files.Update(file);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Dosya başarıyla güncellendi.", fileId = file.Id });
            }
            catch (Exception ex)
            {
                // Loglama yapılabilir
                return StatusCode(500, "Dosya güncellenirken bir hata oluştu.");
            }
        }

        // Dosya Versiyonlarını Listeleme
        [HttpGet("versions/{id}")]
        public async Task<IActionResult> GetFileVersions(int id)
        {
            var file = await _context.Files.FindAsync(id);

            if (file == null)
            {
                return NotFound("Dosya bulunamadı.");
            }

            try
            {
                var versions = await _context.FileVersions
                                                .Where(v => v.FileId == id)
                                                .ToListAsync();

                return Ok(versions);
            }
            catch (Exception ex)
            {
                // Loglama yapılabilir
                return StatusCode(500, "Dosya versiyonları alınırken bir hata oluştu.");
            }
        }
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
