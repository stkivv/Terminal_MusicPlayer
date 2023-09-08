using ConsoleApp;
using DAL;

//==========================================SETUP=============================================
var optionsRepo = new OptionsRepo();
var repo = new MusicRepo(optionsRepo.GetPlaylistPath());
var brain = new Brain(repo, optionsRepo);

UI ui = new UI(brain, optionsRepo);
var observer = new ObserverForUI(ui);
brain.Subscribe(observer);


//==========================================PROGRAM============================================
Console.CursorVisible = false;
Console.Title = "Terminal_MusicPlayer";
ui.RunUI();
