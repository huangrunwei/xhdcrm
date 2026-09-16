<template>
	<view class="container">
		<!-- 顶部导航栏 -->
		<!-- <view class="navbar">
			<view class="navbar-content">
				<text class="navbar-title">CRM系统</text>
			</view>
		</view> -->

		<!-- 主内容区域 -->
		<scroll-view class="main-content" scroll-y="true">
			<view class="content-wrapper">
				<!-- 数据统计模块 -->
				<view class="stats-module rounded-module">
					<!-- 用户名展示区域 -->
					<view class="user-greeting">
						<text class="greeting-text">您好，</text>
						<text class="user-name">{{username}}</text>
						<text class="greeting-text">，欢迎回来</text>
					</view>

					<text class="module-title">业务概览</text>
					<view class="stats-grid">
						<view class="stat-item rounded-item">
							<text class="stat-label">总客户数</text>
							<text class="stat-value">{{ countData.cuscount }}</text>
							<!-- <text class="stat-trend up">+8.2%</text> -->
						</view>
						<view class="stat-item rounded-item">
							<text class="stat-label">总跟进数</text>
							<text class="stat-value">{{ countData.followcount }}</text>
							<!-- <text class="stat-trend up">+12.5%</text> -->
						</view>
						<view class="stat-item rounded-item">
							<text class="stat-label">总订单数</text>
							<text class="stat-value">{{ countData.ordercount }}</text>
							<!-- <text class="stat-trend up">+1.8%</text> -->
						</view>
						<view class="stat-item rounded-item">
							<text class="stat-label">合同总数</text>
							<text class="stat-value">{{ countData.contractcount }}</text>
							<!-- <text class="stat-trend down">-3.1%</text> -->
						</view>

					</view>
				</view>

				<!-- 快捷功能宫格 -->
				<view class="shortcut-module rounded-module">
					<text class="module-title">快捷功能</text>
					<view class="shortcut-grid">
						<view class="shortcut-item rounded-item" @click="navigateTo('crm')">
							<view class="shortcut-icon rounded-icon">
								<uni-icons type="plusempty" size="16" color="#fff"></uni-icons>
							</view>
							<text class="shortcut-text">客户</text>
						</view>
						<view class="shortcut-item rounded-item" @click="navigateTo('follow')">
							<view class="shortcut-icon rounded-icon">
								<uni-icons type="chatboxes" size="16" color="#fff"></uni-icons>
							</view>
							<text class="shortcut-text">跟进</text>
						</view>
						<view class="shortcut-item rounded-item" @click="navigateTo('Contact')">
							<view class="shortcut-icon rounded-icon">
								<uni-icons type="person" size="16" color="#fff"></uni-icons>
							</view>
							<text class="shortcut-text">联系人</text>
						</view>
						<view class="shortcut-item rounded-item" @click="navigateTo('Order')">
							<view class="shortcut-icon rounded-icon">
								<uni-icons type="cart" size="16" color="#fff"></uni-icons>
							</view>
							<text class="shortcut-text">订单</text>
						</view>
						<view class="shortcut-item rounded-item" @click="navigateTo('Contract')">
							<view class="shortcut-icon rounded-icon">
								<uni-icons type="folder-add" size="16" color="#fff"></uni-icons>
							</view>
							<text class="shortcut-text">合同</text>
						</view>

						<view class="shortcut-item rounded-item" @click="navigateTo('Receive')">
							<view class="shortcut-icon rounded-icon">
								<uni-icons type="wallet" size="16" color="#fff"></uni-icons>
							</view>
							<text class="shortcut-text">收款</text>
						</view>

					</view>
				</view>

				<!-- 最近动态 -->
				<view class="activities-module rounded-module">
					<text class="module-title">最近动态</text>
					<view class="activity-list" v-for="(item, index) in followList" :key="index"
						@click="CustomerClick(item, index)">


						<view class="activity-item rounded-item">
							<view class="activity-icon follow-up rounded-icon">
								<uni-icons type="chatboxes" size="16" color="#fff"></uni-icons>
							</view>
							<view class="activity-content">
								<text class="activity-title">{{ item.customer&&item.customer.cus_name }}</text>
								<text class="activity-time">{{ item.follow_content }}</text>
								<text class="activity-time">{{ item.follow_time }}</text>
							</view>
						</view>
					</view>
				</view>
			</view>
		</scroll-view>
	</view>
</template>

<script>
	export default {
		data() {
			return {
				username: "",
				// 列表数据
				followList: [],

				countData: {}
			};
		},
		onLoad() {
			// 页面加载时执行
			const userInfo = uni.getStorageSync('userInfo');
			if (userInfo) {
				this.username = userInfo.realname;
			}

			// 请求
			var postdata = {
				"serchtxt": this.keyword,
				"page": 1,
				"limit": 5
			};
			this.$request("/api/FollowList", postdata)
				.then(res => {
					console.log(res);

					this.followList = res.data;

					console.log(this.followList)
				});

			this.$request("/api/CountData")
				.then(res => {
					console.log(res);

					this.countData = res.data[0];
				});
		},
		methods: {

			navigateTo(page) {
				if (page === 'crm') {
					uni.switchTab({
						url: '/pages/CRM/CRM'
					});
					return;
				}
				uni.navigateTo({
					url: `/pages/${page}/${page}`
				});
			}
		}
	};
</script>

