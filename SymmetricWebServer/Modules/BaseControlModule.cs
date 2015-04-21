using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServer.Database;

namespace WebServer.Modules
{
    public abstract class BaseControlModule : BaseWebModule
    {
        protected dynamic Model { private set; get; }

        public string Heading { private set; get; }
        public string Alias { private set; get; }
        public string MainPage { private set; get; }

        public BaseControlModule(AccessLevels accessLevel, string heading, 
                                string alias, string mainPage)
            : base(accessLevel)
        {
            this.Model = new System.Dynamic.ExpandoObject();

            this.Heading = heading;
            this.Alias = alias;
            this.MainPage = mainPage;

            this.Model.Heading = this.Heading;
            this.Model.Alias = this.Alias;
            this.Model.MainPage = this.MainPage;
        }

        protected abstract List<BasicEntry> Items();

        protected List<BasicEntry> SortedList()
        {
            List<BasicEntry> items = this.Items();
            if (items == null) return new List<BasicEntry>();
            
            items.Sort((entry1, entry2) => entry1.GroupName.CompareTo(entry2.GroupName));
            string groupName = "";
            foreach (BasicEntry item in items)
            {
                if (groupName.Equals(item.GroupName))
                {
                    item.GroupName = "";
                }
                else
                {
                    groupName = item.GroupName;
                }
            }

            return items;
        }
    }
}
