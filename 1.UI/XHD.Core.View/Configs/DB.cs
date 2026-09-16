using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;

using FreeSql;

using XHD.Core.Common;
using System.Data;

namespace XHD.Core.View
{
    public static class DB
    {
        public static void AddDb(this IServiceCollection services, IHostEnvironment env)
        {
            var dbConfig = new ConfigHelper().Get<DbConfig>("appsettings", env.EnvironmentName);

            IFreeSql fsql = new FreeSqlBuilder()
                        .UseConnectionString(dbConfig.Type, dbConfig.ConnectionString)
                        .UseAutoSyncStructure(true)
                        .Build();


            fsql.Aop.CurdAfter += (s, e) =>
            {
                //Log.Debug($"ManagedThreadId:{Thread.CurrentThread.ManagedThreadId}: FullName:{e.EntityType.FullName}" +
                //          $" ElapsedMilliseconds:{e.ElapsedMilliseconds}ms, {e.Sql}");

                //Console.WriteLine(e.Sql);

                //NLogger.WriteLog("SQL_", $"FullName:{e.EntityType.FullName} ElapsedMilliseconds:{e.ElapsedMilliseconds}ms, {e.Sql}");

                if (e.ElapsedMilliseconds > 200)
                {
                    //记录日志
                    NLogger.WriteLog("SQL_", $"FullName:{e.EntityType.FullName} ElapsedMilliseconds:{e.ElapsedMilliseconds}ms, {e.Sql}");
                    //发送短信给负责人
                }
            };

            services.AddSingleton(fsql);
        }
    }
}
