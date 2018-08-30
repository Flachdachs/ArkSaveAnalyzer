using System.Collections.Generic;

namespace MonoOptions {

    public class ResponseFileSource : ArgumentSource {

        public override string[] GetNames() {
            return new string[] { "@file" };
        }

        public override string Description => "Read response file for more options.";

        public override bool GetArguments(string value, out IEnumerable<string> replacement) {
            if (string.IsNullOrEmpty(value) || !value.StartsWith("@")) {
                replacement = null;
                return false;
            }

            replacement = GetArgumentsFromFile(value.Substring(1));
            return true;
        }

    }

}