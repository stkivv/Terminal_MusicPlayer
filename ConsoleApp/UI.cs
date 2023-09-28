using System.Diagnostics;
using DAL;
using Domain;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace ConsoleApp;

public class UI
{
    private Brain _brain;
    
    private readonly Stopwatch _stopwatch = new (); //this is for determining timeline filled percentage
    private TimeSpan _duration = TimeSpan.Zero; //this represents the total duration of the current selected song

    private Window _timeLineWin;
    private ProgressBar _timeline;

    private List<Song> _songs;

    private StatusBar _statusBar;

    private readonly OptionsUI _optionsUI;

    private readonly OptionsRepo _optionsRepo;

    private Attribute _nrmColor;
    private Attribute _highlightFgColor;
    private Attribute _highlightBgColor;
    private ColorScheme _baseColScheme;

    private Color _normalBg = Color.Black;
    private Color _normalFg = Color.White;
    private Color _highlightCol = Color.White;
    private Color _highlightText = Color.Black;

    private Color _timeLineUnfilled = Color.DarkGray;
    private Color _timeLineFilled = Color.White;
    

    public UI(Brain brain, OptionsRepo optionsRepo)
    {
        _brain = brain;
        Application.Init();
        
        _optionsUI = new OptionsUI(_brain.Repo);
        _optionsRepo = optionsRepo;

        SetUpColorTheme();
    }

    private void SetUpColorTheme()
    {
        var theme = _optionsRepo.GetTheme();

        switch (theme)
        {
            case "Ocean":
                _normalBg = Color.Cyan;
                _normalFg = Color.White;
                _highlightCol = Color.BrightCyan;

                _timeLineUnfilled = Color.Gray;
                _timeLineFilled = Color.BrightCyan;
                break;
            case "Black":
                _normalBg = Color.Black;
                _normalFg = Color.White;
                _highlightCol = Color.White;

                _timeLineUnfilled = Color.DarkGray;
                _timeLineFilled = Color.White;
                break;
            case "Red":
                _normalBg = Color.Red;
                _normalFg = Color.White;
                _highlightCol = Color.BrightYellow;

                _timeLineUnfilled = Color.Brown;
                _timeLineFilled = Color.BrightYellow;
                break;
            case "Matrix":
                _normalBg = Color.Black;
                _normalFg = Color.BrightGreen;
                _highlightCol = Color.BrightGreen;

                _timeLineUnfilled = Color.Green;
                _timeLineFilled = Color.BrightGreen;
                break;
            case "Blue":
                _normalBg = Color.Blue;
                _normalFg = Color.White;
                _highlightCol = Color.White;

                _timeLineUnfilled = Color.DarkGray;
                _timeLineFilled = Color.White;
                break;
            case "Green":
                _normalBg = Color.Green;
                _normalFg = Color.White;
                _highlightCol = Color.BrightGreen;

                _timeLineUnfilled = Color.Gray;
                _timeLineFilled = Color.BrightGreen;
                break;
            case "Light":
                _normalBg = Color.White;
                _normalFg = Color.Black;
                _highlightCol = Color.Blue;
                _highlightText = Color.White;

                _timeLineUnfilled = Color.Gray;
                _timeLineFilled = Color.Blue;
                break;
            case "Midnight":
                _normalBg = Color.Black;
                _normalFg = Color.DarkGray;
                _highlightCol = Color.Gray;

                _timeLineUnfilled = Color.DarkGray;
                _timeLineFilled = Color.Gray;
                break;
            case "Magenta":
                _normalBg = Color.Magenta;
                _normalFg = Color.White;
                _highlightCol = Color.BrightGreen;

                _timeLineUnfilled = Color.DarkGray;
                _timeLineFilled = Color.Gray;
                break;
            case "2077":
                _normalBg = Color.BrightYellow;
                _normalFg = Color.Black;
                _highlightCol = Color.Blue;
                _highlightText = Color.White;

                _timeLineUnfilled = Color.Blue;
                _timeLineFilled = Color.BrightBlue;
                break;
            case "Grayscale":
                _normalBg = Color.DarkGray;
                _normalFg = Color.White;
                _highlightCol = Color.Gray;
                _highlightText = Color.Black;

                _timeLineUnfilled = Color.Gray;
                _timeLineFilled = Color.White;
                break;
            case "DarkSide":
                _normalBg = Color.Black;
                _normalFg = Color.BrightRed;
                _highlightCol = Color.BrightRed;
                _highlightText = Color.Black;

                _timeLineUnfilled = Color.Red;
                _timeLineFilled = Color.BrightRed;
                break;
            default:
                _normalBg = Color.Black;
                _normalFg = Color.White;
                _highlightCol = Color.White;

                _timeLineUnfilled = Color.DarkGray;
                _timeLineFilled = Color.White;
                break;
        }

        _nrmColor = Application.Driver.MakeAttribute(_normalFg, _normalBg);
        _highlightFgColor = Application.Driver.MakeAttribute(_highlightCol, _normalBg);
        _highlightBgColor = Application.Driver.MakeAttribute(_highlightText, _highlightCol);
        _baseColScheme = new ColorScheme()
        {
            Normal = _nrmColor,
            HotFocus = _nrmColor,
            HotNormal = _nrmColor,
            Focus = _highlightBgColor
        };
        Colors.Base = _baseColScheme;
    }
    
