// using src.DeviceManager.Repositories;
// using src.DeviceManager.Services;
// using src.DeviceProject.Repository;
//
// var builder = WebApplication.CreateBuilder(args);
//
// // Add services to the container.
// // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
//
// var connectionString = builder.Configuration.GetConnectionString("UniversityDatabase");
//
// builder.Services.AddSingleton<IDeviceRepository>(new DeviceRepository(connectionString));
// builder.Services.AddSingleton<IDeviceService, DeviceService>();
//
// var app = builder.Build();
//
// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }
//
// app.UseHttpsRedirection();
//
//
// app.MapGet("/weatherforecast", () =>
//     {
//        
//     })
//     .WithName("GetWeatherForecast")
//     .WithOpenApi();
//
// app.Run();
//
//

using System.Text.Json.Nodes;
using src.DeviceManager.Repositories;
using src.DeviceManager.Services;
using src.DeviceProject.Repository;

var builder = WebApplication.CreateBuilder(args);

// Swagger + Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("UniversityDatabase");
builder.Services.AddSingleton<IDeviceRepository>(new DeviceRepository(connectionString));
builder.Services.AddSingleton<IDeviceService, DeviceService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ========== Minimal API Endpoints ==========

app.MapGet("/api/devices", async (IDeviceService deviceService) =>
{
    var devices = deviceService.GetAllDevices().ToList();
    return devices.Any() ? Results.Ok(devices) : Results.NotFound();
});

app.MapGet("/api/devices/{id}", async (IDeviceService deviceService, string id) =>
{
    var device = deviceService.GetDeviceById(id);
    return device != null ? Results.Json(device) : Results.NotFound();
});

app.MapPost("/api/devices", async (HttpRequest request, IDeviceService deviceService) =>
{
    var contentType = request.ContentType?.ToLower();

    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();

    return contentType switch
    {
        "application/json" => await TryAsync(async () =>
        {
            var json = JsonNode.Parse(body);
            if (json == null) return Results.BadRequest("Invalid JSON format.");

            await deviceService.AddDeviceByJson(json);
            return Results.Created();
        }),

        "text/plain" => await TryAsync(async () =>
        {
            await deviceService.AddDeviceByRawText(body);
            return Results.Created();
        }),

        _ => Results.Conflict("Unsupported Content-Type.")
    };
});

app.MapPut("/api/devices", async (HttpRequest request, IDeviceService deviceService) =>
{
    var contentType = request.ContentType?.ToLower();
    if (contentType != "application/json")
        return Results.Conflict("Unsupported Content-Type.");

    using var reader = new StreamReader(request.Body);
    var rawJson = await reader.ReadToEndAsync();
    var json = JsonNode.Parse(rawJson);

    if (json == null)
        return Results.BadRequest("Invalid JSON format.");

    try
    {
        await deviceService.UpdateDevice(json);
        return Results.Ok();
    }
    catch (FileNotFoundException e)
    {
        return Results.NotFound(e.Message);
    }
    catch (Exception e)
    {
        return Results.BadRequest(e.Message);
    }
});

app.MapDelete("/api/devices/{id}", async (IDeviceService deviceService, string id) =>
{
    try
    {
        await deviceService.DeleteDevice(id);
        return Results.Ok();
    }
    catch (FileNotFoundException e)
    {
        return Results.NotFound(e.Message);
    }
    catch (Exception e)
    {
        return Results.BadRequest(e.Message);
    }
});

app.Run();

// Helper method to wrap common try-catch usage
static IResult Try(Func<IResult> action)
{
    try
    {
        return action();
    }
    catch (Exception e)
    {
        return Results.BadRequest(e.Message);
    }
}

static async Task<IResult> TryAsync(Func<Task<IResult>> func)
{
    try
    {
        return await func();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
}

