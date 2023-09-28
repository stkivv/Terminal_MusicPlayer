using System.Diagnostics;
using DAL;
using Terminal.Gui;

namespace ConsoleApp;

public class OptionsUI
{
    private readonly OptionsRepo _optionsRepo;
    private readonly MusicRepo _musicRepo;

    public OptionsUI(MusicRepo repo)
    {
        _optionsRepo = new OptionsRepo();
        _musicRepo = repo;
    }

    private static readonly ColorScheme _colorScheme = new ColorScheme()
    {
        Normal = Application.Driver.MakeAttribute(Color.Black, Color.Gray),
        HotNormal = Application.Driver.MakeAttribute(Color.Black, Color.Gray),
        Focus = Application.Driver.MakeAttribute(Color.BrightYellow, Color.Gray),
        HotFocus = Application.Driver.MakeAttribute(Color.BrightYellow, Color.Gray),
    };

    public Dialog GetOptionsDialog()
    {
        var optionsDialog = new Dialog()
        {
            Title = "Options (esc to exit)",
            Text = "Choose option to change:",
            TextAlignment = TextAlignment.Centered,
            ButtonAlignment = Dialog.ButtonAlignments.Center,
            ColorScheme = _colorScheme,
            Width = Dim.Percent(70),
            Height = 5
        };
        
        var themeButton = new Button("choose theme");
        themeButton.Clicked += () =>
        {
            var themeDialog = GetThemeSelectDialog();
            Application.Run(themeDialog);
        };
        
        var playListButton = new Button("change playlist directory");
        playListButton.Clicked += () =>
        {
            var playListDialog = GetPlaylistDialog();
            Application.Run(playListDialog);
            if (!playListDialog.Canceled && playListDialog.FilePaths.Count == 1)
            {
                _optionsRepo.ChangePlaylistPath(playListDialog.FilePaths.First());
                RestartApp();
            }
        };
        
        var songAddButton = new Button("add song");
        songAddButton.Clicked += () =>
        {
            var songAddDialog = GetSongAddDialog();
            Application.Run(songAddDialog);
        };
        
        var changeOrderingButton = new Button("change song order");
        changeOrderingButton.Clicked += () =>
        {
            var changeOrderingDialog = GetOrderingDialog();
            Application.Run(changeOrderingDialog);
        };
 
        optionsDialog.AddButton(themeButton);
        optionsDialog.AddButton(changeOrderingButton);
        optionsDialog.AddButton(playListButton);
        optionsDialog.AddButton(songAddButton);

        return optionsDialog;
    }

    private Dialog GetThemeSelectDialog()
    {
        var dialog = new Dialog()
        {
            Title = "Choose theme (will restart app)",
            ButtonAlignment = Dialog.ButtonAlignments.Center,
            ColorScheme = _colorScheme,
            Width = Dim.Percent(50),
            Height = Dim.Percent(50),
            AutoSize = true
        };

        var list = new List<string>() { "Light", "Midnight", "Ocean", "Black", "Blue", "Grayscale", "Red", "DarkSide", "Matrix", 
            "Green", "Magenta", "2077"};
        var listView = new ListView(list)
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };

        listView.OpenSelectedItem += OnThemeSelect;

        dialog.Add(listView);

