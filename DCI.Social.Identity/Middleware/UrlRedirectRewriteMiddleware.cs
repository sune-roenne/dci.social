using DCI.Social.Domain.Util;
using DCI.Social.Identity.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DCI.Social.Identity.Middleware;
public class UrlRedirectRewriteMiddleWare {
    private readonly RequestDelegate _next;
    private readonly ILogger<UrlRedirectRewriteMiddleWare> _logger;
    private bool _rewriteToHttps;

    public UrlRedirectRewriteMiddleWare(
        RequestDelegate next, 
        ILogger<UrlRedirectRewriteMiddleWare> logger,
        IOptions<IdentityConfiguration> conf
        )
    {
        _next = next;
        _logger = logger;
        _rewriteToHttps = conf.Value.ChangeSchemeToHttps;
    }


    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogDebug($"Request headers: ");
        var headersString = context.Request.Headers
            .Select(_ => (_.Key, (string?) _.Value.ToString()))
            .AsLoggableString();
        _logger.LogInformation(headersString);

        //if(_rewriteToHttps)
           // context.Request.Scheme = "https";

        await _next(context);
    }


}