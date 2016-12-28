using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public static class Program
{
    private const string REGEX = "Version\\s*=\\s*\"([0-9.]*)\"";
    private const string REGEX1 = "BuildDate\\s*=\\s*\"([0-9-]*)\"";
    private const string REGEX2 = "Commit\\s*=\\s*\"(.*)\"";

    static int Main(string[] args)
    {
        return PerformJob(args?[0]?.Trim('"'), args?[1]?.Trim('"'));
    }

    static int PerformJob(string fn, string path)
    {
        if (string.IsNullOrEmpty(fn))
            return Error("File not specified");

        string content;

        try
        {
            content = File.ReadAllText(fn);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }

        var regex = new Regex(REGEX);
        var m = regex.Match(content);

        if (!m.Success || m.Groups.Count < 2)
            return Error("Unable to find pattern.");

        var g = m.Groups[1];
        Version v;

        try
        {
            v = new Version(g.Value);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }

        var rev = v.Revision + 1;
        var sb = new StringBuilder(content);
        sb.Replace(g.Value, $"{v.Major}.{v.Minor}.{v.Build}.{rev}", g.Index - 1, g.Value.Length + 1);

        regex = new Regex(REGEX1);
        m = regex.Match(content);

        if (m.Success && m.Groups.Count > 1)
        {
            g = m.Groups[1];
            sb.Replace(g.Value, DateTime.Today.ToString("yyyy-MM-dd"), g.Index - 1, g.Value.Length + 1);
        }

        regex = new Regex(REGEX2);
        m = regex.Match(content);

        if (m.Success && m.Groups.Count > 1)
        {
            g = m.Groups[1];
            sb.Replace(g.Value, GetGitHash(path), g.Index - 1, g.Value.Length + 1);
        }

        try
        {
            File.WriteAllText(fn, sb.ToString());
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }

        return OK(rev);
    }

    static string GetGitHash(string path)
    {
        var p = new Process();
        var processInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            FileName = "git.exe",
            CreateNoWindow = true,
            WorkingDirectory = (path != null && Directory.Exists(path)) ? path : Environment.CurrentDirectory
        };
        p.StartInfo = processInfo;
        p.StartInfo.Arguments = "rev-parse HEAD";
        p.Start();

        var output = p.StandardOutput.ReadToEnd().Trim();
        p.WaitForExit();
        return output;
    }

    static int Error(string reason = null)
    {
        Console.WriteLine($"Build number not incremented. Reason: {reason ?? "not specified"}");
        return 1;
    }

    static int OK(int rev)
    {
        Console.WriteLine($"Build number is incremented. New build number: {rev}");
        return 0;
    }
}
