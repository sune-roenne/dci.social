using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Fortification.Authentication;
public static class FortificationAuthenticationConstants
{
    public const string HeaderName = "FOBFortification";
    public const string SampleString = "rfrf2rrfwesceswd";

    public const string FOBAuthenticatePath = "/fob-authenticate";

    public const string AuthenticationType = "FOBFortification";
    public static class ClaimsIdentity
    {
        public const string HqNameType = "name";
        public const string HqName = "HeadQuarters";
    }

}
