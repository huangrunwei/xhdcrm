<template>
	<view class="content">
		<view class="topBox">
			<view class="search-container">
				<input type="text" v-model="searchText" placeholder="请输入搜索内容" />
				<button @click="performSearch">搜索</button>
			</view>
		</view>
		<view class="inputBox">
			<view class="icon-grid">
				<!-- 图标项1 -->
				<view @click="goToPage('/pages/CRM/CRM')" class="icon-item">
					<view class="icon-wrapper">
						<uni-icons type="auth-filled" size="40" color="#722ED1"></uni-icons>
					</view>
					<view class="icon-text">客户</view>
				</view>

				<!-- 图标项2 -->
				<view @click="goToPage('/pages/follow/follow')" class="icon-item">
					<view class="icon-wrapper">
						<uni-icons type="chatbubble-filled" size="40" color="#722ED1"></uni-icons>
					</view>
					<view class="icon-text">跟进</view>
				</view>

				<!-- 图标项3 -->
				<view class="icon-item">
					<view class="icon-wrapper">
						<uni-icons type="cart-filled" size="40" color="#722ED1"></uni-icons>
					</view>
					<view class="icon-text">订单</view>
				</view>

				<!-- 图标项4 -->
				<view class="icon-item">
					<view class="icon-wrapper">
						<uni-icons type="compose" size="40" color="#722ED1"></uni-icons>
					</view>
					<view class="icon-text">合同</view>
				</view>
			</view>
		</view>
		<view class="newsbox">
			<uni-section title="新闻公告" type="line">
				<!-- 新闻列表 -->
				<view class="news-list">
					<!-- 新闻项 - 循环渲染 -->
					<view class="news-item" v-for="(news, index) in newsList" :key="index"
						@click="handleNewsClick(news)">
						<!-- 左侧图片 -->
						<view class="news-image" v-if="news.imageUrl">
							<image :src="news.imageUrl" mode="aspectFill" class="news-img" lazy-load></image>
						</view>

						<!-- 右侧文字内容 -->
						<view class="news-content">
							<!-- 第一行：主标题和时间 -->
							<view class="news-first-line">
								<text class="news-title">{{ news.title }}</text>
								<text class="news-time">{{ formatTime(news.time) }}</text>
							</view>

							<!-- 第二行：副标题 -->
							<view class="news-second-line">
								<text class="news-subtitle">{{ news.subtitle || '暂无摘要' }}</text>
							</view>
						</view>
					</view>
				</view>
				<!-- 空状态处理 -->
				<view class="empty-state" v-if="newsList.length === 0">
					<text>暂无新闻数据</text>
				</view>
			</uni-section>
		</view>
	</view>
</template>

<script>
	export default {
		props: {
			// 可以从父组件传入新闻数据
			newsData: {
				type: Array,
				default: () => []
			}
		},
		data() {
			return {
				// 新闻列表数据，优先使用父组件传入的数据
				newsList: this.newsData.length > 0 ? this.newsData : [{
						id: 1,
						title: '小黄豆软件3.0APP上线了',
						subtitle: '基于UNI-APP开发的APP上线了，能适应各种平台',
						time: '08:30',
						imageUrl: 'https://img.mobanwang.com/file/mb/2025/08-09/small8754c329b0e792e4b6a1c6cd2f73cb0d.jpg'
					},
					{
						id: 2,
						title: '视频行业动态',
						subtitle: '超过150个摄像头平台',
						time: '09:15',
						imageUrl: 'https://img.mobanwang.com/file/mb/2025/07-02/small50334f3b01fc16c62430928f41395bbd.jpg'
					},
					{
						id: 3,
						title: '医疗行业迎来利好',
						subtitle: '国家推进医疗改革，医疗设备全面',
						time: '10:45',
						imageUrl: 'https://img.mobanwang.com/file/mb/2025/06-09/smallb6fd6ed3a3eebbad99ac823c38ef0662.jpg'
					},
					{
						id: 4,
						title: '国内新能源汽车销量连续6个月保持增长',
						subtitle: '市场占有率已突破25%，充电桩建设速度同步加快',
						time: '13:20'
						// 演示没有图片的情况
					},
					{
						id: 5,
						title: '首届数字经济博览会将于下月在深圳举办',
						subtitle: '聚焦人工智能、大数据和区块链等前沿技术应用',
						time: '15:10',
						imageUrl: 'https://img.mobanwang.com/file/mb/2025/05-29/small6e32bf836366f1bdffaf272eff38bd67.jpg'
					}
				],
				startX: 0, // 用于记录触摸起始X坐标
			};
		},
		onLoad() {
			// 初始化时添加一条历史记录，避免直接退出
			history.pushState(null, null, location.href);

			window.addEventListener('popstate', this.handlePopState);
		},

		methods: {
			// 返回上一页
			navigateBack() {
				uni.navigateBack({
					delta: 1
				});
			},

			handleNewsClick(news) {
				this.$emit('news-click', news);
			},
			// 时间格式化方法
			formatTime(timeStr) {
				// 这里可以根据实际需求实现更复杂的时间格式化
				return timeStr;
			},
			// 通用跳转方法
			goToPage(url) {
				// 判断是否为 tabBar 页面（根据实际情况修改）
				const tabBarPages = ['/pages/index/index', '/pages/CRM/CRM'];

				if (tabBarPages.includes(url)) {
					// 跳转到 tabBar 页面
					uni.switchTab({
						url
					});
				} else {
					// 跳转到普通页面
					uni.navigateTo({
						url
					});
				}
			},
			handlePopState() {
				history.pushState(null, null, location.href);
				// // 拦截后退，执行自定义逻辑
				// uni.showModal({
				// 	title: '提示',
				// 	content: '确定要退出应用吗？',
				// 	success: (res) => {
				// 		if (res.confirm) {
				// 			// 用户确认退出，再添加一条记录后后退（触发关闭）
				// 			history.pushState(null, null, location.href);
				// 			//window.history.back();
				// 		} else {
				// 			// 用户取消，重新添加历史记录阻止后退
				// 			history.pushState(null, null, location.href);
				// 		}
				// 	}
				// });
			}
		}
	}
