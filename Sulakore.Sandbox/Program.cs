using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Sulakore.Habbo;
using Sulakore.Habbo.Messages;

namespace Sulakore.Sandbox
{
    public class Program
    {
        private const string URL_PREFIX = "https://www.Habbo.com";
        private const string EXTERNAL_VARIABLES_SUFFIX = "/gamedata/external_variables";
        private const string CHROME_USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36";

        public static HttpClient Client { get; }
        public static HttpClientHandler Handler { get; }

        static Program()
        {
            Handler = new HttpClientHandler
            {
                UseProxy = false,
                AutomaticDecompression = (DecompressionMethods.GZip | DecompressionMethods.Deflate)
            };

            Client = new HttpClient(Handler);
            Client.BaseAddress = new Uri(URL_PREFIX);
            Client.DefaultRequestHeaders.ConnectionClose = true;
            Client.DefaultRequestHeaders.Add("User-Agent", CHROME_USER_AGENT);
        }
        public static void Main(string[] args)
        {
            var app = new Program();
            app.RunAsync().Wait();
        }

        public async Task RunAsync()
        {
            HGame game = await FetchLatestClientAsync();

            var @in = new Incoming();
            @in.Load(game, "Hashes.ini");

            var @out = new Outgoing();
            @out.Load(game, "Hashes.ini");

            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }

        public static async Task<HGame> FetchLatestClientAsync()
        {
            Uri flashClientUri = null;
            using (Stream gameDataStream = await Client.GetStreamAsync(EXTERNAL_VARIABLES_SUFFIX).ConfigureAwait(false))
            using (var gameDataReader = new StreamReader(gameDataStream))
            {
                while (!gameDataReader.EndOfStream)
                {
                    string line = await gameDataReader.ReadLineAsync().ConfigureAwait(false);
                    if (!line.StartsWith("flash.client.url")) continue;

                    int urlStart = (line.IndexOf('=') + 1);
                    flashClientUri = new Uri("http:" + line.Substring(urlStart) + "Habbo.swf");
                    break;
                }
            }

            string clientPath = Path.GetFullPath("Clients/" + flashClientUri.LocalPath);
            string clientDirectory = Path.GetDirectoryName(clientPath);
            Directory.CreateDirectory(clientDirectory);

            if (!File.Exists(clientPath))
            {
                using (Stream responseStream = await Client.GetStreamAsync(flashClientUri).ConfigureAwait(false))
                using (var clientFileStream = File.Create(clientPath))
                {
                    await responseStream.CopyToAsync(clientFileStream).ConfigureAwait(false);
                }
            }

            var game = new HGame(clientPath);
            await Task.Factory.StartNew(game.Disassemble).ConfigureAwait(false);
            await Task.Factory.StartNew(game.GenerateMessageHashes).ConfigureAwait(false);
            return game;
        }
    }
}