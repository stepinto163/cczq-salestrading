using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netapi.Api
{
    public interface ILoginHandler
    {
        object Login(string username, string password, bool isEncrypt,
            string host, string port, string protocal, object loginMsgListener);
    }
}
