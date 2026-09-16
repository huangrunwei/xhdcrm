<template>
	<view class="order-detail-page">
		<!-- 顶部导航栏 -->
		<!-- <view class="navbar">
			<text class="navbar-title">合同详情</text>
		</view> -->

		<!-- 订单信息卡片 -->
		<view class="order-info-card">
			<view class="card-header">
				<view class="card-title">订单信息</view>
				<view class="toggle-btn" @click="toggleDetails">
					<uni-icons :type="detailsExpanded ? 'arrowup' : 'arrowdown'" size="16" color="#999"></uni-icons>
					<text class="toggle-text">{{ detailsExpanded ? '收起' : '更多' }}</text>
				</view>
			</view>

			<!-- 固定显示的常见项 -->
			<view class="info-common">
				<view class="info-item">
					<view class="info-label">合同编号</view>
					<view class="info-value">{{ contractInfo.Serialnumber }}</view>
				</view>

				<view class="info-item">
					<view class="info-label">客户名称</view>
					<view class="info-value">{{ contractInfo.customer && contractInfo.customer.cus_name }}</view>
				</view>

				<view class="info-item">
					<view class="info-label">签订日期</view>
					<view class="info-value">{{ contractInfo.Sign_date }}</view>
				</view>

				<view class="info-item">
					<view class="info-label">开始日期</view>
					<view class="info-value">{{ contractInfo.Start_date }}</view>
				</view>

				<view class="info-item">
					<view class="info-label">到期日期</view>
					<view class="info-value">{{ contractInfo.End_date }}</view>
				</view>

				<view class="info-item total-amount">
					<view class="info-label">合同金额</view>
					<view class="info-value">{{ contractInfo.Contract_amount | formatPrice }}</view>
				</view>
			</view>

			<!-- 可折叠的详细项 -->
			<view class="info-details" v-if="detailsExpanded">
				<view class="info-item">
					<view class="info-label">我方签约</view>
					<view class="info-value">{{ contractInfo.employee.name }}</view>
				</view>
				<view class="info-item">
					<view class="info-label">对方签约</view>
					<view class="info-value">{{ contractInfo.Customer_Contractor }}</view>
				</view>


				<view class="info-item">
					<view class="info-label">主要条款</view>
					<view class="info-value">{{ contractInfo.Main_Content || '无' }}</view>
				</view>
				<view class="info-item">
					<view class="info-label">备注</view>
					<view class="info-value">{{ contractInfo.Remarks || '无' }}</view>
				</view>
			</view>
		</view>

		<!-- 产品列表卡片 -->
		<view class="product-list-card">
			<view class="card-title">附件信息</view>

			<!-- 单个附件项 -->
			<view class="attachment-item" v-for="(item, index) in AttaList" :key="index">
				<!-- 左侧图标 -->
				<view class="attachment-icon">
					<uni-icons type="paperclip" size="24" color="#606266"></uni-icons>
				</view>

				<!-- 附件名称 -->
				<view class="attachment-name">
					<text>{{ item.file_name }}</text>
				</view>

				<!-- 下载按钮 -->
				<view class="download-btn" @click="downloadFile(item)">
					<uni-icons type="download" size="20" color="#409EFF"></uni-icons>
					<text class="btn-text">下载</text>
				</view>
			</view>
		</view>
		<!-- 编辑按钮 -->
		<view class="btn-wrap">
			<button class="edit-btn" @click="onEdit">编辑合同</button>
		</view>

	</view>
</template>

<script>
// 引入外部JS文件中的变量
const envConfig = require('../../config/env.js');

// 配置基础URL
const baseUrl = envConfig.baseApi;

import DownloadUtil from '@/utils/downloadUtil.js';

