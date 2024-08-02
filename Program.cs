using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

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
            builder.WithOrigins("http://localhost:4200")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

var app = builder.Build();

// Ensure the web root and hls directories exist
var webRoot = builder.Configuration["WebRootPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
Console.WriteLine($"WebRoot path: {webRoot}");
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
app.UseCors("AllowAngularApp");

// Serve static files from the specified directory
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(webRoot),
    RequestPath = "",
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/x-mpegURL"
});

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();