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
    public interface IBaseService<TEntity>
    {
        #region 同步方法
        int Add(TEntity entity);

        int Add(List<TEntity> entity);

        int Delete(string id);

        int Delete(Expression<Func<TEntity, bool>> expression);

        XHDData<TEntity> Grid(Expression<Func<TEntity, bool>> expWhere, int Page, int Limit);

        XHDData<TEntity> Grid(Expression<Func<TEntity, bool>> expWhere, int Page, int Limit, string orderby);

        XHDData<TEntity> Grid(Expression<Func<TEntity, bool>> expWhere);

        XHDData<TEntity> Grid(Expression<Func<TEntity, bool>> expWhere, string OrderBy);

        int Update(Expression<Func<TEntity, TEntity>> ColumnWhere, Expression<Func<TEntity, bool>> expWhere);

        int Update(TEntity model);

        #endregion

        #region 异步方法
        Task<int> AddAsync(TEntity entity);

        Task<int> AddAsync(List<TEntity> entity);


        Task<int> DeleteAsync(string id);

        Task<int> DeleteAsync(Expression<Func<TEntity, bool>> expression);

        Task<XHDData<TEntity>> GridAsync(Expression<Func<TEntity, bool>> expWhere, int Page, int Limit);

        Task<XHDData<TEntity>> GridAsync(Expression<Func<TEntity, bool>> expWhere, int Page, int Limit, string orderby);

        Task<XHDData<TEntity>> GridAsync(Expression<Func<TEntity, bool>> expWhere);

        Task<XHDData<TEntity>> GridAsync(Expression<Func<TEntity, bool>> expWhere, string OrderBy);

        Task<int> UpdateAsync(Expression<Func<TEntity, TEntity>> ColumnWhere, Expression<Func<TEntity, bool>> expWhere);

        Task<int> UpdateAsync(TEntity model);

        #endregion

    }
}
