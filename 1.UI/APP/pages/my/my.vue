<template>
	<view class="content">
		<view class="topBox">
			<view class="image-text-container">
				<!-- 左侧圆形图片 -->
				<view class="image-wrapper">
					<image class="round-image" :src="imageUrl" mode="aspectFill" alt="展示图片"></image>
				</view>

				<!-- 右侧文字区域 -->
				<view class="text-container">
					<text class="text-top">{{ username }}</text>
					<text class="text-bottom">{{ phone }}</text>
				</view>
			</view>
		</view>
		<view class="inputBox">
			<view class="setting-list">
				<!-- 修改密码项 -->
				<view class="list-item" @click="handleModifyPassword">
					<!-- 左侧图标 -->
					<view class="item-icon">
						<uni-icons type="locked" size="24" color="#333"></uni-icons>
					</view>

					<!-- 中间文字 -->
					<view class="item-text">
						<text class="text-title">修改密码</text>
					</view>

					<!-- 右侧箭头 -->
					<view class="item-arrow">
						<uni-icons type="right" size="18" color="#999"></uni-icons>
					</view>
				</view>

				<!-- 分割线 -->
				<view class="divider"></view>

				<!-- 退出登录项 -->
				<view class="list-item" @click="logout">
					<!-- 左侧图标 -->
					<view class="item-icon">
						<uni-icons type="redo" size="24" color="#F53F3F"></uni-icons>
					</view>

					<!-- 中间文字 -->
					<view class="item-text">
						<text class="text-title">退出登录</text>
					</view>

					<!-- 右侧箭头 -->
					<view class="item-arrow">
						<uni-icons type="right" size="18" color="#999"></uni-icons>
					</view>
				</view>
			</view>
		</view>
	</view>
</template>

<script>
	export default {
		data() {
			return {
				imageUrl: '/static/image/5-002.png',
				username: 'hello',
				phone: '13467644655'
			}
		},
		onLoad() {
			const userInfo = uni.getStorageSync('userInfo');
			this.username = userInfo.realname;
			this.phone = userInfo.phone;
		},
		methods: {
			// 返回上一页
			navigateBack() {
				uni.navigateBack({
					delta: 1
				});
			},
			handleModifyPassword(){
				uni.navigateTo({
					url:"/pages/my/password"
				})
			},
			
			logout() {
				console.log('用户点击确定');
				uni.showModal({
					title: '提示',
					content: '确定退出吗？',
					cancelText: '取消', // 取消按钮文字，默认"取消"
					confirmText: '确定', // 确认按钮文字，默认"确定"
					success: (res) => {
						if (res.confirm) {
							// 用户点击了确认按钮
							console.log('用户点击确定');
							// 执行你的确认逻辑
							// 清除本地存储
							uni.removeStorageSync('isLogin')

							setTimeout(() => {
								uni.reLaunch({
									url: '../../pages/Login/Login'
								});
							}, 500);
						} else if (res.cancel) {
							// 用户点击了取消按钮
							console.log('用户点击取消');
						}
					}
				});
			}


		}
	}
</script>

<style scoped>
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
		height: 50vh;
		background-color: aquamarine;
		background: url("../../static/image/bg_366x650.jpg") no-repeat;
		background-size: cover;
	}

	.topBox {
		font-size: 34rpx;
		color: #fff;
		padding: 80rpx 50rpx;
	}

	h3 {
		margin-bottom: 10rpx;
	}

	.inputBox {
		position: fixed;

		left: 0;
		width: 750rpx;
		height: 85vh;
		background-color: #fff;
		border-top-left-radius: 40rpx;
		border-top-right-radius: 40rpx;
		padding: 60rpx;
		box-sizing: border-box;
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

	.list-item {
	  display: flex;
	  align-items: center;
	  padding: 24rpx 30rpx;
	  height: 100rpx;
	  box-sizing: border-box;
	}
	
	.item-icon {
	  width: 60rpx;
	  display: flex;
	  justify-content: center;
	}
	
	.item-text {
	  flex: 1;
	  margin-left: 20rpx;
	}
	
	.text-title {
	  font-size: 32rpx;
	  color: #333333;
	}
	
	.item-arrow {
	  width: 60rpx;
	  display: flex;
	  justify-content: center;
	}
	
	.divider {
	  height: 1rpx;
	  background-color: #EEEEEE;
	  margin: 0 30rpx;
	}
</style>