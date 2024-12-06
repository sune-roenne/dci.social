using DCI.Social.FOB;

var builder = WebApplication.CreateBuilder(args);
builder
    .AddFOBConfiguration()
    .AddFOBServices();
var app = builder.Build();
app
    .UseFOBRequestPipeline();

app.Run();
