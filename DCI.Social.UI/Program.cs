using DCI.Social.UI;

var builder = WebApplication.CreateBuilder(args)
    .AddConfiguration();

builder.AddServices();

var app = builder.Build();
app.UseMiddlewarePipeline();

app.Run();
