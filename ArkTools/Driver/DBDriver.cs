using System;
using System.Collections.Generic;
using ArkTools.Data;
using SavegameToolkit;

namespace ArkTools.Driver {
    public interface DBDriver {
        void openConnection(Uri uri);

        void openConnection(string path);

        IList<string> getUrlSchemeList();

        bool canHandlePath();

        string getParameter(string name);

        DBDriver setParameter(string name, string value);

        IDictionary<string, string> getSupportedParameters();

        void write(DataCollector data, WritingOptions writingOptions);

        void close();
    }

}
