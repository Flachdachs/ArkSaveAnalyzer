namespace ArkSaveAnalyzer.Infrastructure.Messages {

    public class ExcludeWildlifeMessage {
        public string Name { get; }

        public ExcludeWildlifeMessage(string name) {
            Name = name;
        }
    }

}
