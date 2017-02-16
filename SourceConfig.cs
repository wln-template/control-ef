using System;
using Wlniao;
/// <summary>
/// 从源码读取的配置
/// </summary>
public class SourceConfig
{
    #region 常量
    /// <summary>
    /// 系统版本号
    /// </summary>
    internal const string Version = "0.1.0";
    /// <summary>
    /// 程序系统名称
    /// </summary>
    internal const string SystemName = "XXX系统";
    /// <summary>
    /// 数据库加密字段
    /// </summary>
    internal const string DataSecret = "wlniao";
    #endregion

    #region 静态变量
    private static string _HomeUrl = null;
    private static string _ResPath = null;

    #endregion


    /// <summary>
    /// 首页地址
    /// </summary>
    public static string HomeUrl
    {
        get
        {
            if (_HomeUrl == null)
            {
                _HomeUrl = Config.GetConfigsAutoWrite("HomeUrl", "");
                if (string.IsNullOrEmpty(_HomeUrl))
                {
                    _HomeUrl = "http://www." + Wlniao.XServer.Common.Domain;
                }
                else if (_HomeUrl.IndexOf("://") < 0)
                {
                    _HomeUrl = "http://" + _HomeUrl;
                }
                _HomeUrl = _HomeUrl.TrimEnd('/');
            }
            return _HomeUrl;
        }
    }
    /// <summary>
    /// 定制资源的相对路径
    /// </summary>
    public static string ResPath
    {
        get
        {
            if (_ResPath == null)
            {
                _ResPath = Config.GetConfigsAutoWrite("ResPath", "");
                _ResPath = string.IsNullOrEmpty(ResPath) ? "" : "/" + ResPath.TrimEnd('/').TrimStart('/');
            }
            return _ResPath;
        }
    }
    /// <summary>
    /// 新用户默认密码
    /// </summary>
    public static String DefaultPwd = Models.Setting.Get("DefaultPwd", "111111", "用户默认密码");
    /// <summary>
    /// Cookie加密密码
    /// </summary>
    public static String CookieSafe = Models.Setting.Get("CookieSafe", "wlnsite", "Cookie加密密码");
    /// <summary>
    /// 静态资源路径
    /// </summary>
    public static String AssetsPath = "http://static.wlniao.com";

    /// <summary>
    /// 是否使用子系统功能
    /// </summary>
    public static Boolean SubSystem = false;
}