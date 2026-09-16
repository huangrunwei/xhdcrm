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
    public class hr_departmentService : BaseService<hr_department>,Ihr_departmentService
    {
        public hr_departmentService(IXHDBaseRepository<hr_department> repository)
        {
            _irepository = repository;
        }

        public async Task<JObject> Combo()
        {
            Expression<Func<hr_department, bool>> expWhere = a => true;
            var result = await _irepository.GridAsync(expWhere);

            JArray arr = getDepTree(result, "root");

            return XHDResult.Success(arr);
        }
            
        public async Task<JObject> Combo(string id)
        {
            Expression<Func<hr_department, bool>> expWhere = a => a.id != id;
            var result = await _irepository.GridAsync(expWhere);

            JArray arr = getDepTree(result, "root");

            JObject firstObj = new JObject();
            firstObj.Add("id", "root");
            firstObj.Add("title", "无");
            firstObj.Add("parentid", "");
            arr.AddFirst(firstObj);

            return XHDResult.Success(arr);
        }

        private JArray getDepTree(List<hr_department> list, string id)
        {
            var selectList = list.FindAll(c => c.parentid == id);

            JArray arr = new JArray();

            foreach (var a in selectList)
            {
                JObject obj = new JObject();

                obj.Add("id", a.id);
                obj.Add("dep_name", a.dep_name);
                obj.Add("title", a.dep_name);
                obj.Add("parentid", a.parentid);
                obj.Add("dep_chief", a.dep_chief);
                obj.Add("dep_tel", a.dep_tel);
                obj.Add("dep_descript", a.dep_descript);
                obj.Add("dep_order", a.dep_order);

                if (getDepTree(list, a.id).Count > 0)
                {
                    obj.Add("children", getDepTree(list, a.id));
                }

                arr.Add(obj);
            }

            return arr;
        }
    }
}
