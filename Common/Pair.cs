using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraConnector.Common
{
    class Pair<F, S>
    {
        public F first;
        public S second;
        public Pair(F first, S second)
        {
            this.first = first;
            this.second = second;
        }
    }
}
