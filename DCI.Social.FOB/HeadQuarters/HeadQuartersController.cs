using DCI.Social.Fortification.Encryption;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DCI.Social.FOB.Controllers;

[AllowAnonymous]
[Route("hq")]
public class HeadQuartersController : ControllerBase
{

    [HttpGet("setup-shop")]
    public async Task<string> SetupShop([FromServices] IFortificationEncryptionService encryptionService) => 
        await encryptionService.EncryptSymmetricKey();

    [HttpGet("ping")]
    public string Ping([FromServices] ILogger<HeadQuartersController> logger)
    {
        logger.LogInformation($"Pinged by: {HttpContext.Connection.RemoteIpAddress?.ToString()}");
        return "pong";
    }

}
