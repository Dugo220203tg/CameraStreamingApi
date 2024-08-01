using System.Diagnostics;
using System.Text;

public class RtspService
{
    public async Task<string> ConvertRtspToHlsAsync(string rtspUrl, string outputPath)
    {
        string outputUrl = Path.Combine(outputPath, "output.m3u8");

        var processStartInfo = new ProcessStartInfo
        {
            FileName = @"C:\FFMPEG\bin\ffmpeg.exe",
            Arguments = $"-rtsp_transport tcp -i \"{rtspUrl}\" -c:v libx264 -hls_time 2 -hls_list_size 6 -hls_flags delete_segments \"{outputUrl}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = processStartInfo };

        var errorOutput = new StringBuilder();

        process.ErrorDataReceived += (sender, args) => errorOutput.AppendLine(args.Data);

        process.Start();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"FFmpeg process failed with exit code {process.ExitCode}: {errorOutput}");
        }

        if (File.Exists(outputUrl))
        {
            return outputUrl;
        }
        else
        {
            throw new Exception("HLS file not created");
        }
    }
}
