using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using XHD.Core.Models;
using XHD.Core.Common;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;

namespace XHD.Core.IServices
{
    public interface IProduct_categoryService:IBaseService<Product_category>
    {
        Task<JArray> Tree();

        Task<JArray> Tree(string id);
    }
}
