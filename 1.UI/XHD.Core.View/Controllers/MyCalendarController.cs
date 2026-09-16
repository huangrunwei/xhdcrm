using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using XHD.Core.Common;
using XHD.Core.IRepository;
using XHD.Core.IServices;
using XHD.Core.Models;
using XHD.Core.View.Configs;

namespace XHD.Core.View.Controllers
{
    public class MyCalendarController : Controller
    {
        private readonly IMy_CalendarService _service;
        public MyCalendarController(IMy_CalendarService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<string> Grid()
        {
            var emp_id = User.FindFirst(ClaimTypes.Sid).Value;

            Expression<Func<My_Calendar, bool>> exp = a => a.emp_id == emp_id;

            var result = await _service.GridAsync(exp);

            return result.ToString();
        }

        public async Task<string> Save()
        {
            string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            var model = JsonConvert.DeserializeObject<My_Calendar>(requestBody);

            // 1. 验证必要字段
            if (string.IsNullOrWhiteSpace(model.title))
            {
                return XHDResult.Error("任务标题不能为空").ToString();
            }

            if (string.IsNullOrWhiteSpace(model.startDate) || string.IsNullOrWhiteSpace(model.endDate))
            {
                return XHDResult.Error("开始日期或结束日期不能为空").ToString();
            }

            // 2. 根据是否全天任务，构建 StartDateTime 和 EndDateTime
            try
            {
                if (model.allDay == 1) // 全天任务
                {
                    // 开始时间：当天 00:00:00
                    var startDateOnly = DateTime.ParseExact(model.startDate, "yyyy-MM-dd", null);
                    model.StartDateTime = startDateOnly.Date;

                    // 结束时间：结束日期的 23:59:59
                    var endDateOnly = DateTime.ParseExact(model.endDate, "yyyy-MM-dd", null);
                    model.EndDateTime = endDateOnly.Date.AddDays(1).AddSeconds(-1); // 当天 23:59:59
                }
                else // 非全天任务，需要拼接时间
                {
                    // 如果时间为空，给默认值（可选，根据业务决定）
                    var startTime = string.IsNullOrWhiteSpace(model.startTime) ? "00:00" : model.startTime;
                    var endTime = string.IsNullOrWhiteSpace(model.endTime) ? "23:59" : model.endTime;

                    var startDateTimeStr = $"{model.startDate} {startTime}";
                    var endDateTimeStr = $"{model.endDate} {endTime}";

                    model.StartDateTime = DateTime.ParseExact(startDateTimeStr, "yyyy-MM-dd HH:mm", null);
                    model.EndDateTime = DateTime.ParseExact(endDateTimeStr, "yyyy-MM-dd HH:mm", null);
                }
            }
            catch (FormatException)
            {
                return XHDResult.Error("日期或时间格式错误，请使用 yyyy-MM-dd 和 HH:mm 格式").ToString();
            }

            // 3. 执行保存
            int result = 0;
            if (string.IsNullOrWhiteSpace(model.id))
            {
                // 新增：生成主键
                model.id = UUIDNext.Uuid.NewSequential().ToString();
                model.emp_id = User.FindFirst(ClaimTypes.Sid).Value;
                result = await _service.AddAsync(model);
            }
            else
            {
                // 更新：先检查数据是否存在
                Expression<Func<My_Calendar, bool>> exp = a => a.id == model.id;

                var calendardata = await _service.GridAsync(exp);


                if (calendardata.count == 0)
                {
                    return XHDResult.Error("找不到数据").ToString();
                }

                result = await _service.UpdateAsync(model);
            }

            if (result == 0)
            {
                return XHDResult.Error("操作失败，系统错误").ToString();
            }

            return XHDResult.Success(model.id).ToString();

        }

        public async Task<string> Delete(string id)
        {
            var result = 0;

            result = await _service.DeleteAsync(id);


            return XHDResult.Success().ToString();
        }
    }
}
