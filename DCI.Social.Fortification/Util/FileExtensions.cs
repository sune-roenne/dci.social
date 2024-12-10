namespace DCI.Social.Fortification.Util;

public static class FileExtensions {
    public static string ReadCertificateFile(this string fileName) 
    {
        var read = File.ReadAllLines(fileName);
        var polished = read
           .Select(_ => _.Trim())
           .Where(_ => _.Length > 0)
           .Where(_ => !_.StartsWith("----"))
           .ToList();
        var returnee = string.Join("\n", polished);
        return returnee;   
    }
}