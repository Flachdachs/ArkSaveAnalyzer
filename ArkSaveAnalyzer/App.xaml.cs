using System.Windows;
using ArkSaveAnalyzer.Infrastructure.Messages;
using GalaSoft.MvvmLight.Messaging;

namespace ArkSaveAnalyzer {
    public partial class App : Application {
        protected override void OnExit(ExitEventArgs e) {
            Messenger.Default.Send(new ApplicationShutdownMessage());
            base.OnExit(e);
        }
    }
}