        return dialog;
    }

    private Dialog GetSongAddDialog()
    {
        var dialog = new Dialog()
        {
            Title = "Add song",
            ButtonAlignment = Dialog.ButtonAlignments.Center,
            ColorScheme = _colorScheme,
            Width = Dim.Percent(70),
            Height = Dim.Percent(30),
            Text = "Add song from a youtube link. This action will download the audio of the youtube video. Changes will not take effect before restart."
        };

        var textField = new TextField()
        {
            Height = 1,
            Width = Dim.Percent(100),
            Y = Pos.Center(),
            ColorScheme = new ColorScheme()
            {
                Focus = Application.Driver.MakeAttribute(Color.White, Color.Black)
            }
        };

        var confirm = new Button("confirm");
        confirm.Clicked += () =>
        {
            Application.MainLoop.Invoke(async () =>
            {
                var originalCount = _musicRepo.GetSongs(null).Count;
                dialog.Text = "please wait...";
                Task.Run(() => OnConfirmSongAdd(textField.Text.ToString()!)).Wait();
                //await Task.Delay(4000);
                await WaitUntilSongAdded(originalCount);
                dialog.Text = "Add song from a youtube link. This action will download the audio of the youtube video. Changes will not take effect before restart.";
            });

        };

        dialog.Add(textField);
        dialog.AddButton(confirm);

        return dialog;
    }
    
    private Dialog GetOrderingDialog()
    {
        var dialog = new Dialog()
        {
            Title = "Choose what the song order is based on (will restart app)",
            ButtonAlignment = Dialog.ButtonAlignments.Center,
            ColorScheme = _colorScheme,
            Width = Dim.Percent(50),
            Height = Dim.Percent(50),
            AutoSize = true
        };

        var list = new List<string>() { "ALBUM", "ARTIST", "TITLE"};
        var listView = new ListView(list)
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };

        listView.OpenSelectedItem += OnOrderingSelect;

        dialog.Add(listView);

        return dialog;
    }

    private async Task WaitUntilSongAdded(int originalCount)
    {
        // keep in mind yt-dlp creates temp files. that's why you need to check 2 conditions.
        var t = new Task(() =>
        {
            bool added = false;
            while (true)
            {
                var dirFiles = Directory.GetFiles(_musicRepo.URL).ToList();
                var newCount = dirFiles.Count;
                if (newCount == originalCount + 3) added = true;
                if (newCount == originalCount + 1 && added) return;

                Task.Delay(1000);
            }
        });
        t.Start();
        await t;
    }

    private void OnConfirmSongAdd(string url)
    {
        try
        {
            OptionsRepo.AddSong(url, _musicRepo.URL);
        }
        catch
        {
            var attr = Application.Driver.MakeAttribute(Color.White, Color.Red);
            var exceptionDialog = new Dialog()
            {
                ColorScheme = new ColorScheme()
                {
                    Normal = attr,
                    Focus = attr,
                    HotFocus = attr,
                    HotNormal = attr
                },
                Width = Dim.Percent(40),
                Height = Dim.Percent(40),
                Text = "Error saving audio. Please make sure the URL you gave is correct.",
                TextAlignment = TextAlignment.Centered,
                VerticalTextAlignment = VerticalTextAlignment.Middle,
                CanFocus = false
            };
            Application.Run(exceptionDialog);
        }
    }
    
    private static OpenDialog GetPlaylistDialog()
    {
        var dialog = new OpenDialog()
        {
            Title = "Choose playlist directory (will restart app)",
            ButtonAlignment = Dialog.ButtonAlignments.Center,
            ColorScheme = _colorScheme,
            Width = Dim.Percent(85),
            Height = Dim.Percent(85),
            DirectoryPath = "C:\\",
            CanChooseFiles = false,
            CanChooseDirectories = true,
            CanCreateDirectories = false,
            AllowsMultipleSelection = false
        };

        return dialog;
    }
    
    private void OnThemeSelect(ListViewItemEventArgs args)
    {
        string name = (string) args.Value;
        Application.MainLoop.Invoke(() =>
        {
            _optionsRepo.ChangeTheme(name);
            RestartApp();
        });
    }
    
    private void OnOrderingSelect(ListViewItemEventArgs args)
    {
        string name = (string) args.Value;
        Application.MainLoop.Invoke(() =>
        {
            _optionsRepo.SaveOrderByOption(name);
            RestartApp();
        });
    }

    private static void RestartApp()
    {
        string currentApplication = Process.GetCurrentProcess().MainModule!.FileName;
        System.Diagnostics.Process.Start(currentApplication);
        Environment.Exit(0);
    }
}