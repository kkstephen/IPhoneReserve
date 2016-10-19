using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IR.Common
{
    public class Product
    {
        public string Name { get; set; }
        public string ModelNumber { get; set; }
        public string Shop { get; set; }
        public string Status { get; set; }

        public string Location
        {
            get
            {
                switch (this.Shop)
                {
                    case "R409": return "Causeway Bay";

                    case "R485": return "Festival Walk";

                    case "R428" : return "IFC";

                    case "R499" : return "TST";
                       
                    case "R610": return "ShaTin";

                    default: return "";
                }
            }
        }

        public bool IsPlus
        {
            get
            {
                return this.Name.Contains("Plus");
            }
        }

        public bool Available
        {
            get
            {
                return this.Status != "NONE";
            }
        }
    }
}
