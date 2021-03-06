﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Template
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "Webc程序服务端，端口号：" + Wlniao.XCore.ListenPort;
            try
            {
                using (var db = new Models.MyContext())
                {
                    try
                    {
                        if (!db.Database.EnsureCreated())
                        {
                            db.Database.Migrate();
                        }
                    }
                    catch { }
                    var isInit = Models.Setting.Get(db,"IsInit");
                    if (isInit != "true")
                    {
                        Models.Setting.Set(db,"IsInit","true");
                        if (db.User.Count() == 0)
                        {
                            Models.User.Add(db, "super", "系统管理员");
                        }
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Wlniao.log.Error("初始化数据库错误：" + ex.Message);
            }
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls("http://*:" + Wlniao.XCore.ListenPort)
                .Build();
            host.Run();
        }
    }
}
