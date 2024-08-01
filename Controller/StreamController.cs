using Microsoft.AspNetCore.Mvc;

namespace CameraStreamingApi.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class StreamController : ControllerBase
    {
        private readonly RtspService _rtspService;

        public StreamController(RtspService rtspService)
        {
            _rtspService = rtspService;
        }

        [HttpGet("camera")]
        public async Task<IActionResult> GetCameraStream()
        {
            string rtspUrl = "rtsp://itsvn:Itsvn@123@14.224.129.98:554/Streaming/Channels/101?transportmode=unicast&profile=Profile_1";
            string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "hls");

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            try
            {
                string hlsUrl = await _rtspService.ConvertRtspToHlsAsync(rtspUrl, outputPath);
                return Ok(new { streamUrl = $"/hls/{Path.GetFileName(hlsUrl)}" });
            }
            catch (Exception ex)
            {
                return Problem($"Error converting RTSP to HLS: {ex.Message}");
            }
        }
    }
}
