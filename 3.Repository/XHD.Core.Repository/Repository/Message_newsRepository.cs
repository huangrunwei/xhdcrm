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
    public class Message_newsRepository : BaseRepository<Message_news>, IMessage_newsRepository
    {
        public Message_newsRepository(IFreeSql freesql)
        {
            _fsql = freesql;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async new Task<int> UpdateAsync(Message_news model)
        {
            var result = await _fsql.Update<Message_news>()
                .SetSource(model)
                .IgnoreColumns(a => new { a.create_id,a.create_time })
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
        public async new Task<XHDData<Message_news>> GridAsync(Expression<Func<Message_news, bool>> expWhere, int Page, int Limit)
        {
            var data = await _fsql.Select<Message_news>()
                .LeftJoin(a => a.NewsType.id == a.news_type_id)
                .Where(expWhere)
                //.OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync();

            //构建返回数据
            XHDData<Message_news> result = new XHDData<Message_news>()
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
        public async new Task<XHDData<Message_news>> GridAsync(Expression<Func<Message_news, bool>> expWhere, int Page, int Limit, string orderby)
        {
            if (string.IsNullOrWhiteSpace(orderby))
            {
                return await GridAsync(expWhere, Page, Limit);
            }

            var data = await _fsql.Select<Message_news>()
                .LeftJoin(a => a.NewsType.id == a.news_type_id)
                .Where(expWhere)
                .OrderBy(orderby)
                .Page(Page, Limit)
                .Count(out var total)
                .ToListAsync();

            //构建返回数据
            XHDData<Message_news> result = new XHDData<Message_news>()
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
        public async new Task<List<Message_news>> GridAsync(Expression<Func<Message_news, bool>> expWhere)
        {
            var data = await _fsql.Select<Message_news>()
                .LeftJoin(a => a.NewsType.id == a.news_type_id)
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
        public async new Task<List<Message_news>> GridAsync(Expression<Func<Message_news, bool>> expWhere, string OrderBy)
        {
            if (string.IsNullOrWhiteSpace(OrderBy))
            {
                return await GridAsync(expWhere);
            }

            var data = await _fsql.Select<Message_news>()
                .LeftJoin(a => a.NewsType.id == a.news_type_id)
                .Where(expWhere)
                .OrderBy(OrderBy)
                .ToListAsync();

            return data;
        }
    }
}
