<template>
	<view class="contact-edit-page">
		<!-- 顶部导航栏 -->
		<!-- <view class="navbar">
			<text class="navbar-title">修改密码</text>
		</view> -->


		<!-- 表单内容区域 -->
		<view class="form-container">

			<!-- 表单字段 -->
			<view class="form-fields">
				<!-- 姓名 -->
				<view class="form-item">

					<view class="form-label">
						<text class="required">*</text>
						<text>新密码</text>
					</view>
					<view class="form-input">
						<input type="password" v-model="password" placeholder="请输入新密码" @focus="onInputFocus"
							@blur="onInputBlur" @input="checkPasswordStrength" class="rounded-input"></input>
					</view>
				</view>
				<!-- 密码强度指示器 -->
				<view class="strength-indicator">
					<!-- 强度条 -->
					<view class="strength-bars">
						<view class="strength-bar"
							:class="{'weak': strength >= 1, 'medium': strength >= 2, 'strong': strength >= 3}"></view>
						<view class="strength-bar" :class="{'medium': strength >= 2, 'strong': strength >= 3}"></view>
						<view class="strength-bar" :class="{'strong': strength >= 3}"></view>
					</view>

					<!-- 强度文本和提示 -->
					<view class="strength-info">
						<text class="strength-text" :class="strengthClass">
							{{ strengthText }}
						</text>
						<text class="requirements-text">{{ requirementsText }}</text>
					</view>
				</view>

				<!-- 密码要求说明 -->
				<view class="requirements-list" v-if="password.length > 0">
					<view class="requirement-item" :class="{ met: hasMinLength }">
						<text class="icon">{{ hasMinLength ? '⚪' : '×' }}</text>
						<text>至少6个字符</text>
					</view>
					<view class="requirement-item" :class="{ met: hasLetters }">
						<text class="icon">{{ hasLetters ? '⚪' : '×' }}</text>
						<text>包含字母（a-z, A-Z）</text>
					</view>
					<view class="requirement-item" :class="{ met: hasNumbers }">
						<text class="icon">{{ hasNumbers ? '⚪' : '×' }}</text>
						<text>包含数字（0-9）</text>
					</view>
					<view class="requirement-item" :class="{ met: hasSymbols }">
						<text class="icon">{{ hasSymbols ? '⚪' : '×' }}</text>
						<text>包含符号（如 !@#$% 等）</text>
					</view>
				</view>
				<view class="form-item">

					<view class="form-label">
						<text class="required">*</text>
						<text>确认密码</text>
					</view>
					<view class="form-input">
						<input type="password" v-model="checkpass" placeholder="请输入确认密码" @focus="onInputFocus"
							@blur="onInputBlur" class="rounded-input"></input>
					</view>
				</view>
			</view>

			<!-- 保存按钮 -->
			<view class="save-btn-container">
				<button class="custom-save-btn" @click="handleSave">
					保存
				</button>
			</view>
		</view>
	</view>
</template>

<script>
	export default {
		data() {
			return {
				password: '',
				checkpass: '',
				strength: 0, // 0-3，0表示未输入，1弱，2中，3强
				hasMinLength: false, // 是否满足最小长度
				hasLetters: false, // 是否包含字母
				hasNumbers: false, // 是否包含数字
				hasSymbols: false // 是否包含符号
			};
		},
		computed: {
			// 强度文本
			strengthText() {
				switch (this.strength) {
					case 0:
						return '';
					case 1:
						return '弱';
					case 2:
						return '中';
					case 3:
						return '强';
					default:
						return '';
				}
			},
			// 强度文本样式
			strengthClass() {
				switch (this.strength) {
					case 1:
						return 'weak-text';
					case 2:
						return 'medium-text';
					case 3:
						return 'strong-text';
					default:
						return '';
				}
			},
			// 提示文本
			requirementsText() {
				if (this.strength === 3) {
					return '密码强度良好';
				} else if (this.password.length > 0) {
					return '请完善密码以提高安全性';
				}
				return '';
			}
		},
		methods: {

			// 输入框获得焦点
			onInputFocus() {
				this.hasFocus = true;
			},

			// 输入框失去焦点
			onInputBlur() {
				this.hasFocus = false;
			},
			// 检测密码强度
			checkPasswordStrength() {
				// 重置状态
				this.hasMinLength = false;
				this.hasLetters = false;
				this.hasNumbers = false;
				this.hasSymbols = false;

				// 检测长度
				if (this.password.length >= 6) {
					this.hasMinLength = true;
				}

				// 检测字母
				if (/[a-zA-Z]/.test(this.password)) {
					this.hasLetters = true;
				}

				// 检测数字
				if (/[0-9]/.test(this.password)) {
					this.hasNumbers = true;
				}

				// 检测符号（非字母数字）
				if (/[^a-zA-Z0-9]/.test(this.password)) {
					this.hasSymbols = true;
				}

				// 计算强度（满足的条件数量）
				let count = 0;
				if (this.hasMinLength) count++;
				if (this.hasLetters) count++;
				if (this.hasNumbers) count++;
				if (this.hasSymbols) count++;

				// 调整强度值（0-3）
				this.strength = count;
			},
			// 保存跟进记录
			handleSave() {
				// this.hasMinLength = false;
				// this.hasLetters = false;
				// this.hasNumbers = false;
				// this.hasSymbols = false;

				if (!this.hasMinLength || !this.hasLetters || !this.hasNumbers || !this.hasSymbols) {
					uni.showToast({
						title: '密码强度不正确!',
						icon: 'none',
						duration: 2000
					});

					return;
				}

				if (this.password != this.checkpass) {
					uni.showToast({
						title: '确认密码不正确!',
						icon: 'none',
						duration: 2000
					});

					return;
				}


				//console.log(this.contactData);
				//return;

				// 模拟接口提交
				uni.showLoading({
					title: '保存中...',
					mask: true
				});





				this.$request("/api/ModifyPWD", {"pwd":this.password})
					.then(res => {
						console.log(res);

						uni.hideLoading();
						uni.showToast({
							title: '保存成功',
							icon: 'success',
							duration: 1500
						});

						setTimeout(() => {
							// uni.navigateTo({
							// 	url: '/pages/index/index' // 目标页面路径
							// });
							uni.reLaunch({
								url: '/pages/my/my'
							});

						}, 500);
					});

			}
		}
	};
