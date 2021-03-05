using System;
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
        private bool _collectionEmpty;
        
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
            
            this.WhenAnyValue(x => x.Albums.Count)
                .Subscribe(x => CollectionEmpty = x == 0);
            
            RxApp.MainThreadScheduler.Schedule(LoadAlbums);
        }
        
        public bool CollectionEmpty
        {
            get => _collectionEmpty;
            set => this.RaiseAndSetIfChanged(ref _collectionEmpty, value);
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