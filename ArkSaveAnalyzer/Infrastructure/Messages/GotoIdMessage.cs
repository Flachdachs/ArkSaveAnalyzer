namespace ArkSaveAnalyzer.Infrastructure.Messages {
    public class GotoIdMessage {
        public int Id { get; }

        public GotoIdMessage(int id) {
            Id = id;
        }
    }
}
