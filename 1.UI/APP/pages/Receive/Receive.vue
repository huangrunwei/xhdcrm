<template>
	<view class="container">
		<!-- 顶部导航栏 -->
		<!-- <view class="navbar">
			<text class="navbar-title">收款列表</text>
		</view> -->

		<!-- 搜索区域 -->
		<view class="search-container">
			<!-- 基础搜索栏 -->
			<view class="basic-search">
				<view class="search-input">
					<icon type="search" size="18" color="#999"></icon>
					<input type="text" placeholder="输入客户名称搜索" v-model="keyword"></input>
				</view>
				<button class="expand-btn" @click="dosearch">
					<text>搜索</text>
					<icon type="arrow-down" size="16"></icon>
				</button>
			</view>
		</view>

		<!-- 列表区域 - 使用scroll-view实现滚动控制 -->
		<scroll-view class="list-container" scroll-y="true" ref="listScroll" @scrolltolower="onReachBottom"
			@scroll="onScroll">
			<view class="list-content">
				<view class="customer-item" v-for="(item, index) in dataList" :key="index"
					@click="CustomerClick(item, index)">
					<view class="item-row">
						<!-- <text class="labelTitle">客户名：</text> -->
						<text class="contentTitle">{{ item.Order&&item.Order.customer.cus_name }}</text>
					</view>
					<view class="item-row">
						<!-- <text class="label">电话：</text> -->
						<text class="content">{{ item.Receive_amount }}</text>
					</view>
					<view class="item-row">
						<!-- <text class="label">地址：</text> -->
						<text class="content">{{ item.Receive_date }}</text>
					</view>
					<!-- <view class="item-row extra-info">
            <text class="content">最后跟进：{{ item.lastFollowTime }}</text>
          </view> -->
				</view>

				<!-- 加载更多 -->
				<view class="load-more" v-if="loading">
					<loading :show="true" size="16"></loading>
					<text class="load-text">加载中...</text>
				</view>

				<!-- 没有更多数据 -->
				<view class="no-more" v-if="!hasMore && !loading">
					<text>没有更多数据了</text>
				</view>

				<!-- 空状态 -->
				<view class="empty-state" v-if="dataList.length === 0 && !loading">
					<image src="/static/images/empty.png" mode="widthFix" class="empty-img"></image>
					<text class="empty-text">暂无数据</text>
				</view>
			</view>
		</scroll-view>

		<!-- 右下角悬浮添加按钮 -->
		<button class="float-add-btn" @click="handleAdd">
			<uni-icons type="plusempty" size="21" color="#fff"></uni-icons>
		</button>

		<!-- 回到顶部按钮 - 滚动一定距离后显示 -->
		<button class="back-to-top" @click="scrollToTop" v-if="showBackToTop">
			<icon type="arrowup" size="20" color="#fff"></icon>
		</button>
	</view>
</template>

<script>
	export default {
		data() {
			return {
				// 基础搜索关键词
				keyword: '',
				// 列表数据
				dataList: [],
				// 分页参数
				page: 1,
				pageSize: 10,
				// 加载状态
				loading: false,
				// 是否还有更多数据
				hasMore: true,
				// 控制回到顶部按钮显示
				showBackToTop: false,
				// 滚动位置记录
				scrollTop: 0
			};
		},
		onLoad() {
			// 未登录，跳转到登录页，并记录当前页面以便登录后返回
			// 获取当前登录状态 - 从storage或store中获取
			const isLogin = uni.getStorageSync('isLogin')

			console.log("isLogin=>" + isLogin)

			if (!isLogin) {
				setTimeout(() => {
					uni.reLaunch({
						url: '/pages/Login/Login'
					});
				}, 500);
			}

			// 初始化加载数据
			this.loadList();


		},
		methods: {
			updateData(data) {
			    if (!data || !data.id) return;
				const index = this.dataList.findIndex(item => item.id === data.id);
				if (index !== -1) {
					// 使用 $set 触发响应式更新
					this.$set(this.dataList, index, data);
					console.log('列表页已更新数据:', data);
				} else {
					console.warn('列表页未找到对应ID的数据:', data.id);
				}
			},			
			// 加载列表
			loadList() {
				this.loading = true;

				// 请求
				var postdata = {
					"serchtxt": this.keyword,
					"page": this.page,
					"limit": this.pageSize
				};

				var newData = [];

				this.$request("/api/ReceiveList", postdata)
					.then(res => {
						console.log(res);

						newData.push(res.data);

						// 合并数据
						this.dataList = this.page === 1 ? res.data : [...this.dataList, ...res.data];

						console.log(this.dataList)

						this.loading = false;

						var total = res.count;

						var maxpage = 1;
						if (total <= 0) {
							maxpage = 1;
						} else {
							maxpage = Math.ceil(total / this.pageSize);
						}

						if (this.page >= maxpage) {
							this.hasMore = false;
						}
					});
			},

			// 下拉加载更多
			onReachBottom() {
				if (!this.loading && this.hasMore) {
					this.page++;
					this.loadList();
				}
			},

			// 处理搜索
			dosearch() {
				// 重置分页
				this.page = 1;
				this.hasMore = true;
				// 执行搜索逻辑，这里简单模拟
				this.loaddataList();

				// 收起键盘
				uni.hideKeyboard();
			},

			// 监听滚动事件
			onScroll(e) {
				this.scrollTop = e.detail.scrollTop;
				// 滚动超过500px显示回到顶部按钮
				this.showBackToTop = true;
			},

			// 回到顶部
			scrollToTop() {
				this.$refs.listScroll.scrollTo({
					scrollTop: 0,
					duration: 300 // 滚动动画时长
				});
			},

			// 点击添加按钮
			handleAdd() {
				// 跳转到添加客户页面
				uni.navigateTo({
					url: '/pages/Receive/edit'
				});
			},
			// 点击事件处理函数
			CustomerClick(item, index) {
				console.log('点击了项：', item);
				//console.log('项的索引：', index);

				// 将JSON对象转为字符串（encodeURIComponent处理特殊字符）
				const cusinfo = encodeURIComponent(JSON.stringify(item));

				setTimeout(() => {
					uni.navigateTo({
						url: '/pages/Receive/info?data=' + cusinfo
					});
				}, 500);
			}
		}
	};
