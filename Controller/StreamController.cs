using CameraStreamingApi.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CameraStreamingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StreamController : ControllerBase
    {
        private readonly RtspService _rtspService;
        private readonly CameraControlService _cameraControlService;
        private readonly IConfiguration _configuration;

        public StreamController(RtspService rtspService, CameraControlService cameraControlService, IConfiguration configuration)
        {
            _rtspService = rtspService;
            _cameraControlService = cameraControlService;
            _configuration = configuration;
        }

        [HttpGet("camera")]
        public IActionResult GetCameraStream()
        {
            string rtspUrl = _configuration["RtspSettings:Url"];
            _rtspService.StartRtspToHls(rtspUrl);

            string baseUrl = $"{Request.Scheme}://{Request.Host}";
            string streamUrl = $"{baseUrl}/hls/output.m3u8";

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

        [HttpPost("control/{direction}")]
        public async Task<IActionResult> ControlCamera(string direction)
        {
            if (!new[] { "up", "down", "left", "right" }.Contains(direction.ToLower()))
            {
                return BadRequest("Invalid direction. Use 'up', 'down', 'left', or 'right'.");
            }

            bool success = await _cameraControlService.MoveCamera(direction);
            return success ? Ok($"Camera moved {direction}") : BadRequest("Failed to control camera");
        }

        [HttpPost("zoom/{operation}")]
        public async Task<IActionResult> ZoomCamera(string operation)
        {
            if (!new[] { "in", "out" }.Contains(operation.ToLower()))
            {
                return BadRequest("Invalid zoom operation. Use 'in' or 'out'.");
            }

            bool success = await _cameraControlService.Zoom(operation);
            return success ? Ok($"Camera zoomed {operation}") : BadRequest("Failed to zoom camera");
        }
        [HttpPost("stop")]
        public async Task<IActionResult> StopCamera()
        {
            bool success = await _cameraControlService.StopCamera();
            return success ? Ok("Camera stopped") : BadRequest("Failed to stop camera");
        }
    }
}
