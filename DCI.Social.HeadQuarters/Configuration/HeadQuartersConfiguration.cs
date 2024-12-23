﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.HeadQuarters.Configuration;
public class HeadQuartersConfiguration
{
    public const string ConfigurationElementName = "HeadQuarters";

    public string FOBUrl { get; set; }

    public HeadQuartersDbConfiguration Db { get; set; }

    public bool Activate { get; set; } = false;

}
