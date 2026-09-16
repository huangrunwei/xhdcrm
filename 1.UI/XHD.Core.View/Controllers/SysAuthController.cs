using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System;
using XHD.Core.Common;
using XHD.Core.IServices;
using XHD.Core.Models;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace XHD.Core.View.Controllers
{
    [Authorize]
    public class SysAuthController : Controller
    {
        private readonly ILogger<SysAuthController> _logger;
        private readonly ISys_MenuService _MenuService;
        private readonly ISys_ButtonService _ButtonService;
        private readonly ISys_authorityService _authorityService;

        public SysAuthController(ILogger<SysAuthController> logger,
             ISys_MenuService MenuService,
             ISys_ButtonService ButtonService,
             ISys_authorityService authorityService
            )
        {
            _MenuService = MenuService;
            _authorityService = authorityService;
            _ButtonService = ButtonService;
            _logger = logger;
        }

        public async Task<string> Grid()
        {
            Expression<Func<Sys_Menu, bool>> expMenu = a => true;
            var MenuData = await _MenuService.GridAsync(expMenu, "Menu_order");

            Expression<Func<Sys_Button, bool>> extBtn = a => true;
            var BtnData = await _ButtonService.GridAsync(extBtn, "Btn_order");

            Expression<Func<Sys_authority, bool>> extAuth = a => a.Role_id == Request.Query["role_id"];
            var AuthData = await _authorityService.GridAsync(extAuth);

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

                var auth = 0;

                //if (uid == "admin")
                //{
                //    auth = 1;
                //}

                //权限
                var authdata = AuthData.data.Where(a => a.Auth_id == menu.id).ToList();

                if (authdata.Count > 0)
                {
                    auth = 1;
                }

                objapp.Add("auth_on", auth);

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

                    var authbtn = 0;

                    //if (uid == "admin")
                    //{
                    //    auth = 1;
                    //}

                    //权限
                    var authdatabtn = AuthData.data.Where(a => a.Auth_id == btn.id).ToList();

                    if (authdatabtn.Count > 0)
                    {
                        authbtn = 1;
                    }

                    objbtn.Add("auth_on", authbtn);

                    arrbtn.Add(objbtn);
                }

                objapp.Add("btn", arrbtn);

                arrapp.Add(objapp);
            }

            ////按钮
            //foreach (var btn in BtnData.data)
            //{
            //    JObject objapp = new JObject();

            //    objapp.Add("id", btn.id);
            //    objapp.Add("title", btn.Btn_name);
            //    objapp.Add("icon", $"fa ");
            //    objapp.Add("href", "");
            //    objapp.Add("isMenu", 1);
            //    objapp.Add("parentid", btn.Menu_id);
            //    objapp.Add("order", btn.Btn_order);

            //    var auth = 0;

            //    //if (uid == "admin")
            //    //{
            //    //    auth = 1;
            //    //}

            //    //权限
            //    var authdata = AuthData.data.Where(a => a.Auth_id == btn.id).ToList();

            //    if (authdata.Count > 0)
            //    {
            //        auth = 1;
            //    }

            //    objapp.Add("auth_on", auth);

            //    arrapp.Add(objapp);
            //}

            //构建返回数据
            JObject obj = new JObject();
            obj.Add("code", 0);
            obj.Add("msg", "");
            obj.Add("count", 0);
            obj.Add("data", arrapp);

            return obj.ToString();
        }

        public async Task<string> save()
        {
            Expression<Func<Sys_authority, bool>> exp = a => a.Role_id == Request.Form["role_id"] && a.Auth_id == Request.Form["auth_id"];

            await _authorityService.DeleteAsync(exp);

            var auth_on = Request.Form["auth_on"];

            if (auth_on == "1")
            {
                Sys_authority models = new Sys_authority();
                models.Role_id = Request.Form["role_id"];
                models.Auth_id = Request.Form["auth_id"].ToString().Trim();
                Guid result;
                var isGuid = Guid.TryParse(models.Auth_id.ToString(), out result);

                if (isGuid)
                {
                    models.Auth_type = 1;
                }
                else
                {
                    models.Auth_type = 2;
                }


                await _authorityService.AddAsync(models);
            }

            return XHDResult.Success().ToString();

        }
    }
}
