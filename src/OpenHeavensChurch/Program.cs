using OpenHeavensChurch.Composing;
using OpenHeavensChurch.Services;

var builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

// Site-specific services. These are registered alongside Umbraco's container so
// that surface controllers and view components can resolve them.
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IYouTubeService, YouTubeService>();
builder.Services.AddSingleton<ISongsService, SongsService>();
builder.Services.AddSingleton<IArticlesService, ArticlesService>();
builder.Services.AddScoped<IContactSubmissionService, ContactSubmissionService>();

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
