using System;
using System.Collections.Generic;
using System.Text;

namespace MonoOptions {

    public class Command {

        public string Name { get; }
        public string Help { get; }

        public OptionSet Options { get; set; }
        public Action<IEnumerable<string>> Run { get; set; }

        public CommandSet CommandSet { get; internal set; }

        public Command(string name, string help = null) {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            Name = normalizeCommandName(name);
            Help = help;
        }

        /// <summary>
        /// replaces one or multiple consecutive whitespaces with one space
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string normalizeCommandName(string name) {
            StringBuilder value = new StringBuilder(name.Length);
            bool space = false;
            for (int i = 0; i < name.Length; ++i) {
                if (!char.IsWhiteSpace(name, i)) {
                    space = false;
                    value.Append(name[i]);
                } else if (!space) {
                    space = true;
                    value.Append(' ');
                }
            }

            return value.ToString();
        }

        public virtual int Invoke(IEnumerable<string> arguments) {
            IEnumerable<string> rest = Options?.Parse(arguments) ?? arguments;
            Run?.Invoke(rest);
            return 0;
        }

    }

}