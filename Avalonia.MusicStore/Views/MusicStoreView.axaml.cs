using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Avalonia.MusicStore.Views
{
    public partial class MusicStoreView : UserControl
    {
        public MusicStoreView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}