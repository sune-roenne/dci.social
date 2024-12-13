// See https://aka.ms/new-console-template for more information
using DCI.Social.Fortification;
using DCI.Social.Fortification.Encryption;
using DCI.Social.HeadQuarters;
using DCI.Social.HeadQuarters.FOB;
using DCI.Social.Spelunking;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QRCoder;

var gener = new QRCodeGenerator();
var codeData = gener.CreateQrCode("https://eutonies.com/blackops2024/", QRCodeGenerator.ECCLevel.Q);
var code = new SvgQRCode(codeData);
var graphic = code.GetGraphic(20);
File.WriteAllText("c:\\temp2\\blacops2024.qr.svg", graphic);


var builder = WebApplication.CreateBuilder();
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.local.json", optional: true);
builder.AddFortificationConfiguration();
builder.AddHeadQuartersConfiguration();


builder.AddHeadQuarters();
builder.AddFortificationEncryption();
builder.Services.AddHttpClient();

var app = builder.Build();

var service = app.Services.GetRequiredService<IFOBService>();

await Task.Delay(110000);

var tessa = "";