export default {
    data() {
        return {
            // 详情部分是否展开
            detailsExpanded: false,

            // 合同信息
            contractInfo: {},

            // 附件列表
            AttaList: [],
        };
    },

    filters: {
        formatPrice(price) {
            if (price) {
                return '¥' + (price).toFixed(2);
            }
        },
    },

    onLoad(options) {
        // 将接收的字符串解析为JSON对象
        const receivedData = JSON.parse(decodeURIComponent(options.data));
        
        // 格式化日期
        receivedData.Sign_date = this.formatDate(receivedData.Sign_date);
        receivedData.Start_date = this.formatDate(receivedData.Start_date);
        receivedData.End_date = this.formatDate(receivedData.End_date);

        this.contractInfo = receivedData;
        console.log('onLoad 加载数据:', this.contractInfo);

        // 首次加载附件
        this.loadAtta();
    },

    methods: {
		updateData(data) {
		    this.contractInfo = data;
			console.log('详情页已更新数据:', data);
		},
		
        // ===== 加载合同详情（从接口获取最新数据） =====
        loadContractDetail() {
            if (this.contractInfo && this.contractInfo.id) {
                uni.showLoading({ title: '刷新中...', mask: true });
                // 如果接口名不同，请替换为实际的接口地址
                this.$request('/api/ContractInfo', {
                    id: this.contractInfo.id,
                })
                    .then((res) => {
                        uni.hideLoading();
                        const data = res.data;
                        // 格式化日期
                        data.Sign_date = this.formatDate(data.Sign_date);
                        data.Start_date = this.formatDate(data.Start_date);
                        data.End_date = this.formatDate(data.End_date);
                        this.contractInfo = data;
                        // 同时刷新附件列表
                        this.loadAtta();
                        console.log('onShow 刷新数据:', this.contractInfo);
                    })
                    .catch(() => {
                        uni.hideLoading();
                        uni.showToast({ title: '刷新失败', icon: 'none' });
                    });
            } else {
                // 如果没有 id，从路由数据恢复
                if (this.routeData) {
                    this.contractInfo = this.routeData;
                }
            }
        },

        // ===== 格式化日期为YYYY-MM-DD =====
        formatDate(value) {
            if (!value) return '';
            const date = new Date(value);
            const year = date.getFullYear();
            const month = (date.getMonth() + 1).toString().padStart(2, '0');
            const day = date.getDate().toString().padStart(2, '0');
            return `${year}-${month}-${day}`;
        },

        // ===== 编辑按钮点击事件 =====
        onEdit() {
            console.log('进入编辑模式');
            const cusinfo = encodeURIComponent(JSON.stringify(this.contractInfo));
            // ★★★ 关键改动：使用 navigateTo 而不是 reLaunch ★★★
            // 这样编辑保存后返回，才会触发 onShow 刷新
            setTimeout(() => {
                uni.navigateTo({
                    url: '/pages/Contract/edit?data=' + cusinfo,
                });
            }, 500);
        },

        // ===== 切换详情展开/折叠 =====
        toggleDetails() {
            this.detailsExpanded = !this.detailsExpanded;
        },

        // ===== 加载附件列表 =====
        loadAtta() {
            if (!this.contractInfo.id) return;
            this.$request('/api/ContractAtta', {
                contract_id: this.contractInfo.id,
            }).then((res) => {
                console.log('附件列表:', res);
                this.AttaList = res.data || [];
            });
        },

        // ===== 下载文件 =====
        downloadFile(item) {
            uni.showToast({
                title: '开始下载文件',
                icon: 'none',
            });
            console.log(item);
            const fileUrl = baseUrl + '/upload/contract/' + item.contract_id + '/' + item.real_name;

            // #ifdef H5
            window.location.href = fileUrl;
            // #endif

            // #ifdef MP-WEIXIN
            uni.navigateTo({
                url: `/pages/webview/webview?url=${encodeURIComponent(fileUrl)}`,
            });
            // #endif

            // #ifdef APP-PLUS
            uni.showActionSheet({
                itemList: ['在应用内打开', '在浏览器中打开'],
                success: (res) => {
                    if (res.tapIndex === 0) {
                        uni.navigateTo({
                            url: `/pages/webview/webview?url=${encodeURIComponent(fileUrl)}`,
                        });
                    } else {
                        plus.runtime.openURL(fileUrl, (res) => {
                            console.log('打开结果：', res);
                        });
                    }
                },
            });
            // #endif
        },
    },
};
</script>

