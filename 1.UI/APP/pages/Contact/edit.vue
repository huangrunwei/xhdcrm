<template>
	<view class="contact-edit-page">
		<!-- 顶部导航栏 -->
		<!-- <view class="navbar">
			<text class="navbar-title">编辑联系人</text>
		</view> -->


		<!-- 表单内容区域 -->
		<view class="form-container">
			<!-- 头像上传区域，暂时用不到，先隐藏 -->
			<!-- <view class="avatar-container">
				<view class="avatar-upload">
					<image :src="contactData.avatar || defaultAvatar" mode="widthFix" class="avatar-image"></image>
					<view class="avatar-upload-mask" @click="chooseAvatar">
						<text class="iconfont icon-camera"></text>
					</view>
				</view>
			</view> -->

			<!-- 表单字段 -->
			<view class="form-fields">
				<!-- 姓名（必填） -->
				<view class="form-item">
					<view class="form-label">
						<text class="required">*</text>
						<text>客户</text>
					</view>
					<view class="form-input">
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

				<!-- 姓名 -->
				<view class="form-item">

					<view class="form-label">
						<text class="required">*</text>
						<text>姓名</text>
					</view>
					<view class="form-input">
						<input v-model="contactData.C_name" placeholder="请输入姓名" @focus="onInputFocus"
							@blur="onInputBlur" @input="onFormChange" class="rounded-input"></input>
					</view>
				</view>
				<!-- 手机号码（必填） -->
				<view class="form-item">
					<view class="form-label">
						<text class="required">*</text>
						<text>电话</text>
					</view>
					<view class="form-input">
						<input v-model="contactData.C_tel" placeholder="请输入电话号码" type="number" @focus="onInputFocus"
							@blur="onInputBlur" @input="onFormChange" class="rounded-input"></input>
					</view>
				</view>
				<!-- 手机号码（必填） -->
				<view class="form-item">
					<view class="form-label">
						<text>手机</text>
					</view>
					<view class="form-input">
						<input v-model="contactData.C_mob" placeholder="请输入手机号码" type="number" @focus="onInputFocus"
							@blur="onInputBlur" @input="onFormChange" class="rounded-input"></input>
					</view>
				</view>
				<view class="form-item">
					<view class="form-label">
						<text>微信</text>
					</view>
					<view class="form-input">
						<input v-model="contactData.C_weichat" placeholder="请输入微信" @focus="onInputFocus"
							@blur="onInputBlur" @input="onFormChange" class="rounded-input"></input>
					</view>
				</view>
				<view class="form-item">
					<view class="form-label">
						<text>邮箱</text>
					</view>
					<view class="form-input">
						<input v-model="contactData.C_email" placeholder="请输入邮箱" @focus="onInputFocus"
							@blur="onInputBlur" @input="onFormChange" class="rounded-input"></input>
					</view>
				</view>

				<view class="form-item">
					<view class="form-label">
						<text>部门</text>
					</view>
					<view class="form-input">
						<input v-model="contactData.C_department" placeholder="请输入部门" @focus="onInputFocus" @blur="onInputBlur"
							class="rounded-input"></input>
					</view>
				</view>
				<!-- 职位 -->
				<view class="form-item">
					<view class="form-label">职位</view>
					<view class="form-input">
						<input v-model="contactData.C_position" placeholder="请输入职位" @focus="onInputFocus"
							@blur="onInputBlur" class="rounded-input"></input>
					</view>
				</view>
				<view class="form-item">
					<view class="form-label">
						<text>QQ</text>
					</view>
					<view class="form-input">
						<input v-model="contactData.C_QQ" placeholder="请输入QQ" type="number" @focus="onInputFocus"
							@blur="onInputBlur" @input="onFormChange" class="rounded-input"></input>
					</view>
				</view>
				<view class="form-item">
					<view class="form-label">
						<text>生日</text>
					</view>
					<view class="form-input">
						<input v-model="contactData.C_birthday" placeholder="请输入生日" @focus="onInputFocus"
							@blur="onInputBlur" @input="onFormChange" class="rounded-input"></input>
					</view>
				</view>
				<view class="form-item">
					<view class="form-label">
						<text>爱好</text>
					</view>
					<view class="form-input">
						<input v-model="contactData.C_hobby" placeholder="请输入爱好" @focus="onInputFocus"
							@blur="onInputBlur" @input="onFormChange" class="rounded-input"></input>
					</view>
				</view>
				<!-- 地址 -->
				<view class="form-item">
					<view class="form-label">地址</view>
					<view class="form-input">
						<input v-model="contactData.C_add" placeholder="请输入地址" @focus="onInputFocus" @blur="onInputBlur"
						 @input="onFormChange"	class="rounded-input"></input>
					</view>
				</view>

				<!-- 备注 -->
				<view class="form-item form-item-textarea">
					<view class="form-label">备注</view>
					<view class="form-input">
						<textarea v-model="contactData.C_remarks" placeholder="请输入备注信息" rows="2" @focus="onInputFocus"
							@blur="onInputBlur" @input="onFormChange" class="rounded-textarea"></textarea>
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
            showCustomerModal: false,
            selectedCustomer: {},
            allCustomers: [],
            customerSearchKey: '',
            currentPage: 1,
            pageSize: 10,
            hasMoreCustomers: true,
            isLoadingCustomers: false,
            hasFocus: false,

            // ===== 返回拦截所需 =====
            hasChanged: false,
            isShowingModal: false,
        };
    },
    onLoad(options) {
        // 加载客户列表（用于选择）
        this.loadCustomers();

        if (Object.keys(options).length !== 0) {
            const receivedData = JSON.parse(decodeURIComponent(options.data));
            this.contactData = receivedData;
            this.selectedCustomer = this.contactData.customer;
        }
    },

    // ============================================================
    //  返回拦截（与 onLoad 平级）
    // ============================================================
    onBackPress(options) {
        // 1. 如果是代码主动 navigateBack，放行
        if (options.from === 'navigateBack') {
            return false;
        }
        // 2. 未修改，直接返回
        if (!this.hasChanged) {
            return false;
        }
        // 3. 防止弹窗叠加
        if (this.isShowingModal) {
            return true;
        }
        this.isShowingModal = true;
        uni.showModal({
            title: '提示',
            content: '当前有未保存的修改，确定要退出吗？',
            success: (res) => {
                if (res.confirm) {
                    this.hasChanged = false;
                    // 延时保证 onBackPress 完全执行完毕
                    setTimeout(() => {
                        uni.navigateBack({ delta: 1 });
                    }, 100);
                }
            },
            complete: () => {
                this.isShowingModal = false;
            }
        });
        return true; // 拦截默认返回
    },

    methods: {
        // ===== 修改标记 =====
        onFormChange() {
            this.hasChanged = true;
        },

        // ===== 客户列表相关 =====
        loadCustomers() {
            if (this.isLoadingCustomers || !this.hasMoreCustomers) return;
            this.isLoadingCustomers = true;

            const postdata = {
                serchtxt: this.customerSearchKey,
                page: this.currentPage,
                limit: this.pageSize,
            };

            this.$request('/api/CustomerList', postdata)
                .then((res) => {
                    const data = res.data || [];
                    this.allCustomers =
                        this.currentPage === 1
                            ? data
                            : [...this.allCustomers, ...data];
                    this.isLoadingCustomers = false;

                    const total = res.count || 0;
                    const maxPage = Math.ceil(total / this.pageSize);
                    this.currentPage++;
                    if (this.currentPage > maxPage) {
                        this.hasMoreCustomers = false;
                    }
                })
                .catch(() => {
                    this.isLoadingCustomers = false;
                });
        },

        loadMoreCustomers() {
            this.loadCustomers();
        },

        handleCustomerSearch() {
            this.currentPage = 1;
            this.allCustomers = [];
            this.hasMoreCustomers = true;
            this.loadCustomers();
        },

        clearCustomerSearch() {
            this.customerSearchKey = '';
            this.handleCustomerSearch();
        },

        selectCustomer(customer) {
            this.selectedCustomer = customer;
            this.closeCustomerModal();
            this.contactData.customer_id = customer.id;
            this.onFormChange(); // 标记已修改
        },

        closeCustomerModal() {
            this.showCustomerModal = false;
        },

        // ===== 输入焦点 =====
        onInputFocus() {
            this.hasFocus = true;
        },
        onInputBlur() {
            this.hasFocus = false;
        },

        // ===== 保存 =====
        handleSave() {
            const contact = this.contactData;
            const postdata = {
                id: contact.id,
                C_add: contact.C_add,
                C_birthday: contact.C_birthday,
                C_department: contact.C_department,
                C_email: contact.C_email,
                C_weichat: contact.C_weichat,
                C_hobby: contact.C_hobby,
                C_mob: contact.C_mob,
                C_name: contact.C_name,
                C_position: contact.C_position,
                C_QQ: contact.C_QQ,
                C_remarks: contact.C_remarks,
                C_sex: contact.C_sex,
                C_tel: contact.C_tel,
                customer_id: contact.customer_id,
            };

            // 必填校验
            if (!postdata.customer_id) {
                uni.showToast({ title: '请选择客户!', icon: 'none', duration: 2000 });
                return;
            }
            if (!postdata.C_name || postdata.C_name.trim() === '') {
                uni.showToast({ title: '请输入姓名!', icon: 'none', duration: 2000 });
                return;
            }
            if (!postdata.C_tel || postdata.C_tel.trim() === '') {
                uni.showToast({ title: '请输入电话!', icon: 'none', duration: 2000 });
                return;
            }

            uni.showLoading({ title: '保存中...', mask: true });

            this.$request('/api/ContactSave', postdata, 'POST')
                .then((res) => {
                    uni.hideLoading();
                    uni.showToast({ title: '保存成功', icon: 'success', duration: 1500 });
                    this.hasChanged = false; // 重置标记
					
					// ★★★ 获取页面栈，更新前两个页面 ★★★
					const pages = getCurrentPages();
					const detailPage = pages[pages.length - 2]; // 详情页
					const listPage = pages[pages.length - 3];   // 列表页
									
					// 1. 更新详情页
					if (detailPage && detailPage.$vm && typeof detailPage.$vm.updateData === 'function') {
						if(contact.id)
						{
							detailPage.$vm.updateData(contact);
						}
						else
						{
							detailPage.$vm.loadList();
						}
					}
									
					// 2. ★★★ 直接更新列表页的那一条数据 ★★★
					if (listPage && listPage.$vm && typeof listPage.$vm.updateData === 'function') {
						listPage.$vm.updateData(contact);
					}
					
                    setTimeout(() => {
                        uni.navigateBack({ delta: 1 }); // 返回上一页（列表页）
                    }, 500);
					
                })
                .catch(() => {
                    uni.hideLoading();
                    uni.showToast({ title: '保存失败', icon: 'none', duration: 2000 });
                });
        },
    },
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

	/* 头像上传区域 */
	.avatar-container {
		display: flex;
		justify-content: center;
		padding: 20px 0;
		background-color: #FFFFFF;
	}

	.avatar-upload {
		position: relative;
		width: 100px;
		height: 100px;
	}

	.avatar-image {
		width: 100%;
		height: 100%;
		border-radius: 50%;
		object-fit: cover;
		border: 1px solid #EEEEEE;
	}

	.avatar-upload-mask {
		position: absolute;
		bottom: 0;
		right: 0;
		width: 30px;
		height: 30px;
		background-color: rgba(0, 0, 0, 0.3);
		border-radius: 50%;
		display: flex;
		align-items: center;
		justify-content: center;
		color: #FFFFFF;
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