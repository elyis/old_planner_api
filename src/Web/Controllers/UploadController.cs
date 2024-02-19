using System.Net;
using old_planner_api.src.Domain.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeDetective;
using Swashbuckle.AspNetCore.Annotations;
using webApiTemplate.src.App.IService;
using System.ComponentModel.DataAnnotations;

namespace old_planner_api.src.Web.Controllers
{
    [ApiController]
    [Route("api/upload")]
    public class UploadController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IFileUploaderService _fileUploaderService;
        private readonly ContentInspector _contentInspector;
        private readonly string _supportedIconMime;


        public UploadController(
            IUserRepository userRepository,
            IJwtService jwtService,
            IFileUploaderService fileUploaderService,
            ContentInspector contentInspector
        )
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _fileUploaderService = fileUploaderService;
            _contentInspector = contentInspector;
            _supportedIconMime = "image/";
        }

        [HttpPost("profileIcon"), Authorize]
        [SwaggerOperation("Загрузить иконку профиля")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(string))]

        public async Task<IActionResult> UploadProfileIcon(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var file = Request.Form.Files.FirstOrDefault();
            var resultUpload = await UploadIconAsync(Constants.localPathToProfileIcons, file);

            if (resultUpload is OkObjectResult result)
            {
                var filename = (string)result.Value;
                var tokenInfo = _jwtService.GetTokenInfo(token);
                await _userRepository.UpdateProfileIconAsync(tokenInfo.UserId, filename);
            }
            return resultUpload;
        }


        [HttpGet("profileIcon/{filename}")]
        [SwaggerOperation("Получить иконку профиля")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(File))]
        [SwaggerResponse(404, Description = "Неверное имя файла")]

        public async Task<IActionResult> GetProfileIcon([Required] string filename)
            => await GetIconAsync(Constants.localPathToProfileIcons, filename);

        private async Task<IActionResult> GetIconAsync(string path, string filename)
        {
            var bytes = await _fileUploaderService.GetStreamFileAsync(path, filename);
            if (bytes == null)
                return NotFound();

            var fileExtension = Path.GetExtension(filename);
            return File(bytes, $"image/{fileExtension}", filename);
        }

        private async Task<IActionResult> UploadIconAsync(string path, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var stream = file.OpenReadStream();
            var mimeTypes = _contentInspector.Inspect(stream).ByMimeType();
            var bestMatchMimeType = mimeTypes.MaxBy(e => e.Points)?.MimeType;
            if (string.IsNullOrEmpty(bestMatchMimeType) || !bestMatchMimeType.StartsWith(_supportedIconMime))
                return new UnsupportedMediaTypeResult();

            var fileExtension = bestMatchMimeType.Split("/").Last();
            string? filename = await _fileUploaderService.UploadFileAsync(path, stream, $".{fileExtension}");
            if (filename == null)
                return BadRequest("Failed to upload the file");

            return Ok(filename);
        }
    }
}