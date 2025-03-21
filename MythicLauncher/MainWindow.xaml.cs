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
    const string VersionFileName = "\\\\VRYZL\\DistributedGames\\VersionFile.txt";

    private string rootPath;
    private string versionFile;
    private string gameZip;
    private string gameExe;

    private ModuleState _state;

    internal ModuleState State
    {
        get => _state;
        set
        {
            _state = value;
            switch(_state)
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
    }

    public MainWindow()
    {
        InitializeComponent();

        State = ModuleState.Idle_GameUptoDate;

        rootPath = Directory.GetCurrentDirectory();
        versionFile = Path.Combine(rootPath, "Version.txt");
        gameZip = Path.Combine(rootPath, "Build.Zip");
        gameExe = Path.Combine(rootPath, "Builds\\MythicUniverse\\Windows", "MythicUniverse.exe");

    }

    private void ChechForUpdates()
    {
        if(File.Exists(versionFile))
        {
            Version localversion = new Version(File.ReadAllText(versionFile));
            VersionText.Text = localversion.ToString();

            try
            {
                WebClient webClient = new WebClient();
                Version onlineVersion = new Version(webClient.DownloadString(VersionFileName));

                if(onlineVersion.IsDifferentThan(localversion))
                {
                    InstallGameFiles(false, onlineVersion);
                }
                else
                {
                    State = ModuleState.Idle_GameUptoDate;
                }
            }
            catch(Exception ex)
            {
                State = ModuleState.Failed;
                MessageBox.Show($"Error checking for game update: {ex}");
            }
        }
        else
        {
            InstallGameFiles(false, Version.zero);
        }
    }

    private void InstallGameFiles(bool _isUpdate, Version _onlineVersion)
    {
        try
        {
            WebClient webClient = new WebClient();
            if(_isUpdate)
            {
                State = ModuleState.DownloadingUpdate;
            }
            else
            {
                State = ModuleState.DownloadingGame;
                _onlineVersion = new Version(webClient.DownloadString(VersionFileName));
            }

            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
            webClient.DownloadFileAsync(new Uri("GameLink"), gameZip, _onlineVersion);
        }
        catch (Exception ex)
        {
            State = ModuleState.Failed;
            MessageBox.Show($"Error installing game files: {ex}");
        }
    }

    private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs eventArgs)
    {
        try
        {
            string onlineVersion = ((Version)eventArgs.UserState).ToString();
            ZipFile.ExtractToDirectory(gameZip, rootPath, true);
            File.Delete(gameZip);

            File.WriteAllText(versionFile, onlineVersion);

            VersionText.Text = onlineVersion;
        }
        catch (Exception ex)
        {
            State = ModuleState.Failed;
            MessageBox.Show($"Error finishing download: {ex}");
        }
    }

    private void Launch_Clicked(object sender, RoutedEventArgs e)
    {
        if(File.Exists(gameExe) && State == ModuleState.Idle_GameUptoDate)
        {
            Process GameProcess = new Process();
            GameProcess.StartInfo.FileName = gameExe;
            GameProcess.StartInfo.WorkingDirectory = Path.Combine(rootPath, "Builds\\MythicUniverse\\Windows");
            GameProcess.EnableRaisingEvents = true;
            GameProcess.Start();
            State = ModuleState.GameRunning;
            GameProcess.Exited += new EventHandler(OnGameExited);
        }
        else
        {
            ChechForUpdates();
        }
    }

    private void OnGameExited(object sender, EventArgs e)
    {
        this.Dispatcher.Invoke((Action)(() =>
        {
            State = ModuleState.Idle_GameUptoDate;
        }));
    }
}

struct Version
{
    internal static Version zero = new Version(0, 0, 0);

    private short major;
    private short minor;
    private short subMinor;

    internal Version(short _major, short _minor, short _subMinor)
    {
        major = _major;
        minor = _minor;
        subMinor = _subMinor;
    }

    internal Version(string _version)
    {
        string[] versionStrings = _version.Split('.');
        if(versionStrings.Length != 3 )
        {
            major = 0;
            minor = 0;
            subMinor = 0;
        }

        major = short.Parse(versionStrings[0]);
        minor = short.Parse(versionStrings[1]);
        subMinor = short.Parse(versionStrings[2]);
    }

    internal bool IsDifferentThan(Version _otherVersion)
    {
        if (major != _otherVersion.major)
        {
            return true;
        }
        else if (minor != _otherVersion.minor)
        {
            return true;
        }
        else if (subMinor != _otherVersion.subMinor)

        {
            return true;
        }
        return false;
    }

    public override string ToString()
    {
        return $"{major}.{minor}.{subMinor}";
    }
}