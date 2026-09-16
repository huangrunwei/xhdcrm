<template>
	<view>
		<!-- 顶部导航栏 -->
		<!-- <view class="navbar">
			<text class="navbar-title">收款详情</text>
		</view> -->
		<view class="container">
			<!-- 跟进详情卡片 -->
			<view class="detail-card">
				<!-- 联系人信息 -->
				<view class="info-item">
					<text class="label">收款单号：</text>
					<text class="value">{{ receiveData.Receive_num }}</text>
				</view>

				<view class="info-item">
					<text class="label">收款金额：</text>
					<text class="value">{{ receiveData.Receive_amount }}</text>
				</view>

				<view class="info-item">
					<text class="label">支付方式：</text>
					<text class="value">{{ receiveData.PayType && receiveData.PayType.params_name  }}</text>
				</view>

				<view class="info-item">
					<text class="label">收款日期：</text>
					<text class="value">{{ receiveData.Receive_date }}</text>
				</view>

				<view class="info-item">
					<text class="label">收款人：</text>
					<text class="value">{{ receiveData.Payee&&receiveData.Payee.name }}</text>
				</view>

				<view class="info-item">
					<text class="label">备注：</text>
					<text class="value">{{ receiveData.Remarks }}</text>
				</view>

				<!-- 编辑按钮 -->
				<button class="edit-btn" @click="onEdit">编辑</button>
			</view>

			<view class="detail-card">
				<view class="info-item">
					<view class="info-label">订单编号:</view>
					<view class="info-value">{{ receiveData.Order.sn }}</view>
				</view>

				<view class="info-item">
					<view class="info-label">客户名称:</view>
					<view class="info-value">{{ receiveData.Order && receiveData.Order.customer.cus_name }}</view>
				</view>

				<view class="info-item">
					<view class="info-label">订单日期:</view>
					<view class="info-value">{{ receiveData.Order.Order_date }}</view>
				</view>

				<view class="info-item total-amount">
					<view class="info-label">订单总金额:</view>
					<view class="info-value">{{ receiveData.Order.total_amount | formatPrice }}</view>
				</view>
			</view>
		</view>
	</view>
</template>

<script>
export default {
    data() {
        return {
            // 收款数据
            receiveData: {},
            
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
        receivedData.Receive_date = this.formatDate(receivedData.Receive_date);

        this.receiveData = receivedData;
        console.log('onLoad 加载数据:', this.receiveData);
    },


    methods: {
		updateData(data) {
		    this.receiveData = data;
			console.log('详情页已更新数据:', data);
		},
		
        // ===== 加载收款详情（从接口获取最新数据） =====
        loadReceiveDetail() {
            if (this.receiveData && this.receiveData.id) {
                uni.showLoading({ title: '刷新中...', mask: true });
                // 如果接口名不同，请替换为实际的接口地址
                this.$request('/api/ReceiveInfo', {
                    id: this.receiveData.id,
                })
                    .then((res) => {
                        uni.hideLoading();
                        const data = res.data;
                        // 格式化日期
                        data.Receive_date = this.formatDate(data.Receive_date);
                        this.receiveData = data;
                        console.log('onShow 刷新数据:', this.receiveData);
                    })
                    .catch(() => {
                        uni.hideLoading();
                        uni.showToast({ title: '刷新失败', icon: 'none' });
                    });
            } else {
                // 如果没有 id，从路由数据恢复
                if (this.routeData) {
                    this.receiveData = this.routeData;
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
            const cusinfo = encodeURIComponent(JSON.stringify(this.receiveData));
            // ★★★ 关键改动：使用 navigateTo 而不是 reLaunch ★★★
            // 这样编辑保存后返回，才会触发 onShow 刷新
            setTimeout(() => {
                uni.navigateTo({
                    url: '/pages/Receive/edit?data=' + cusinfo,
                });
            }, 500);
        },
    },
};
</script>

<style scoped>
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

	.container {
		padding: 16px;
		background-color: #f5f5f5;
		min-height: 100vh;
	}

	/* 详情卡片样式 */
	.detail-card {
		background-color: #ffffff;
		border-radius: 12px;
		padding: 20px;
		box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
		margin-bottom: 10px;
	}

	/* 信息项样式 */
	.info-item {
		display: flex;
		padding: 12px 0;
		border-bottom: 1px solid #f0f0f0;
		align-items: flex-start;
	}

	/* 最后一项去掉下边框 */
	.info-item:last-child:not(.content-item) {
		border-bottom: none;
	}

	/* 标签样式 */
	.label {
		color: #666666;
		width: 100px;
		font-size: 14px;
		line-height: 22px;
	}

	/* 值样式 */
	.value {
		flex: 1;
		color: #333333;
		font-size: 14px;
		line-height: 22px;
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

	/* 跟进内容项特殊样式 */
	.content-item {
		align-items: flex-start;
	}

	.content-item .value {
		white-space: pre-line;
	}

	/* 编辑按钮样式 */
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

	.call-icon {
		/* border:1px solid #ddd;
		border-radius: 5px;
		padding: 5px; */
	}
</style>