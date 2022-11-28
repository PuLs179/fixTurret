using System.Windows;
using System.Windows.Controls;

namespace fixTurret
{
    public partial class fixTurretControl : UserControl
    {

        private fixTurret Plugin { get; }

        private fixTurretControl()
        {
            InitializeComponent();
        }

        public fixTurretControl(fixTurret plugin) : this()
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
