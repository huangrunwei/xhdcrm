<template>
	<view class="customer-detail-page">
		<!-- 顶部导航栏 -->
		<!-- <view class="navbar" :class="{ 'navbar-scrolled': isScrolled }">
			<view class="navbar-content">
				<view class="back-btn" @click="navigateBack">
					<i class="iconfont icon-arrow-left"></i>
				</view>
				<view class="navbar-title">客户详情</view>
				<view class="action-buttons">
					<button class="more-btn" @click="showActionSheet">
						<i class="iconfont icon-more"></i>
					</button>
				</view>
			</view>
		</view> -->

		<!-- 客户基本信息头部 -->
		<view class="customer-header">
			<view class="customer-main-info">
				<view class="customer-name">
					{{ cusInfo.cus_name || '未知客户' }}
					<button class="name-edit-btn" @click="handleEdit">
						<i class="iconfont icon-edit"></i>
						<text>编辑</text>
					</button>
				</view>
				<view class="customer-category">{{ cusInfo.Serialnumber  }} </view>
			</view>
		</view>



		<!-- 客户详细信息（折叠面板） -->
		<view class="customer-info-section">
			<view class="section-title" @click="toggleInfoExpand">
				<text>基本信息</text>
				<view class="expand-toggle">
					<i class="iconfont" :class="infoExpanded ? 'icon-arrow-up' : 'icon-arrow-down'"></i>
					<text>{{ infoExpanded ? '收起' : '展开' }}</text>
				</view>
			</view>

			<!-- 默认显示的基本信息 -->
			<view class="info-list default-info">
				<view class="info-item">
					<view class="info-label">电话:</view>
					<view class="info-value">{{ cusInfo.cus_tel || '' }}</view>
					<!-- 电话图标（点击触发拨号） -->
					<view class="call-icon" @click="makePhoneCall(cusInfo.cus_tel)">
						<!-- 使用 UniApp 内置图标（需确保项目已引入图标库） -->
						<uni-icons type="phone" size="20" color="#007AFF"></uni-icons>
					</view>
				</view>
				<view class="info-item">
					<view class="info-label">地址:</view>
					<view class="info-value">
						{{ cusInfo.Provinces && cusInfo.Provinces.Provinces || '' }}{{ cusInfo.City && cusInfo.City.City || '' }}{{ cusInfo.cus_add || '' }}
					</view>
				</view>
				<view class="info-item">
					<view class="info-label">归属:</view>
					<view class="info-value">{{ cusInfo.Employee && cusInfo.Employee.name || '' }}</view>
				</view>
				<view class="info-item">
					<view class="info-label">最后跟进:</view>
					<view class="info-value">{{ cusInfo.lastfollow || '' }}</view>
				</view>

			</view>

			<!-- 展开后显示的更多信息 -->
			<view class="info-list expandable-info" :class="{ 'expanded': infoExpanded }">
				<view class="info-item">
					<view class="info-label">客户行业:</view>
					<view class="info-value">{{ cusInfo.cus_industry && cusInfo.cus_industry.params_name || '' }}</view>
				</view>
				<view class="info-item">
					<view class="info-label">客户级别:</view>
					<view class="info-value">{{ cusInfo.cus_level && cusInfo.cus_level.params_name || '' }}</view>
				</view>
				<view class="info-item">
					<view class="info-label">客户类型:</view>
					<view class="info-value">{{ cusInfo.cus_type && cusInfo.cus_type.params_name || '' }}</view>
				</view>
				<view class="info-item">
					<view class="info-label">客户来源:</view>
					<view class="info-value">{{ cusInfo.cus_source && cusInfo.cus_source.params_name || '' }}</view>
				</view>
				<view class="info-item">
					<view class="info-label">传真:</view>
					<view class="info-value">{{ cusInfo.cus_fax  || '' }}</view>
				</view>
				<view class="info-item">
					<view class="info-label">网站:</view>
					<view class="info-value">{{ cusInfo.cus_website  || '' }}</view>
				</view>
				<view class="info-item">
					<view class="info-label">描述:</view>
					<view class="info-value">{{ cusInfo.DesCripe  || '' }}</view>
				</view>
				<view class="info-item">
					<view class="info-label">备注:</view>
					<view class="info-value">{{ cusInfo.Remarks  || '' }}</view>
				</view>

			</view>
		</view>

		<!-- 选项卡导航 -->
		<view class="tabs-nav">
			<view class="tab-item" v-for="(tab, index) in tabs" :key="index"
				:class="{ 'tab-active': activeTab === index }" @click="switchTab(index)">
				<text>{{ tab.name }}</text>
				<view class="tab-indicator" :class="{ 'indicator-active': activeTab === index }"></view>
			</view>
		</view>

		<!-- 选项卡内容 -->
		<view class="tabs-content">
			<!-- 联系人列表 -->
			<view class="tab-panel" v-if="activeTab === 0">
				<view class="list-header">
					<text>共 {{ contactsList.length }} 个联系人</text>
					<button class="add-btn" @click="Add('contact')">
						<i class="iconfont icon-add"></i> 新增
					</button>
				</view>
				<view class="contact-list">
					<view class="contact-item" v-for="(contact, index) in contactsList" :key="index"
						@click="viewDataInfo('contact',contact)">
						<view class="contact-avatar">
							<image :src="contact.avatar || '/static/image/5-002.png'" mode="aspectFill">
							</image>
						</view>
						<view class="contact-info">
							<view class="contact-name">
								<text>{{ contact.C_name }}</text>
								<text class="contact-position">{{ contact.C_position }}</text>
							</view>
							<view class="contact-contact">
								<view class="contact-phone" >
									<i class="iconfont icon-phone"></i>
									<text>{{ contact.C_tel }}</text>
								</view>
							</view>
						</view>
						<view class="contact-actions">
							<i class="iconfont icon-arrow-right"></i>
						</view>
					</view>
				</view>
				<view class="empty-state" v-if="contactsList.length === 0">
					<i class="iconfont icon-contact-empty"></i>
					<text>暂无联系人信息</text>
					<button @click="Add('contact')">新增联系人</button>
				</view>
			</view>

			<!-- 跟进记录列表 -->
			<view class="tab-panel" v-if="activeTab === 1">
				<view class="list-header">
					<text>共 {{ followList.length }} 条跟进记录</text>
					<button class="add-btn" @click="Add('follow')">
						<i class="iconfont icon-add"></i> 新增
					</button>
				</view>
				<view class="follow-up-list">
					<view class="follow-up-item" v-for="(follow, index) in followList" :key="index"
						@click="viewDataInfo('follow',follow)">
						<!-- <view class="follow-type" :class="'type-' + follow.type">
							{{ getFollowTypeText(follow.type) }}
						</view> -->
						<view class="follow-content">
							<!-- <view class="follow-title">{{ follow.FollowType && follow.FollowType.params_name }}</view> -->
							<view class="follow-desc">{{ follow.follow_content }}</view>
							<view class="follow-meta">
								<text class="follow-operator">{{ follow.contact && follow.contact.C_name }}</text>
								<text class="follow-time">{{ follow.follow_time }}</text>
							</view>
						</view>
					</view>
				</view>
				<view class="empty-state" v-if="followList.length === 0">
					<i class="iconfont icon-follow-empty"></i>
					<text>暂无跟进记录</text>
					<button @click="Add('follow')">新增跟进记录</button>
				</view>
			</view>

			<!-- 订单列表 -->
			<view class="tab-panel" v-if="activeTab === 2">
				<view class="list-header">
					<text>共 {{ orderList.length }} 个订单</text>
					<button class="filter-btn" @click="Add('order')">
						<i class="iconfont icon-filter"></i> 新增
					</button>
				</view>
				<view class="order-list">
					<view class="order-item" v-for="(order, index) in orderList" :key="index"
						@click="viewDataInfo('order',order)">
						<view class="order-header">
							<view class="order-number">订单编号: {{ order.sn }}</view>
							<!-- <view class="order-status" :class="'status-' + order.status">
								{{ getOrderStatusText(order.status) }}
							</view> -->
						</view>
						<view class="order-info">
							<text>创建时间: {{ formatDate(order.Order_date) }}</text>
							<view class="order-amount">
								<text class="amount-label">金额:</text>
								<text class="amount-value">¥{{ order.Order_amount.toFixed(2) }}</text>
							</view>
						</view>
						<!-- <view class="order-time">
							<text>创建时间: {{ formatDate(order.Order_date) }}</text>
						</view> -->
						<view class="order-actions">
							<i class="iconfont icon-arrow-right"></i>
						</view>
					</view>
				</view>
				<view class="empty-state" v-if="orderList.length === 0">
					<i class="iconfont icon-order-empty"></i>
					<text>暂无订单信息</text>
					<button @click="Add('order')">创建订单</button>
				</view>
			</view>

			<!-- 合同列表 -->
			<view class="tab-panel" v-if="activeTab === 3">
				<view class="list-header">
					<text>共 {{ contractList.length }} 个合同</text>
					<button class="filter-btn" @click="Add('contract')">
						<i class="iconfont icon-filter"></i> 新增
					</button>
				</view>
				<view class="order-list">
					<view class="order-item" v-for="(contract, index) in contractList" :key="index"
						@click="viewDataInfo('contract',contract)">
						<view class="order-header">
							<view class="order-number">合同名字: {{ contract.Contract_name }}</view>
							<!-- <view class="order-status" :class="'status-' + order.status">
								{{ getOrderStatusText(order.status) }}
							</view> -->
						</view>
						<view class="order-info">
							<text>合同时间: {{ formatDate(contract.Sign_date) }}</text>
							<view class="order-amount">
								<text class="amount-label">金额:</text>
								<text
									class="amount-value">¥{{ contract.Contract_amount && contract.Contract_amount.toFixed(2) }}</text>
							</view>
						</view>
						<!-- <view class="order-time">
							<text>创建时间: {{ formatDate(order.Order_date) }}</text>
						</view> -->
						<view class="order-actions">
							<i class="iconfont icon-arrow-right"></i>
						</view>
					</view>
				</view>
				<view class="empty-state" v-if="contractList.length === 0">
					<i class="iconfont icon-order-empty"></i>
					<text>暂无合同信息</text>
					<button @click="Add('contract')">创建合同</button>
				</view>
			</view>

			<!-- 合同列表 -->
			<view class="tab-panel" v-if="activeTab === 4">
				<view class="list-header">
					<text>共 {{ receiveList.length }} 个收款</text>
					<button class="filter-btn" @click="Add('receive')">
						<i class="iconfont icon-filter"></i> 新增
					</button>
				</view>
				<view class="order-list">
					<view class="order-item" v-for="(receive, index) in receiveList" :key="index"
						@click="viewDataInfo('receive',receive)">
						<view class="order-header">
							<view class="order-number">收款单号: {{ receive.Receive_num }}</view>
							<!-- <view class="order-status" :class="'status-' + order.status">
								{{ getOrderStatusText(order.status) }}
							</view> -->
						</view>
						<view class="order-info">
							<text>收款时间: {{ formatDate(receive.Receive_date) }}</text>
							<view class="order-amount">
								<text class="amount-label">收款金额:</text>
								<text
									class="amount-value">¥{{ receive.Receive_amount && receive.Receive_amount.toFixed(2) }}</text>
							</view>
						</view>
						<!-- <view class="order-time">
							<text>创建时间: {{ formatDate(order.Order_date) }}</text>
						</view> -->
						<view class="order-actions">
							<i class="iconfont icon-arrow-right"></i>
						</view>
					</view>
				</view>
				<view class="empty-state" v-if="receiveList.length === 0">
					<i class="iconfont icon-order-empty"></i>
					<text>暂无收款信息</text>
					<button @click="Add('receive')">创建收款</button>
				</view>
			</view>
		</view>
	</view>
