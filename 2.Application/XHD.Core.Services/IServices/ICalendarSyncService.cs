using System.Threading.Tasks;

namespace XHD.Core.IServices
{
    /// <summary>
    /// 业务日程同步：收款计划、合同到期自动写入我的日程
    /// </summary>
    public interface ICalendarSyncService
    {
        /// <summary>
        /// 同步收款计划到日程（计划修改/删除/收款后调用）
        /// </summary>
        Task SyncReceivePlanAsync(string planId);

        /// <summary>
        /// 同步合同到期到日程（合同修改后调用）
        /// </summary>
        Task SyncContractAsync(string contractId);

        /// <summary>
        /// 删除某业务来源的日程
        /// </summary>
        Task RemoveBySourceAsync(string sourceType, string sourceId);

        /// <summary>
        /// 删除某订单下所有收款计划日程
        /// </summary>
        Task RemoveReceivePlansByOrderAsync(string orderId);

        /// <summary>
        /// 重新同步某订单下所有收款计划日程（成交人变更时调用）
        /// </summary>
        Task ResyncReceivePlansByOrderAsync(string orderId);
    }
}
