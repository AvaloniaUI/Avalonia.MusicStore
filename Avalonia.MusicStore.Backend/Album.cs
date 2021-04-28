using iTunesSearch.Library;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Avalonia.MusicStore.Backend
{
    public class Album
    {
        private static HttpClient s_httpClient = new();
        private static iTunesSearchManager s_SearchManager = new();
        
        public Album(string artist, string title, string coverUrl)
        {
            Artist = artist;
            Title = title;
            CoverUrl = coverUrl;
        }
        
        public string Artist { get; set; }
        
        public string Title { get; set; }
        
        public string CoverUrl { get; set; }

        // fix issue
        //private string CachePath => $"./Cache/{Artist} - {Title}";
        private string CachePath => $"./Cache/{GetSafeFilename(Artist)} - {GetSafeFilename(Title)}";

        public static string GetSafeFilename(string arbitraryString)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var replaceIndex = arbitraryString.IndexOfAny(invalidChars, 0);
            if (replaceIndex == -1) return arbitraryString;

            var r = new StringBuilder();
            var i = 0;

            do
            {
                r.Append(arbitraryString, i, replaceIndex - i);

                switch (arbitraryString[replaceIndex])
                {
                    case '"':
                        r.Append("''");
                        break;
                    case '<':
                        r.Append('\u02c2'); // '˂' (modifier letter left arrowhead)
                        break;
                    case '>':
                        r.Append('\u02c3'); // '˃' (modifier letter right arrowhead)
                        break;
                    case '|':
                        r.Append('\u2223'); // '∣' (divides)
                        break;
                    case ':':
                        r.Append('-');
                        break;
                    case '*':
                        r.Append('\u2217'); // '∗' (asterisk operator)
                        break;
                    case '\\':
                    case '/':
                        r.Append('\u2044'); // '⁄' (fraction slash)
                        break;
                    case '\0':
                    case '\f':
                    case '?':
                        break;
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\v':
                        r.Append(' ');
                        break;
                    default:
                        r.Append('_');
                        break;
                }

                i = replaceIndex + 1;
                replaceIndex = arbitraryString.IndexOfAny(invalidChars, i);
            } while (replaceIndex != -1);

            r.Append(arbitraryString, i, arbitraryString.Length - i);

            return r.ToString();
        }

        public async Task<Stream> LoadCoverBitmapAsync()
        {
            if (File.Exists(CachePath + ".bmp"))
            {
                return File.OpenRead(CachePath + ".bmp");
            }
            else
            {
                var data = await s_httpClient.GetByteArrayAsync(CoverUrl);

                return new MemoryStream(data);
            }
        }

        public async Task SaveAsync()
        {
            if (!Directory.Exists("./Cache"))
            {
                Directory.CreateDirectory("./Cache");
            }

            using (var fs = File.OpenWrite(CachePath))
            {
                await SaveToStreamAsync(this, fs);
            }
        }

        public Stream SaveCoverBitmapSteam()
        {
            return File.OpenWrite(CachePath + ".bmp");
        }

        private static async Task SaveToStreamAsync(Album data, Stream stream)
        {
            await JsonSerializer.SerializeAsync(stream, data).ConfigureAwait(false);
        }

        public static async Task<Album> LoadFromStream(Stream stream)
        {
            return (await JsonSerializer.DeserializeAsync<Album>(stream).ConfigureAwait(false))!;
        }

        public static async Task<IEnumerable<Album>> LoadCachedAsync()
        {
            if (!Directory.Exists("./Cache"))
            {
                Directory.CreateDirectory("./Cache");
            }

            var results = new List<Album>();

            foreach (var file in Directory.EnumerateFiles("./Cache"))
            {
                if (!string.IsNullOrWhiteSpace(new DirectoryInfo(file).Extension)) continue;
                
                await using var fs = File.OpenRead(file);
                results.Add(await Album.LoadFromStream(fs).ConfigureAwait(false));
            }

            return results;
        }

        public static async Task<IEnumerable<Album>> SearchAsync(string searchTerm)
        {
            var query = await s_SearchManager.GetAlbumsAsync(searchTerm).ConfigureAwait(false);

            return query.Albums.Select(x =>
                new Album(x.ArtistName, x.CollectionName, x.ArtworkUrl100.Replace("100x100bb", "600x600bb")));
        }
    }
}