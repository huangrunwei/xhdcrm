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
    internal class Product_categoryService : BaseService<Product_category>, IProduct_categoryService
    {
        public Product_categoryService(IProduct_categoryRepository repository)
        {
            _irepository = repository;
        }

        public async Task<JArray> Tree()
        {
            return await Tree(null);
        }

        public async Task<JArray> Tree(string id)
        {
            Expression<Func<Product_category, bool>> expWhere = a => 1==1;

            if (!string.IsNullOrWhiteSpace(id))
            {
                expWhere = expWhere.And(a => a.parentid != id);
            }

            var result = await _irepository.GridAsync(expWhere);

            var json = getMenuJson(result, "root");

            return json;
        }

        private JArray getMenuJson(List<Product_category> list, string id)
        {
            var selectList = list.FindAll(c => c.parentid == id);

            JArray arr = new JArray();

            foreach (var a in selectList)
            {
                JObject obj = new JObject();

                obj.Add("id", a.id);
                obj.Add("title", a.category_name);
                obj.Add("parentid", a.parentid);
                obj.Add("spread", true);

                if (getMenuJson(list, a.id).Count > 0)
                {
                    obj.Add("children", getMenuJson(list, a.id));
                }

                arr.Add(obj);
            }

            return arr;
        }
    }
}