</script>

<style scoped>
	.container {
		display: flex;
		flex-direction: column;
		min-height: 100vh;
		background-color: #f5f5f5;
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

	/* 搜索区域样式 */
	.search-container {
		padding: 10px;
		background-color: #fff;
		border-bottom: 1px solid #eee;
	}

	.basic-search {
		display: flex;
		align-items: center;
	}

	.search-input {
		flex: 1;
		display: flex;
		align-items: center;
		background-color: #f5f5f5;
		border-radius: 20px;
		padding: 8px 15px;
		margin-right: 10px;
	}

	.search-input input {
		flex: 1;
		margin-left: 5px;
		font-size: 14px;
		background-color: transparent;
	}

	.expand-btn {
		background-color: transparent;
		color: #1677ff;
		font-size: 14px;

		display: flex;
		align-items: center;
		border: 1px solid #ddd;
	}

	.expand-btn icon {
		margin-left: 3px;
		transition: transform 0.3s ease;
	}

	.expand-btn .rotate {
		transform: rotate(180deg);
	}

	/* 列表区域样式 */
	.list-container {
		flex: 1;
		width: 100%;
		overflow: hidden;
	}

	.list-content {
		padding: 10px;
	}

	.customer-item {
		background-color: #fff;
		border-radius: 10px;
		padding: 12px 15px;
		margin-bottom: 10px;
		box-shadow: 0 2px 5px rgba(0, 0, 0, 0.05);
	}

	.item-row {
		display: flex;
		margin-bottom: 8px;
	}

	.item-row:last-child {
		margin-bottom: 0;
	}

	.item-row .label {
		color: #666;
		font-size: 14px;
		width: 70px;
	}

	.item-row .content {
		flex: 1;
		color: #333;
		font-size: 14px;
		word-break: break-all;
	}

	.item-row .contentTitle {
		flex: 1;
		color: #333;
		font-size: 16px;
		word-break: break-all;
		font-weight: bolder;
	}

	.extra-info {
		margin-top: 5px;
		padding-top: 5px;
		border-top: 1px dashed #eee;
	}

	.extra-info .content {
		color: #999;
		font-size: 13px;
	}

	/* 加载更多样式 */
	.load-more {
		display: flex;
		align-items: center;
		justify-content: center;
		padding: 15px 0;
		color: #999;
		font-size: 14px;
	}

	.load-text {
		margin-left: 5px;
	}

	/* 没有更多数据 */
	.no-more {
		text-align: center;
		padding: 15px 0;
		color: #999;
		font-size: 14px;
	}

	/* 空状态 */
	.empty-state {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		padding: 50px 0;
	}

	.empty-img {
		width: 120px;
		margin-bottom: 15px;
	}

	.empty-text {
		color: #999;
		font-size: 14px;
	}

	/* 悬浮添加按钮样式 */
	.float-add-btn {
		position: fixed;
		right: 20px;
		bottom: 80px;
		/* 留出回到顶部按钮的位置 */
		width: 50px;
		height: 50px;
		border-radius: 50%;
		background-color: #1677ff;
		color: #fff;
		display: flex;
		align-items: center;
		justify-content: center;
		box-shadow: 0 4px 10px rgba(22, 119, 255, 0.3);
		z-index: 999;
		padding: 0;
	}

	.float-add-btn::after {
		border: none;
	}

	/* 回到顶部按钮样式 */
	.back-to-top {
		position: fixed;
		right: 20px;
		bottom: 20px;
		width: 44px;
		height: 44px;
		border-radius: 50%;
		background-color: #666;
		color: #fff;
		display: flex;
		align-items: center;
		justify-content: center;
		box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
		z-index: 999;
		padding: 0;
		opacity: 0;
		transform: translateY(20px);
		animation: fadeInUp 0.3s ease forwards;
	}

	.back-to-top::after {
		border: none;
	}

	@keyframes fadeInUp {
		from {
			opacity: 0;
			transform: translateY(20px);
		}

		to {
			opacity: 1;
			transform: translateY(0);
		}
	}
</style>