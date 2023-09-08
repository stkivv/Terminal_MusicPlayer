using DAL;
using Domain;
using NAudio.Utils;
using NAudio.Wave;

namespace ConsoleApp;

public class Brain : IObservable<TimeSpan>
{
    public List<Song> Songs;
    public MusicRepo Repo;
    public OptionsRepo OptionsRepository;
    public Song SelectedSong;
    
    public bool Shuffle;
    public bool Repeat;

    private AudioFileReader? _reader;
    public WaveOutEvent? WaveOut;
    public Timer SongTimer;
    public TimeSpan RemainingTime;
    
    private readonly List<IObserver<TimeSpan>> _observers = new List<IObserver<TimeSpan>>();

    private static readonly Random Rnd = new Random();

    public Brain(MusicRepo repo, OptionsRepo optionsRepo)
    {
        Repo = repo;
        OptionsRepository = optionsRepo;
        Repeat = optionsRepo.GetRepeatOption();
        Shuffle = optionsRepo.GetShuffleOption();
        Songs = Repo.GetSongs();
        SelectedSong = Songs[0];
    }

    public void ChangeSong(Song? song )
    {
        if (SongTimer != null)
        {
            SongTimer.Dispose();
        }
        
        //given a specific song to play
        if (song != null)
        {
            SelectedSong = song;
            PlaySong();
            return;
        }

        //repeat same song
        if (Repeat)
        {
            PlaySong();
            return;
        }
        
        //new undefined song from playlist
        if (Shuffle)
        {
            int newIndex = Rnd.Next(Songs.Count);
            SelectedSong = Songs[newIndex];
            PlaySong();
        }
        else
        {
            int index = Songs.FindIndex(a => a.Title == SelectedSong.Title && a.Artist == SelectedSong.Artist);
            int newIndex = index + 1;
            if (newIndex >= Songs.Count - 1)
            {
                newIndex = 0;
            }
            SelectedSong = Songs[newIndex];
            PlaySong();
        }
        
    }

    public void Skip()
    {
        ChangeSong(null);
    }

    public void PauseUnpause()
    {
        if (WaveOut == null || _reader == null)
        {
            PlaySong();
            return;
        }
        if (WaveOut.PlaybackState == PlaybackState.Playing)
        {
            WaveOut.Pause();
            RemainingTime = SelectedSong.Duration - WaveOut.GetPositionTimeSpan();
            SongTimer.Dispose();
            return;
        }
        if (WaveOut.PlaybackState == PlaybackState.Paused)
        {
            WaveOut.Play();
            SetTimeOut(RemainingTime);
            RemainingTime = TimeSpan.Zero;
        }
    }

    public void ToggleShuffle()
    {
        Shuffle = !Shuffle;
    }
    
    public void ToggleRepeat()
    {
        Repeat = !Repeat;
    }

    private void PlaySong()
    {
        if (_reader != null || WaveOut != null)
        {
            _reader!.Dispose();
            WaveOut!.Dispose();
        }
        _reader = new AudioFileReader(SelectedSong.FileLink);
        WaveOut = new WaveOutEvent();

        WaveOut.Init(_reader);
        WaveOut.Play();
        var duration = SelectedSong.Duration;
        SetTimeOut(duration);
        
        foreach (var observer in _observers)
        {
            observer.OnNext(SelectedSong.Duration);
        }
    }

    private void SetTimeOut(TimeSpan duration)
    {
        TimerCallback callback = TimerTick;
        TimeSpan duedate = TimeSpan.FromMilliseconds(duration.TotalMilliseconds);
        TimeSpan period = TimeSpan.FromMilliseconds(5000);
        SongTimer = new Timer(callback,null, duedate, period);
    }

    private void TimerTick(object state)
    {
        ChangeSong(null);
    }
    
    
    
    //===========================OBSERVER STUFF=================================
    public IDisposable Subscribe(IObserver<TimeSpan> observer)
    {
        // Check whether observer is already registered. If not, add it
        if (! _observers.Contains(observer)) {
            _observers.Add(observer);
            
            // Provide observer with existing data.
            // foreach (var item in flights)
            //     observer.OnNext(item);
        }
        return new Unsubscriber(_observers, observer);
    }

    private class Unsubscriber : IDisposable
    {
        private List<IObserver<TimeSpan>>_observers;
        private IObserver<TimeSpan> _observer;

        public Unsubscriber(List<IObserver<TimeSpan>> observers, IObserver<TimeSpan> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
}