</template>

<script>
export default {
	data() {
		return {
			// 页面滚动状态
			isScrolled: false,
			// 当前激活的选项卡
			activeTab: 0,
			// 基本信息折叠状态
			infoExpanded: false,
			// 选项卡列表
			tabs: [
				{ name: '联系人' },
				{ name: '跟进' },
				{ name: '订单' },
				{ name: '合同' },
				{ name: '收款' }
			],
			// 客户基本信息
			cusInfo: {},
			// 联系人列表
			contactsList: [],
			// 跟进记录列表
			followList: [],
			// 订单列表
			orderList: [],
			// 合同列表
			contractList: [],
			// 收款列表
			receiveList: [],
			
		};
	},
	onLoad(options) {
		// 解析路由参数
		const receivedData = JSON.parse(decodeURIComponent(options.cusinfo));
		this.cusInfo = receivedData;
		// 首次加载所有数据
		this.loadAllData();
	},	
	onPageScroll(e) {
		this.isScrolled = e.scrollTop > 50;
	},
	methods: {
		updateData(data) {
		    this.cusInfo = data;
			console.log('详情页已更新数据:', data);
		},
		// -------------------- 统一加载所有列表数据 --------------------
		loadAllData() {
			uni.showLoading({ title: '刷新中...', mask: true });
			// 使用 Promise.all 并发请求，提升效率
			Promise.all([
				this.loadContact(),
				this.loadFollow(),
				this.loadOrder(),
				this.loadContract(),
				this.loadReceive()
			])
				.then(() => {
					uni.hideLoading();
					if (this.isFirstLoad) {
						this.isFirstLoad = false; // 标记首次加载完成
					}
				})
				.catch((err) => {
					uni.hideLoading();
					uni.showToast({ title: '加载失败，请重试', icon: 'none' });
					console.error(err);
				});
		},

		// -------------------- 各列表请求方法（返回 Promise） --------------------
		loadReceive() {
			return this.$request('/api/ReceiveList', {
				customer_id: this.cusInfo.id,
				pageSize: 100
			}).then(res => {
				this.receiveList = res.data;
			});
		},
		loadContract() {
			return this.$request('/api/ContractList', {
				customer_id: this.cusInfo.id,
				pageSize: 100
			}).then(res => {
				this.contractList = res.data;
			});
		},
		loadOrder() {
			return this.$request('/api/OrderList', {
				customer_id: this.cusInfo.id,
				pageSize: 100
			}).then(res => {
				this.orderList = res.data;
			});
		},
		loadContact() {
			return this.$request('/api/ContactList', {
				customer_id: this.cusInfo.id,
				pageSize: 100
			}).then(res => {
				this.contactsList = res.data;
			});
		},
		loadFollow() {
			return this.$request('/api/FollowList', {
				customer_id: this.cusInfo.id,
				pageSize: 100
			}).then(res => {
				this.followList = res.data;
			});
		},

		// -------------------- 其他原有方法（未改动） --------------------
		toggleInfoExpand() {
			this.infoExpanded = !this.infoExpanded;
		},
		navigateBack() {
			uni.navigateBack();
		},
		handleEdit() {
			const cusinfo = encodeURIComponent(JSON.stringify(this.cusInfo));
			setTimeout(() => {
				uni.navigateTo({
					url: '/pages/CRM/edit?data=' + cusinfo
				});
			}, 500);
		},
		showActionSheet() {
			uni.showActionSheet({
				itemList: ['分享客户', '新增到分组', '标记为重要', '删除客户'],
				success: (res) => {
					console.log('选中了第' + (res.tapIndex + 1) + '个按钮');
				}
			});
		},
		makePhoneCall(phone) {
			if (!phone) return;
			uni.showModal({
				title: '提示',
				content: '确定拨号吗？',
				success: (res) => {
					if (res.confirm) {
						if (uni.getSystemInfoSync().platform === 'h5') {
							const a = document.createElement('a');
							a.href = `tel:${phone}`;
							a.click();
						} else {
							uni.makePhoneCall({
								phoneNumber: phone,
								success: () => console.log('拨号请求已发起'),
								fail: (err) => {
									console.error('拨号失败：', err);
									uni.showToast({ title: '拨号失败，请重试', icon: 'none' });
								}
							});
						}
					}
				}
			});
		},
		switchTab(index) {
			if (this.activeTab !== index) {
				this.activeTab = index;
				uni.pageScrollTo({ scrollTop: 0, duration: 300 });
			}
		},
		viewDataInfo(type, data) {
			data = encodeURIComponent(JSON.stringify(data));
			var url = '';
			switch (type) {
				case 'follow': url = '/pages/follow/info?data=' + data; break;
				case 'contact': url = '/pages/Contact/info?data=' + data; break;
				case 'order': url = '/pages/Order/info?data=' + data; break;
				case 'contract': url = '/pages/Contract/info?data=' + data; break;
				case 'receive': url = '/pages/Receive/info?data=' + data; break;
			}
			if (url) {
				setTimeout(() => {
					uni.navigateTo({ url });
				}, 100);
			}
		},
		Add(type) {
			var url = '';
			switch (type) {
				case 'follow': url = '/pages/follow/edit'; break;
				case 'contact': url = '/pages/Contact/edit'; break;
				case 'order': url = '/pages/Order/edit'; break;
				case 'contract': url = '/pages/Contract/edit'; break;
				case 'receive': url = '/pages/Receive/edit'; break;
			}
			if (url) {
				setTimeout(() => {
					uni.navigateTo({ url });
				}, 100);
			}
		},
		formatTime(timestamp) {
			const date = new Date(timestamp);
			return date.toLocaleString('zh-CN', {
				month: 'short',
				day: 'numeric',
				hour: '2-digit',
				minute: '2-digit'
			});
		},
		formatDate(timestamp) {
		    if (!timestamp) return '';
		    
		    const date = new Date(timestamp);
		    const year = date.getFullYear();
		    // 月份从0开始，所以需要 +1，padStart(2, '0') 用于补零
		    const month = String(date.getMonth() + 1).padStart(2, '0');
		    const day = String(date.getDate()).padStart(2, '0');
		    
		    return `${year}-${month}-${day}`;
		  }
	}
};
</script>

