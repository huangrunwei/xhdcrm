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


namespace XHD.Core.Services
{
    internal class Sys_MenuService : BaseService<Sys_Menu>,ISys_MenuService
    {
        ISys_MenuRepository MenuRepository;
        public Sys_MenuService(ISys_MenuRepository repository)
        {
            _irepository = repository;
            MenuRepository= repository;
        }

        public async Task<JArray> Tree()
        {
            Expression<Func<Sys_Menu, bool>> expWhere = a => true;
            return await Tree(expWhere,true);
        }


        public async Task<JArray> Tree(Expression<Func<Sys_Menu, bool>> expWhere,bool isChildren)
        {
            //expWhere = a => true;
            var result = await _irepository.GridAsync(expWhere, "Menu_order");

            var json = getMenuJson(result, "root",isChildren);

            return json;
        }

        public async Task<JArray> Combo()
        {
            Expression<Func<Sys_Menu, bool>> expWhere = a => a.parentid=="root";
            var result = await _irepository.GridAsync(expWhere);

            var json = getMenuJson(result, "root",false);

            JObject firstObj = new JObject();
            firstObj.Add("id", "root");
            firstObj.Add("title", "无");
            firstObj.Add("icon", "fa-ban");
            firstObj.Add("href", "");
            firstObj.Add("isMenu", 1);
            firstObj.Add("parentid", "");
            firstObj.Add("order", 0);
            json.AddFirst(firstObj);

            return json;
        }
        private JArray getMenuJson(List<Sys_Menu> list, string id,bool isChildren)
        {
            var selectList = list.FindAll(c => c.parentid == id);

            JArray arr = new JArray();

            foreach (var a in selectList)
            {
                JObject obj = new JObject();

                obj.Add("id", a.id);
                obj.Add("title", a.Menu_name);
                obj.Add("href", a.Menu_url);
                obj.Add("target", "_self");
                obj.Add("icon", a.Menu_icon);
                obj.Add("isMenu", 1);
                obj.Add("parentid", a.parentid);
                obj.Add("order", a.Menu_order);

                if (getMenuJson(list, a.id, isChildren).Count > 0)
                {
                    //需要区分child和children，是因为Layui的tree需要用children，但是首页目录需要用child
                    if (isChildren)
                    {
                        obj.Add("children", getMenuJson(list, a.id, isChildren));
                    }
                    else
                    {
                        obj.Add("child", getMenuJson(list, a.id, isChildren));
                    }
                    
                    
                }

                arr.Add(obj);
            }

            return arr;
        }

        public async Task<List<string>> GetMenuByEmpID(string emp_id)
        {
            return await MenuRepository.GetMenuByEmpID(emp_id);
        }
    }
}
