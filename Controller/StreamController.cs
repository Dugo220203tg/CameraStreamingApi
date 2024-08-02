using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CameraStreamingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StreamController : ControllerBase
    {
        private readonly RtspService _rtspService;
        private readonly IConfiguration _configuration;

        public StreamController(RtspService rtspService, IConfiguration configuration)
        {
            _rtspService = rtspService;
            _configuration = configuration;
        }

        [HttpGet("camera")]
        public IActionResult GetCameraStream()
        {
            string rtspUrl = _configuration["RtspSettings:Url"];
            _rtspService.StartRtspToHls(rtspUrl);

            string baseUrl = $"{Request.Scheme}://{Request.Host}";
            string streamUrl = $"/hls/output.m3u8";

            return Ok(new { streamUrl });
        }
        [HttpGet("check")]
        public IActionResult CheckFile()
        {
            string filePath = Path.Combine(_configuration["WebRootPath"], "hls", "output.m3u8");
            if (System.IO.File.Exists(filePath))
            {
                return Ok($"File exists at {filePath}");
            }
            return NotFound($"File not found at {filePath}");
        }
    }
}