using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDF_project
{
    internal interface IPdfManager
    {
        List<object> getResults(string pdfUrl);
    }
}
