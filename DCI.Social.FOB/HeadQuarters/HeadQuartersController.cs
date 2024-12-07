using DCI.Social.Fortification.Encryption;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DCI.Social.FOB.Controllers;

[AllowAnonymous]
public class HeadQuartersController : ControllerBase
{

    [HttpGet("setup-shop")]
    public async Task<string> SetupShop([FromServices] IFortificationEncryptionService encryptionService) => 
        await encryptionService.EncryptSymmetricKey();


}
