<template>
	<view class="follow-up-page">
		<!-- 顶部导航栏 -->
		<view class="navbar">
			<text class="navbar-title">编辑联系人</text>
		</view>
		<!-- 主内容区 -->
		<view class="content-container">
			<!-- 表单卡片 -->
			<view class="form-card">
				<!-- 1. 客户选择（弹窗选择器） -->
				<view class="form-item">
					<view class="item-label">
						<span class="label-text">客户</span>
						<span class="required-mark">*</span>
					</view>
					<view class="item-control">
						<view class="custom-picker" @click="showCustomerModal = true">
							<view class="picker-display">
								<span v-if="selectedCustomer" class="picker-value">
									{{ selectedCustomer.cus_name }}
								</span>
								<span v-else class="picker-placeholder">请选择客户</span>
								<i class="iconfont icon-arrow-down"></i>
							</view>
						</view>
					</view>
				</view>
				<view class="info-item">
					<text class="label">姓名：</text>
					<text class="value"><input type="text" value="" class="content-textarea" v-model="contactData.C_name"
							placeholder="请输入账号" /></text>
				</view>
				<view class="form-item form-item-textarea">
					<view class="item-label">
						<span class="label-text">姓名</span>
						<span class="required-mark">*</span>
					</view>
					<view class="item-control">
						<input type="text" value="" class="content-textarea" v-model="contactData.C_name"
							placeholder="请输入账号" />
					</view>
				</view>

				<view class="form-item form-item-textarea">
					<view class="item-label">
						<span class="label-text">电话</span>
						<span class="required-mark">*</span>
					</view>
					<view class="item-control">
						<input type="text" value="" class="content-textarea" v-model="contactData.C_name"
							placeholder="请输入账号" />
					</view>
				</view>

				<view class="form-item form-item-textarea">
					<view class="item-label">
						<span class="label-text">手机</span>
						<span class="required-mark">*</span>
					</view>
					<view class="item-control">
						<input type="text" value="" class="content-textarea" v-model="contactData.C_name"
							placeholder="请输入账号" />
					</view>
				</view>


				<view class="form-item form-item-textarea">
					<view class="item-label">
						<span class="label-text">微信</span>
						<span class="required-mark">*</span>
					</view>
					<view class="item-control">
						<input type="text" value="" class="content-textarea" v-model="contactData.C_name"
							placeholder="请输入账号" />
					</view>
				</view>
				<view class="form-item form-item-textarea">
					<view class="item-label">
						<span class="label-text">邮箱</span>
						<span class="required-mark">*</span>
					</view>
					<view class="item-control">
						<input type="text" value="" class="content-textarea" v-model="contactData.C_name"
							placeholder="请输入账号" />
					</view>
				</view>

				<view class="form-item form-item-textarea">
					<view class="item-label">
						<span class="label-text">部门</span>
						<span class="required-mark">*</span>
					</view>
					<view class="item-control">
						<input type="text" value="" class="content-textarea" v-model="contactData.C_name"
							placeholder="请输入账号" />
					</view>
				</view>

				<view class="form-item form-item-textarea">
					<view class="item-label">
						<span class="label-text">职务</span>
						<span class="required-mark">*</span>
					</view>
					<view class="item-control">
						<input type="text" value="" class="content-textarea" v-model="contactData.C_name"
							placeholder="请输入账号" />
					</view>
				</view>

				<view class="form-item form-item-textarea">
					<view class="item-label">
						<span class="label-text">QQ</span>
						<span class="required-mark">*</span>
					</view>
					<view class="item-control">
						<input type="text" value="" class="content-textarea" v-model="contactData.C_name"
							placeholder="请输入账号" />
					</view>
				</view>

				<view class="form-item form-item-textarea">
					<view class="item-label">
						<span class="label-text">生日</span>
						<span class="required-mark">*</span>
					</view>
					<view class="item-control">
						<input type="text" value="" class="content-textarea" v-model="contactData.C_name"
							placeholder="请输入账号" />
					</view>
				</view>

				<view class="form-item form-item-textarea">
					<view class="item-label">
						<span class="label-text">爱好</span>
						<span class="required-mark">*</span>
					</view>
					<view class="item-control">
						<input type="text" value="" class="content-textarea" v-model="contactData.C_name"
							placeholder="请输入账号" />
					</view>
				</view>

				<view class="form-item form-item-textarea">
					<view class="item-label">
						<span class="label-text">地址</span>
						<span class="required-mark">*</span>
					</view>
					<view class="item-control">
						<input type="text" value="" class="content-textarea" v-model="contactData.C_name"
							placeholder="请输入账号" />
					</view>
				</view>

				<view class="form-item form-item-textarea">
					<view class="item-label">
						<span class="label-text">备注</span>
						<span class="required-mark">*</span>
					</view>
					<view class="item-control">
						<input type="text" value="" class="content-textarea" v-model="contactData.C_name"
							placeholder="请输入账号" />
					</view>
				</view>
				<!-- 保存按钮 -->
				<view class="save-btn-container">
					<button class="custom-save-btn" @click="handleSave">
						保存跟进记录
					</button>
				</view>
			</view>
		</view>

		<!-- 客户选择弹窗（支持搜索和分页） -->
		<view v-if="showCustomerModal" class="customer-modal">
			<!-- 遮罩层 -->
			<view class="modal-mask" @click="closeCustomerModal"></view>

			<!-- 弹窗内容 -->
			<view class="modal-content">
				<!-- 弹窗头部 -->
				<view class="modal-header">
					<view class="header-title">选择客户</view>
					<view class="header-close" @click="closeCustomerModal">
						<i class="iconfont icon-close"></i>
					</view>
				</view>

				<!-- 搜索框 -->
				<view class="search-container">
					<view class="search-box">
						<i class="iconfont icon-search"></i>
						<input v-model="customerSearchKey" placeholder="搜索客户名称或代码" @input="handleCustomerSearch"
							@confirm="handleCustomerSearch" />
						<i class="iconfont icon-clear" v-if="customerSearchKey" @click="clearCustomerSearch"></i>
					</view>
				</view>

				<!-- 客户列表（带滚动加载） -->
				<scroll-view class="customer-list" scroll-y @scrolltolower="loadMoreCustomers">
					<view class="customer-item" v-for="(customer, index) in allCustomers" :key="customer.id"
						@click="selectCustomer(customer)"
						:class="{ 'selected': selectedCustomer && customer.id === selectedCustomer.id }">
						<view class="customer-info">
							<view class="customer-name">{{ customer.cus_name }}</view>
							<view class="customer-code"> {{ customer.cus_tel }}</view>
						</view>
						<i class="iconfont icon-check"
							v-if="selectedCustomer && customer.id === selectedCustomer.id"></i>
					</view>

					<!-- 加载状态 -->
					<view class="loading-more" v-if="isLoadingCustomers">
						<view class="spinner"></view>
						<text>加载中...</text>
					</view>

					<!-- 无数据状态 -->
					<view class="no-data" v-if="!isLoadingCustomers && allCustomers.length === 0">
						<i class="iconfont icon-empty"></i>
						<text>未找到匹配的客户</text>
					</view>

					<!-- 已加载全部 -->
					<view class="load-all" v-if="!isLoadingCustomers  && !hasMoreCustomers">
						<text>已加载全部客户</text>
					</view>
				</scroll-view>
			</view>
		</view>
	</view>
