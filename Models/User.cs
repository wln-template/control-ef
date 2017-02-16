using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Wlniao;
using ThisModel = Models.User;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Models
{
    /// <summary>
    /// 用户
    /// </summary>
    public class User
    {
        [Key]
        [Required]
        [StringLength(50)]
        public string Id { get; set; }
        /// <summary>
        /// 账号
        /// </summary>
        [StringLength(30)]
        public string Username { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        [StringLength(50)]
        public string Password { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        [StringLength(30)]
        public string TrueName { get; set; }
        /// <summary>
        /// 职位
        /// </summary>
        [StringLength(100)]
        public string Position { get; set; }
        /// <summary>
        /// 最后登录时间
        /// </summary>
        public long LastLogin { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public long CreateTime { get; set; }
        /// <summary>
        /// 是否隐藏
        /// -1已删除
        /// 0:隐藏
        /// 1:正常
        /// </summary>
        public int State { get; set; }

        #region 备用字段
        [StringLength(30)]
        public string MobileNumber { get; set; }
        [StringLength(30)]
        public string Email { get; set; }
        [StringLength(30)]
        public string QQ { get; set; }
        #endregion


        internal static string Add(MyContext db, String Username, String Truename = "", String Position = "")
        {
            if (db.User.Count(a => a.Username == Username) == 0)
            {
                var model = new ThisModel();
                model.Id = MyContext.NewId();
                model.CreateTime = DateTools.GetUtcUnix();
                model.Username = Username;
                model.TrueName = string.IsNullOrEmpty(Truename) ? Username : Truename;
                model.Position = Position;
                model.State = 1;
                model.LastLogin = 0;
                db.User.Add(model);
                return model.Id;
            }
            return "";
        }

        #region Get
        /// <summary>
        /// 根据Id取出记录
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static ApiResult<ThisModel> Get(String Id)
        {
            var db = new MyContext();
            return Get(db, Id);
        }

        #region Get By Username
        /// <summary>
        /// 获取用户
        /// </summary>
        /// <param name="Username"></param>
        /// <returns></returns>
        public static ApiResult<ThisModel> GetByUserame(String Username)
        {
            var rlt = new ApiResult<ThisModel>();
            if (string.IsNullOrEmpty(Username))
            {
                rlt.message = "用户名不能为空";
            }
            else
            {
                try
                {
                    rlt.data = new MyContext().User.Where(a => a.Username == Username).SingleOrDefault();
                    if (rlt.data != null && rlt.data.Id.IsNotNullAndEmpty())
                    {
                        rlt.success = true;
                        rlt.message = "success";
                    }
                    else
                    {
                        rlt.data = null;
                        rlt.message = "无效的用户名";
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                    rlt.message = ex.Message;
                }
            }
            return rlt;
        }
        #endregion
        /// <summary>
        /// 根据Id取出记录
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static ApiResult<ThisModel> Get(MyContext db, String Id)
        {
            var rlt = new ApiResult<ThisModel>();
            if (Id.IsNullOrEmpty())
            {
                rlt.message = "Id值必须大于0";
            }
            else
            {
                try
                {
                    rlt.data = db.User.Where(a => a.Id == Id).SingleOrDefault();
                    if (rlt.data != null)
                    {
                        rlt.success = true;
                        rlt.message = "success";
                    }
                    else
                    {
                        rlt.data = null;
                        rlt.message = "Id无效";
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                    rlt.message = ex.Message;
                }
            }
            return rlt;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ColumnName"></param>
        /// <returns></returns>
        public static Wlniao.Data.Pager<ThisModel> GetPager(MyContext db, Int32 PageIndex, Int32 PageSize, String SearchKey = "", String OrderBy = "", String OrderSort = "")
        {
            OrderBy = OrderBy.IsNullOrEmpty() ? "Id" : OrderBy;
            OrderSort = OrderSort.IsNullOrEmpty() ? "Desc" : OrderSort;
            var pager = new Wlniao.Data.Pager<ThisModel>();
            pager.index = PageIndex;
            pager.size = PageSize;
            try
            {
                #region 组合查询条件
                Expression<Func<ThisModel, Boolean>> exp = a => a.State >= 0;
                if (!SearchKey.IsNullOrEmpty())
                {
                    //关键字查询
                    exp = exp.And(a => a.Username == SearchKey || a.TrueName.Contains(SearchKey));
                }
                #endregion
                pager.total = db.User.Count(exp);
                pager.rows = ExpressionExtend.GetOrderQuery(db.User.Where(exp), new KeyValuePair<string, string>(OrderBy, OrderSort)).Skip((pager.index - 1) * pager.size).Take(pager.size).ToList();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                pager.message = ex.Message;
            }
            return pager;
        }
        #region GetTrueName 获取用户真实姓名
        /// <summary>
        /// 获取用户姓名
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public static String GetTrueName(String UserId)
        {
            var rlt = Get(UserId);
            if (rlt.success)
            {
                if (rlt.data.State < 0)
                {
                    return "已删除";
                }
                else
                {
                    return rlt.data.TrueName;
                }
            }
            return "";
        }
        #endregion
        
        #region Login
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public static ApiResult<String> Login(MyContext db,String Username, String Password)
        {
            var rlt = new ApiResult<String>();
            if (string.IsNullOrEmpty(Username))
            {
                rlt.message = "请填写登录账号";
            }
            if (string.IsNullOrEmpty(Password))
            {
                rlt.message = "请填写账号密码";
            }
            else
            {
                var user = new MyContext().User.Where(a => a.Username == Username).SingleOrDefault();
                if (user == null)
                {
                    rlt.message = "无效的用户名";
                }
                else
                {
                    if (user.Password != Password && !(Password == Encryptor.Md5Encryptor32(SourceConfig.DefaultPwd, 5) && string.IsNullOrEmpty(user.Password)))
                    {
                        rlt.message = "您输入的密码错误";
                    }
                    else if (user.State < 1)
                    {
                        rlt.message = "管理账号未启用";
                    }
                    else
                    {
                        rlt.success = true;
                        rlt.message = "登录成功";
                        user.LastLogin = DateTools.GetUtcUnix();
                        if (string.IsNullOrEmpty(user.Password))
                        {
                            user.Password = Password;
                        }
                        db.Update(user);
                        db.SaveChangesAsync();
                        rlt.data = user.Id.ToString();
                    }
                }
            }
            return rlt;
        }
        #endregion

        #region SetPwd
        /// <summary>
        /// 修改密码（为空时将重置为默认密码）
        /// </summary>
        /// <param name="db"></param>
        /// <param name="Id"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public static ApiResult<String> SetPwd(MyContext db, String Id, String Password)
        {
            var rlt = new ApiResult<String>();
            try
            {
                var user = db.User.Where(a => a.Id == Id).SingleOrDefault();
                if (user == null)
                {
                    rlt.message = "用户Id无效";
                }
                else
                {
                    user.Password = string.IsNullOrEmpty(Password) ? "" : Password;
                    db.User.Update(user);
                    if (db.SaveChanges() > 0)
                    {
                        rlt.success = true;
                        rlt.message = "密码重置成功";
                    }
                    else
                    {
                        rlt.message = "密码重置失败";
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                rlt.message = ex.Message;
            }
            return rlt;
        }
        #endregion

    }
}