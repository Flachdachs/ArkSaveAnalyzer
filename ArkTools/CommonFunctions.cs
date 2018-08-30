using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using Newtonsoft.Json;
using SavegameToolkit;
using SavegameToolkit.Structs;

namespace ArkTools {

    public static class CommonFunctions {
        public static T ReadBinary<T>(this IConversionSupport conversionSupport, string filePath, ReadingOptions readingOptions) where T : IConversionSupport {
            if (new FileInfo(filePath).Length > int.MaxValue) {
                throw new Exception("Input file is too large.");
            }

            Stream stream;
            if (readingOptions.UsesMemoryMapping) {
                MemoryMappedFile memoryMappedFile = MemoryMappedFile.CreateFromFile(filePath);
                stream = memoryMappedFile.CreateViewStream();
            } else {
                stream = new MemoryStream(File.ReadAllBytes(filePath));
            }

            using (ArkArchive archive = new ArkArchive(stream)) {
                conversionSupport.ReadBinary(archive, readingOptions);
            }

            return (T)conversionSupport;
        }

        public static void WriteBinary(this IConversionSupport conversionSupport, string filePath, WritingOptions writingOptions) {
            int size = conversionSupport.CalculateSize();

            Stream stream;
            if (writingOptions.UsesMemoryMapping) {
                File.Create(filePath).Dispose();
                MemoryMappedFile memoryMappedFile = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, size, MemoryMappedFileAccess.Write);
                stream = memoryMappedFile.CreateViewStream();
            } else {
                stream = new MemoryStream(new byte[size], true);
            }

            using (ArkArchive archive = new ArkArchive(stream)) {
                conversionSupport.WriteBinary(archive, writingOptions);

                if (!writingOptions.UsesMemoryMapping)
                    using (FileStream file = File.Create(filePath)) {
                        stream.Position = 0;
                        stream.CopyTo(file);
                    }
            }
        }

        public static void WriteJson(Stream outStream, WriteJsonCallback writeJsonCallback, WritingOptions writingOptions) {
            if (outStream == null) {
                throw new ArgumentNullException(nameof(outStream));
            }
            if (writeJsonCallback == null) {
                throw new ArgumentNullException(nameof(writeJsonCallback));
            }

            using (JsonTextWriter generator = new JsonTextWriter(new StreamWriter(outStream))) {
                if (GlobalOptions.PrettyPrinting) {
                    generator.UseDefaultPrettyPrint();
                }

                writeJsonCallback(generator, writingOptions);
            }
        }

        public static void WriteJson(string outPath, WriteJsonCallback writeJsonCallback, WritingOptions writingOptions) {
            if (outPath == null) {
                throw new ArgumentNullException(nameof(outPath));
            }
            if (writeJsonCallback == null) {
                throw new ArgumentNullException(nameof(writeJsonCallback));
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outPath));
            WriteJson(File.Create(outPath), writeJsonCallback, writingOptions);
        }

        public static string GetRGBA(this StructLinearColor lc) {
            double clampR = Math.Min(1, Math.Max(lc.R, 0));
            double clampG = Math.Min(1, Math.Max(lc.G, 0));
            double clampB = Math.Min(1, Math.Max(lc.B, 0));
            double clampA = Math.Min(1, Math.Max(lc.A, 0));

            // Gamma correction
            clampR = clampR <= 0.0031308 ? clampR * 12.92 : 1.055 * Math.Pow(clampR, 1.0 / 2.4) - 0.055;
            clampG = clampG <= 0.0031308 ? clampG * 12.92 : 1.055 * Math.Pow(clampG, 1.0 / 2.4) - 0.055;
            clampB = clampB <= 0.0031308 ? clampB * 12.92 : 1.055 * Math.Pow(clampB, 1.0 / 2.4) - 0.055;

            string r = "0" + ((int)Math.Floor(clampR * 255.999999)).ToString("X4");
            string g = "0" + ((int)Math.Floor(clampG * 255.999999)).ToString("X4");
            string b = "0" + ((int)Math.Floor(clampB * 255.999999)).ToString("X4");
            string a = "0" + ((int)Math.Floor(clampA * 255.999999)).ToString("X4");

            return "#" + r.Substring(r.Length - 2) + g.Substring(g.Length - 2) + b.Substring(b.Length - 2) + a.Substring(a.Length - 2);
        }
    }

}