</template>

<script>
	export default {
		data() {
			return {
				contactData: {},

				// 客户选择相关
				showCustomerModal: false,
				selectedCustomer: {},
				allCustomers: [], // 所有客户数据

				customerSearchKey: '',
				currentPage: 1,
				pageSize: 10,
				hasMoreCustomers: true,
				isLoadingCustomers: false


			};
		},

		onLoad(options) {

			// 初始加载客户数据
			this.loadCustomers();

			//参数
			this.loadparams();

			console.log(options);

			if (Object.keys(options).length !== 0) {
				// 将接收的字符串解析为JSON对象
				const receivedData = JSON.parse(decodeURIComponent(options.data));
				this.contactData = receivedData;

				this.selectedCustomer = this.contactData.customer;
				this.selectedContact = this.contactData.contact;

				console.log(this.contactData);
			}

		},

		methods: {
			// 返回上一页
			handleBack() {
				uni.navigateBack({
					delta: 1
				});
			},

			// 加载客户数据（修复了数组扩展错误）
			loadCustomers() {
				if (this.isLoadingCustomers || !this.hasMoreCustomers) return;

				this.isLoadingCustomers = true;


				// 请求
				var postdata = {
					"serchtxt": this.customerSearchKey,
					"page": this.currentPage,
					"limit": this.pageSize
				};

				var newData = [];

				this.$request("/api/CustomerList", postdata)
					.then(res => {
						//console.log(res);

						newData.push(res.data);

						// 合并数据
						this.allCustomers = this.page === 1 ? res.data : [...this.allCustomers, ...res.data];

						console.log(this.allCustomers)

						this.isLoadingCustomers = false;

						var total = res.count;

						var maxpage = 1;
						if (total <= 0) {
							maxpage = 1;
						} else {
							maxpage = Math.ceil(total / this.pageSize);
						}

						this.currentPage++;

						if (this.page >= maxpage) {
							this.hasMoreCustomers = false;
						}
					});
			},

			// 加载更多客户
			loadMoreCustomers() {
				this.loadCustomers();
			},

			// 客户搜索
			handleCustomerSearch() {
				// 搜索时重置分页
				this.currentPage = 1;
				this.allCustomers = [];
				this.hasMoreCustomers = true;

				// 执行搜索
				this.loadCustomers();
			},

			// 清除搜索
			clearCustomerSearch() {
				this.customerSearchKey = '';
				this.handleCustomerSearch();
			},

			// 选择客户
			selectCustomer(customer) {
				this.selectedCustomer = customer;
				this.closeCustomerModal();

				this.contactData.customer_id = customer.id;

				console.log(this.contactData);
			},

			// 关闭客户选择弹窗
			closeCustomerModal() {
				this.showCustomerModal = false;
			},

			// 保存跟进记录
			handleSave() {
				if (!this.selectedCustomer) {
					uni.showToast({
						title: '请选择客户!',
						icon: 'none',
						duration: 2000
					});

					return;
				}

				if (!this.selectedContact) {
					uni.showToast({
						title: '请选择联系人!',
						icon: 'none',
						duration: 2000
					});

					return;
				}
				if (this.selectedTypeIndex === '') {
					uni.showToast({
						title: '请选择跟进方式!',
						icon: 'none',
						duration: 2000
					});

					return;
				}
				if (this.selectedAimIndex === '') {
					uni.showToast({
						title: '请选择跟进目的!',
						icon: 'none',
						duration: 2000
					});

					return;
				}

				if (this.contactData.follow_content.trim().length == 0) {
					uni.showToast({
						title: '请输入跟进内容!',
						icon: 'none',
						duration: 2000
					});

					return;
				}

				console.log(this.contactData);
				//return;

				// 模拟接口提交
				uni.showLoading({
					title: '保存中...',
					mask: true
				});

				const follow = this.contactData;
				const postdata = {
					id: follow.id,
					contact_id: follow.contact_id,
					customer_id: follow.customer_id,
					follow_aim_id: follow.follow_aim_id,
					follow_content: follow.follow_content,
					follow_type_id: follow.follow_type_id
				};

				this.$request("/api/ContactSave", postdata, "POST")
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
							// uni.reLaunch({
							// 	url: '/pages/follow/follow'
							// });
							uni.navigateBack({
								delta: 2
							});
						}, 500);
					});

			}
		}
	};
