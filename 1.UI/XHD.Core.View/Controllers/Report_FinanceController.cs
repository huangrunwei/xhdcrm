using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using System.Security.Claims;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using XHD.Core.IServices;
using XHD.Core.IRepository;
using XHD.Core.Common;
using XHD.Core.Models;

using System.Linq.Expressions;
using Newtonsoft.Json.Converters;
using System.Collections;
using XHD.Core.View.Configs;


namespace XHD.Core.View.Controllers
{
    [Authorize]
    public class Report_FinanceController : Controller
    {
        private readonly ILogger<Report_FinanceController> _logger;
        private readonly IFinance_InvoiceService _InvoiceService;
        private readonly IFinance_ReceiveService _ReceiveService;

        private readonly ISys_logService _LogService;
        private readonly IDBAuthService _dBAuthService;
        private readonly SysLogExt<Sale_order> logext = new SysLogExt<Sale_order>();  //日志

        public Report_FinanceController(
            ILogger<Report_FinanceController> logger,            
            IFinance_InvoiceService InvoiceService,
            IFinance_ReceiveService ReceiveService, 
            ISys_logService LogService,
            IDBAuthService dBAuthService)
        {
           
            _logger = logger;
            _InvoiceService = InvoiceService;
            _ReceiveService = ReceiveService;


            _LogService = LogService;
            _dBAuthService = dBAuthService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<string> ReportYear()
        {
            Expression<Func<Finance_Receive, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.Receive_date.Value.Year == Request.Query["year"]);
            }
            else
            {
                exp = exp.And(a => a.Receive_date.Value.Year == DateTime.Now.Year);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["emp_id"]))
            {
                string[] emplist = Request.Query["emp_id"].ToString().Split(',');

                if (emplist.Length > 0)
                {
                    var list = new List<string>();

                    foreach (var emp in emplist)
                    {
                        list.Add(emp.ToString());
                    }
                    exp = exp.And(a => list.Contains(a.Payee_id));
                }
            }


            var result = await _ReceiveService.ReportYear(exp);

            JArray arr = new JArray();
            for (int i = 1; i <= 12; i++)
            {
                JObject obj = new JObject();
                obj.Add("xmonth", i);

                var sdata = result.Where(a => a.Value<int>("xmonth") == i).FirstOrDefault();

                if (sdata == null)
                {
                    obj.Add("count", 0);
                }
                else
                {
                    obj.Add("count", sdata.Value<int>("count"));
                }

                arr.Add(obj);
            }

