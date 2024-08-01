using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSingleton<RtspService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Camera Streaming API", Version = "v1" });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        builder =>
        {
            builder.WithOrigins("http://localhost:4200") // URL của ứng dụng Angular
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

var app = builder.Build();

// Ensure the web root and hls directories exist
var webRoot = "E:\\Ngay317\\CameraStreamingApi\\wwwroot";
var hlsDirectory = Path.Combine(webRoot, "hls");

if (!Directory.Exists(webRoot))
{
    Directory.CreateDirectory(webRoot);
}

if (!Directory.Exists(hlsDirectory))
{
    Directory.CreateDirectory(hlsDirectory);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Camera Streaming API v1"));
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Serve static files from the specified directory
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
    RequestPath = ""
});

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();