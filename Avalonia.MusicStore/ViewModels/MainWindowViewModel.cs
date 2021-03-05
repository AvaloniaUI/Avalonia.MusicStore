using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;

namespace Avalonia.MusicStore.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            ShowDialog = new Interaction<MusicStoreViewModel, AlbumViewModel?>();
            
            BuyMusicCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var store = new MusicStoreViewModel();

                var result = await ShowDialog.Handle(store);

                if (result != null)
                {
                    Albums.Add(result);
                }
            });
            
            RxApp.MainThreadScheduler.Schedule(LoadAlbums);
        }
        
        private async void LoadAlbums()
        {
            var albums = await AlbumViewModel.LoadCached();

            foreach (var album in albums)
            {
                Albums.Add(album);
            }
            
            LoadCovers();
        }
        
        private async void LoadCovers()
        {
            foreach (var album in Albums.ToList())
            {
                await album.LoadCover();
            }
        }

        public ObservableCollection<AlbumViewModel> Albums { get; } = new();
        
        public ICommand BuyMusicCommand { get; }
        
        public Interaction<MusicStoreViewModel, AlbumViewModel?> ShowDialog { get; }
    }
}