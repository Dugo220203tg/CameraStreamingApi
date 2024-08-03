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
        private double _currentPan = 0; // Trạng thái hiện tại của pan
        private double _currentTilt = 0; // Trạng thái hiện tại của tilt
        private double _currentZoom = 0; // Trạng thái hiện tại của zoom

        private readonly double _panStep = 350; // Bước di chuyển pan (độ)
        private readonly double _tiltStep = 350; // Bước di chuyển tilt (độ)
        private readonly double _zoomStep = 0.5; // Bước di chuyển zoom

        public CameraControlService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            string username = _configuration["CameraSettings:Username"];
            string password = _configuration["CameraSettings:Password"];
            string authString = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{username}:{password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authString);
        }

        public async Task<bool> MoveCamera(string direction)
        {
            UpdatePtzState(direction);

            string cameraIp = _configuration["CameraSettings:IpAddress"];
            string url = $"http://{cameraIp}/ISAPI/PTZCtrl/channels/1/continuous";
            string xmlBody = GetPtzXml();

            var content = new StringContent(xmlBody, System.Text.Encoding.UTF8, "application/xml");
            var response = await _httpClient.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                // Gửi lệnh dừng sau lệnh di chuyển
                await StopCamera();
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
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
            string cameraIp = _configuration["CameraSettings:IpAddress"];
            string url = $"http://{cameraIp}/ISAPI/PTZCtrl/channels/1/continuous";
            string xmlBody = GetZoomXml(operation);

            var content = new StringContent(xmlBody, System.Text.Encoding.UTF8, "application/xml");
            var response = await _httpClient.PutAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                // Gửi lệnh dừng sau lệnh zoom
                await StopCamera();
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to zoom camera. Status code: {response.StatusCode}, Response: {responseContent}");
            }

            return response.IsSuccessStatusCode;
        }

        private string GetZoomXml(string operation)
        {
            int zoom = operation.ToLower() == "in" ? 1 : -1;

            return $@"
                <?xml version=""1.0"" encoding=""UTF-8""?>
                <PTZData>
                    <zoom>{zoom}</zoom>
                </PTZData>";
        }

        //public async Task<bool> Zoom(string operation)
        //{
        //    UpdateZoomState(operation);

        //    string cameraIp = _configuration["CameraSettings:IpAddress"];
        //    string url = $"http://{cameraIp}/ISAPI/PTZCtrl/channels/1/continuous"; // Đảm bảo endpoint chính xác
        //    string xmlBody = GetZoomXml();

        //    var content = new StringContent(xmlBody, System.Text.Encoding.UTF8, "application/xml");
        //    var response = await _httpClient.PutAsync(url, content);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        // Gửi lệnh dừng sau lệnh zoom
        //        await StopCamera();
        //    }
        //    else
        //    {
        //        var responseContent = await response.Content.ReadAsStringAsync();
        //        Console.WriteLine($"Failed to zoom camera. Status code: {response.StatusCode}, Response: {responseContent}");
        //    }

        //    return response.IsSuccessStatusCode;
        //}


        //private void UpdateZoomState(string operation)
        //{
        //    if (operation.ToLower() == "in")
        //    {
        //        _currentZoom += _zoomStep;
        //    }
        //    else if (operation.ToLower() == "out")
        //    {
        //        _currentZoom -= _zoomStep;
        //    }

        //    // Đảm bảo giá trị zoom nằm trong phạm vi hợp lệ (ví dụ: 1 đến 25)
        //    _currentZoom = Math.Max(1, Math.Min(_currentZoom, 25));
        //}


        //private string GetZoomXml()
        //{
        //    return $@"
        //<?xml version=""1.0"" encoding=""UTF-8""?>
        //<PTZData>
        //    <zoom>
        //        <value>{_currentZoom}</value>
        //    </zoom>
        //</PTZData>";
        //}


        public async Task<bool> StopCamera()
        {
            string cameraIp = _configuration["CameraSettings:IpAddress"];
            string url = $"http://{cameraIp}/ISAPI/PTZCtrl/channels/1/continuous";  // Thay đổi URL nếu cần
            string xmlBody = @"
                <?xml version=""1.0"" encoding=""UTF-8""?>
                <PTZData>
                    <pan>0</pan>
                    <tilt>0</tilt>
                    <zoom>0</zoom>
                </PTZData>";

            var content = new StringContent(xmlBody, System.Text.Encoding.UTF8, "application/xml");
            var response = await _httpClient.PutAsync(url, content);

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Stop camera response. Status code: {response.StatusCode}, Response: {responseContent}");

            return response.IsSuccessStatusCode;
        }
    }
}
