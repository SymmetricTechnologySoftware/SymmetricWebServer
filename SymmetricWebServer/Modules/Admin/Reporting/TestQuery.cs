using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Modules.Admin.Reporting
{
    [Serializable]
    public class TestQuery
    {
        public bool Success { private set; get; }
        public string Table { private set; get; }
        public string Message { private set; get; }

        public TestQuery(bool success, string table, string errorMessage)
        {
            this.Success = success;
            this.Table = table;
            this.Message = errorMessage;
        }
    }
}
