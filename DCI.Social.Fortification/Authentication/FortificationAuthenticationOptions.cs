using DCI.Social.Fortification.Configuration;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Fortification.Authentication;
public class FortificationAuthenticationOptions : AuthenticationSchemeOptions
{
    public FortificationConfiguration FortificationConfiguration { get; set; }


}
