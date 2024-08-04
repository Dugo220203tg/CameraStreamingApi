using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CameraStreamingApi.Service
{
    public class CameraControlService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private double _currentPan = 0; // Current pan state
        private double _currentTilt = 0; // Current tilt state
        private double _currentZoom = 1; // Current zoom state, initialized to a valid value

        private readonly double _panStep = 350; // Pan step (degrees)
        private readonly double _tiltStep = 350; // Tilt step (degrees)
        private readonly double _zoomStep = 1; // Zoom step

        public CameraControlService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpClient = new HttpClient();

            string username = _configuration["CameraSettings:Username"]
                ?? throw new ArgumentException("CameraSettings:Username is not configured.");
            string password = _configuration["CameraSettings:Password"]
                ?? throw new ArgumentException("CameraSettings:Password is not configured.");

            string authString = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authString);
        }

        public async Task<bool> MoveCamera(string direction)
        {
            UpdatePtzState(direction);

            string cameraIp = _configuration["CameraSettings:IpAddress"]
                ?? throw new ArgumentException("CameraSettings:IpAddress is not configured.");
            string url = $"http://{cameraIp}/ISAPI/PTZCtrl/channels/1/continuous";
            string xmlBody = GetPtzXml();

            Console.WriteLine($"MoveCamera Request URL: {url}");
            Console.WriteLine($"MoveCamera Request XML: {xmlBody}");

            var content = new StringContent(xmlBody, Encoding.UTF8, "application/xml");
            var response = await _httpClient.PutAsync(url, content);

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"MoveCamera Response. Status code: {response.StatusCode}, Response: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                await StopCamera();
            }
            else
            {
                Console.WriteLine($"Failed to move camera. Status code: {response.StatusCode}, Response: {responseContent}");
            }

            return response.IsSuccessStatusCode;
        }

        private void UpdatePtzState(string direction)
        {
            switch (direction.ToLower())
            {
                case "left": _currentPan -= _panStep; break;
                case "right": _currentPan += _panStep; break;
                case "up": _currentTilt += _tiltStep; break;
                case "down": _currentTilt -= _tiltStep; break;
            }
        }

        private string GetPtzXml()
        {
            return $@"
                <?xml version=""1.0"" encoding=""UTF-8""?>
                <PTZData>
                    <pan>{_currentPan}</pan>
                    <tilt>{_currentTilt}</tilt>
                </PTZData>";
        }

        public async Task<bool> Zoom(string operation)
        {
            UpdateZoomState(operation);

            string cameraIp = _configuration["CameraSettings:IpAddress"]
                ?? throw new ArgumentException("CameraSettings:IpAddress is not configured.");
            string url = $"http://{cameraIp}/ISAPI/PTZCtrl/channels/1/continuous";
            string xmlBody = GetZoomXml();

            Console.WriteLine($"Zoom Request URL: {url}");
            Console.WriteLine($"Zoom Request XML: {xmlBody}");

            var content = new StringContent(xmlBody, Encoding.UTF8, "application/xml");
            var response = await _httpClient.PutAsync(url, content);

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Zoom Response. Status code: {response.StatusCode}, Response: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                // Đợi một khoảng thời gian ngắn để zoom có hiệu lực
                await Task.Delay(500);
                await StopCamera();
            }
            else
            {
                Console.WriteLine($"Failed to zoom camera. Status code: {response.StatusCode}, Response: {responseContent}");
            }

            return response.IsSuccessStatusCode;
        }

        private void UpdateZoomState(string operation)
        {
            if (string.Equals(operation, "in", StringComparison.OrdinalIgnoreCase))
            {
                _currentZoom = Math.Min(_currentZoom + _zoomStep, 25);
            }
            else if (string.Equals(operation, "out", StringComparison.OrdinalIgnoreCase))
            {
                _currentZoom = Math.Max(_currentZoom - _zoomStep, 1);
            }
        }

        private string GetZoomXml()
        {
            int zoomSpeed = (_currentZoom > 1) ? 50 : -50; // Positive for zoom in, negative for zoom out

            return $@"
        <?xml version=""1.0"" encoding=""UTF-8""?>
        <PTZData>
            <pan>0</pan>
            <tilt>0</tilt>
            <zoom>{zoomSpeed}</zoom>
        </PTZData>";
        }

        public async Task<bool> StopCamera()
        {
            string cameraIp = _configuration["CameraSettings:IpAddress"]
                ?? throw new ArgumentException("CameraSettings:IpAddress is not configured.");
            string url = $"http://{cameraIp}/ISAPI/PTZCtrl/channels/1/continuous";
            string xmlBody = @"
                <?xml version=""1.0"" encoding=""UTF-8""?>
                <PTZData>
                    <pan>0</pan>
                    <tilt>0</tilt>
                    <zoom>0</zoom>
                </PTZData>";

            Console.WriteLine($"StopCamera Request URL: {url}");
            Console.WriteLine($"StopCamera Request XML: {xmlBody}");

            var content = new StringContent(xmlBody, Encoding.UTF8, "application/xml");
            var response = await _httpClient.PutAsync(url, content);

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"StopCamera Response. Status code: {response.StatusCode}, Response: {responseContent}");

            return response.IsSuccessStatusCode;
        }
    }
}
