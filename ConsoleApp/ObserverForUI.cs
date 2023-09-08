using Timer = System.Timers.Timer;

namespace ConsoleApp;

//https://learn.microsoft.com/en-us/dotnet/standard/events/observer-design-pattern

//Observer is used to notify UI when brain plays a song.
public class ObserverForUI : IObserver<TimeSpan>
{
    private UI UI;
    public ObserverForUI(UI ui)
    {
        UI = ui;
    }

    public void OnCompleted()
    {
        throw new Exception("You shouldn't reach this line, ever");
    }

    public void OnError(Exception error)
    {
        throw new Exception("Observer error!");
    }

    public void OnNext(TimeSpan timespan)
    {
        UI.InitDuration(timespan);
    }
}