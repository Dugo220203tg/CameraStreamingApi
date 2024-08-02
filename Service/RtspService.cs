using System.Diagnostics;
using System.Text;

public class RtspService
{
    private Process? _process;
    private readonly IConfiguration _configuration;

    public RtspService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void StartRtspToHls(string rtspUrl)
    {
        var outputPath = Path.Combine(_configuration["WebRootPath"], "hls");
        Console.WriteLine($"Output path: {outputPath}");

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        var outputUrl = Path.Combine(outputPath, "output.m3u8");

        var processStartInfo = new ProcessStartInfo
        {
            FileName = @"C:\FFMPEG\bin\ffmpeg.exe",
            Arguments = $"-rtsp_transport tcp -i \"{rtspUrl}\" -c:v libx264 -hls_time 2 -hls_list_size 6 -hls_flags delete_segments \"{outputUrl}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        _process = new Process { StartInfo = processStartInfo };
        var errorOutput = new StringBuilder();
        _process.ErrorDataReceived += (sender, args) => errorOutput.AppendLine(args.Data);
        _process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data); // Log standard output
        _process.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data); // Log error output

        _process.Start();
        _process.BeginErrorReadLine();
        _process.BeginOutputReadLine(); // Start reading standard output

        // Do not wait for the process to exit
    }

    // ... (rest of the class remains the same)
}