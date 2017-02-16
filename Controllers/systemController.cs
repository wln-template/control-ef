using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Wlniao;
using Wlniao.XServer;
namespace Template.Controllers
{
    /// <summary>
    /// 系统模块
    /// </summary>
    public partial class systemController : BaseController
    {
        /// <summary>
        /// 后台首页(登录页)
        /// </summary>
        /// <returns></returns>
        public IActionResult index()
        {
            var username = GetRequest("username");
            var password = GetRequest("password");
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                if (method == "logout")
                {
                    Response.Cookies.Delete("uid");
                    return Redirect(Request.Path.ToString());
                }
                else if (_UserId.IsNotNullAndEmpty())
                {
                    var api = Models.User.Get(_UserId);
                    if (api.success)
                    {
                        ViewBag.TrueName = api.data.TrueName;
                        ViewBag.SubSystem = SourceConfig.SubSystem;
                        return View();
                    }
                    else
                    {
                        return Redirect(Request.Path.ToString() + "?do=logout");
                    }
                }
                else
                {
                    if (file.Exists(Wlniao.IO.PathTool.Map("wwwroot", SourceConfig.ResPath, "img", "loginBg.png")))
                    {
                        ViewBag.SystemNameForLogin = "";
                    }
                    else
                    {
                        ViewBag.SystemNameForLogin = "<font>" + Models.Setting.Get("SystemName", SourceConfig.SystemName) + "</font>";
                    }
                    return View("login");
                }
            }
            else
            {
                var rlt = Models.User.Login(db,username, Encryptor.Md5Encryptor32(password, 5));
                if (rlt.success)
                {
                    Response.Cookies.Append("uid", Encryptor.AesEncrypt(rlt.data, SourceConfig.CookieSafe));
                }
                return Json(rlt);
            }
        }
        /// <summary>
        /// 用户密码修改
        /// </summary>
        /// <returns></returns>
        public IActionResult pwd()
        {
            if (method == "save")
            {
                var opwd = GetRequest("opwd");
                var npwd = GetRequest("npwd");
                var rpwd = GetRequest("rpwd");
                var user = Models.User.Get(db, _UserId);
                if (user == null)
                {
                    errorMsg = "登录超时";
                }
                else if (opwd.IsNullOrEmpty())
                {
                    errorMsg = "旧密码未填写，请填写";
                }
                else if (npwd.IsNullOrEmpty())
                {
                    errorMsg = "新密码未填写，请填写";
                }
                else if (npwd != rpwd)
                {
                    errorMsg = "两次输入的密码不一致";
                }
                else
                {
                    var api = Models.User.Login(db, user.data.Username, Encryptor.Md5Encryptor32(opwd, 5));
                    if (api.success)
                    {
                        api = Models.User.SetPwd(db, user.data.Id, Encryptor.Md5Encryptor32(npwd, 5));
                        if (api.success)
                        {
                            return Json(new { success = true, message = "密码修改成功" });
                        }
                        else
                        {
                            errorMsg = api.message;
                        }
                    }
                    else
                    {
                        errorMsg = "旧密码错误";
                    }
                }
            }
            else
            {
                return View();
            }
            return Json(new { message = errorMsg });
        }


