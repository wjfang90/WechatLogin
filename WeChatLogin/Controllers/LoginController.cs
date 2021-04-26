using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WeChatLogin.common;

namespace WeChatLogin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private HttpClient _httpClient;
        private IConfiguration _configuration;
        private SettingConfig _settingConfig;

        public LoginController(ILogger<LoginController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration, SettingConfig settingConfig)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://api.weixin.qq.com");


            var AppId = GetConfig<string>("AppSettings:WxLogin", "AppId");
            
            _settingConfig = settingConfig;
        }


        public async Task<JsonResult> Login(string code, string state)
        {

            //第二步：通过code获取access_token
            if (string.IsNullOrEmpty(code) || state != "fang")
                return new JsonResult(new { State = -1, Message = "参数错误" });


            var accessTokenRequest = new AccessTokenRequest(_settingConfig)
            {
                Code = code
            };
            var accessTokenRes = await GetAccessToken(accessTokenRequest);


            if (!string.IsNullOrEmpty(accessTokenRes.errcode) || string.IsNullOrEmpty(accessTokenRes.access_token) || string.IsNullOrEmpty(accessTokenRes.openid))
            {
                _logger.LogError("获取access token error");
                return new JsonResult(new { State = -2, Message = "获取access token error" });

            }


            //第三步：检验授权凭证（access_token）是否有效

            var AuthRequest = new UserInfoRequest()
            {
                AccessToken = accessTokenRes.access_token,
                OpenId = accessTokenRes.openid
            };
            var authRes = await AuthAccessToken(AuthRequest);

            if (!authRes)
            {
                //刷新token
                var refreshTokenRequest = new RefreshTokenRequest(_settingConfig)
                {
                    RefreshToken = accessTokenRes.refresh_token
                };
                await RefreshAccessToken(refreshTokenRequest);

                //再次验证
                authRes = await AuthAccessToken(AuthRequest);

                if (!authRes)
                {
                    return new JsonResult(new { State = -3, Message = "验证失败" });
                }
            }



            //第三步:获取用户个人信息（UnionID机制）

            if (authRes)
            {
                var userRequest = new UserInfoRequest()
                {
                    AccessToken = accessTokenRes.access_token,
                    OpenId = accessTokenRes.openid
                };

                var userRes = await GetUserInfo(userRequest);

                if (!string.IsNullOrEmpty(userRes.errcode) && userRes.errcode == "40003")
                {
                    return new JsonResult(new { State = -4, Message = "获取用户信息失败" });
                }

                _logger.LogInformation(JsonConvert.SerializeObject(userRes));

                return new JsonResult(new { State = 0, Message = "登录成功" });
            }

            return new JsonResult(new { State = -3, Message = "登录失败" });

        }

        private async Task<AccessTokenResponse> GetAccessToken(AccessTokenRequest accessTokenRequest)
        {
            var url = $"/sns/oauth2/access_token?appid={accessTokenRequest.AppId}&secret={accessTokenRequest.Secret}&code={accessTokenRequest.Code}&grant_type=authorization_code";
            var res = await _httpClient.GetAsync(url);

            if (res.IsSuccessStatusCode)
            {
                var responseStr = await res.Content.ReadAsStringAsync();
                var resObj = JsonConvert.DeserializeObject<AccessTokenResponse>(responseStr);

                return resObj;
            }

            return null;
        }

        /// <summary>
        /// refresh_token拥有较长的有效期（30天），当refresh_token失效的后，需要用户重新授权。
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        private async Task<AccessTokenResponse> RefreshAccessToken(RefreshTokenRequest refreshToken)
        {
            var url = $"/sns/oauth2/refresh_token?appid={refreshToken.AppId}&refresh_token={refreshToken.RefreshToken}&grant_type=refresh_token";
            var res = await _httpClient.GetAsync(url);

            if (res.IsSuccessStatusCode)
            {
                var responseStr = await res.Content.ReadAsStringAsync();
                var resObj = JsonConvert.DeserializeObject<AccessTokenResponse>(responseStr);

                return resObj;
            }

            return null;
        }

        /// <summary>
        /// 检验授权凭证（access_token）是否有效
        /// </summary>
        /// <param name="authRequest"></param>
        /// <returns></returns>
        private async Task<bool> AuthAccessToken(UserInfoRequest authRequest)
        {
            var url = $"/sns/auth?access_token={authRequest.AccessToken}&openid={authRequest.OpenId}";
            var res = await _httpClient.GetAsync(url);

            if (res.IsSuccessStatusCode)
            {
                var responseStr = await res.Content.ReadAsStringAsync();
                var resObj = JsonConvert.DeserializeObject<AuthResponse>(responseStr);

                return resObj.errcode == "0";
            }

            return false;
        }

        private async Task<UserInfoResponse> GetUserInfo(UserInfoRequest userInfoRequest)
        {
            var url = $"/sns/userinfo?access_token={userInfoRequest.AccessToken}&openid={userInfoRequest.OpenId}";
            var res = await _httpClient.GetAsync(url);

            if (res.IsSuccessStatusCode)
            {
                var responseStr = await res.Content.ReadAsStringAsync();
                var resObj = JsonConvert.DeserializeObject<UserInfoResponse>(responseStr);

                return resObj;
            }

            return null;
        }

        public T GetConfig<T>(string sectionName, string key)
        {
            return _configuration.GetSection(sectionName).GetValue<T>(key);
        }

    }

    public class LoginRequest
    {
        public string Code { get; set; }
        public string State { get; set; }
    }

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
