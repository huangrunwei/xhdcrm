using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Threading.Tasks;

using XHD.Core.IServices;
using XHD.Core.IRepository;
using XHD.Core.Models;
using XHD.Core.Common;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace XHD.Core.Services
{
internal class Sys_logService : BaseService<Sys_log>, ISys_logService
    {
        private ISys_logRepository irepository;
        public Sys_logService(ISys_logRepository repository)
        {
            _irepository = repository;
            irepository = repository;
        }


        public async Task<JObject> LogType()
        {
            var result = await irepository.Logtype();

            var json = JsonConvert.SerializeObject(result, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" });
            JArray arr = JArray.Parse(json);

            return XHDResult.Result(0, "", arr);
        }

        //更新日志
        public async Task<int> UpdateLog(Sys_log models)
        {
           return await _irepository.AddAsync(models);
        }

        //删除日志
        public async Task<int> DeleteLog(Sys_log models)
        {
            return await _irepository.AddAsync(models);
        }

        //登录日志
        public async Task<int> LoginLog(string emp_id, string emp_name, string ip)
        {
            Sys_log models = new Sys_log();

            models.id = System.Guid.NewGuid().ToString();
            models.EventType = "用户登录";
            models.EventID = emp_id;
            models.EventTitle = emp_name;
            models.UserID = emp_id;
            models.UserName = emp_name;
            models.IPStreet = ip;
            models.EventDate = DateTime.Now;

            return await _irepository.AddAsync(models);
        }
    }
}
