using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Wlniao;

namespace Models
{
    public class Right
    {
        private static Dictionary<String, String> sysRights = new Dictionary<String, String>();
        private static Dictionary<String, String> columnRights = new Dictionary<String, String>();
        public static Dictionary<String, String> SysRights
        {
            get
            {
                if (sysRights.Count == 0)
                {
                    sysRights.Add("System_Setting", "系统设置");
                    sysRights.Add("System_User_Manage", "用户管理");
                    sysRights.Add("System_User_Rights", "权限管理");
                    sysRights.Add("System_User_Column", "栏目管理");
                }
                return sysRights;
            }
        }
        public static Dictionary<String, String> ColumnRights
        {
            get
            {
                if (columnRights.Count == 0)
                {
                    columnRights.Add("Column_View", "查看栏目");
                    columnRights.Add("Column_Setting", "修改栏目设置");
                    columnRights.Add("Column_Publish", "栏目内容发布");
                    columnRights.Add("Column_Verify", "栏目内容审核");
                    columnRights.Add("Column_Delete", "栏目内容删除");
                }
                return columnRights;
            }
        }

        public int Id { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        [Required]
        [StringLength(50)]
        public String UserId { get; set; }
        /// <summary>
        /// 权限点
        /// </summary>
        [Required]
        [StringLength(50)]
        public string RightCode { get; set; }
        /// <summary>
        /// 区域ID
        /// </summary>
        [Required]
        [StringLength(50)]
        public string RightFor { get; set; }

        /// <summary>
        /// 检查用户权限
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="RightCode"></param>
        /// <param name="RightFor"></param>
        /// <returns></returns>
        public static Boolean Check(String UserId, String RightCode, String RightFor)
        {
            if (UserId.IsNotNullAndEmpty() && RightFor.IsNotNullAndEmpty() && !string.IsNullOrEmpty(RightCode))
            {
                var user = User.Get(UserId);
                if (user.data != null)
                {
                    if (user.data.Username == "super")
                    {
                        return true;
                    }
                    else
                    {
                        var row = new MyContext().Right.Where(r => r.UserId == UserId && r.RightCode == RightCode && r.RightFor == RightFor).FirstOrDefault();
                        if (row != null)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
