<template>
	<view class="content">
		<view class="topBox">
	
			<h3>欢迎使用小黄豆CRM</h3>
		</view>

		<view class="inputBox">
			<view class="ipt">
				<h4>账号</h4>
				<input type="text" value="" v-model="username" placeholder="请输入账号" />
			</view>
			<view class="ipt">
				<h4>密码</h4>
				<input type="password" value="" v-model="password" placeholder="请输入密码" />
			</view>
			<button class="loginBtn" @click="handleLogin">登录</button>

			<view class="tipbox">
				<view class="txt">
					—— v3.1.0 ——
				</view>
				<!-- <view class="otherUser">
					<uni-icons type="qq" size="40" color="rgb(66,157,250)"></uni-icons>
					<uni-icons type="weixin" size="40" color="rgb(2,187,17)"></uni-icons>
				</view> -->
			</view>
		</view>
	</view>
</template>

<script>
	// 导入封装的请求工具
	import request from '@/utils/request';

	// 引入加密模块
	import md5 from '@/utils/md5.js';


	export default {
		data() {
			return {
				// 表单数据
				username: '',
				password: '',
				rememberMe: false,
				showPassword: false,
				isLoading: false

			}
		},
		onLoad() {
			// 读取本地存储的账号密码（实际项目中密码应加密存储）
			this.loadSavedAccount();
		},
		methods: {
			// 返回上一页
			navigateBack() {
				uni.navigateBack({
					delta: 1
				});
			},

			// 读取保存的账号密码
			loadSavedAccount() {
				const userInfo = uni.getStorageSync('userInfo');
				if (userInfo) {
					this.username = userInfo.username;
					this.password = userInfo.password;
				}
			},

			// 处理登录
			handleLogin() {
				if (!this.username.trim()) {
					uni.showToast({
						title: '请输入账号!',
						icon: 'none',
						duration: 2000
					});
					return;
				}

				if (!this.password) {
					uni.showToast({
						title: '请输入密码！',
						icon: 'none',
						duration: 2000
					});
					return;
				} else if (this.password.length < 6) {
					uni.showToast({
						title: '密码长度不能小于6位!',
						icon: 'none',
						duration: 2000
					});
					return;
				}

				uni.showToast({
					title: '登录中...',
					icon: 'loading',
					mask: true, // 遮罩层，防止用户操作
					duration: 20000
				});

				// 登录请求	
				var postdata = {
					"uid": this.username,
					"pwd": md5(this.password)
				};

				this.$request("/api/login", postdata)
					.then(res => {
						console.log(res);

						// 关闭加载提示
						uni.hideToast()

						if (res.code === 0) {
							// 保存账号信息
							var data = res.data[0];

							var userinfo = {
								username: this.username,
								password: this.password,
								realname: data.RealName,
								phone: data.phone,
								outtime: data.outtime,
								token: data.token
							};

							// 登录成功，更新store状态
							uni.setStorageSync('isLogin', true)
							uni.setStorageSync('userInfo', userinfo)

							// 登录成功，跳转到首页
							uni.showToast({
								title: '登录成功',
								icon: 'success',
								duration: 1500
							});

							setTimeout(() => {
								// uni.navigateTo({
								// 	url: '/pages/index/index' // 目标页面路径
								// });
								uni.reLaunch({
									url: '/pages/index/index'
								});
							}, 500);
						} else {
							uni.showToast({
								title: res.msg,
								icon: 'none',
								duration: 2000
							});
						}



					})


				return;

			},
			inputDialogToggle() {
				this.$refs.inputDialog.open()
			}
		}
	}
</script>

<style scoped>
	body {
		overflow: hidden;
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
		padding: 150rpx 50rpx;
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

	/* 右上角配置文字样式 */
	.config-text {
		font-size: 32rpx;
		color: #ddd;
		padding: 10rpx;
		cursor: pointer;
		position: absolute;
		right: 10px;
		top: 10px;
	}

	/* 弹窗样式 */
	.config-popup {
		display: flex;
		justify-content: center;
		align-items: center;
	}

	.popup-content {
		width: 600rpx;
		background-color: #ffffff;
		border-radius: 16rpx;
		padding: 40rpx 30rpx;
	}

	.popup-title {
		font-size: 36rpx;
		font-weight: bold;
		color: #333333;
		margin-bottom: 30rpx;
		text-align: center;
	}

	.input-container {
		margin-bottom: 40rpx;
	}

	.input-field {
		width: 100%;
	}

	/* 文字按钮样式 */
	.text-button-group {
		display: flex;
		justify-content: space-around;
		margin-top: 20rpx;
	}

	.text-btn {
		font-size: 32rpx;
		padding: 15rpx 30rpx;
		border-radius: 8rpx;
		cursor: pointer;
	}

	.cancel-btn {
		color: #666666;
		background-color: #f5f5f5;
	}

	.confirm-btn {
		color: #ffffff;
		background-color: #007aff;
	}
</style>