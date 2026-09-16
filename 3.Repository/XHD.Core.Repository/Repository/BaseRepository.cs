using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

using FreeSql;

using XHD.Core.IRepository;
using System.Linq.Expressions;
using XHD.Core.Common;
using XHD.Core.Models;

namespace XHD.Core.Repository
{
    public class BaseRepository<TEntity> : IXHDBaseRepository<TEntity> where TEntity : class
    {
        protected IFreeSql _fsql;

        #region 异步方法

        public async Task<int> AddAsync(TEntity entity)
        {
            var result = await _fsql.Insert(entity).ExecuteAffrowsAsync();

            return result;
        }

        public async Task<int> AddAsync(List<TEntity> entity)
        {
            var result = await _fsql.Insert(entity).ExecuteAffrowsAsync();

            return result;
        }


        public async Task<int> DeleteAsync(string id)
        {
            var result = await _fsql.Delete<TEntity>()
                .Where("id = @id", new { id = id })
                .ExecuteAffrowsAsync();

            return result;
        }

        public async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> exp)
        {
            var result = await _fsql.Delete<TEntity>()
                .Where(exp)
                .ExecuteAffrowsAsync();

            return result;
        }

        /// <summary>
        /// 条件更新
        /// </summary>
        /// <param name="ColumnWhere"></param>
        /// <param name="expWhere"></param>
        /// <returns></returns>
        public async Task<int> UpdateAsync(Expression<Func<TEntity, TEntity>> ColumnWhere, Expression<Func<TEntity, bool>> expWhere)
        {
            var result = await _fsql.Update<TEntity>()
                .Set(ColumnWhere)
                .Where(expWhere)
                .ExecuteAffrowsAsync();

            return result;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<int> UpdateAsync(TEntity model)
        {
            var result = await _fsql.Update<TEntity>()
                .SetSource(model)
                //.IgnoreColumns(a => new { a.id })
                .ExecuteAffrowsAsync();

            return result;
        }


        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="expWhere"></param>
        /// <param name="Page"></param>
        /// <param name="Limit"></param>
        /// <param name="expOrder"></param>
        /// <returns></returns>
        public async Task<XHDData<TEntity>> GridAsync(Expression<Func<TEntity, bool>> expWhere, int Page, int Limit)
        {
            var data = await _fsql.Select<TEntity>()
                //.WhereIf()
                .Where(expWhere)
                //.OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync();

            //构建返回数据
            XHDData<TEntity> result = new XHDData<TEntity>()
            {
                data = data,
                count = total
            };

            return result;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="expWhere"></param>
        /// <param name="Page"></param>
        /// <param name="Limit"></param>
        /// <param name="expOrder"></param>
        /// <returns></returns>
        public async Task<XHDData<TEntity>> GridAsync(Expression<Func<TEntity, bool>> expWhere, int Page, int Limit, string orderby)
        {
            if (string.IsNullOrWhiteSpace(orderby))
            {
                return await GridAsync(expWhere, Page, Limit);
            }

            var data = await _fsql.Select<TEntity>()
                //.WhereIf()
                .Where(expWhere)
                .OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync();

            //构建返回数据
            XHDData<TEntity> result = new XHDData<TEntity>()
            {
                data = data,
                count = total
            };

            return result;
        }

        /// <summary>
        /// 普通条件查询
        /// </summary>
        /// <param name="expWhere"></param>
        /// <returns></returns>
        public async Task<List<TEntity>> GridAsync(Expression<Func<TEntity, bool>> expWhere)
        {
            var data = await _fsql.Select<TEntity>()
                    .Where(expWhere)
                    .ToListAsync();

            return data;
        }

        /// <summary>
        /// 普通条件查询带排序
        /// </summary>
        /// <param name="expWhere"></param>
        /// <param name="OrderBy"></param>
        /// <returns></returns>
        public async Task<List<TEntity>> GridAsync(Expression<Func<TEntity, bool>> expWhere, string OrderBy)
        {
            if (string.IsNullOrWhiteSpace(OrderBy))
            {
                return await GridAsync(expWhere);
            }

            var data = await _fsql.Select<TEntity>()
                    .Where(expWhere)
                    .OrderBy(OrderBy)
                    .ToListAsync();

            return data;
        }

        #endregion

        #region 同步方法
        public int Add(TEntity entity)
        {
            var result = _fsql.Insert(entity).ExecuteAffrows();

            return result;
        }

        public int Add(List<TEntity> entity)
        {
            var result = _fsql.Insert(entity).ExecuteAffrows();

            return result;
        }

        public int Delete(string id)
        {
            var result = _fsql.Delete<TEntity>()
                .Where("id = @id", new { id = id })
                .ExecuteAffrows();

            return result;
        }

        public int Delete(Expression<Func<TEntity, bool>> exp)
        {
            var result = _fsql.Delete<TEntity>()
                .Where(exp)
                .ExecuteAffrows();

            return result;
        }

        /// <summary>
        /// 条件更新
        /// </summary>
        /// <param name="ColumnWhere"></param>
        /// <param name="expWhere"></param>
        /// <returns></returns>
        public int Update(Expression<Func<TEntity, TEntity>> ColumnWhere, Expression<Func<TEntity, bool>> expWhere)
        {
            var result = _fsql.Update<TEntity>()
                .Set(ColumnWhere)
                .Where(expWhere)
                .ExecuteAffrows();

            return result;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(TEntity model)
        {
            var result = _fsql.Update<TEntity>()
                .SetSource(model)
                //.IgnoreColumns(a => new { a.id })
                .ExecuteAffrows();

            return result;
        }


        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="expWhere"></param>
        /// <param name="Page"></param>
        /// <param name="Limit"></param>
        /// <param name="expOrder"></param>
        /// <returns></returns>
        public XHDData<TEntity> Grid(Expression<Func<TEntity, bool>> expWhere, int Page, int Limit)
        {
            var data = _fsql.Select<TEntity>()
                //.WhereIf()
                .Where(expWhere)
                //.OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToList();

            //构建返回数据
            XHDData<TEntity> result = new XHDData<TEntity>()
            {
                data = data,
                count = total
            };

            return result;
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="expWhere"></param>
        /// <param name="Page"></param>
        /// <param name="Limit"></param>
        /// <param name="expOrder"></param>
        /// <returns></returns>
        public XHDData<TEntity> Grid(Expression<Func<TEntity, bool>> expWhere, int Page, int Limit, string orderby)
        {
            if (string.IsNullOrWhiteSpace(orderby))
            {
                return Grid(expWhere, Page, Limit);
            }

            var data = _fsql.Select<TEntity>()
                //.WhereIf()
                .Where(expWhere)
                .OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToList();

            //构建返回数据
            XHDData<TEntity> result = new XHDData<TEntity>()
            {
                data = data,
                count = total
            };

            return result;
        }

        /// <summary>
        /// 普通条件查询
        /// </summary>
        /// <param name="expWhere"></param>
        /// <returns></returns>
        public List<TEntity> Grid(Expression<Func<TEntity, bool>> expWhere)
        {
            var data = _fsql.Select<TEntity>()
                    .Where(expWhere)
                    .ToList();

            return data;
        }

        /// <summary>
        /// 普通条件查询带排序
        /// </summary>
        /// <param name="expWhere"></param>
        /// <param name="OrderBy"></param>
        /// <returns></returns>
        public List<TEntity> Grid(Expression<Func<TEntity, bool>> expWhere, string OrderBy)
        {
            if (string.IsNullOrWhiteSpace(OrderBy))
            {
                return Grid(expWhere);
            }

            var data = _fsql.Select<TEntity>()
                    .Where(expWhere)
                    .OrderBy(OrderBy)
                    .ToList();

            return data;
        }
        #endregion

    }
}
