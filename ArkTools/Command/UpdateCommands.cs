using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using MonoOptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ArkTools.Command {

    public abstract class UpdateBaseCommand : BaseCommand {

        protected virtual string[] HelpHeaders { get; set; } = { };

        protected UpdateBaseCommand(string name, string help = null) : base(name, help) {
            Options = new OptionSet();
            // ReSharper disable once VirtualMemberCallInConstructor
            foreach (string helpHeader in HelpHeaders) {
                Options.Add(helpHeader);
            }
        }

    }

    public class UpdateDataCommand : UpdateBaseCommand {
        private const string languagesFilename = "ark_data_languages.json";

        private static Uri BASE_URI = new Uri("https://ark-tools.seen-von-ragan.de/data-download/");

        private static Uri MAIN_URI = new Uri(BASE_URI, "ark_data.json");

        private static Uri LANG_URI = new Uri(BASE_URI, languagesFilename);

        private const string names = "update-data, updateData";
        private const string help = "Checks for ark_data.json and translation updates, then downloads them if available.";

        protected override string[] HelpHeaders { get; set; } = {
                "",
                "update-data|updateData [OPTIONS]",
                ""
        };

        private string withLanguage;
        private bool allLanguages;
        private bool withoutStrictSsl;

        public UpdateDataCommand() : base(names, help) {
            Options.Add("with-language=", "Downloads specified languages (comma separated)", s => withLanguage = s);
            Options.Add("all-languages", "Downloads all available languages", s => allLanguages = s != null);
            Options.Add("without-strict-ssl", "Disables the validation of Certificates", s => withoutStrictSsl = s != null);
            Options.Add("h|?|help", "command specific help", s => showHelp = s != null);
        }

        protected override void RunCommand(IEnumerable<string> args) {
            List<string> argsList = args.ToList();
            if (showCommandHelp(argsList)) {
                return;
            }

            string version = typeof(App).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

            string basePath = GlobalOptions.ArkToolsConfiguration.BasePath;

            tryDownload(MAIN_URI, Path.Combine(basePath, "ark_data.json"), version, GlobalOptions.Quiet, withoutStrictSsl);

            if (!allLanguages && withLanguage == null) {
                //Environment.Exit(0);
                return;
            }

            tryDownload(LANG_URI, Path.Combine(basePath, languagesFilename), version, GlobalOptions.Quiet, withoutStrictSsl);

            JObject languageList = (JObject)JToken.ReadFrom(new JsonTextReader(File.OpenText(Path.Combine(GlobalOptions.ArkToolsConfiguration.BasePath, languagesFilename))));

            List<string> languages; 
            if (allLanguages) {
                languages = new List<string>(languageList.Count);
                languages.AddRange(languageList.Properties().Select(property => property.Name));
            } else {
                languages = withLanguage.Split(",").Select(s => s.Trim()).ToList();
            }

            List<string> languageFiles = new List<string>();
            bool valid = true;
            foreach (string language in languages) {
                string languageValue = languageList.Value<string>(language);
                if (languageValue != null) {
                    languageFiles.Add(languageValue);
                } else {
                    valid = false;
                    Console.Error.WriteLine("Unknown language " + language);
                }
            }

            if (!valid) {
                //Environment.Exit(2);
                return;
            }

            foreach (string languageFile in languageFiles) {
                tryDownload(new Uri(BASE_URI, languageFile), Path.Combine(basePath, languageFile), version, GlobalOptions.Quiet, withoutStrictSsl);
            }
        }

        private static void tryDownload(Uri uri, string path, string myVersion, bool quiet, bool withoutStrictSSL) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.UserAgent = "ark-tools/" + myVersion;
            if (File.Exists(path)) {
                request.IfModifiedSince = File.GetLastWriteTimeUtc(path);
            }
            request.Timeout = 10000;

            using (HttpWebResponse response = request.GetHttpResponse()) {
                switch (response.StatusCode) {
                    case HttpStatusCode.OK:
                        using (FileStream fileStream = File.Create(path)) {
                            response.GetResponseStream().CopyTo(fileStream);
                        }

                        File.SetLastWriteTimeUtc(path, response.LastModified);
                        if (!quiet) {
                            Console.WriteLine("Updated " + path);
                        }
                        break;
                    case HttpStatusCode.NotModified:
                        break;
                    default:
                        throw new Exception("Error during update of " + path + ", HTTP Code: " + response.StatusCode);
                }
            }

        }
    }

    public class VersionCommand : UpdateBaseCommand {

        private const string names = "version";
        private const string help = "Shows version and exits.";

        public VersionCommand() : base(names, help) {
            Options = new OptionSet {
                    "",
                    "This command has no specific options.",
                    "",
                    { "h|?|help", "command specific help", s => showHelp = s != null }
            };
        }

        protected override void RunCommand(IEnumerable<string> args) {
            if (showCommandHelp(args)) {
                return;
            }

            string version = typeof(App).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            Console.WriteLine(version);

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
                return (HttpWebResponse)request.GetResponse();
            } catch (WebException ex) {
                // only handle protocol errors that have valid responses

                if (ex.Response == null || ex.Status != WebExceptionStatus.ProtocolError)
                    throw;

                return (HttpWebResponse)ex.Response;
            }
        }
    }
}