    public void RunUI()
    {
        try
        {
            var top = Application.Top;

            var songsWin = InitSongs();
            var  searchWin = InitSearchbar(songsWin);
            InitTimeLineWin();
            InitStatusBar();
            
            top.Add(songsWin, _timeLineWin, searchWin, _statusBar);
            
            //I'm honestly not sure why, but you need to call this on searchWin if you
            //want to ensure the actual focus is on songsWin
            searchWin.SetFocus();

            Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(100), l => 
            {
                UpdateTimeLine();
                return true;
            } );
            
            Application.Run();
            
        }
        finally
        {
            Application.Shutdown();
        }
    }
    
    //==================================INIT=======================================

    private Window InitSongs()
    {
        var win = new Window("Playlist (Enter to select)")
        {
            X = 0,
            Y = 3,
            Width = Dim.Percent(100),
            Height = Dim.Percent(80) - 4
        };

        AddLabelToSongList(win);

        _songs = _brain.Repo.GetSongs(_optionsRepo.GetOrderByOption());

        if (_songs.Count < 1)
        {
            return DisplayPlaylistError();
        }
        
        var listView = new ListView(_songs){
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        
        listView.OpenSelectedItem += OnSongOpen;

        win.Add(listView);

        return win;
    }

    private static Window DisplayPlaylistError()
    {
        return new Window("ERROR - PLAYLIST EMPTY/PLAYLIST PATH INCORRECT")
        {
            X = 0,
            Y = 3,
            Width = Dim.Percent(100),
            Height = Dim.Percent(80) - 4
        };
    }

    private Window InitSearchbar( Window songsWin)
    {
        var searchWin = new Window()
        {
            X = 0,
            Y = 0,
            Height = 3,
            Width = Dim.Percent(100),
            Title = "Search song",
            ColorScheme = new ColorScheme()
            {
                Normal = _nrmColor,
                HotNormal = _highlightFgColor
            }
        };

        var searchBar = new TextField()
        {
            Height = 1,
            Width = Dim.Percent(100),
            ColorScheme = new ColorScheme()
            {
                Normal = _nrmColor,
                HotFocus = _nrmColor,
                Focus = _nrmColor,
                HotNormal = _nrmColor
            }
        };

        searchBar.TextChanged += (prev) =>
        {
            Application.MainLoop.Invoke(() =>
            {
                var filteredList = new List<Song>();
        
                filteredList.AddRange(_songs
                    .Where(s => s.Title!.ToLower().Contains(searchBar.Text.ToString()!.ToLower()) 
                                || s.Album!.ToLower().Contains(searchBar.Text.ToString()!.ToLower())
                                || s.Artist!.ToLower().Contains(searchBar.Text.ToString()!.ToLower())));

                var listView = new ListView(filteredList){
                    X = 0,
                    Y = 1,
                    Width = Dim.Fill(),
                    Height = Dim.Fill(),
                };
        
                listView.OpenSelectedItem += OnSongOpen;

                songsWin.RemoveAll();
                AddLabelToSongList(songsWin);
                songsWin.Add(listView);
                songsWin.SetNeedsDisplay();
                    
                searchWin.SetFocus();
            });
        };
            
        searchWin.Add(searchBar);
        return searchWin;
    }
    
    private void InitStatusBar()
    {
        _statusBar = new StatusBar()
        {
            ColorScheme = _baseColScheme,
        };

        var shuffleTitle = _optionsRepo.GetShuffleOption() ? "Shuffle ON (s)" : "Shuffle OFF (s)";
        var repeatTitle = _optionsRepo.GetRepeatOption() ? "Repeat ON (r)" : "Repeat OFF (r)";

        
        StatusItem[] items =
        {
            new (Key.Space, "Play (Space)", () =>
            {
                PlayPause();
                UpdateStatusBarStrings();
            }),
                
            new (Key.s, shuffleTitle, () =>
            {
                _brain.ToggleShuffle();
                UpdateStatusBarStrings();
                _optionsRepo.SaveShuffleOptions();
            }),
                
            new (Key.r, repeatTitle, () =>
            {
                _brain.ToggleRepeat();
                UpdateStatusBarStrings();
                _optionsRepo.SaveRepeatOptions();
            }),
                
            new (Key.f, "Skip (f)", () =>
            {
                _brain.Skip();
                UpdateStatusBarStrings();
            }),
            new (Key.o, "Options (o)", () =>
            {
                Dialog optionsDialog = _optionsUI.GetOptionsDialog();
                Application.Run(optionsDialog);
            })
        };

        _statusBar.Items = items;

    }
    
    private void InitTimeLineWin()
    {
        ColorScheme progressBarColScheme = new ColorScheme()
        {
            Normal = Application.Driver.MakeAttribute(_timeLineFilled, _timeLineUnfilled)
        };
        
        _timeLineWin = new Window()
        {
            X = 0,
            Y = Pos.Percent(80) - 1,
            Width = Dim.Fill(),
            Height = Dim.Percent(20),
            CanFocus = false,
        };

        _timeline = new ProgressBar()
        {
            X = 2,
            Y = Pos.Percent(50),
            Width = Dim.Fill() - 2,
            Height = 3,
            ProgressBarStyle = ProgressBarStyle.Continuous,
            Fraction = 0.0f,
            ColorScheme = progressBarColScheme
        };
        _timeLineWin.Add(_timeline);
        
    }
    

    private void OnSongOpen(ListViewItemEventArgs args)
    {
        var selectedItem = args.Value;
        Application.MainLoop.Invoke(() => _brain.ChangeSong((Song) selectedItem));
    }
    
    public void InitDuration(TimeSpan timespan)
    {
        _duration = timespan;
        _stopwatch.Restart();
        UpdateStatusBarStrings();
        _timeLineWin.Title = "[ NOW PLAYING -- " + _brain.SelectedSong.Title + " (" + _brain.SelectedSong.Album + ") ]";
    }

    //============================UPDATE=====================================

    private void AddLabelToSongList(Window win)
    {
        win.Add(new Label()
        {
            Text = "TITLE".PadRight(Application.Current.Frame.Width / 3) 
                   + "ARTIST".PadRight(Application.Current.Frame.Width / 3)
                   + "ALBUM",
            Y = 0,
            ColorScheme = new ColorScheme()
            {
                Normal = _highlightFgColor
            }
        });
    }
    
    private void UpdateTimeLine()
    {
        double percentage = _stopwatch.Elapsed.TotalSeconds / (double) _duration.TotalSeconds;
        _timeLineWin.Text = GetSongTimeElapsedText();
        _timeline.Fraction = (float) percentage;
    }
    
    private string GetSongTimeElapsedText()
    {
        string elapsedMinutes = _stopwatch.Elapsed.Minutes.ToString();
        string elapsedSeconds = _stopwatch.Elapsed.Seconds.ToString().PadLeft(2, '0');

        string totalMinutes = _duration.Minutes.ToString();
        string totalSeconds = _duration.Seconds.ToString().PadLeft(2, '0');
        
        return "\n  " + elapsedMinutes + ":" + elapsedSeconds + " / " + totalMinutes + ":" + totalSeconds;
    }
    
    private void UpdateStatusBarStrings()
    {
        var items = _statusBar.Items;
        items[0].Title = _stopwatch.IsRunning ? "Pause (Space)" : "Play (Space)";
        items[1].Title = _brain.Shuffle ? "Shuffle ON (s)" : "Shuffle OFF (s)";
        items[2].Title = _brain.Repeat ? "Repeat ON (r)" : "Repeat OFF (r)";
    }
    
    private void PlayPause()
    {
        Application.MainLoop.Invoke(() => _brain.PauseUnpause());
        if (_stopwatch.IsRunning)
        {
            _stopwatch.Stop();
        }
        else
        {
            _stopwatch.Start();
        }
    }
}