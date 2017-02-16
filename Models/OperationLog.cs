using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Wlniao;
namespace Models
{
    /// <summary>
    /// 操作日志
    /// </summary>
    public class OperationLog
    {
        [Key]
        [Required]
        [StringLength(50)]
        public string Id { get; set; }
        /// <summary>
        /// 所属区域
        /// </summary>
        [StringLength(50)]
        public string OrganId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [StringLength(50)]
        public string UserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [StringLength(50)]
        public string Model { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [StringLength(50)]
        public string Method { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [StringLength(50)]
        public string Key { get; set; }
        /// <summary>
        /// 备注内容
        /// </summary>
        [StringLength(500)]
        public string Comments { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public long CreateTime { get; set; }
        
        public static void Add(MyContext db, String Model, String Method, String Key, String Comments, String UserId, String OrganId)
        {
            var item = new OperationLog();
            item.Id = MyContext.NewId();
            item.CreateTime = DateTools.GetUtcUnix();
            item.OrganId = OrganId;
            item.UserId = UserId;
            item.Comments = Comments;
            item.Model = Model.ToLower();
            item.Method = Method.ToLower();
            item.Key = Key;
            db.Add(item);
            db.SaveChangesAsync();
        }
        public static void Add(String Model, String Method, String Key, String UserId)
        {
            Add(new MyContext(), Model, Method, Key, "", UserId, "");
        }

        public static void Add(String Model, String Method, String Key, String UserId, String Comments)
        {
            Add(new MyContext(), Model, Method, Key, Comments, UserId, "");
        }
    }
}