</script>

<style scoped>
	/* 基础样式 */
	.follow-up-page {
		min-height: 100vh;
		background-color: #f5f7fa;
		font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
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

	/* 内容容器和表单卡片 */
	.content-container {
		padding: 16px;
	}

	.form-card {
		background-color: #fff;
		border-radius: 12px;
		padding: 20px;
		box-shadow: 0 4px 16px rgba(0, 0, 0, 0.05);
	}

	/* 表单项目样式 */
	.form-item {
		display: flex;
		flex-direction: column;
		margin-bottom: 20px;
	}

	.item-label {
		display: flex;
		align-items: center;
		margin-bottom: 8px;
	}

	.label-text {
		font-size: 15px;
		color: #333;
		font-weight: 500;
	}

	.required-mark {
		color: #ff4d4f;
		margin-left: 4px;
		font-size: 16px;
	}

	.item-control {
		width: 100%;
	}

	/* 自定义选择器样式 */
	.custom-picker {
		width: 100%;
	}

	.picker-display {
		display: flex;
		align-items: center;
		justify-content: space-between;
		height: 44px;
		padding: 0 12px;
		border: 1px solid #e5e7eb;
		border-radius: 8px;
		background-color: #f9fafb;
		transition: all 0.2s;
	}

	.picker-display:hover {
		border-color: #c7d2fe;
		background-color: #f9fafb;
	}

	.picker-disabled {
		opacity: 0.7;
		cursor: not-allowed;
	}

	.picker-value {
		font-size: 15px;
		color: #333;
	}

	.picker-placeholder {
		font-size: 15px;
		color: #9ca3af;
	}

	/* 多行输入框样式 */
	.form-item-textarea {
		margin-bottom: 16px;
	}

	.content-textarea {
		width: auto;
		padding: 12px;
		border: 1px solid #e5e7eb;
		border-radius: 8px;
		font-size: 15px;
		color: #333;
		resize: none;
		background-color: #f9fafb;
		transition: all 0.2s;
	}

	.content-textarea:focus {
		outline: none;
		border-color: #1677ff;
		/* 聚焦边框使用指定颜色 */
		box-shadow: 0 0 0 2px rgba(22, 119, 255, 0.2);
	}

	.content-count {
		display: flex;
		justify-content: flex-end;
		margin-top: 6px;
		font-size: 12px;
		color: #9ca3af;
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


	/* 客户选择弹窗样式 */
	.customer-modal {
		position: fixed;
		top: 0;
		left: 0;
		right: 0;
		bottom: 0;
		z-index: 999;
	}

	.modal-mask {
		position: absolute;
		top: 0;
		left: 0;
		right: 0;
		bottom: 0;
		background-color: rgba(0, 0, 0, 0.5);
		animation: fadeIn 0.3s;
	}

	.modal-content {
		position: absolute;
		left: 0;
		right: 0;
		bottom: 0;
		background-color: #fff;
		border-top-left-radius: 16px;
		border-top-right-radius: 16px;
		height: 80vh;
		animation: slideUp 0.3s;
	}

	/* 弹窗头部 */
	.modal-header {
		display: flex;
		align-items: center;
		justify-content: center;
		height: 50px;
		position: relative;
		border-bottom: 1px solid #f0f0f0;
	}

	.header-title {
		font-size: 18px;
		font-weight: 500;
		color: #333;
	}

	.header-close {
		position: absolute;
		right: 16px;
		width: 36px;
		height: 36px;
		display: flex;
		align-items: center;
		justify-content: center;
	}

	/* 搜索框样式 */
	.search-container {
		padding: 12px 16px;
		border-bottom: 1px solid #f0f0f0;
	}

	.search-box {
		display: flex;
		align-items: center;
		padding: 0 12px;
		height: 40px;
		background-color: #f5f7fa;
		border-radius: 8px;
	}

	.search-box input {
		flex: 1;
		height: 100%;
		background: transparent;
		border: none;
		outline: none;
		font-size: 14px;
		margin: 0 8px;
	}

	.search-box input::placeholder {
		color: #9ca3af;
	}

	/* 客户列表样式 */
	.customer-list {
		height: calc(80vh - 50px - 65px);
		overflow-y: auto;
	}

	.customer-item {
		display: flex;
		align-items: center;
		justify-content: space-between;
		padding: 14px 16px;
		border-bottom: 1px solid #f0f0f0;
		transition: background-color 0.2s;
	}

	.customer-item:hover {
		background-color: #f9fafb;
	}

	.customer-item.selected {
		background-color: #e6f4ff;
		/* 选中状态使用指定颜色的浅色 */
	}

	.customer-info {
		flex: 1;
	}

	.customer-name {
		font-size: 16px;
		color: #333;
		margin-bottom: 4px;
	}

	.customer-code {
		font-size: 13px;
		color: #6b7280;
	}

	/* 加载状态样式 */
	.loading-more {
		display: flex;
		align-items: center;
		justify-content: center;
		padding: 16px;
		color: #9ca3af;
		font-size: 14px;
	}

	.spinner {
		width: 18px;
		height: 18px;
		border: 2px solid #e5e7eb;
		border-top-color: #1677ff;
		/* 加载动画使用指定颜色 */
		border-radius: 50%;
		animation: spin 1s linear infinite;
		margin-right: 8px;
	}

	/* 无数据样式 */
	.no-data {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		padding: 40px 20px;
		color: #9ca3af;
		font-size: 14px;
	}

	.no-data .iconfont {
		font-size: 48px;
		margin-bottom: 16px;
		opacity: 0.5;
	}

	/* 已加载全部 */
	.load-all {
		display: flex;
		justify-content: center;
		padding: 12px;
		color: #9ca3af;
		font-size: 13px;
	}

	/* 图标样式 */
	.iconfont {
		font-family: "iconfont" !important;
		font-size: 18px;
		font-style: normal;
		-webkit-font-smoothing: antialiased;
		-moz-osx-font-smoothing: grayscale;
	}

	.icon-back {
		color: #fff;
	}

	.icon-close {
		color: #333;
	}

	.icon-arrow-down {
		color: #9ca3af;
		font-size: 16px;
	}

	.icon-search {
		color: #9ca3af;
		font-size: 16px;
	}

	.icon-clear {
		color: #9ca3af;
		font-size: 16px;
		cursor: pointer;
	}

	.icon-check {
		color: #1677ff;
		/* 勾选图标使用指定颜色 */
		font-size: 20px;
	}

	.icon-empty {
		font-size: 48px;
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
</style>