using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeChatLogin.common
{
    public class SettingConfig
    {
       public WxLogin WxLogin { get; set; }
    }

    public class WxLogin
    {
        public string AppId { get; set; }
        public string Secret { get; set; }
    }


}
