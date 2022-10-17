using System.Windows;
using System.Windows.Controls;

namespace FactionRepSync
{
    public partial class FactionRepSyncControl : UserControl
    {

        private FactionRepSync Plugin { get; }

        private FactionRepSyncControl()
        {
            InitializeComponent();
        }

        public FactionRepSyncControl(FactionRepSync plugin) : this()
        {
            Plugin = plugin;
            DataContext = plugin.Config;
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            Plugin.Save();
        }
    }
}
