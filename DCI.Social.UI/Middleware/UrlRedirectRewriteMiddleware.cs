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
       var requestPath = context.Request.Path.HasValue ? context.Request.Path.Value.ToLower() : "";
       _logger.LogInformation($"Unprocessed requestPath: {requestPath}");
        if(_rewriteToHttps) {
            context.Request.Headers["X-Forwarded-Proto"]="https";
            context.Request.Headers["X-Original-Proto"]="https";
        }
        if(_basePath != null) {
            var lastSlash = _redirectPath.LastIndexOf("/");
            var redirUrlEnding = _redirectPath.Substring(lastSlash);
            if(requestPath.EndsWith(redirUrlEnding.ToLower())) {
                var editedRequestPath = requestPath.Replace(redirUrlEnding, "");
                editedRequestPath = editedRequestPath + _redirectPath;
                _logger.LogInformation($"Changed request path: {requestPath} to: {editedRequestPath}");
                context.Request.Path = editedRequestPath;   

            }
            /*if(requestPath.Length > 0 && !requestPath.ToLower().Contains(_basePath.ToLower()) && _basePath.Length > 0) 
            {
                var editedRequestPath = requestPath[0] == '/' ? 
                    $"/{_basePath}{requestPath}" : 
                    $"/{_basePath}/{requestPath}";
                context.Request.Path = editedRequestPath;
                _logger.LogInformation($"Changed request path: {requestPath} to: {editedRequestPath}");
            }*/
            /*if(requestPath.Contains(_basePath)) {
                var editedRequestPath = requestPath.Replace(_basePath, "");
                if(editedRequestPath.StartsWith("//"))
                   editedRequestPath = editedRequestPath.Substring(1, editedRequestPath.Length - 1);
                _logger.LogInformation($"Changed request path: {requestPath} to: {editedRequestPath}");
            }*/




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