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
using System.Linq.Expressions;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using XHD.Core.IServices;
using XHD.Core.IRepository;
using XHD.Core.Common;
using XHD.Core.View;

using XHD.Core.Models;


namespace XHD.Core.View.Controllers
{
    public class SysMenuController : Controller
    {
        private readonly ILogger<SysMenuController> _logger;
        private readonly ISys_MenuService _service;
        private readonly ISys_ButtonService _serviceBtn;

        public SysMenuController(ILogger<SysMenuController> logger, ISys_MenuService service, ISys_ButtonService serviceBtn)
        {
            _service = service;
            _serviceBtn = serviceBtn;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
               

        public IActionResult Add()
        {
            return View();
        }
               

        /// <summary>
        /// 按钮目录的混合表格
        /// </summary>
        /// <returns></returns>
        public async Task<string> Grid()
        {
            Expression<Func<Sys_Menu, bool>> expMenu = a => true;
            var MenuData = await _service.GridAsync(expMenu, "Menu_order");

            Expression<Func<Sys_Button, bool>> extBtn = a => true;
            var BtnData = await _serviceBtn.GridAsync(extBtn, "Btn_order");


            //构建树

            JArray arrapp = new JArray();

            //目录
            foreach (var menu in MenuData.data)
            {
                JObject objapp = new JObject();

                objapp.Add("id", menu.id);
                objapp.Add("title", menu.Menu_name);
                objapp.Add("icon", $"fa {menu.Menu_icon}");
                objapp.Add("href", menu.Menu_url);
                objapp.Add("isMenu", 0);
                objapp.Add("parentid", menu.parentid);
                objapp.Add("order", menu.Menu_order);

                //按钮
                JArray arrbtn = new JArray();

                var btnlist = BtnData.data.Where(a => a.Menu_id == menu.id).ToList();
                foreach (var btn in btnlist)
                {
                    JObject objbtn = new JObject();

                    objbtn.Add("id", btn.id);
                    objbtn.Add("title", btn.Btn_name);
                    objbtn.Add("parentid", btn.Menu_id);
                    objbtn.Add("order", btn.Btn_order);

                    arrbtn.Add(objbtn);
                }

                objapp.Add("btn", arrbtn);

                arrapp.Add(objapp);
            }



            //构建返回数据
            JObject obj = new JObject();
            obj.Add("code", 0);
            obj.Add("msg", "");
            obj.Add("count", 0);
            obj.Add("data", arrapp);

            return obj.ToString();
        }

        /// <summary>
        /// 目录树
        /// </summary>
        /// <returns></returns>
        public async Task<string> Tree()
        {
            var json = await _service.Tree();

            return json.ToString();
        }

        /// <summary>
        /// 目录的下拉
        /// </summary>
        /// <returns></returns>
        public async Task<string> Combo()
        {
            var json = await _service.Combo();

            return json.ToString();
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public async Task<string> Save(Sys_Menu menu)
        {
            Expression<Func<Sys_Menu, bool>> expression = a => a.id == menu.id;
            var list = await _service.GridAsync(expression);

            var result = 0;
            if (list.count == 0)
            {
                result = await _service.AddAsync(menu);
            }
            else
            {
                result = await _service.UpdateAsync(menu);
            }

            //按钮，先清理
            Expression<Func<Sys_Button, bool>> expbtn = a => a.Menu_id == menu.id;
            await _serviceBtn.DeleteAsync(expbtn);

            //再添加
            Sys_Button model = new Sys_Button();
            model.Menu_id = menu.id;


            if (Request.Form.ContainsKey("btn_add"))
            {
                model.id = $"{model.Menu_id}|add";
                model.Btn_name = "新增";
                model.Btn_order = 10;
                model.Btn_handler = "add";
                model.Btn_icon = "fa-plus-circle";

                await _serviceBtn.AddAsync(model);
            }

            if (Request.Form.ContainsKey("btn_edit"))
            {
                model.id = $"{model.Menu_id}|edit";
                model.Btn_name = "修改";
                model.Btn_order = 20;
                model.Btn_handler = "edit";
                model.Btn_icon = "fa-pencil-square-o";

                await _serviceBtn.AddAsync(model);
            }

            if (Request.Form.ContainsKey("btn_del"))
            {
                model.id = $"{model.Menu_id}|del";
                model.Btn_name = "删除";
                model.Btn_order = 30;
                model.Btn_handler = "del";
                model.Btn_icon = "fa-minus-circle";

                await _serviceBtn.AddAsync(model);
            }

            if (Request.Form.ContainsKey("btn_import"))
            {
                model.id = $"{model.Menu_id}|import";
                model.Btn_name = "导入";
                model.Btn_order = 40;
                model.Btn_handler = "import";
                model.Btn_icon = "fa-sign-in";

                await _serviceBtn.AddAsync(model);
            }

            if (Request.Form.ContainsKey("btn_export"))
            {
                model.id = $"{model.Menu_id}|export";
                model.Btn_name = "导出";
                model.Btn_order = 50;
                model.Btn_handler = "export";
                model.Btn_icon = "fa-sign-out";

                await _serviceBtn.AddAsync(model);
            }


            if (Request.Form.ContainsKey("btn_auth"))
            {
                model.id = $"{model.Menu_id}|auth";
                model.Btn_name = "权限";
                model.Btn_order = 60;
                model.Btn_handler = "auth";
                model.Btn_icon = "fa-lock";

                await _serviceBtn.AddAsync(model);
            }

            if (Request.Form.ContainsKey("btn_pwd"))
            {
                model.id = $"{model.Menu_id}|pwd";
                model.Btn_name = "修改密码";
                model.Btn_order = 70;
                model.Btn_handler = "pwd";
                model.Btn_icon = "fa-lock";

                await _serviceBtn.AddAsync(model);
            }


            return XHDResult.Success().ToString();
        }

        
        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string> Delete(string id)
        {
            //删除按钮
            Expression<Func<Sys_Button, bool>> exp = a => a.Menu_id == id;

            await _serviceBtn.DeleteAsync(exp);

            //删除目录
            await _service.DeleteAsync(id);

            return XHDResult.Success().ToString();
        }        
    }
}