        /// <summary>
        /// 用户管理
        /// </summary>
        /// <returns></returns>
        public IActionResult user()
        {
            if (Right("System_UserManage"))
            {
                if (method == "set")
                {
                    #region 保存
                    var Username = GetRequestDecode("Username");
                    var TrueName = GetRequestDecode("TrueName");
                    var Position = GetRequestDecode("Position");
                    if (string.IsNullOrEmpty(Username))
                    {
                        return Json(new { message = "用户名必须填写" });
                    }
                    else if (string.IsNullOrEmpty(TrueName))
                    {
                        return Json(new { message = "姓名必须填写" });
                    }
                    else
                    {
                        var Id = GetRequest("Id");
                        var old = db.User.Where(a => a.Username == Username).FirstOrDefault();
                        if (old != null && (Id.IsNullOrEmpty() || old.Id != Id))
                        {
                            return Json(new { message = "用户名“" + Username + "”已使用" });
                        }
                        else
                        {
                            var row = old != null ? old : db.User.Where(a => a.Id == Id).FirstOrDefault();
                            if (row == null)
                            {
                                var tempId = Models.User.Add(db, Username, TrueName, Position);
                                Models.OperationLog.Add("User", "Add", tempId, _UserId, "创建用户【" + Username + "】");
                            }
                            else
                            {
                                var tempName = row.Username;
                                row.TrueName = TrueName;
                                row.Username = Username;
                                row.Position = Position;
                                row.State = GetRequestInt("State");
                                db.Update(row);
                                if (tempName == row.Username)
                                {
                                    Models.OperationLog.Add("User", "Update", row.Id, _UserId, "编辑用户【" + row.Username + "】");
                                }
                                else
                                {
                                    Models.OperationLog.Add("User", "Update", row.Id, _UserId, "编辑用户【" + tempName + " >>> " + row.Username + "】");
                                }
                            }
                            if (db.SaveChanges() > 0)
                            {
                                return Json(new { success = true, message = "用户保存成功" });
                            }
                            else
                            {
                                return Json(new { message = "用户保存失败" });
                            }
                        }
                    }
                    #endregion
                }
                else if (method == "del")
                {
                    #region 删除
                    var rlt = new ApiResult<String>();
                    var Id = GetRequest("Id");
                    if (Id.IsNullOrEmpty())
                    {
                        rlt.message = "请先选择要删除的用户";
                    }
                    else
                    {
                        try
                        {
                            var user = db.User.Where(a => a.Id == Id).SingleOrDefault();
                            if (user == null)
                            {
                                rlt.message = "Id无效";
                            }
                            else
                            {
                                var tempName = user.Username;
                                user.State = -1;
                                user.Username = tempName + "[DEL:" + DateTools.GetUtcUnix() + "]";
                                db.Update(user);
                                if (db.SaveChanges() > 0)
                                {
                                    Models.OperationLog.Add("User", "del", user.Id, _UserId, "删除用户【" + tempName + "】");
                                    rlt.success = true;
                                    rlt.message = "删除成功";
                                }
                                else
                                {
                                    rlt.message = "删除失败";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex.Message);
                            rlt.message = ex.Message;
                        }
                    }
                    return Json(rlt);
                    #endregion
                }
                else if (method == "repwd")
                {
                    return Json(Models.User.SetPwd(db, GetRequest("Id"), ""));
                }
                else if (method == "get")
                {
                    #region
                    var rlt = Models.User.Get(db, GetRequest("Id"));
                    if (rlt.success)
                    {
                        var obj = rlt.data;
                        return Json(new
                        {
                            Id = obj.Id
                            ,
                            Username = obj.Username
                            ,
                            TrueName = obj.TrueName
                            ,
                            Position = obj.Position
                            ,
                            State = obj.State
                        });
                    }
                    else
                    {
                        return Json(null);
                    }
                    #endregion
                }
                else if (method == "list")
                {
                    #region 获取列表
                    var pager = Models.User.GetPager(db, GetRequestInt("page"), GetRequestInt("rows"), GetRequest("key"), GetRequest("orderby"), GetRequest("ascdesc"));
                    var list = new List<Object>();
                    foreach (var row in pager.rows)
                    {
                        list.Add(new
                        {
                            Id = row.Id
                            ,
                            State = row.State == 1 ? "正常" : "停用"
                            ,
                            TrueName = row.TrueName
                            ,
                            Username = row.Username
                            ,
                            Position = row.Position
                            ,
                            CreateTime = row.CreateTime == 0 ? "" : DateTools.ConvertToTimeZoneAndFormat(row.CreateTime, 8, "yyyy年MM月dd日")
                            ,
                            LastLogin = row.LastLogin == 0 ? "" : DateTools.ConvertToTimeZoneAndFormat(row.LastLogin, 8, "yyyy年MM月dd日 HH:mm")
                        });
                    }
                    return Json(Wlniao.Data.Pager<Object>.Format(list, pager.total, pager.index, pager.size));
                    #endregion
                }
                else
                {
                    ViewBag.DefaultPwd = SourceConfig.DefaultPwd;
                    return View();
                }
            }
            else
            {
                return NoRight();
            }
        }

        /// <summary>
        /// 基础设置
        /// </summary>
        /// <returns></returns>
        public IActionResult setting()
        {
            if (method == "set")
            {
                #region 保存
                var Key = GetRequest("key");
                var Value = GetRequest("value");
                if (string.IsNullOrEmpty(Key))
                {
                    return Json(new { success = false, message = "Key未指定" });
                }
                else
                {
                    return Json(Models.Setting.Set(Key, Value));
                }
                #endregion
            }
            else if (method == "get")
            {
                #region 获取
                return Json(Models.Setting.Get(GetRequest("key")));
                #endregion
            }
            else
            {
                ViewBag.SystemName = Models.Setting.Get("SystemName", SourceConfig.SystemName);
                return View();
            }
        }

        /// <summary>
        /// 权限设置
        /// </summary>
        /// <returns></returns>
        public IActionResult rights()
        {
            var UserId = GetRequest("UserId");
            if (method == "set")
            {
                #region 授权
                var code = GetRequest("code");
                var rightfor = GetRequest("rightfor");
                if (string.IsNullOrEmpty(UserId))
                {
                    return Json(new { success = false, message = "UserId未指定" });
                }
                else if (string.IsNullOrEmpty(code))
                {
                    return Json(new { success = false, message = "Code未指定" });
                }
                else if (string.IsNullOrEmpty(rightfor))
                {
                    return Json(new { success = false, message = "RightFor未指定" });
                }
                else
                {
                    var row = db.Right.Where(r => r.UserId == UserId && r.RightCode == code && r.RightFor == rightfor).FirstOrDefault();
                    if (row == null)
                    {
                        row = new Models.Right();
                        row.UserId = UserId;
                        row.RightCode = code;
                        row.RightFor = rightfor;
                        db.Add(row);
                        if (db.SaveChanges() > 0)
                        {
                            return Json(new { success = true });
                        }
                        else
                        {
                            return Json(new { message = "保存失败" });
                        }
                    }
                    else
                    {
                        return Json(new { success = true });
                    }
                }
                #endregion
            }
            else if (method == "del")
            {
                #region 取消
                var code = GetRequest("code");
                var rightfor = GetRequest("rightfor");
                var row = db.Right.Where(r => r.UserId == UserId && r.RightCode == code && r.RightFor == rightfor).FirstOrDefault();
                if (row == null)
                {
                    return Json(new { success = true });
                }
                else
                {
                    db.Remove(row);
                    if (db.SaveChanges() > 0)
                    {
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { message = "保存失败" });
                    }
                }
                #endregion
            }
            else if (method == "pager")
            {
                #region 获取
                var rightfor = GetRequest("rightfor");
                var rightcode = Models.Right.SysRights;
                var right = new Dictionary<String, Boolean>();
                if (rightfor == "column")
                {
                    rightcode = Models.Right.ColumnRights;
                }
                else
                {
                    rightfor = "system";
                }
                foreach (var key in rightcode.Keys)
                {
                    right.Add(key, false);
                }
                var list = new List<Object>();
                #region 数据库中读取权限
                var rows = db.Right.Where(a => a.UserId == UserId && a.RightFor == rightfor).ToList();
                foreach (var row in rows)
                {
                    if (right.ContainsKey(row.RightCode))
                    {
                        right[row.RightCode] = true;
                    }
                }
                #endregion
                var em = rightcode.GetEnumerator();
                while (em.MoveNext())
                {
                    list.Add(new
                    {
                        Code = em.Current.Key
                        ,
                        Name = em.Current.Value
                        ,
                        RightFor = rightfor
                        ,
                        State = right.ContainsKey(em.Current.Key) ? right[em.Current.Key] : false
                    });
                }
                return Json(Wlniao.Data.Pager<Object>.Format(list, list.Count, 1, list.Count));
                #endregion
            }
            else
            {
                ViewBag.UserId = UserId;
                return View();
            }
        }
    }
}
