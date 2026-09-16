<template>
	<view class="order-detail-page">
		<!-- 顶部导航栏 -->
		<!-- <view class="navbar">
			<text class="navbar-title">订单详情</text>
		</view> -->

		<!-- 订单信息卡片 -->
		<view class="order-info-card">
			<view class="card-header">
				<view class="card-title">订单信息</view>
				<view class="toggle-btn" @click="toggleOrderDetails">
					<uni-icons :type="detailsExpanded ? 'arrowup' : 'arrowdown'" size="16" color="#999"></uni-icons>
					<text class="toggle-text">{{ detailsExpanded ? '收起' : '更多' }}</text>
				</view>
			</view>

			<!-- 固定显示的常见项 -->
			<view class="info-common">
				<view class="info-item">
					<view class="info-label">订单编号</view>
					<view class="info-value">{{ orderInfo.Serialnumber }}</view>
				</view>

				<view class="info-item">
					<view class="info-label">客户名称</view>
					<view class="info-value">{{ orderInfo.customer && orderInfo.customer.cus_name }}</view>
				</view>

				<view class="info-item">
					<view class="info-label">订单日期</view>
					<view class="info-value">{{ orderInfo.Order_date }}</view>
				</view>

				<view class="info-item total-amount">
					<view class="info-label">订单总金额</view>
					<view class="info-value">{{ orderInfo.total_amount | formatPrice }}</view>
				</view>
			</view>

			<!-- 可折叠的详细项 -->
			<view class="info-details" v-if="detailsExpanded">
				<view class="info-item">
					<view class="info-label">原始金额</view>
					<view class="info-value">{{ orderInfo.Order_amount }}</view>
				</view>

				<view class="info-item">
					<view class="info-label">优惠金额</view>
					<view class="info-value">{{ orderInfo.discount_amount }}</view>
				</view>

				<view class="info-item">
					<view class="info-label">订单状态</view>
					<view class="info-value">{{ orderInfo.OrderStatus && orderInfo.OrderStatus .params_name }}</view>
				</view>


				<view class="info-item">
					<view class="info-label">支付方式</view>
					<view class="info-value">{{ orderInfo.PayType && orderInfo.PayType.params_name }}</view>
				</view>

				<view class="info-item">
					<view class="info-label">成交人员</view>
					<view class="info-value">{{ orderInfo.employee.name }}</view>
				</view>

				<view class="info-item">
					<view class="info-label">备注</view>
					<view class="info-value">{{ orderInfo.Remarks || '无' }}</view>
				</view>
			</view>
		</view>

		<!-- 产品列表卡片 -->
		<view class="product-list-card">
			<view class="card-title">产品信息</view>

			<view class="product-item" v-for="(product, index) in productList" :key="index">
				<view class="product-image">
					<image :src="product.imageUrl" mode="aspectFill"></image>
				</view>

				<view class="product-info">
					<view class="product-name">{{ product.Product && product.Product.product_name }}</view>
					<view class="product-price">{{ product.price | formatPrice }}</view>
				</view>

				<view class="product-quantity">x{{ product.quantity }}</view>

				<view class="product-amount">
					{{ (product.price * product.quantity) | formatPrice }}
				</view>
			</view>
		</view>
<!-- 编辑按钮 -->
<view class="btn-wrap">
	<button class="edit-btn" @click="onEdit">编辑订单</button>
</view>
				
	</view>
</template>

<script>
export default {
    data() {
        return {
            // 详情部分是否展开
            detailsExpanded: false,

            // 订单信息
            orderInfo: {},

            // 产品列表
            productList: [],

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
        receivedData.Order_date = this.formatDate(receivedData.Order_date);

        this.orderInfo = receivedData;
        console.log('onLoad 加载数据:', this.orderInfo);

        // 首次加载产品列表
        this.loadProduct();
    },

    methods: {
		updateData(data) {
		    this.orderInfo = data;
			console.log('详情页已更新数据:', data);
		},
		updateProduct(data) {
		    this.productList = data;
			console.log('详情页已更新数据:', data);
		},
        // ===== 加载订单详情（从接口获取最新数据） =====
        loadOrderDetail() {
            if (this.orderInfo && this.orderInfo.id) {
                uni.showLoading({ title: '刷新中...', mask: true });
                // 如果接口名不同，请替换为实际的接口地址
                this.$request('/api/OrderInfo', {
                    id: this.orderInfo.id,
                })
                    .then((res) => {
                        uni.hideLoading();
                        const data = res.data;
                        // 格式化日期
                        data.Order_date = this.formatDate(data.Order_date);
                        this.orderInfo = data;
                        // 同时刷新产品列表
                        this.loadProduct();
                        console.log('onShow 刷新数据:', this.orderInfo);
                    })
                    .catch(() => {
                        uni.hideLoading();
                        uni.showToast({ title: '刷新失败', icon: 'none' });
                    });
            } else {
                // 如果没有 id，从路由数据恢复
                if (this.routeData) {
                    this.orderInfo = this.routeData;
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
            const cusinfo = encodeURIComponent(JSON.stringify(this.orderInfo));
            // ★★★ 关键改动：使用 navigateTo 而不是 reLaunch ★★★
            // 这样编辑保存后返回，才会触发 onShow 刷新
            setTimeout(() => {
                uni.navigateTo({
                    url: '/pages/Order/edit?data=' + cusinfo,
                });
            }, 500);
        },

        // ===== 切换详情展开/折叠 =====
        toggleOrderDetails() {
            this.detailsExpanded = !this.detailsExpanded;
        },

        // ===== 加载产品列表 =====
        loadProduct() {
            if (!this.orderInfo.id) return;
            this.$request('/api/OrderDetails', {
                order_id: this.orderInfo.id,
            }).then((res) => {
                console.log('产品列表:', res);
                this.productList = res.data || [];
            });
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

	/* 产品列表 */
	.product-item {
		display: flex;
		align-items: center;
		padding: 12px 0;
		border-bottom: 1px solid #f5f5f5;
	}

	.product-item:last-child {
		border-bottom: none;
	}

	.product-image {
		width: 80px;
		height: 80px;
		border-radius: 8px;
		overflow: hidden;
		flex-shrink: 0;
		background-color: #f5f5f5;
	}

	.product-image image {
		width: 100%;
		height: 100%;
	}

	.product-info {
		flex: 1;
		margin: 0 16px;
		min-width: 0;
	}

	.product-name {
		font-size: 14px;
		color: #333;
		margin-bottom: 8px;
		white-space: nowrap;
		overflow: hidden;
		text-overflow: ellipsis;
	}

	.product-price {
		font-size: 14px;
		color: #f44336;
	}

	.product-quantity {
		font-size: 14px;
		color: #666;
		margin-right: 16px;
	}

	.product-amount {
		font-size: 14px;
		color: #333;
		font-weight: 500;
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
	.btn-wrap
	{padding:10px;}
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