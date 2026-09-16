using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FreeSql;
using XHD.Core.Common;

namespace XHD.Core.IRepository
{
    public interface IXHDBaseRepository<TEntity>
    {
        #region 异步方法
        Task<int> AddAsync(TEntity entity);

        Task<int> AddAsync(List<TEntity> entity);

        Task<int> DeleteAsync(string id);

        Task<int> DeleteAsync(Expression<Func<TEntity, bool>> expression);

        Task<XHDData<TEntity>> GridAsync(Expression<Func<TEntity, bool>> expWhere, int Page, int Limit);

        Task<XHDData<TEntity>> GridAsync(Expression<Func<TEntity, bool>> expWhere, int Page, int Limit, string orderby);

        Task<List<TEntity>> GridAsync(Expression<Func<TEntity, bool>> expWhere);

        Task<List<TEntity>> GridAsync(Expression<Func<TEntity, bool>> expWhere, string OrderBy);

        Task<int> UpdateAsync(Expression<Func<TEntity, TEntity>> ColumnWhere, Expression<Func<TEntity, bool>> expWhere);

        Task<int> UpdateAsync(TEntity model);

        #endregion


        #region 同步方法
        int Add(TEntity entity);

        int Add(List<TEntity> entity);

        int Delete(string id);

        int Delete(Expression<Func<TEntity, bool>> expression);

        XHDData<TEntity> Grid(Expression<Func<TEntity, bool>> expWhere, int Page, int Limit);

        XHDData<TEntity> Grid(Expression<Func<TEntity, bool>> expWhere, int Page, int Limit, string orderby);

        List<TEntity> Grid(Expression<Func<TEntity, bool>> expWhere);

        List<TEntity> Grid(Expression<Func<TEntity, bool>> expWhere, string OrderBy);

        int Update(Expression<Func<TEntity, TEntity>> ColumnWhere, Expression<Func<TEntity, bool>> expWhere);

        int Update(TEntity model);

        #endregion
    }

    public interface IXHDBaseRepository<TEntity, TKey>
    {
    }
}