</script>

<style scoped>
	.contact-edit-page {
		display: flex;
		flex-direction: column;
		min-height: 100vh;
		background-color: #F5F5F7;
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

	/* 表单容器 */
	.form-container {
		flex: 1;
		padding: 10px;
	}



	/* 表单字段区域 */
	.form-fields {
		background-color: #FFFFFF;
		border-radius: 12px;
		overflow: hidden;
		padding-top: 10px;
	}

	/* 表单项样式 */
	.form-item {
		display: flex;
		align-items: center;
		padding: 0 16px;
		height: 54px;

		transition: background-color 0.2s;
	}

	/* 获得焦点时的背景色变化 */
	.form-item:active {
		background-color: #F5F5F5;
	}

	/* 文本域项样式 */
	.form-item-textarea {
		height: auto;
		padding: 12px 16px;
		align-items: flex-start;
		padding-top: 16px;
	}

	/* 标签样式 */
	.form-label {
		width: 80px;
		font-size: 16px;
		color: #1D1D1F;
		flex-shrink: 0;
		padding-top: 2px;
	}

	/* 必填项星号 */
	.required {
		color: #FF3B30;
		margin-right: 4px;
	}

	/* 输入框区域 */
	.form-input {
		flex: 1;
	}

	/* 圆角输入框样式 */
	.rounded-input {
		width: 100%;
		font-size: 16px;
		color: #1D1D1F;

		height: 44px;
		padding: 6px 12px;
		background-color: #f9fafb;
		border-radius: 8px;
		border: 1px solid #c7d2fe;
		box-sizing: border-box;
	}

	/* 圆角文本域样式 */
	.rounded-textarea {
		width: 100%;
		font-size: 16px;
		color: #1D1D1F;
		padding: 12px;
		background-color: #F5F5F7;
		border-radius: 8px;
		border: 1px solid #ddd;
		box-sizing: border-box;
		resize: none;
		line-height: 1.5;
		min-height: 80px;
	}

	/* 输入框占位符样式 */
	.rounded-input::placeholder,
	.rounded-textarea::placeholder {
		color: #8E8E93;
	}

	/* 输入框聚焦样式 */
	.rounded-input:focus,
	.rounded-textarea:focus {
		outline: none;
		box-shadow: 0 0 0 2px rgba(0, 122, 255, 0.2);
	}

	/* 保存按钮样式 - 修改为#1677ff纯色 */
	.save-btn-container {
		margin-top: 24px;
	}

	.custom-save-btn {
		width: 100%;
		height: 48px;
		background-color: #1677ff !important;
		/* 替换渐变为指定颜色 */
		color: #fff !important;
		border: none;
		border-radius: 8px;
		font-size: 16px;
		font-weight: 500;
		transition: all 0.2s;
		box-shadow: 0 2px 8px rgba(22, 119, 255, 0.3);
	}



	/* 动画效果 */
	@keyframes fadeIn {
		from {
			opacity: 0;
		}

		to {
			opacity: 1;
		}
	}

	@keyframes slideUp {
		from {
			transform: translateY(100%);
		}

		to {
			transform: translateY(0);
		}
	}

	@keyframes spin {
		from {
			transform: rotate(0deg);
		}

		to {
			transform: rotate(360deg);
		}
	}

	.strength-indicator {
		margin-bottom: 20rpx;
	}

	.strength-bars {
		display: flex;
		gap: 10rpx;
		height: 12rpx;
		margin-bottom: 10rpx;
	}

	.strength-bar {
		flex: 1;
		background-color: #eee;
		border-radius: 6rpx;
		transition: background-color 0.3s ease;
	}

	.strength-bar.weak {
		background-color: #ff4d4f;
	}

	.strength-bar.medium {
		background-color: #faad14;
	}

	.strength-bar.strong {
		background-color: #52c41a;
	}

	.strength-info {
		display: flex;
		justify-content: space-between;
		font-size: 28rpx;
	}

	.strength-text {
		font-weight: bold;
	}

	.weak-text {
		color: #ff4d4f;
	}

	.medium-text {
		color: #faad14;
	}

	.strong-text {
		color: #52c41a;
	}

	.requirements-text {
		color: #888;
	}

	.requirements-list {
		background-color: #f9f9f9;
		border-radius: 10rpx;
		padding: 20rpx;
	}

	.requirement-item {
		display: flex;
		align-items: center;
		margin-bottom: 15rpx;
		font-size: 28rpx;
		color: #666;
	}

	.requirement-item:last-child {
		margin-bottom: 0;
	}

	.requirement-item.met {
		color: #52c41a;
	}

	.icon {
		margin-right: 15rpx;
		font-size: 30rpx;
	}
</style>