<style scoped>
	.order-detail-page {
		background-color: #f5f5f5;
		min-height: 100vh;
	}


	/* 导航栏样式 */
	.navbar {
		height: 44px;
		background-color: #1677ff;
		display: flex;
		align-items: center;
		justify-content: center;
		position: relative;
	}

	.navbar-title {
		color: #fff;
		font-size: 18px;
		font-weight: 500;
	}

	/* 订单状态 */
	.order-status {
		height: 40px;
		line-height: 40px;
		text-align: center;
		font-size: 16px;
		font-weight: 500;
		color: #fff;
	}

	.status-pending {
		background-color: #ff9800;
	}

	.status-paid {
		background-color: #4caf50;
	}

	.status-shipped {
		background-color: #2196f3;
	}

	.status-completed {
		background-color: #607d8b;
	}

	.status-canceled {
		background-color: #f44336;
	}

	/* 卡片通用样式 */
	.order-info-card,
	.product-list-card {
		background-color: #fff;
		border-radius: 12px;
		margin: 16px;
		padding: 16px;
		box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
	}

	.card-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 16px;
	}

	.card-title {
		font-size: 16px;
		font-weight: 500;
		color: #333;
	}

	.toggle-btn {
		display: flex;
		align-items: center;
		color: #666;
		font-size: 14px;
		padding: 4px 8px;
		border-radius: 4px;
		background-color: #f5f5f5;
	}

	.toggle-text {
		margin-left: 4px;
	}

	/* 订单信息项 */
	.info-item {
		display: flex;
		justify-content: space-between;
		padding: 10px 0;
		border-bottom: 1px solid #f5f5f5;
	}

	.info-common .info-item:last-child {
		border-bottom: 1px dashed #e0e0e0;
		padding-bottom: 12px;
		margin-bottom: 8px;
	}

	.info-details .info-item:last-child {
		border-bottom: none;
	}

	.info-label {
		font-size: 14px;
		color: #666;
		width: 80px;
		flex-shrink: 0;
	}

	.info-value {
		font-size: 14px;
		color: #333;
		text-align: right;
		flex: 1;
		margin-left: 20px;
		word-break: break-all;
	}

	.total-amount .info-label {
		color: #333;
		font-weight: 500;
	}

	.total-amount .info-value {
		color: #f44336;
		font-weight: 500;
		font-size: 16px;
	}

	.attachment-page {
		padding: 16px;
		background-color: #f5f7fa;
		min-height: 100vh;
	}

	.page-title {
		font-size: 18px;
		font-weight: 500;
		color: #303133;
		margin-bottom: 20px;
		padding-bottom: 10px;
		border-bottom: 1px solid #eee;
	}

	/* 附件 */
	.attachment-list {
		background-color: #fff;
		border-radius: 8px;
		overflow: hidden;
	}

	.attachment-item {
		display: flex;
		align-items: center;
		padding: 14px 5px;
		border-bottom: 1px solid #f5f5f5;
	}

	.attachment-item:last-child {
		border-bottom: none;
	}

	.attachment-icon {
		width: 40px;
		text-align: center;
	}

	.attachment-name {
		flex: 1;
		margin: 0 16px;
		overflow: hidden;
	}

	.attachment-name text {
		display: block;
		white-space: nowrap;
		overflow: hidden;
		text-overflow: ellipsis;
		font-size: 14px;
		color: #606266;
	}

	.download-btn {
		display: flex;
		align-items: center;
		color: #409EFF;
		font-size: 14px;
	}


	/* 底部操作按钮 */
	.bottom-actions {
		display: flex;
		padding: 16px;
		background-color: #fff;
		border-top: 1px solid #f5f5f5;
		position: fixed;
		bottom: 0;
		left: 0;
		right: 0;
	}

	.action-btn {
		flex: 1;
		height: 44px;
		line-height: 44px;
		border-radius: 22px;
		font-size: 16px;
		margin: 0 8px;
	}

	.cancel-btn {
		background-color: #f5f5f5;
		color: #666;
		border: none;
	}

	.pay-btn {
		background-color: #f44336;
		color: #fff;
		border: none;
	}

	.confirm-btn {
		background-color: #4caf50;
		color: #fff;
		border: none;
	}

	/* 编辑按钮样式 */
	.btn-wrap {
		padding: 10px;
	}

	.edit-btn {
		width: 100%;
		height: 44px;
		line-height: 44px;
		background-color: #1677ff;
		color: #ffffff;
		border-radius: 8px;
		font-size: 16px;
		margin-top: 20px;
		border: none;
	}

	/* 按钮激活状态样式 */
	.edit-btn::after {
		border: none;
	}

	.edit-btn:active {
		background-color: #0e65d9;
	}
</style>