<style scoped>
	/* 基础样式 */
	.container {
		display: flex;
		flex-direction: column;
		height: 100%;
		background-color: #f5f5f5;
	}

	/* 导航栏样式 */
	.navbar {
		background-color: #1677ff;
		height: 44px;
		line-height: 44px;
		padding: 0 16px;
	}

	.navbar-content {
		display: flex;
		justify-content: center;
		align-items: center;
		height: 100%;
		position: relative;
	}

	.navbar-title {
		color: #ffffff;
		font-size: 18px;
		font-weight: bold;
	}

	.user-avatar {
		width: 30px;
		height: 30px;
		border-radius: 50%;
		border: 1px solid #ffffff;
		position: absolute;
		right: 16px;
	}

	/* 主内容区域 */
	.main-content {
		flex: 1;
	}

	/* 内容包裹层 - 控制最大宽度和边距 */
	.content-wrapper {
		padding: 12px;
		/* 限制最大宽度，在大屏设备上不会无限变宽 */
		max-width: 800px;
		/* 水平居中，在大屏设备上内容居中显示 */
		margin: 0 auto;
		width: 100%;
		box-sizing: border-box;
	}

	/* 通用模块样式 */
	.rounded-module {
		background-color: #ffffff;
		border-radius: 12px;
		padding: 16px;
		margin-bottom: 16px;
		box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
	}

	.rounded-item {
		border-radius: 8px;
	}

	.rounded-icon {
		border-radius: 50%;
	}

	/* 模块标题样式 */
	.module-title {
		font-size: 16px;
		color: #333333;
		font-weight: 500;
		margin-bottom: 16px;
		display: block;
		padding-top: 8px;
	}

	/* 用户名展示样式 */
	.user-greeting {
		padding: 8px 0;
		margin-bottom: 8px;
	}

	.greeting-text {
		font-size: 14px;
		color: #666666;
	}

	.user-name {
		font-size: 16px;
		color: #333333;
		font-weight: bold;
		margin: 0 4px;
	}

	/* 数据统计基础样式 */
	.stats-grid {
		display: flex;
		flex-wrap: wrap;
		justify-content: space-between;
		gap: 12px;
	}

	.stat-item {
		background-color: #f0f7ff;
		padding: 16px 12px;
		box-sizing: border-box;
	}

	.stat-label {
		font-size: 12px;
		color: #666666;
	}

	.stat-value {
		font-size: 22px;
		color: #1677ff;
		font-weight: bold;
		margin: 8px 0;
		display: block;
	}

	.stat-trend {
		font-size: 12px;
	}

	.stat-trend.up {
		color: #52c41a;
	}

	.stat-trend.down {
		color: #ff4d4f;
	}

	/* 快捷功能基础样式 */
	.shortcut-grid {
		display: flex;
		flex-wrap: wrap;
		justify-content: space-between;
		gap: 12px;
	}

	.shortcut-item {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		padding: 15px 0;
		background-color: #f0f7ff;
	}

	.shortcut-icon {
		width: 40px;
		height: 40px;
		background-color: #1677ff;
		color: #ffffff;
		display: flex;
		align-items: center;
		justify-content: center;
		font-size: 20px;
		margin-bottom: 8px;
	}

	.shortcut-text {
		font-size: 12px;
		color: #333333;
	}

	/* 最近动态样式 */
	.activity-list {
		display: flex;
		flex-direction: column;
		gap: 8px;
		margin-bottom: 5px;
	}

	.activity-item {
		display: flex;
		padding: 12px;
		background-color: #fafafa;
		align-items: center;
	}

	.activity-icon {
		width: 24px;
		height: 24px;
		margin-right: 12px;
		flex-shrink: 0;
		display: flex;
		align-items: center;
		justify-content: center;
		font-size: 14px;
		color: white;
	}

	.add-customer {
		background-color: #52c41a;
	}

	.create-order {
		background-color: #1890ff;
	}

	.follow-up {
		background-color: #faad14;
	}

	.activity-content {
		flex: 1;
	}

	.activity-title {
		font-size: 14px;
		color: #333333;
	}

	.activity-time {
		font-size: 12px;
		color: #999999;
		margin-top: 4px;
		display: block;
	}

	/* 自适应布局 - 小屏幕手机 (默认) */
	.stat-item {
		width: calc(50% - 6px);
		/* 两列布局 */
	}

	.shortcut-item {
		width: calc(33.33% - 8px);
		/* 三列布局 */
	}

	/* 平板及中等屏幕设备 (≥ 600px) */
	@media screen and (min-width: 600px) {
		.content-wrapper {
			padding: 20px;
		}

		.rounded-module {
			padding: 20px;
			border-radius: 16px;
		}

		.stat-item {
			width: calc(25% - 9px);
			/* 四列布局 */
		}

		.shortcut-item {
			width: calc(16.66% - 10px);
			/* 六列布局 */
		}

		.module-title {
			font-size: 18px;
		}

		.stat-value {
			font-size: 24px;
		}

		.shortcut-text {
			font-size: 14px;
		}
	}

	/* 大屏幕设备 (≥ 800px) */
	@media screen and (min-width: 800px) {
		.navbar {
			height: 50px;
			line-height: 50px;
		}

		.navbar-title {
			font-size: 20px;
		}

		.user-avatar {
			width: 36px;
			height: 36px;
		}

		.activity-title {
			font-size: 16px;
		}

		.activity-time {
			font-size: 14px;
		}
	}
</style>