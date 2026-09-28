using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction
{
  public abstract class PopDataModel
    {
        public string Item1Id { get; set; }

        public string Item1Name { get; set; }


        public string Item2Id { get; set; }


        public string Item2Name { get; set; }


        public abstract string PopShow { get; set; }
    }
}
