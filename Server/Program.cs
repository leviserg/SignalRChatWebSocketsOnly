

using Microsoft.AspNetCore.ResponseCompression;
using Server.Extensions;

var builder = WebApplication.CreateBuilder(args);

// =========== SERVICES ===========

builder.Services.AddCustomWebSocketProtocol();

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyCorsPolicy", builder =>
    {
        builder.SetIsOriginAllowed(origin => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
    });
});

builder.Services.AddResponseCompression(
    options => options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" })
    );

// =========== APP ===========

var app = builder.Build();

app.UseResponseCompression();

app.UseRouting();

app.UseWebSockets();

app.UseCustomWebSocketProtocol();

app.UseCors("MyCorsPolicy");

app.UseEndpoints(endpoints =>
{

});

app.Run();
