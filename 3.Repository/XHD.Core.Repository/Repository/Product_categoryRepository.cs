using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using FreeSql;
using XHD.Core.IRepository;
using XHD.Core.Models;
using XHD.Core.Common;

namespace XHD.Core.Repository
{
    public class Product_categoryRepository : BaseRepository<Product_category>, IProduct_categoryRepository
    {
        public Product_categoryRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }
    }
}
