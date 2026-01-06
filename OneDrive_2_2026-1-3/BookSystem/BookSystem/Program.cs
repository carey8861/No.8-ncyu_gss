using BookSystem.Model;
using Microsoft.Extensions.FileProviders;
using System.IO;
using BookSystem.Service;

var builder = WebApplication.CreateBuilder(args);

// 其他服務
builder.Services.AddControllers();

// 註冊 service（BookService 使用 Dapper 的實作）
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<CodeService>();

var app = builder.Build();

// 靜態檔案：把 workspace 的 Integrate 資料夾當成靜態檔案來源
var env = app.Environment;
var integratePath = Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "..", "Integrate"));
if (Directory.Exists(integratePath))
{
	var fileProvider = new PhysicalFileProvider(integratePath);
	var defaultFilesOptions = new DefaultFilesOptions { FileProvider = fileProvider };
	defaultFilesOptions.DefaultFileNames.Clear();
	defaultFilesOptions.DefaultFileNames.Add("index.html");
	app.UseDefaultFiles(defaultFilesOptions);
	app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });
}

app.MapControllers();
app.Run();