using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    public class OptionItem
    {
        public OptionItem(int id, string name)
            : this(id, name, false)
        {

        }

        public OptionItem(int id, string name, bool selected)
        {
            _isSelected = selected;
            this.ID = id;
            this.Name = name;
        }

        public int ID { set; get; }
        public string Name { set; get; }
        private bool _isSelected;
        public string IsSelected
        {
            get
            {
                if (_isSelected)
                {
                    return "selected";
                }
                else
                {
                    return "";
                }
            }
        }

        public void SetSelected(bool value)
        {
            _isSelected = value;
        }

        public object Tag { set; get; }



    }
}
