using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeChatLogin.common;

namespace WeChatLogin.Models
{
    public class WechatLogin
    {

        private SettingConfig _settingConfig;
        public WechatLogin(SettingConfig setting)
        {
            _settingConfig = setting;
        }
        /// <summary>
        /// 应用唯一标识，在微信开放平台提交应用审核通过后获得
        /// </summary>
        public string AppId { get { return _settingConfig?.WxLogin?.AppId; } }

        /// <summary>
        /// 应用密钥AppSecret，在微信开放平台提交应用审核通过后获得
        /// </summary>
        public string Secret { get { return _settingConfig?.WxLogin?.Secret; } }

    }

    public class AccessTokenRequest : WechatLogin
    {
        public AccessTokenRequest(SettingConfig setting) : base(setting)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        public string GrantType { get; } = "authorization_code";
        /// <summary>
        /// 描码后取到的code参数
        /// </summary>
        public string Code { get; set; }


    }

    public class AccessTokenResponse : ErrorResponse
    {
        /// <summary>
        /// 调用凭证
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        /// access_token接口调用凭证超时时间，单位（秒）
        /// </summary>
        public long expires_in { get; set; }

        /// <summary>
        /// 填写通过access_token获取到的refresh_token参数
        /// </summary>
        public string refresh_token { get; set; }

        /// <summary>
        /// 普通用户的标识，对当前开发者帐号唯一
        /// </summary>
        public string openid { get; set; }
        /// <summary>
        /// 用户授权的作用域，使用逗号（,）分隔
        /// </summary>
        public string scope { get; set; }
        /// <summary>
        /// 当且仅当该网站应用已获得该用户的userinfo授权时，才会出现该字段。
        /// </summary>
        public string unionid { get; set; }


    }

    public class RefreshTokenRequest : WechatLogin
    {
        public RefreshTokenRequest(SettingConfig setting) : base(setting)
        {

        }
        /// <summary>
        /// 填写通过access_token获取到的refresh_token参数
        /// </summary>
        public string RefreshToken { get; set; }

        public string GrantType { get; } = "refresh_token";
    }

    public class ErrorResponse
    {
        /// <summary>
        /// 错误码，0表示成功
        /// </summary>
        public string errcode { get; set; }
        /// <summary>
        /// 错误信息，成功时结果 为ok
        /// </summary>
        public string errmsg { get; set; }
    }

    public class AuthResponse : ErrorResponse
    {

    }

    public class UserInfoRequest
    {
        /// <summary>
        /// 调用凭证
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// 普通用户的标识，对当前开发者帐号唯一
        /// </summary>
        public string OpenId { get; set; }

        /// <summary>
        /// 国家地区语言版本，zh_CN 简体，zh_TW 繁体，en 英语，默认为zh-CN
        /// </summary>
        public string Lang { get; set; }
    }

    public class UserInfoResponse : ErrorResponse
    {
        /// <summary>
        /// 普通用户的标识，对当前开发者帐号唯一
        /// </summary>
        public string openid { get; set; }
        /// <summary>
        /// 普通用户昵称
        /// </summary>
        public string nickname { get; set; }
        /// <summary>
        /// 性别，1为男性，2为女性
        /// </summary>
        public int sex { get; set; }
        /// <summary>
        /// 省份
        /// </summary>
        public string province { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string city { get; set; }
        /// <summary>
        /// 国家
        /// </summary>
        public string country { get; set; }
        /// <summary>
        /// 头像地址
        /// </summary>
        public string headimgurl { get; set; }
        /// <summary>
        /// 用户特权信息，json数组，如微信沃卡用户为（chinaunicom）
        /// </summary>
        public string[] privilege { get; set; }
        /// <summary>
        /// 用户统一标识。针对一个微信开放平台帐号下的应用，同一用户的unionid是唯一的
        /// </summary>
        public string unionid { get; set; }
    }
}
