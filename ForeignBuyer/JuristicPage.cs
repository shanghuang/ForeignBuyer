using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForeignBuyer
{
    abstract class JuristicPage
    {
        abstract public string getPath();

        abstract public string download(string url, DateTime date);

        abstract public Boolean parse(DateTime date, String source, ref String[] res);
    }
}
