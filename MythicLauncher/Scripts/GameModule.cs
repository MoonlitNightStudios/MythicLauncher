using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

enum ModuleState
{
    Invalid,                //No game available
    Failed,                 //Download or update failed.
    Idle_RequiresGame,      //Launcher is doing nothing. Game is not yet installed
    Idle_RequiresUpdate,    //Launcher is doing nothing. Update available
    Idle_GameUptoDate,      //Launcher is doing nothing. Game is latest version
    DownloadingGame,        //Launcher is downloading the game
    DownloadingUpdate,      //Launcher is downloading an update
    GameRunning             //Game is running
}

namespace MythicLauncher
{
    class GameModule
    {
        private global::ModuleState _state;

        internal global::ModuleState State
        {
            get => _state;
            set
            {
                _state = value;
                
            }
        }
    }
}
