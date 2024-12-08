using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Identity.Configuration;
public class IdentityConfiguration { 
    public const string ConfigurationElementName = "Identity";
    public string OwnCrtFile { get; set; }
    public string OwnKeyFile {get; set; }

    public bool ChangeSchemeToHttps {get; set;}



}
