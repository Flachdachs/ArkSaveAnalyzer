using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

namespace CompareJson {

    static class Program {
        private static string filename1, filename2;

        static void Main(string[] args) {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            if (args.Length != 2) {
                error("Two filenames expected.");
            }

            filename1 = args[0];
            filename2 = args[1];

            Console.WriteLine("Comparing files:");
            Console.WriteLine(filename1);
            Console.WriteLine(filename2);

            StreamReader file1 = File.OpenText(filename1);
            StreamReader file2 = File.OpenText(filename2);

            JsonTextReader reader1 = new JsonTextReader(file1);
            JsonTextReader reader2 = new JsonTextReader(file2);

            while (reader1.Read()) {
                if (!reader2.Read()) {
                    message("2nd file shorter than 1st file.");
                }

                if (reader1.TokenType != reader2.TokenType) {
                    message($"Token different: {reader1.TokenType} - {reader2.TokenType}", reader1, reader2);
                    continue;
                }

                if (reader1.ValueType == null && reader2.ValueType == null) {
                    continue;
                }

                if (reader1.ValueType != reader2.ValueType) {
                    message($"Value type different: {reader1.ValueType.Name} - {reader2.ValueType.Name}", reader1, reader2);
                    continue;
                }

                if (reader1.ValueType == typeof(float) && reader2.ValueType == typeof(float)) {
                    float value1 = (float)reader1.Value;
                    float value2 = (float)reader2.Value;
                    if (Math.Abs(value1 - value2) > 0.02) {
                        message($"Float values too different ({Math.Abs(value1 - value2)}) {value1} - {value2}", reader1, reader2);
                    }
                    continue;
                }

                if (reader1.ValueType == typeof(double) && reader2.ValueType == typeof(double)) {
                    double value1 = (double)reader1.Value;
                    double value2 = (double)reader2.Value;
                    if (Math.Abs(value1 - value2) > 0.02) {
                        message($"Double values too different ({Math.Abs(value1 - value2)}) {value1} - {value2}", reader1, reader2);
                    }
                    continue;
                }

                if (reader1.ValueType == typeof(decimal) && reader2.ValueType == typeof(decimal)) {
                    decimal value1 = (decimal)reader1.Value;
                    decimal value2 = (decimal)reader2.Value;
                    if (Math.Abs(value1 - value2) > 0.02m) {
                        message($"Decimal values too different ({Math.Abs(value1 - value2)}) {value1} - {value2}", reader1, reader2);
                    }
                    continue;
                }

                if (!reader1.Value.Equals(reader2.Value)) {
                    message($"Value different: {reader1.Value} - {reader2.Value}", reader1, reader2);
                }
            }
            if (reader2.Read()) {
                message("2nd file longer than 1st file.");
            }

            Console.Error.WriteLine("Done.");
            Console.ReadLine();
        }

        private static void message(string message, JsonTextReader reader1 = null, JsonTextReader reader2 = null) {
            Console.WriteLine(message);
            if (reader1 != null) {
                Console.WriteLine($"{filename1}, Line: {reader1.LineNumber}, Pos: {reader1.LinePosition}");
            }
            if (reader2 != null) {
                Console.WriteLine($"{filename2}, Line: {reader2.LineNumber}, Pos: {reader2.LinePosition}");
            }
        }

        private static void error(string message) {
            Console.Error.WriteLine(message);
            Console.Error.WriteLine("Done.");
            Console.ReadLine();

            Environment.Exit(1);
        }
    }

}
