using DCI.Social.UI.Configuration;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace DCI.Social.UI.Server;

public partial class App
{
    [Inject]
    public IOptions<UIConfiguration> Configuration { get; set; }

    private string AppBase => string.IsNullOrWhiteSpace(Configuration.Value.HostingBasePath) ? "/" : $"/{Configuration.Value.HostingBasePath}/";


}
