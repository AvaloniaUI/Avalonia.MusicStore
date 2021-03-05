using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.MusicStore.Backend;
using ReactiveUI;

namespace Avalonia.MusicStore.ViewModels
{
    public class AlbumViewModel : ViewModelBase
    {
        private Bitmap? _cover;
        private readonly Album _album;
        
        public AlbumViewModel(Album album)
        {
            _album = album;
        }

        public string Artist => _album.Artist;

        public string Title => _album.Title;
        
        public Bitmap? Cover
        {
            get => _cover;
            private set => this.RaiseAndSetIfChanged(ref _cover, value);
        }

        public async Task LoadCover()
        {
            await using (var imageStream =await _album.LoadCoverBitmapAsync())
            {
                Cover = await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400));
            }
        }

        public static async Task<IEnumerable<AlbumViewModel>> LoadCached()
        {
            return (await Album.LoadCachedAsync()).Select(x => new AlbumViewModel(x));
        }

        public async Task SaveToDiskAsync()
        {
            await _album.SaveAsync();

            if (Cover != null)
            {
                await Task.Run(() =>
                {
                    using (var fs = _album.SaveCoverBitmapSteam())
                    {
                        Cover.Save(fs);
                    }
                });
            }
        }
    }
}