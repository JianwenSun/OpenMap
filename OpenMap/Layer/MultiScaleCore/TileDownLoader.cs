using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OpenMap
{
    internal class TileDownloader
    {
        private const int TileBodyPartLength = 4096;

        private readonly object synchronizer = new object();
        private WebClient webClient;
        private int contentLength = 0;
        private DateTime expires = new DateTime();
        private TileId imageId;
        private Uri uri;
        private Action<TileDownloader> readerCallback;
        private ValidateDelegate validator;
        private bool aborted = false;
        private byte[] tileBody;

        private Stream stream;
        private MemoryStream memoryStream;
        private byte[] tileBodyPart;
        private DownloadDataCompletedEventArgs completedEventArgs;
        private RequestCacheLevel requestCacheLevel;
        private ICredentials requestCredentials;
        private Func<Uri, string, Dictionary<string, string>, NetworkCredential, ICredentials> httpAuthenticationHandler;
        private string authenticationType;
        private NetworkCredential usedCredentials;

        internal TileDownloader(TileId tileId, Uri downloadingUri, Action<TileDownloader> assyncImageReader, ValidateDelegate validator)
            : this(tileId, assyncImageReader)
        {
            this.uri = downloadingUri;
            this.validator = validator;
        }

        internal TileDownloader(TileId tileId, Stream stream, Action<TileDownloader> assyncImageReader)
            : this(tileId, assyncImageReader)
        {
            this.tileBodyPart = new byte[TileBodyPartLength];
            this.stream = stream;
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
        }

        private TileDownloader(TileId tileId, Action<TileDownloader> assyncImageReader)
        {
            this.imageId = tileId;
            this.readerCallback = assyncImageReader;
        }

        internal delegate bool ValidateDelegate(TileId tileId);

        internal byte[] TileBody
        {
            get
            {
                return this.tileBody;
            }
        }

        internal bool Aborted
        {
            get
            {
                return this.aborted;
            }
        }

        internal TileId TileId
        {
            get
            {
                return this.imageId;
            }
        }

        internal Uri Uri
        {
            get
            {
                return this.uri;
            }
        }

        internal DateTime Expires
        {
            get
            {
                return this.expires;
            }
        }

        public void Dispose()
        {
            lock (this.synchronizer)
            {
                TilesDownloader.RemoveDownloader(this);

                if (this.webClient != null)
                {
                    this.webClient.Dispose();
                    this.webClient = null;
                }

                this.tileBody = null;
                this.tileBodyPart = null;
            }
        }

        internal void StartDownload()
        {
            if (this.stream != null)
            {
                this.memoryStream = new MemoryStream();
                this.stream.BeginRead(this.tileBodyPart, 0, TileBodyPartLength, ReadTileCallback, this);
            }
            else
            {
                TilesDownloader.EnqueueDownload(this, true);
            }
        }

        internal void StartDownload(RequestCacheLevel cacheLevel, ICredentials credentials, Func<Uri, string, Dictionary<string, string>, NetworkCredential, ICredentials> httpUnauthorizedHandler)
        {
            this.requestCacheLevel = cacheLevel;
            this.requestCredentials = credentials;
            this.httpAuthenticationHandler = httpUnauthorizedHandler;

            this.StartDownload();
        }

        internal void Abort()
        {
            this.aborted = true;
            if (this.webClient != null)
            {
                try
                {
                    this.webClient.CancelAsync();
                }
                catch
                {
                }
            }
        }

        internal bool Validate()
        {
            if (this.aborted)
            {
                return false;
            }

            bool valid = this.validator(this.TileId);
            if (!valid)
            {
                this.Abort();
                this.Dispose();
            }

            return valid;
        }

        internal void StartReload()
        {
            if (this.Validate())
            {
                this.StartReloadThread();
            }
        }

        private static void ReadTileCallback(IAsyncResult asyncResult)
        {
            TileDownloader tile = (TileDownloader)asyncResult.AsyncState;
            int loaded = tile.stream.EndRead(asyncResult);
            if (loaded > 0)
            {
                tile.memoryStream.Write(tile.tileBodyPart, 0, loaded);
                tile.stream.BeginRead(tile.tileBodyPart, 0, TileBodyPartLength, ReadTileCallback, tile);
                return;
            }

            tile.tileBody = tile.memoryStream.ToArray();
            tile.memoryStream.Dispose();
            tile.memoryStream = null;
            tile.stream.Dispose();
            tile.stream = null;
            tile.tileBodyPart = null;

            tile.readerCallback(tile);
        }

        private void Reload()
        {
            if (this.Validate())
            {
                TilesDownloader.EnqueueDownload(this, false);
            }
        }

        private void StartReloadThread()
        {
            if (!this.Validate())
            {
                return;
            }

            this.webClient = new WebClient();
            this.webClient.Headers.Add("user-agent", "sunjianwen");
            this.webClient.CachePolicy = new RequestCachePolicy(this.requestCacheLevel);
            this.usedCredentials = null;
            if (this.requestCredentials != null)
            {
                if (!string.IsNullOrEmpty(this.authenticationType))
                {
                    this.usedCredentials = this.requestCredentials.GetCredential(this.uri, this.authenticationType);
                }

                this.webClient.Credentials = this.requestCredentials;
            }

            this.webClient.DownloadDataCompleted += this.OnDownloadDataCompleted;
            this.webClient.DownloadDataAsync(this.uri);
        }

        private void OnDownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            this.webClient.DownloadDataCompleted -= this.OnDownloadDataCompleted;
            this.completedEventArgs = e;
            if (e != null)
            {
                ThreadPool.QueueUserWorkItem(this.DownloadDataCompletedThread);
            }
            else
            {
                this.Dispose();
            }
        }

        private void DownloadDataCompletedThread(object status)
        {
            DownloadDataCompletedEventArgs e = this.completedEventArgs;
            if (e != null)
            {
                if (e.Error != null)
                {
                    if (this.CancelledOrBadResponse(e.Error))
                    {
                        this.Dispose();
                        return;
                    }

                    this.Reload();
                }
                else
                {
                    this.tileBody = e.Result;
                    this.CompleteDownload();
                }
            }
        }

        private bool CancelledOrBadResponse(Exception error)
        {
            var exception = error as WebException;
            if (exception != null)
            {
                if (exception.Status == WebExceptionStatus.RequestCanceled)
                {
                    return true;
                }

                var response = exception.Response as HttpWebResponse;
                if (response != null)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            return true;

                        case HttpStatusCode.Unauthorized:
                            return this.HttpUnauthorizedResponse(response);
                    }
                }
            }

            return false;
        }

        private bool HttpUnauthorizedResponse(HttpWebResponse response)
        {
            this.authenticationType = string.Empty;
            this.requestCredentials = null;

            string authenticationHeader = response.GetResponseHeader("WWW-Authenticate");
            if (!string.IsNullOrEmpty(authenticationHeader))
            {
                Regex authTypeRegex = new Regex(@"^([^\s]+)");
                Match authTypeMatch = authTypeRegex.Match(authenticationHeader);
                if (authTypeMatch.Success)
                {
                    this.authenticationType = authTypeMatch.Groups[1].Value;

                    Dictionary<string, string> authenticationAttributes = new Dictionary<string, string>();
                    Regex authRealmRegex = new Regex(@"(\s+|\s*,\s*)(?<token>[^\s]+)\=""(?<value>[^""]+)""");
                    MatchCollection matches = authRealmRegex.Matches(authenticationHeader);
                    foreach (Match match in matches)
                    {
                        authenticationAttributes.Add(match.Groups["token"].Value, match.Groups["value"].Value);
                    }

                    ICredentials credentials = this.httpAuthenticationHandler(this.uri, this.authenticationType, authenticationAttributes, this.usedCredentials);
                    if (credentials != null)
                    {
                        this.requestCredentials = credentials;
                        NetworkCredential newCredential = credentials.GetCredential(this.uri, this.authenticationType);
                        return newCredential == null || newCredential == this.usedCredentials;
                    }
                }
            }

            return true;
        }

        private void CompleteDownload()
        {
            bool reload = false;
            lock (this.synchronizer)
            {
                if (this.webClient != null && this.webClient.ResponseHeaders != null)
                {
                    int length;
                    if (int.TryParse(this.webClient.ResponseHeaders["Content-Length"], out length))
                    {
                        this.contentLength = length;
                    }

                    DateTime expiresDateTime;
                    if (DateTime.TryParse(this.webClient.ResponseHeaders["Expires"], out expiresDateTime))
                    {
                        this.expires = expiresDateTime;
                    }
                    else
                    {
                        this.expires = DateTime.MinValue;
                    }

                    if (this.contentLength == 0 || this.tileBody.Length == this.contentLength)
                    {
                        TilesDownloader.DownloadingComplete = true;
                    }
                    else
                    {
                        reload = true;
                    }
                }
                else
                {
                    reload = true;
                }
            }

            if (reload)
            {
                this.Reload();
            }
            else
            {
                this.readerCallback(this);
            }
        }
    }
}
