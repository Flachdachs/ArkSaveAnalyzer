using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SavegameToolkit.Propertys;
using SavegameToolkit.Types;

namespace SavegameToolkit {

    public sealed class ArkSavFile : GameObjectContainerMixin, IConversionSupport, IPropertyContainer {

        private string className;

        public List<IProperty> Properties { get; } = new List<IProperty>();

        public void ReadBinary(ArkArchive archive, ReadingOptions options) {
            className = archive.ReadString();

            Properties.Clear();
            try {
                IProperty property = PropertyRegistry.ReadBinary(archive);

                while (property != null) {
                    Properties.Add(property);
                    property = PropertyRegistry.ReadBinary(archive);
                }
            } catch (UnreadablePropertyException upe) {
                Debug.WriteLine(upe.Message);
                Debug.WriteLine(upe.StackTrace);
            }

            // TODO: verify 0 int at end
        }

        public void WriteBinary(ArkArchive archive, WritingOptions options) {
            archive.WriteString(className);

            Properties?.ForEach(p => p.WriteBinary(archive));

            archive.WriteName(ArkName.NameNone);
            archive.WriteInt(0);
        }

        public int CalculateSize() {
            int size = sizeof(int) + ArkArchive.GetStringLength(className);

            NameSizeCalculator nameSizer = ArkArchive.GetNameSizer(false);

            size += nameSizer(ArkName.NameNone);

            size += Properties.Sum(p => p.CalculateSize(nameSizer));
            return size;
        }

        public void ReadJson(JToken node, ReadingOptions options) {
            /* todo
            className = node.path("className").asText();

            Properties.Clear();
            if (node.hasNonNull("properties")) {
                foreach (JsonNode propertyNode in node.get("properties")) {
                    Properties.Add(PropertyRegistry.readJson(propertyNode));
                }
            }
            */
        }

        public void WriteJson(JsonTextWriter generator, WritingOptions writingOptions) {
            generator.WriteStartObject();

            generator.WriteField("className", className);

            if (Properties.Any()) {
                generator.WriteArrayFieldStart("properties");

                foreach (IProperty property in Properties) {
                    property.WriteJson(generator, writingOptions);
                }

                generator.WriteEndArray();
            }

            generator.WriteEndObject();
        }

    }

}
