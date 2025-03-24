using MythicLauncher.Controls;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;

namespace MythicLauncher;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private GameModule currentGameModule;

    void OnModuleStateSet(object sender, ModuleState state)
    {
        switch(state)
        {
            case ModuleState.Invalid:
                LaunchButton.IsEnabled = true;
                LaunchButton.Visibility = Visibility.Collapsed;
                LaunchButton.Content = "Invalid";
                break;
            case ModuleState.Failed:
                LaunchButton.IsEnabled = true;
                LaunchButton.Visibility = Visibility.Collapsed;
                LaunchButton.Content = "Download failed - Retry?";
                break;
            case ModuleState.Idle_RequiresGame:
                LaunchButton.IsEnabled = true;
                LaunchButton.Visibility = Visibility.Visible;
                LaunchButton.Content = "Download";
                break;
            case ModuleState.Idle_RequiresUpdate:
                LaunchButton.IsEnabled = true;
                LaunchButton.Visibility = Visibility.Visible;
                LaunchButton.Content = "Update";
                break;
            case ModuleState.Idle_GameUptoDate:
                LaunchButton.IsEnabled = true;
                LaunchButton.Visibility = Visibility.Visible;
                LaunchButton.Content = "Launch";
                break;
            case ModuleState.DownloadingGame:
                LaunchButton.IsEnabled = true;
                LaunchButton.Visibility = Visibility.Visible;
                LaunchButton.Content = "Downloading";
                break;
            case ModuleState.DownloadingUpdate:
                LaunchButton.IsEnabled = true;
                LaunchButton.Visibility = Visibility.Visible;
                LaunchButton.Content = "Updating";
                break;
            case ModuleState.GameRunning:
                LaunchButton.IsEnabled = false;
                LaunchButton.Visibility = Visibility.Visible;
                LaunchButton.Content = "Running";
                break;
            default:
                break;
        }
    }

    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnContentRendered(object sender, EventArgs e)
    {
        string[] Games = Directory.GetDirectories(Directory.GetCurrentDirectory() + "\\Builds");
        foreach (string Game in Games)
        {
            string GameName = Game.Split("\\").Last();
            GameModule Module = new GameModule(GameName);
            Module.StateChanged += OnModuleStateSet;
            MythicModuleButton GameButton = new MythicModuleButton();
            GameButton.Game = Module;
            GameButton.ClickedEvent += OnModuleSelected;
            GamesPanel.Children.Add(GameButton);
        }
    }

    private void OnModuleSelected(object sender, GameModule Module)
    {
        currentGameModule = Module;
        GameName.Text = Module.GameName;
        VersionText.Text = Module.localversion.ToString();
    }

    private void OnModuleUpdated(GameModule Module)
    {
        VersionText.Text = Module.localversion.ToString();
    }

    private void Launch_Clicked(object sender, RoutedEventArgs e)
    {
        if (currentGameModule != null)
        {
            if (File.Exists(currentGameModule.gameExe) && currentGameModule.State == ModuleState.Idle_GameUptoDate)
            {
                Process GameProcess = new Process();
                GameProcess.StartInfo.FileName = currentGameModule.gameExe;
                GameProcess.StartInfo.WorkingDirectory = currentGameModule.workingDirectory;
                GameProcess.EnableRaisingEvents = true;
                GameProcess.Start();
                currentGameModule.State = ModuleState.GameRunning;
                GameProcess.Exited += new EventHandler(OnGameExited);
            }
            else
            {
                currentGameModule.ChechForUpdates();
            }
        }
    }

    private void OnGameExited(object sender, EventArgs e)
    {
        this.Dispatcher.Invoke((Action)(() =>
        {
            currentGameModule.State = ModuleState.Idle_GameUptoDate;
        }));
    }
}