            return arr.ToString();
        }

        public async Task<string> ReportYearSum()
        {
            Expression<Func<Finance_Receive, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.Receive_date.Value.Year == Request.Query["year"]);
            }
            else
            {
                exp = exp.And(a => a.Receive_date.Value.Year == DateTime.Now.Year);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["emp_id"]))
            {
                string[] emplist = Request.Query["emp_id"].ToString().Split(',');

                if (emplist.Length > 0)
                {
                    var list = new List<string>();

                    foreach (var emp in emplist)
                    {
                        list.Add(emp.ToString());
                    }
                    exp = exp.And(a => list.Contains(a.Payee_id));
                }
            }

            var result = await _ReceiveService.ReportYearSum(exp);

            JArray arr = new JArray();
            for (int i = 1; i <= 12; i++)
            {
                JObject obj = new JObject();
                obj.Add("xmonth", i);

                var sdata = result.Where(a => a.Value<int>("xmonth") == i).FirstOrDefault();

                if (sdata == null)
                {
                    obj.Add("count", 0);
                }
                else
                {
                    obj.Add("count", sdata.Value<decimal>("count"));
                }

                arr.Add(obj);
            }

            return arr.ToString();
        }

        public async Task<string> ReportPayType()
        {
            Expression<Func<Finance_Receive, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.Receive_date.Value.Year == Request.Query["year"]);
            }
            else
            {
                exp = exp.And(a => a.Receive_date.Value.Year == DateTime.Now.Year);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["emp_id"]))
            {
                string[] emplist = Request.Query["emp_id"].ToString().Split(',');

                if (emplist.Length > 0)
                {
                    var list = new List<string>();

                    foreach (var emp in emplist)
                    {
                        list.Add(emp.ToString());
                    }
                    exp = exp.And(a => list.Contains(a.Payee_id));
                }
            }

            var result = await _ReceiveService.ReportPayType(exp);

            JArray arr = new JArray();
            for (int i = 0; i < result.Count; i++)
            {
                JObject obj = new JObject();
                obj.Add("value", result[i].Value<int>("count"));
                obj.Add("name", result[i].Value<string>("xmonth"));

                arr.Add(obj);
            }

            return arr.ToString();
        }

        public async Task<string> ReportInvoice()
        {
            Expression<Func<Finance_Invoice, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.invoice_date.Value.Year == Request.Query["year"]);
            }
            else
            {
                exp = exp.And(a => a.invoice_date.Value.Year == DateTime.Now.Year);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["emp_id"]))
            {
                string[] emplist = Request.Query["emp_id"].ToString().Split(',');

                if (emplist.Length > 0)
                {
                    var list = new List<string>();

                    foreach (var emp in emplist)
                    {
                        list.Add(emp.ToString());
                    }
                    exp = exp.And(a => list.Contains(a.emp_id));
                }
            }

            var result = await _InvoiceService.ReportYear(exp);

            JArray arr = new JArray();
            for (int i = 1; i <= 12; i++)
            {
                JObject obj = new JObject();
                obj.Add("xmonth", i);

                var sdata = result.Where(a => a.Value<int>("xmonth") == i).FirstOrDefault();

                if (sdata == null)
                {
                    obj.Add("count", 0);
                }
                else
                {
                    obj.Add("count", sdata.Value<int>("count"));
                }

                arr.Add(obj);
            }

            return arr.ToString();
        }

        public async Task<string> ReportInvoiceSum()
        {
            Expression<Func<Finance_Invoice, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.invoice_date.Value.Year == Request.Query["year"]);
            }
            else
            {
                exp = exp.And(a => a.invoice_date.Value.Year == DateTime.Now.Year);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["emp_id"]))
            {
                string[] emplist = Request.Query["emp_id"].ToString().Split(',');

                if (emplist.Length > 0)
                {
                    var list = new List<string>();

                    foreach (var emp in emplist)
                    {
                        list.Add(emp.ToString());
                    }
                    exp = exp.And(a => list.Contains(a.emp_id));
                }
            }

            var result = await _InvoiceService.ReportYearSum(exp);

            JArray arr = new JArray();
            for (int i = 1; i <= 12; i++)
            {
                JObject obj = new JObject();
                obj.Add("xmonth", i);

                var sdata = result.Where(a => a.Value<int>("xmonth") == i).FirstOrDefault();

                if (sdata == null)
                {
                    obj.Add("count", 0);
                }
                else
                {
                    obj.Add("count", sdata.Value<decimal>("count"));
                }

                arr.Add(obj);
            }

            return arr.ToString();
        }

        public async Task<string> ReportInvoiceType()
        {
            Expression<Func<Finance_Invoice, bool>> exp = a => 1 == 1;

            if (!string.IsNullOrWhiteSpace(Request.Query["year"]))
            {
                exp = exp.And(a => a.invoice_date.Value.Year == Request.Query["year"]);
            }
            else
            {
                exp = exp.And(a => a.invoice_date.Value.Year == DateTime.Now.Year);
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["emp_id"]))
            {
                string[] emplist = Request.Query["emp_id"].ToString().Split(',');

                if (emplist.Length > 0)
                {
                    var list = new List<string>();

                    foreach (var emp in emplist)
                    {
                        list.Add(emp.ToString());
                    }
                    exp = exp.And(a => list.Contains(a.emp_id));
                }
            }

            var result = await _InvoiceService.ReportInvoiceType(exp);

            JArray arr = new JArray();
            for (int i = 0; i < result.Count; i++)
            {
                JObject obj = new JObject();
                obj.Add("value", result[i].Value<int>("count"));
                obj.Add("name", result[i].Value<string>("xmonth"));

                arr.Add(obj);
            }

            return arr.ToString();
        }
    }
}