<style scoped lang="scss">
	// 全局样式变量
	$primary-color: #165DFF;
	$primary-light: #E8F3FF;
	$success-color: #00B42A;
	$warning-color: #FF7D00;
	$danger-color: #F53F3F;
	$gray-light: #F2F3F5;
	$gray-medium: #C9CDD4;
	$gray-dark: #4E5969;
	$text-primary: #1D2129;
	$text-secondary: #86909C;

	.customer-detail-page {
		min-height: 100vh;
		background-color: #F7F8FA;
		font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Open Sans', 'Helvetica Neue', sans-serif;
	}

	// 导航栏样式
	.navbar {
		position: fixed;
		top: 0;
		left: 0;
		right: 0;
		height: 44px;
		background-color: #1677ff;
		color: #fff;
		z-index: 100;
		transition: all 0.3s ease;

		&.navbar-scrolled {
			background-color: #FFFFFF;
			box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
		}

		.navbar-content {
			display: flex;
			align-items: center;
			justify-content: space-between;
			height: 100%;
			padding: 0 16px;

			.back-btn {
				width: 44px;
				height: 44px;
				display: flex;
				align-items: center;
				justify-content: center;
				color: $text-primary;

				.iconfont {
					font-size: 20px;
				}
			}

			.navbar-title {
				flex: 1;
				text-align: center;
				font-size: 17px;
				font-weight: 500;
				color: $text-primary;
				overflow: hidden;
				text-overflow: ellipsis;
				white-space: nowrap;
				color: #fff;
			}

			.action-buttons {
				display: flex;

				.more-btn {
					width: 44px;
					height: 44px;
					display: flex;
					align-items: center;
					justify-content: center;
					background: transparent;
					border: none;
					padding: 0;
					margin: 0;
					color: $text-primary;

					.iconfont {
						font-size: 20px;
					}
				}
			}
		}
	}

	// 客户头部信息
	.customer-header {
		padding: 60px 16px 20px;
		background-color: #FFFFFF;
		display: flex;
		align-items: flex-start;
		position: relative;

		&::after {
			content: '';
			position: absolute;
			left: 0;
			bottom: 0;
			right: 0;
			height: 1px;
			background-color: $gray-light;
			transform: scaleY(0.5);
		}

		.customer-main-info {
			width: 100%;
			min-width: 0;

			.customer-name {
				font-size: 20px;
				font-weight: 600;
				color: $text-primary;
				margin-bottom: 4px;
				overflow: hidden;
				text-overflow: ellipsis;
				white-space: nowrap;
				display: flex;
				align-items: center;
				justify-content: space-between;

				.name-edit-btn {
					background-color: $primary-light;
					color: $primary-color;
					border: none;
					padding: 4px 8px;
					border-radius: 4px;
					font-size: 14px;
					display: flex;
					align-items: center;
					margin-right: 0;

					.iconfont {
						font-size: 14px;
						margin-right: 4px;
					}
				}
			}

			.customer-category {
				font-size: 14px;
				color: $text-secondary;
				margin-bottom: 8px;
			}


		}
	}

	// 核心数据卡片
	.key-data-cards {
		padding: 16px;
		background-color: #FFFFFF;
		display: flex;
		justify-content: space-between;

		.data-card {
			flex: 1;
			text-align: center;
			padding: 0 8px;

			.data-value {
				font-size: 18px;
				font-weight: 600;
				color: $text-primary;
				margin-bottom: 4px;
			}

			.data-label {
				font-size: 13px;
				color: $text-secondary;
			}
		}
	}

	// 客户详细信息区域（折叠面板）
	.customer-info-section {
		margin-top: 12px;
		background-color: #FFFFFF;
		padding: 0 16px;
		border-radius: 8px;
		overflow: hidden;

		.section-title {
			font-size: 16px;
			font-weight: 600;
			color: $text-primary;
			padding: 16px 0;
			position: relative;
			display: flex;
			align-items: center;
			justify-content: space-between;
			cursor: pointer;

			&::after {
				content: '';
				position: absolute;
				left: 0;
				bottom: 0;
				right: 0;
				height: 1px;
				background-color: $gray-light;
				transform: scaleY(0.5);
			}

			.expand-toggle {
				display: flex;
				align-items: center;
				color: $primary-color;
				font-size: 14px;
				font-weight: 500;

				.iconfont {
					font-size: 16px;
					margin-right: 4px;
					transition: transform 0.3s ease;
				}
			}
		}

		.info-list {
			display: flex;
			flex-direction: column;

			.info-item {
				display: flex;
				padding: 12px 0;
				border-bottom: 1px solid $gray-light;

				&:last-child {
					border-bottom: none;
				}

				.info-label {
					width: 80px;
					font-size: 14px;
					color: $text-secondary;
					flex-shrink: 0;
				}

				.info-value {
					flex: 1;
					font-size: 14px;
					color: $text-primary;
					word-break: break-all;
					line-height: 1.5;
					padding-left: 8px;
				}
			}
		}

		// 默认显示的信息
		.default-info {
			border-bottom: 1px solid $gray-light;
		}

		// 可展开的信息
		.expandable-info {
			max-height: 0;
			overflow: hidden;
			transition: max-height 0.3s ease;

			&.expanded {
				max-height: 500px; // 足够容纳所有展开内容
			}
		}
	}

	// 选项卡导航
	.tabs-nav {
		margin-top: 12px;
		background-color: #FFFFFF;
		display: flex;
		height: 48px;
		border-bottom: 1px solid $gray-light;

		.tab-item {
			flex: 1;
			display: flex;
			flex-direction: column;
			align-items: center;
			justify-content: center;
			position: relative;
			font-size: 16px;
			color: $text-secondary;
			font-weight: 500;
			transition: all 0.3s ease;

			&.tab-active {
				color: $primary-color;
			}

			.tab-indicator {
				position: absolute;
				bottom: 0;
				width: 24px;
				height: 3px;
				background-color: transparent;
				border-radius: 3px;
				transition: all 0.3s ease;

				&.indicator-active {
					background-color: $primary-color;
				}
			}
		}
	}

	// 选项卡内容
	.tabs-content {
		padding-bottom: 20px; // 移除底部操作栏后减少底部 padding
	}

	.tab-panel {
		background-color: #FFFFFF;
		min-height: 300px;
	}

	// 列表头部
	.list-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 12px 16px;
		border-bottom: 1px solid $gray-light;

		text {
			font-size: 14px;
			color: $text-secondary;
		}

		.add-btn,
		.filter-btn {
			display: flex;
			align-items: center;
			justify-content: center;
			height: 28px;
			padding: 0 12px;
			font-size: 14px;
			color: $primary-color;
			background-color: transparent;
			border: 1px solid $primary-color;
			border-radius: 14px;

			.iconfont {
				font-size: 14px;
				margin-right: 4px;
			}
		}

		.filter-btn {
			border-color: $text-secondary;
			color: $text-secondary;
		}
	}

	// 联系人列表
	.contact-list {
		.contact-item {
			display: flex;
			align-items: center;
			padding: 12px 16px;
			border-bottom: 1px solid $gray-light;
			transition: background-color 0.2s ease;

			&:last-child {
				border-bottom: none;
			}

			&:active {
				background-color: $gray-light;
			}

			.contact-avatar {
				width: 48px;
				height: 48px;
				border-radius: 50%;
				overflow: hidden;
				flex-shrink: 0;
				border: 1px solid #ddd;

				image {
					width: 100%;
					height: 100%;
					background-color: $gray-light;
				}
			}

			.contact-info {
				flex: 1;
				margin-left: 12px;
				min-width: 0;

				.contact-name {
					display: flex;
					align-items: center;
					margin-bottom: 4px;

					text {
						font-size: 16px;
						font-weight: 500;
						color: $text-primary;
					}

					.contact-position {
						font-size: 13px;
						color: $text-secondary;
						margin-left: 8px;
						white-space: nowrap;
					}
				}

				.contact-contact {
					display: flex;
					flex-wrap: wrap;
					gap: 8px;

					.contact-phone,
					.contact-email {
						display: flex;
						align-items: center;
						font-size: 13px;
						color: $text-secondary;

						.iconfont {
							font-size: 13px;
							margin-right: 4px;
						}
					}

					.contact-phone {
						color: $primary-color;
					}
				}
			}

			.contact-actions {
				color: $gray-medium;

				.iconfont {
					font-size: 20px;
				}
			}
		}
	}

	// 跟进记录列表
	.follow-up-list {
		.follow-up-item {
			padding: 16px;
			border-bottom: 1px solid $gray-light;
			transition: background-color 0.2s ease;

			&:last-child {
				border-bottom: none;
			}

			&:active {
				background-color: $gray-light;
			}

			.follow-type {
				display: inline-block;
				padding: 2px 8px;
				border-radius: 4px;
				font-size: 12px;
				margin-bottom: 8px;
				font-weight: 500;

				&.type-call {
					background-color: rgba(22, 93, 255, 0.1);
					color: $primary-color;
				}

				&.type-meeting {
					background-color: rgba(0, 180, 42, 0.1);
					color: $success-color;
				}

				&.type-email {
					background-color: rgba(255, 125, 0, 0.1);
					color: $warning-color;
				}

				&.type-visit {
					background-color: rgba(153, 77, 255, 0.1);
					color: #994DFF;
				}
			}

			.follow-content {
				.follow-title {
					font-size: 16px;
					font-weight: 500;
					color: $text-primary;
					margin-bottom: 4px;
				}

				.follow-desc {
					font-size: 14px;
					color: $text-primary;
					line-height: 1.5;
					margin-bottom: 8px;
					display: -webkit-box;
					-webkit-line-clamp: 2;
					-webkit-box-orient: vertical;
					overflow: hidden;
				}

				.follow-meta {
					display: flex;
					justify-content: space-between;
					font-size: 13px;
					color: $text-secondary;
				}
			}
		}
	}

	// 订单列表
	.order-list {
		.order-item {
			padding: 16px;
			border-bottom: 1px solid $gray-light;
			transition: background-color 0.2s ease;
			position: relative;

			&:last-child {
				border-bottom: none;
			}

			&:active {
				background-color: $gray-light;
			}

			.order-header {
				display: flex;
				justify-content: space-between;
				margin-bottom: 12px;

				.order-number {
					font-size: 14px;
					color: $text-primary;
				}

				.order-status {
					font-size: 14px;
					font-weight: 500;

					&.status-paid {
						color: $primary-color;
					}

					&.status-completed {
						color: $success-color;
					}

					&.status-canceled {
						color: $danger-color;
					}

					&.status-pending {
						color: $warning-color;
					}
				}
			}

			.order-info {
				display: flex;
				justify-content: space-between;
				margin-bottom: 8px;
				font-size: 13px;
				color: $text-secondary;

				.order-product {
					font-size: 15px;
					color: $text-primary;
					font-weight: 500;
				}

				.order-amount {
					.amount-label {
						font-size: 14px;
						color: $text-secondary;
					}

					.amount-value {
						font-size: 15px;
						color: $danger-color;
						font-weight: 600;
					}
				}
			}

			.order-time {
				font-size: 13px;
				color: $text-secondary;
			}

			.order-actions {
				position: absolute;
				right: 16px;
				top: 50%;
				transform: translateY(-50%);
				color: $gray-medium;

				.iconfont {
					font-size: 20px;
				}
			}
		}
	}

	// 空状态
	.empty-state {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		padding: 40px 20px;
		text-align: center;

		.iconfont {
			font-size: 60px;
			color: $gray-medium;
			margin-bottom: 16px;
		}

		text {
			font-size: 15px;
			color: $text-secondary;
			margin-bottom: 24px;
		}

		button {
			height: 40px;
			padding: 0 24px;
			font-size: 15px;
			color: #FFFFFF;
			background-color: $primary-color;
			border-radius: 20px;
			border: none;
		}
	}
</style>