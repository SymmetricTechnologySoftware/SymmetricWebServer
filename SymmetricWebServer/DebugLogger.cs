using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    public class DebugLogger
    {
        public const string MessageFilter = "MESSAGE";

        public const int FlushLimit = 2;
        public const int SizeLimit = 5000000;//5 megabytes

        private int _flush;
        private StreamWriter _file;
        private FileInfo _info;
        private Object _fileLock;

        public string Filter { private set; get; }

        public DebugLogger()
        {
            Initialize();
        }

        private void Initialize()
        {
            this._fileLock = new Object();
            this.Filter = DebugLogger.MessageFilter;
            string fileName = String.Format(Globals.ApplicationDirectory + "WebServer_{0}.txt", this.Filter);

            if (File.Exists(fileName))
            {
                _info = new FileInfo(fileName);
                if (_info.Length > SizeLimit)
                {
                    File.Delete(fileName);
                }
            }

            _file = new StreamWriter(fileName, true);
            _flush = 1;
            _file.WriteLine("************************************************");
        }

        public void WriteLine(string message)
        {
            lock (this._fileLock)
            {
                if (_file == null) return;
                if (++_flush == DebugLogger.FlushLimit)
                {
                    _flush = 0;
                    _file.Flush();
                }
                _file.WriteLine(String.Format("{0}\t{1}", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), message));
            }
        }

        public void CloseFile()
        {
            lock (_fileLock)
            {
                if (_file == null)
                {
                    _flush = 0;
                    return;
                }

                if (_flush > 0)
                {
                    _file.Flush();
                }
                _flush = 0;
                try
                {
                    _file.Close();
                }
                catch
                {
                    //ignore
                }
                finally
                {
                    _file = null;
                }
            }
        }
    }
}
