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
        /// 按钮目录混合表格 layui treeTable 无限层级树
        /// children = 下级菜单；btn = 当前菜单绑定按钮数组（附属字段）
        /// 根菜单 parentid = "root"
        /// 【改用字典映射构建树，不再递归Where查询集合，规避诡异重复问题】
        /// </summary>
        /// <returns></returns>
        public async Task<string> GridTree()
        {
            Expression<Func<Sys_Menu, bool>> expMenu = a => true;
            if (User.FindFirst(ClaimTypes.Sid).Value != "admin")
            {
                expMenu = a => a.id != "Sys_Menu";
            }

            // 保留你原来的查询，全量菜单、全量按钮，不动这里
            var MenuData = await _service.GridAsync(expMenu, "Menu_order");
            var BtnData = await _serviceBtn.GridAsync(a => true, "Btn_order");

            var allMenus = MenuData.data.ToList();
            var allBtns = BtnData.data.ToList();

            // 按钮字典 key:MenuId
            var btnDic = allBtns.GroupBy(b => b.Menu_id)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 1. 第一步：预构建所有菜单JObject，放入字典
            var menuDict = new Dictionary<string, JObject>();
            foreach (var menu in allMenus)
            {
                JObject menuObj = new JObject();
                menuObj.Add("id", menu.id);
                menuObj.Add("title", menu.Menu_name);
                menuObj.Add("icon", string.IsNullOrEmpty(menu.Menu_icon) ? "" : $"fa {menu.Menu_icon}");
                menuObj.Add("href", menu.Menu_url);
                menuObj.Add("isMenu", 0);
                menuObj.Add("parentid", menu.parentid);
                menuObj.Add("order", menu.Menu_order);

                // 组装按钮数组
                JArray btnArr = new JArray();
                if (btnDic.TryGetValue(menu.id, out var btnList))
                {
                    foreach (var btn in btnList)
                    {
                        JObject btnObj = new JObject();
                        btnObj.Add("id", btn.id);
                        btnObj.Add("title", btn.Btn_name);
                        btnObj.Add("parentid", btn.Menu_id);
                        btnObj.Add("order", btn.Btn_order);
                        btnArr.Add(btnObj);
                    }
                }
                menuObj.Add("btn", btnArr);
                // 初始化children为空数组
                menuObj.Add("children", new JArray());

                menuDict.Add(menu.id, menuObj);
            }

            // 2. 第二步：遍历所有菜单，挂到父节点的children
            JArray rootTree = new JArray();
            foreach (var menu in allMenus)
            {
                var currentNode = menuDict[menu.id];
                string pid = menu.parentid;

                if (pid == "root")
                {
                    // 根节点，直接加到外层数组
                    rootTree.Add(currentNode);
                }
                else
                {
                    // 找到父节点，把当前节点添加到父节点children
                    if (menuDict.TryGetValue(pid, out var parentNode))
                    {
                        ((JArray)parentNode["children"]).Add(currentNode);
                    }
                    // 找不到父节点的脏数据直接丢弃
                }
            }

            // 3. 根节点按Menu_order排序
            var sortedRoot = new JArray(rootTree.Children()
                .OrderBy(x => (int?)x["order"])
                .ToList());

            JObject resp = new JObject();
            resp.Add("code", 0);
            resp.Add("msg", "");
            resp.Add("count", 0);
            resp.Add("data", sortedRoot);

            return resp.ToString();
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
