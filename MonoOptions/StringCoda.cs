using System;
using System.Collections.Generic;

namespace MonoOptions {

    internal static class StringCoda {

        public static IEnumerable<string> WrappedLines(string self, params int[] widths) {
            IEnumerable<int> w = widths;
            return wrappedLines(self, w);
        }

        private static IEnumerable<string> wrappedLines(string self, IEnumerable<int> widths) {
            if (widths == null)
                throw new ArgumentNullException(nameof(widths));
            return createWrappedLinesIterator(self, widths);
        }

        private static IEnumerable<string> createWrappedLinesIterator(string self, IEnumerable<int> widths) {
            if (string.IsNullOrEmpty(self)) {
                yield return string.Empty;
                yield break;
            }

            using (IEnumerator<int> ewidths = widths.GetEnumerator()) {
                bool? hw = null;
                int width = getNextWidth(ewidths, int.MaxValue, ref hw);
                int start = 0, end;
                do {
                    end = getLineEnd(start, width, self);
                    char c = self[end - 1];
                    if (char.IsWhiteSpace(c))
                        --end;
                    bool needContinuation = end != self.Length && !isEolChar(c);
                    string continuation = "";
                    if (needContinuation) {
                        --end;
                        continuation = "-";
                    }

                    string line = self.Substring(start, end - start) + continuation;
                    yield return line;
                    start = end;
                    if (char.IsWhiteSpace(c))
                        ++start;
                    width = getNextWidth(ewidths, width, ref hw);
                } while (start < self.Length);
            }
        }

        private static int getNextWidth(IEnumerator<int> ewidths, int curWidth, ref bool? eValid) {
            if (!eValid.HasValue || eValid.HasValue && eValid.Value) {
                curWidth = (eValid = ewidths.MoveNext()).Value ? ewidths.Current : curWidth;
                // '.' is any character, - is for a continuation
                const string minWidth = ".-";
                if (curWidth < minWidth.Length)
                    throw new ArgumentOutOfRangeException("widths",
                            string.Format("Element must be >= {0}, was {1}.", minWidth.Length, curWidth));
                return curWidth;
            }

            // no more elements, use the last element.
            return curWidth;
        }

        private static bool isEolChar(char c) {
            return !char.IsLetterOrDigit(c);
        }

        private static int getLineEnd(int start, int length, string description) {
            int end = Math.Min(start + length, description.Length);
            int sep = -1;
            for (int i = start; i < end; ++i) {
                if (description[i] == '\n')
                    return i + 1;
                if (isEolChar(description[i]))
                    sep = i + 1;
            }

            if (sep == -1 || end == description.Length)
                return end;
            return sep;
        }

    }

}