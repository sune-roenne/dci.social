﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Fortification.Configuration;
public class FortificationConfiguration
{
    public const string ConfigurationElementName = "Fortification";

    public string TrustedCertificateFile { get; set; }
    public string? TrustedPrivateKeyFile { get; set; }
    public string? ClientCertificateFile { get; set; }
    public string? ClientPrivateKeyFile { get; set; }


}
