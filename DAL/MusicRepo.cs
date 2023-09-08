using Domain;

namespace DAL;

public class MusicRepo
{
    public MusicRepo(string url)
    {
        URL = url;
    }
    
    public string URL { get; set; }

    public List<Song> GetSongs()
    {
        var dirFiles = Directory.GetFiles(URL);
        return (from i in dirFiles
            let tFile = TagLib.File.Create(i)
            select new Song
            {
                Artist = tFile.Tag.Performers.Length > 0 ? tFile.Tag.Performers[0] : "???",
                Duration = tFile.Properties.Duration,
                FileLink = i,
                Title = tFile.Tag.Title ?? tFile.Name.Split("\\").Last(),
                Album = tFile.Tag.Album ?? "???"
            }).OrderBy(s => s.Album).ToList();
    }
}