using Newtonsoft.Json;

namespace ArkTools.Data {
    public delegate void WriterFunction<in T>(T o, JsonTextWriter generator, IDataContext context, bool writeEmpty);
}