</script>

<style scoped>
	.search-container {
		padding: 10px;
		display: flex;
		justify-content: space-between;
		align-items: center;
	}

	input {
		flex: 1;
		/* 让输入框自适应剩余空间 */
		margin-right: 10px;
		/* 与按钮之间留出空间 */
		padding: 10px;
		border: 1px solid #ccc;
		border-radius: 5px;
		background: #fff;
	}

	button {

		background-color: #007bff;
		color: white;
		border: none;
		border-radius: 5px;
	}

	.image-text-container {
		display: flex;
		align-items: center;
		padding: 16rpx;
	}

	/* 图片容器 - 确保图片显示为圆形 */
	.image-wrapper {
		width: 100rpx;
		height: 100rpx;
		border-radius: 50%;
		overflow: hidden;
		/* 图片与文字之间的间距 */
		margin-right: 20rpx;
	}

	/* 圆形图片样式 */
	.round-image {
		width: 100%;
		height: 100%;
	}

	/* 文字容器 - 垂直排列 */
	.text-container {
		display: flex;
		flex-direction: column;
		justify-content: center;
	}

	/* 上方文字样式 - 16px */
	.text-top {
		color: #ffffff;
		font-size: 16px;
		line-height: 1.2;
	}

	/* 下方文字样式 - 10px */
	.text-bottom {
		color: #ffffff;
		font-size: 10px;
		line-height: 1.2;
		margin-top: 4rpx;
		/* 上下文字之间的间距 */
	}

	.content {
		height: 20vh;
		background-color: aquamarine;
		background: url("../../static/image/bg_366x650.jpg") no-repeat;
		background-size: cover;
		padding: 10px;
	}

	.topBox {
		font-size: 34rpx;
		color: #fff;

	}

	h3 {
		margin-bottom: 10rpx;
	}

	.newsbox {
		left: 0;
		background-color: #fff;
		border-radius: 40rpx;
		padding: 20rpx 40rpx;
		box-sizing: border-box;
		box-shadow: rgba(200, 200, 200, .5) 0px 0px 3px;

		margin-bottom: 10px;
	}

	.inputBox {
		left: 0;
		background-color: #fff;
		border-radius: 40rpx;
		padding: 30rpx;
		box-sizing: border-box;
		box-shadow: rgba(200, 200, 200, .5) 0px 0px 3px;

		margin-bottom: 10px;
	}

	.ipt {
		margin-bottom: 50rpx;
	}

	.ipt h4 {
		margin-bottom: 20rpx;
		font-size: 36rpx;
		color: #333;
	}

	.ipt input {
		border-bottom: 1px solid #dedede;
		padding-bottom: 20rpx;
		font-size: 28rpx;
	}

	.loginBtn {
		margin-top: 20rpx;
		line-height: 85rpx;
		text-align: center;
		background: linear-gradient(to right, rgb(86, 104, 214), rgb(86, 104, 214));
		border-radius: 40rpx;
		color: #fff;
		margin-top: 50rpx;
	}

	.registerBtn {
		margin-top: 20rpx;
		line-height: 85rpx;
		text-align: center;
		border-radius: 40rpx;
		color: rgb(86, 104, 214);
		margin-top: 50rpx;
		border: none;
	}

	.tipbox {
		position: fixed;
		bottom: 120rpx;
		left: 50%;
		transform: translate(-50%, -120px);
	}

	.otherUser {
		margin-top: 30rpx;
		display: flex;
		justify-content: center;
	}

	.txt {
		font-size: 28rpx;
		color: #969696;
	}

	.otherUser .uni-icons {
		margin-left: 20rpx;
	}

	/* 横向列表容器 */
	.icon-grid {
		display: flex;
		justify-content: space-around;
		align-items: center;
		width: 100%;
	}

	/* 每个图标项 */
	.icon-item {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		width: 25%;
		padding: 10rpx;
		box-sizing: border-box;
		position: relative;
		transition: transform 0.2s ease;
	}

	/* 点击效果 */
	.icon-item:active {
		transform: scale(0.95);
	}

	/* 数字样式 */
	.icon-number {
		position: absolute;
		top: 0;
		right: 30rpx;
		background-color: #ff4d4f;
		color: #ffffff;
		font-size: 24rpx;
		font-weight: bold;
		width: 36rpx;
		height: 36rpx;
		border-radius: 50%;
		display: flex;
		align-items: center;
		justify-content: center;
		z-index: 1;
	}

	/* 图标容器 */
	.icon-wrapper {
		width: 80rpx;
		height: 80rpx;
		border-radius: 50%;
		background-color: #f7f3ff;
		display: flex;
		align-items: center;
		justify-content: center;
		margin-bottom: 16rpx;
	}

	/* 文字样式 */
	.icon-text {
		font-size: 28rpx;
		color: #1d2129;
		text-align: center;
		white-space: nowrap;
		overflow: hidden;
		text-overflow: ellipsis;
		max-width: 100%;
	}

	.news-list {
		width: 100%;
	}

	/* 新闻项样式 */
	.news-item {
		padding: 30rpx 0;
		transition: background-color 0.2s ease;
		display: flex;
		align-items: center;
	}

	/* 图片容器 */
	.news-image {
		width: 100rpx;
		height: 100rpx;
		border-radius: 8rpx;
		overflow: hidden;
		margin-right: 24rpx;
		flex-shrink: 0;
		/* 防止图片被压缩 */
	}

	/* 图片样式 */
	.news-img {
		width: 100%;
		height: 100%;
	}

	/* 文字内容容器 */
	.news-content {
		flex: 1;
		/* 占满剩余空间 */
		min-width: 0;
		/* 解决flex子元素文本溢出问题 */
	}

	/* 第一行布局 */
	.news-first-line {
		display: flex;
		justify-content: space-between;
		align-items: flex-start;
		margin-bottom: 10rpx;

	}

	/* 主标题样式 */
	.news-title {
		font-size: 32rpx;
		color: #1d2129;
		font-weight: bolder;
		flex: 1;
		line-height: 1.4;
		display: -webkit-box;
		-webkit-box-orient: vertical;
		-webkit-line-clamp: 1;
		overflow: hidden;
		margin-right: 20rpx;
		color: #722ED1;
	}

	/* 时间样式 */
	.news-time {
		font-size: 26rpx;
		color: #86909c;
		white-space: nowrap;
	}

	/* 第二行布局 */
	.news-second-line {
		width: 100%;
	}

	/* 副标题样式 */
	.news-subtitle {
		font-size: 28rpx;
		color: #4e5969;
		line-height: 1.4;
		display: -webkit-box;
		-webkit-box-orient: vertical;
		-webkit-line-clamp: 1;
		overflow: hidden;
		color: #777;
	}

	/* 点击和悬停效果 */
	.news-item:active {
		background-color: #f5f5f5;
	}

	.news-item-hover {
		background-color: #f5f5f5;
	}

	/* 空状态样式 */
	.empty-state {
		padding: 100rpx 0;
		text-align: center;
		color: #86909c;
		font-size: 28rpx;
	}
</style>