using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Database
{
    public class ProjectItem
    {
        public int ID { set; get; }
        public string AliasName { set; get; }
        public string ProjectPath { set; get; }
        private bool _isActive;
        public bool IsActive 
        {
            set
            {
                if (value)
                {
                    IsActiveHTML = new object();
                }
                else
                {
                    IsActiveHTML = null;
                }
                _isActive = value;
            }
            get
            {
                return _isActive;
            }
        }
        public object IsActiveHTML { private set; get; }

        public ProjectItem(int id, string aliasName, string projectPath, bool isActive)
        {
            this.ID = id;
            this.AliasName = aliasName;
            this.ProjectPath = projectPath;
            this.IsActive = isActive;
        }

        public ProjectItem()
        {

        }
    }
}
