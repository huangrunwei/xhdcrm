using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Linq;

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
    public class BaseService<TEntity> : IBaseService<TEntity> where TEntity : class
    {
        protected IRepository.IXHDBaseRepository<TEntity> _irepository;

        #region 异步方法
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<int> AddAsync(TEntity model)
        {
            return await _irepository.AddAsync(model);
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<int> AddAsync(List<TEntity> model)
        {
            return await _irepository.AddAsync(model);
        }


        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="ColumnWhere"></param>
        /// <param name="expWhere"></param>
        /// <returns></returns>
        public async Task<int> UpdateAsync(Expression<Func<TEntity, TEntity>> ColumnWhere, Expression<Func<TEntity, bool>> expWhere)
        {
            return await _irepository.UpdateAsync(ColumnWhere, expWhere);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<int> UpdateAsync(TEntity model)
        {
            return await _irepository.UpdateAsync(model);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<int> DeleteAsync(string id)
        {
            var result = await _irepository.DeleteAsync(id);

            return result;
        }

        /// <summary>
        /// 条件删除
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> expression)
        {
            var result = await _irepository.DeleteAsync(expression);

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
        public async Task<XHDData<TEntity>> GridAsync(Expression<Func<TEntity, bool>> expWhere, int Page, int Limit, string OrderBy)
        {
            if (string.IsNullOrWhiteSpace(OrderBy))
            {
                return await GridAsync(expWhere, Page, Limit);
            }

            var result = await _irepository.GridAsync(expWhere, Page, Limit, OrderBy);

            var data = new XHDData<TEntity>()
            {
                data = result.data,
                count = result.count
            };

            return data;
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
            var result = await _irepository.GridAsync(expWhere, Page, Limit);

            var data = new XHDData<TEntity>()
            {
                data = result.data,
                count = result.count
            };

            return data;
        }

        /// <summary>
        /// 普通条件查询
        /// </summary>
        /// <param name="expWhere"></param>
        /// <returns></returns>
        public async Task<XHDData<TEntity>> GridAsync(Expression<Func<TEntity, bool>> expWhere)
        {
            var result = await _irepository.GridAsync(expWhere);

            var data = new XHDData<TEntity>()
            {
                data = result,
                count = result.Count
            };

            return data;
        }

        /// <summary>
        /// 普通条件查询带排序
        /// </summary>
        /// <param name="expWhere"></param>
        /// <returns></returns>
        public async Task<XHDData<TEntity>> GridAsync(Expression<Func<TEntity, bool>> expWhere, string OrderBy)
        {
            var result = await _irepository.GridAsync(expWhere, OrderBy);

            var data = new XHDData<TEntity>()
            {
                data = result,
                count = result.Count
            };

            return data;
        }
        #endregion

        #region 同步方法
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Add(TEntity model)
        {
            return _irepository.Add(model);
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Add(List<TEntity> model)
        {
            return _irepository.Add(model);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="ColumnWhere"></param>
        /// <param name="expWhere"></param>
        /// <returns></returns>
        public int Update(Expression<Func<TEntity, TEntity>> ColumnWhere, Expression<Func<TEntity, bool>> expWhere)
        {
            return _irepository.Update(ColumnWhere, expWhere);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Update(TEntity model)
        {
            return _irepository.Update(model);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int Delete(string id)
        {
            var result = _irepository.Delete(id);

            return result;
        }

        /// <summary>
        /// 条件删除
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public int Delete(Expression<Func<TEntity, bool>> expression)
        {
            var result = _irepository.Delete(expression);

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
        public XHDData<TEntity> Grid(Expression<Func<TEntity, bool>> expWhere, int Page, int Limit, string OrderBy)
        {
            if (string.IsNullOrWhiteSpace(OrderBy))
            {
                return Grid(expWhere, Page, Limit);
            }

            var result = _irepository.Grid(expWhere, Page, Limit, OrderBy);

            var data = new XHDData<TEntity>()
            {
                data = result.data,
                count = result.count
            };

            return data;
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
            var result = _irepository.Grid(expWhere, Page, Limit);

            var data = new XHDData<TEntity>()
            {
                data = result.data,
                count = result.count
            };

            return data;
        }

        /// <summary>
        /// 普通条件查询
        /// </summary>
        /// <param name="expWhere"></param>
        /// <returns></returns>
        public XHDData<TEntity> Grid(Expression<Func<TEntity, bool>> expWhere)
        {
            var result = _irepository.Grid(expWhere);

            var data = new XHDData<TEntity>()
            {
                data = result,
                count = result.Count
            };

            return data;
        }

        /// <summary>
        /// 普通条件查询带排序
        /// </summary>
        /// <param name="expWhere"></param>
        /// <returns></returns>
        public XHDData<TEntity> Grid(Expression<Func<TEntity, bool>> expWhere, string OrderBy)
        {
            var result = _irepository.Grid(expWhere, OrderBy);

            var data = new XHDData<TEntity>()
            {
                data = result,
                count = result.Count
            };

            return data;
        }
        #endregion
    }
}
