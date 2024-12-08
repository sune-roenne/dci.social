using DCI.Social.UI;
using DCI.Social.UI.Util;

var builder = WebApplication.CreateBuilder(args)
    .AddConfiguration();

builder.AddServices();

var app = builder.Build();
app.UseMiddlewarePipeline();
app.LogConfiguration();
app.Run();
