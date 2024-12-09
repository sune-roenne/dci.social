using DCI.Social.Domain.Util;
using DCI.Social.Identity.Configuration;
using DCI.Social.UI.Configuration;
using Microsoft.Extensions.Options;

namespace DCI.Social.UI.Middleware;
public class UrlRedirectRewriteMiddleWare {
    private readonly RequestDelegate _next;
    private readonly ILogger<UrlRedirectRewriteMiddleWare> _logger;
    private bool _rewriteToHttps;
    private string? _basePath;
    private string _redirectPath;

    public UrlRedirectRewriteMiddleWare(
        RequestDelegate next, 
        ILogger<UrlRedirectRewriteMiddleWare> logger,
        IOptions<IdentityConfiguration> conf,
        IOptions<UIConfiguration> uiConf,
        IOptions<AzureAdConfiguration> azureConf
        )
    {
        _next = next;
        _logger = logger;
        _rewriteToHttps = conf.Value.ChangeSchemeToHttps;
        _basePath = string.IsNullOrWhiteSpace(uiConf.Value.HostingBasePath) ? null : uiConf.Value.HostingBasePath;
        _redirectPath = azureConf.Value.CallbackPath;
    }


    public async Task InvokeAsync(HttpContext context)
    {
        if(_rewriteToHttps) {
            context.Request.Headers["X-Forwarded-Proto"]="https";
            context.Request.Headers["X-Original-Proto"]="https";
        }
        if(_basePath != null) {
            var requestPath = context.Request.Path.HasValue ? context.Request.Path.Value.ToLower() : "";
            var lastSlash = _redirectPath.LastIndexOf("/");
            var redirUrlEnding = _redirectPath.Substring(lastSlash);
            if(requestPath.EndsWith(redirUrlEnding.ToLower())) {
                requestPath = requestPath.Replace(redirUrlEnding, "");
                requestPath = requestPath + _redirectPath;
                context.Request.Path = requestPath;   
            }
        }
        _logger.LogInformation($"Request for path: {context.Request.Path}");
        _logger.LogInformation($"Request headers: ");
        var headersString = context.Request.Headers
            .Select(_ => (_.Key, (string?) _.Value.ToString()))
            .AsLoggableString();
        _logger.LogInformation(headersString);


        await _next(context);
    }


}