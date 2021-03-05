using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using Avalonia.MusicStore.Backend;
using ReactiveUI;

namespace Avalonia.MusicStore.ViewModels
{
    public class MusicStoreViewModel : ViewModelBase
    {
        private string? _searchText;
        private bool _isBusy;
        private CancellationTokenSource? _cancellationTokenSource;
        private AlbumViewModel? _selectedAlbum;

        public MusicStoreViewModel()
        {
            this.WhenAnyValue(x => x.SearchText)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Throttle(TimeSpan.FromMilliseconds(400))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(DoSearch!);
            
            BuyMusicCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (SelectedAlbum is { })
                {
                    await SelectedAlbum.SaveToDiskAsync();
                    return SelectedAlbum;
                }

                return null;
            });
        }
        
        public string? SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }
        
        public ReactiveCommand<Unit, AlbumViewModel?> BuyMusicCommand { get; }

        public ObservableCollection<AlbumViewModel> SearchResults { get; } = new();
        
        public AlbumViewModel? SelectedAlbum
        {
            get => _selectedAlbum;
            set => this.RaiseAndSetIfChanged(ref _selectedAlbum, value);
        }
        
        private async void DoSearch(string s)
        {
            IsBusy = true;
            SearchResults.Clear();
            
            _cancellationTokenSource?.Cancel();

            _cancellationTokenSource = new CancellationTokenSource();

            var albums = await Album.SearchAsync(s);

            foreach (var album in albums)
            {
                var vm = new AlbumViewModel(album);
                
                SearchResults.Add(vm);
            }

            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                 LoadCovers(_cancellationTokenSource.Token);
            }

            IsBusy = false;
        }

        private async void LoadCovers(CancellationToken cancellationToken)
        {
            foreach (var album in SearchResults.ToList())
            {
                await album.LoadCover();

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }
    }
}