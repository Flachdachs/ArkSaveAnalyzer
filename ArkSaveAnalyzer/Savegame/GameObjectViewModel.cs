using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Newtonsoft.Json;
using SavegameToolkit;
using SavegameToolkit.Types;

namespace ArkSaveAnalyzer.Savegame {
    public class GameObjectViewModel : ViewModelBase {
        private string caption;

        public string Caption {
            get => caption;
            set => Set(ref caption, value);
        }

        private string textCompact;

        public string TextCompact {
            get => textCompact;
            set => Set(ref textCompact, value);
        }

        private string textOriginal;

        public string TextOriginal {
            get => textOriginal;
            set => Set(ref textOriginal, value);
        }

        private string selectedText;

        public string SelectedText {
            get => selectedText;
            set => Set(ref selectedText, value);
        }

        public GameObject GameObject {
            set {
                if (value == null)
                    return;

                StringBuilder sb = new StringBuilder();
                sb.Append(gameObjectToJson(value, true));

                foreach (KeyValuePair<ArkName, GameObject> component in value.Components) {
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine(new string('=', 80));
                    sb.AppendLine(component.Key.ToString());
                    sb.AppendLine(new string('=', 80));
                    sb.Append(gameObjectToJson(component.Value, true));
                }

                TextCompact = sb.ToString();

                sb.Clear();
                sb.Append(gameObjectToJson(value, false));
                foreach (KeyValuePair<ArkName, GameObject> component in value.Components) {
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine(new string('=', 80));
                    sb.AppendLine(component.Key.ToString());
                    sb.AppendLine(new string('=', 80));
                    sb.Append(gameObjectToJson(component.Value, false));
                }

                TextOriginal = sb.ToString();

                Caption = $"{value.Id}: {value.ClassString}";
            }
        }

        private static string gameObjectToJson(GameObject gameObject, bool compact) {
            StringBuilder sb = new StringBuilder();
            JsonTextWriter jsonTextWriter = new JsonTextWriter(new StringWriter(sb)) {
                IndentChar = '\t',
                Indentation = 1,
                Formatting = Formatting.Indented
            };
            //jsonTextWriter.UseDefaultPrettyPrint();
            gameObject.WriteJson(jsonTextWriter, WritingOptions.Create().CompactOutput(compact));

            return sb.ToString();
        }

        public RelayCommand CopyCommand { get; }
        public RelayCommand CopyAllCommand { get; }
        public RelayCommand CopyAllCompactCommand { get; }
        public RelayCommand<Window> CloseCommand { get; }

        public GameObjectViewModel() {
            SelectedText = string.Empty;

            CopyCommand = new RelayCommand(copy);
            CopyAllCommand = new RelayCommand(copyAll);
            CopyAllCompactCommand = new RelayCommand(copyAllCompact);
            CloseCommand = new RelayCommand<Window>(close);
        }

        private void close(Window window) {
            window.Close();
        }

        private void copy() {
            Clipboard.SetText(SelectedText ?? string.Empty);
        }

        private void copyAll() {
            Clipboard.SetText(TextOriginal);
        }

        private void copyAllCompact() {
            Clipboard.SetText(TextCompact);
        }
    }
}
