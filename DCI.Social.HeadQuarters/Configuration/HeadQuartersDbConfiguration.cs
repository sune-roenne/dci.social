using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.HeadQuarters.Configuration;
public class HeadQuartersDbConfiguration
{
    public string DataSource { get; set; }
    public string Password { get; set; }
    public string ConnectionString => $"{DataSource};{(Password.ToLower().StartsWith("password=") ? "" : "Password=" + Password)}";

}
