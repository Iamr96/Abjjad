// Program.cs
using Abjjad.Interface;
using Abjjad.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure storage
builder.Services.Configure<StorageSettings>(builder.Configuration.GetSection("Storage"));

// Register services
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
builder.Services.AddSingleton<IImageProcessor, ImageProcessor>();
builder.Services.AddSingleton<IExifDataExtractor, ExifDataExtractor>();
builder.Services.AddScoped<IImageService, ImageService>();
// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll"); // Add this line here
app.UseAuthorization();
app.MapControllers();

app.Run();