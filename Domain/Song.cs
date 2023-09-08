using Terminal.Gui;

namespace Domain;

public class Song
{
    public string? Title;
    public string? Artist;
    public TimeSpan Duration;
    public string? FileLink;
    public string? Album;

    public override string ToString()
    {
        int celWidth = Application.Current.Frame.Width / 3;

        string title = Title.Length >= celWidth - 2
            ? Title.Substring(0, celWidth - 5) + "..."
            : Title;
        
        string artist = Artist.Length >= celWidth - 2
            ? Artist.Substring(0, celWidth - 5) + "..."
            : Artist;
        
        string album = Album.Length >= celWidth - 2
            ? Album.Substring(0, celWidth - 5) + "..."
            : Album;
        
        var str = title!.PadRight(celWidth) + artist!.PadRight(celWidth) + album;
        return str;
    }
}