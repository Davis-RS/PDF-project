using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDF_project
{
    internal interface ICookieManager
    {
        string getCookieValueFromResponse(HttpResponseMessage response, string cookieName);

        bool verifyCookie();
    }
}
