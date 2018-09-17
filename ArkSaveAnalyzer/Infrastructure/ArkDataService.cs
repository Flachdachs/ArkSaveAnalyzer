using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using ArkSaveAnalyzer.Properties;
using SavegameToolkitAdditions;

namespace ArkSaveAnalyzer.Infrastructure {
    public static class ArkDataService {
        private const string userAgent = "ark-save-analyzer";
        private const string arkDataFilename = "ark_data.json";
        private const string arkDataUrl = "https://ark-tools.seen-von-ragan.de/data-download/" + arkDataFilename;

        private static string filename => Path.Combine(
            string.IsNullOrWhiteSpace(Settings.Default.WorkingDirectory) ? Path.GetTempPath() : Settings.Default.WorkingDirectory,
            arkDataFilename);

        public static ArkData ArkData { get; private set; }

        static ArkDataService() {
            ArkData = new ArkData();
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject())) {
                EnsureArkDataAvailability();
            }
        }

        public static void EnsureArkDataAvailability() {
            Task.Run(async () => {
                if (!File.Exists(filename)) {
                    await DownloadArkData();
                } else {
                    ArkData = ArkDataReader.ReadFromFile(filename);
                }
            });
        }

        public static async Task DownloadArkData() {
            await tryDownload(new Uri(arkDataUrl), filename);

            ArkData = ArkDataReader.ReadFromFile(filename);
        }

        private static async Task tryDownload(Uri uri, string path) {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(uri);
            request.UserAgent = userAgent;
            if (File.Exists(path)) {
                request.IfModifiedSince = File.GetLastWriteTimeUtc(path);
            }

            request.Timeout = 10000;

            using (HttpWebResponse response = await request.GetHttpResponseAsync()) {
                switch (response.StatusCode) {
                    case HttpStatusCode.OK:
                        using (FileStream fileStream = File.Create(path)) {
                            response.GetResponseStream().CopyTo(fileStream);
                        }

                        File.SetLastWriteTimeUtc(path, response.LastModified);
                        break;
                    case HttpStatusCode.NotModified:
                        break;
                    default:
                        throw new Exception("Error during update of " + path + ", HTTP Code: " + response.StatusCode);
                }
            }
        }
    }

    public static class HttpWebRequestUtility {
        /// <summary>
        /// Gets the <see cref="HttpWebResponse"/> from an Internet resource.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>A <see cref="HttpWebResponse"/> that contains the response from the Internet resource.</returns>
        /// <remarks>This method does not throw a <see cref="WebException"/> for "error" HTTP status codes; the caller should
        /// check the <see cref="HttpWebResponse.StatusCode"/> property to determine how to handle the response.</remarks>
        public static HttpWebResponse GetHttpResponse(this HttpWebRequest request) {
            try {
                return (HttpWebResponse) request.GetResponse();
            } catch (WebException ex) {
                // only handle protocol errors that have valid responses

                if (ex.Response == null || ex.Status != WebExceptionStatus.ProtocolError)
                    throw;

                return (HttpWebResponse) ex.Response;
            }
        }

        public static async Task<HttpWebResponse> GetHttpResponseAsync(this HttpWebRequest request) {
            try {
                return (HttpWebResponse) await request.GetResponseAsync();
            } catch (WebException ex) {
                // only handle protocol errors that have valid responses

                if (ex.Response == null || ex.Status != WebExceptionStatus.ProtocolError)
                    throw;

                return (HttpWebResponse) ex.Response;
            }
        }
    }
}
