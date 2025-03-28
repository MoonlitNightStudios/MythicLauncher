﻿using System.Windows.Controls;

namespace MythicLauncher.Controls
{
    /// <summary>
    /// Interaction logic for MythicModuleButton.xaml
    /// </summary>
    public partial class MythicModuleButton : UserControl
    {
        public MythicModuleButton()
        {
            InitializeComponent();
        }

        public GameModule Game { get; set; }
        public EventHandler<GameModule> ClickedEvent;

        private void OnClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            ClickedEvent.Invoke(this, Game);
        }
    }
}
