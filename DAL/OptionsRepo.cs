using System.Diagnostics;
using Domain;
using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;

namespace DAL;

public class OptionsRepo
{
    private string _url = "..\\..\\..\\..\\DAL\\AppOptions.csv";
    
    public string GetTheme()
    {
        var lines = File.ReadAllLines(_url);
        var item = lines[0].Split(",");
        return item[1];
    }
    
    public void ChangeTheme(string name)
    {
        var lines = File.ReadAllLines(_url);
        lines[0] = $"Theme,{name}";
        File.WriteAllLines(_url, lines);
    }
    
    public string GetPlaylistPath()
    {
        var lines = File.ReadAllLines(_url);
        var item = lines[1].Split(",");
        return item[1];
    }
    
    public void ChangePlaylistPath(string path)
    {
        var lines = File.ReadAllLines(_url);
        lines[1] = $"Playlist,{path}";
        File.WriteAllLines(_url, lines);
    }

    public void SaveShuffleOptions()
    {
        var lines = File.ReadAllLines(_url);
        var current = lines[2].Split(",")[1];
        lines[2] = current switch
        {
            "ON" => "Shuffle,OFF",
            "OFF" => "Shuffle,ON",
            _ => lines[2]
        };
        File.WriteAllLines(_url, lines);
    }

    public bool GetShuffleOption()
    {
        var lines = File.ReadAllLines(_url);
        var current = lines[2].Split(",")[1];
        return current switch
        {
            "ON" => true,
            "OFF" => false,
            _ => true
        };
    }
    
    public void SaveRepeatOptions()
    {
        var lines = File.ReadAllLines(_url);
        var current = lines[3].Split(",")[1];
        lines[3] = current switch
        {
            "ON" => "Repeat,OFF",
            "OFF" => "Repeat,ON",
            _ => lines[3]
        };
        File.WriteAllLines(_url, lines);
    }
    
    public bool GetRepeatOption()
    {
        var lines = File.ReadAllLines(_url);
        var current = lines[3].Split(",")[1];
        return current switch
        {
            "ON" => true,
            "OFF" => false,
            _ => false
        };
    }
    
    public string GetOrderByOption()
    {
        var lines = File.ReadAllLines(_url);
        var current = lines[4].Split(",")[1];
        return current;
    }
    
    public void SaveOrderByOption(string orderBy)
    {
        if (orderBy is not ("ALBUM" or "TITLE" or "ARTIST"))
        {
            return;
        }
        var lines = File.ReadAllLines(_url);
        lines[4] = $"OrderBy,{orderBy}";
        File.WriteAllLines(_url, lines);
    }


    public static void AddSong(string url, string playlistPath)
    {
        var command = $"cd {playlistPath} && yt-dlp --audio-format mp3 -x --audio-quality 0 {url}";
        ExecuteCommand(command);
    }
    
    private static void ExecuteCommand(string command)
    {
        var processInfo = new ProcessStartInfo("cmd.exe", "/K " + command)
        {
            CreateNoWindow = true,
            UseShellExecute = false
        };

        Process.Start(processInfo);
    }
}