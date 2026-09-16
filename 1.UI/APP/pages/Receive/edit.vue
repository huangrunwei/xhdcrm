<template>
	<view class="contact-edit-page">
		<!-- 顶部导航栏 -->
		<!-- <view class="navbar">
			<text class="navbar-title">编辑收款</text>
		</view> -->

		<!-- 表单内容区域 -->
		<view class="form-container">
			<!-- 表单字段 -->
			<view class="form-fields">
				<view class="module-title">收款基本信息</view>
				<!-- 姓名（必填） -->
				<view class="form-item">
					<view class="form-label">
						<text class="required">*</text>
						<text>订单</text>
					</view>
					<view class="form-input">
						<view class="custom-picker" @click="showOrderModal = true">
							<view class="picker-display">
								<span v-if="selectedOrder" class="picker-value">
									{{ selectedOrder.sn }}
								</span>
								<span v-else class="picker-placeholder">请选择订单</span>
								<i class="iconfont icon-arrow-down"></i>
							</view>
						</view>
					</view>
				</view>
				<view class="form-item">
					<view class="form-label">
						<text class="required">*</text>
						<text>收款单号</text>
					</view>
					<view class="form-input">
						<input v-model="ReceiveData.Receive_num" placeholder="收款单号" type="text" @focus="onInputFocus"
							@blur="onInputBlur" @input="onFormChange"  class="rounded-input"></input>
					</view>
				</view>
				<view class="form-item">
					<view class="form-label">
						<text class="required">*</text>
						<text>收款金额</text>
					</view>
					<view class="form-input">
						<input v-model.number="ReceiveData.Receive_amount" placeholder="请输入收款金额" type="number"
							@focus="onInputFocus" @blur="onInputBlur"  @input="onFormChange" class="rounded-input"></input>
					</view>
				</view>
				<view class="form-item">
					<view class="form-label">
						<text class="required">*</text>
						<span class="label-text">支付类别</span>
					</view>
					<view class="form-input">
						<picker class="custom-picker" v-model="selectedPaytypeIndex" :range="pay_type"
							:range-key="'params_name'" @change="paytypeChange">
							<view class="picker-display">
								<span v-if="selectedPaytypeIndex !== ''" class="picker-value">
									{{ pay_type[selectedPaytypeIndex].params_name }}
								</span>
								<span v-else class="picker-placeholder">请选择</span>
								<i class="iconfont icon-arrow-down"></i>
							</view>
						</picker>
					</view>
				</view>

				<view class="form-item">
					<view class="form-label">
						<text class="required">*</text>
						<text>收款日期</text>
					</view>
					<view class="form-input">
						<uni-datetime-picker type="date" :clear-icon="false" v-model="ReceiveData.Receive_date" />
					</view>
				</view>




				<view class="form-item">
					<view class="form-label">
						<text class="required">*</text>
						<span class="label-text">收款人</span>
					</view>
					<view class="form-input">
						<picker class="custom-picker" v-model="selectedEmployeeIndex" :range="employeeList"
							:range-key="'name'" @change="selectEmployee">
							<view class="picker-display">
								<span v-if="selectedEmployeeIndex !== ''" class="picker-value">
									{{ employeeList[selectedEmployeeIndex].name }}
								</span>
								<span v-else class="picker-placeholder">请选择</span>
								<i class="iconfont icon-arrow-down"></i>
							</view>
						</picker>
					</view>
				</view>

				<!-- 备注 -->
				<view class="form-item form-item-textarea">
					<view class="form-label">备注</view>
					<view class="form-input">
						<textarea v-model="ReceiveData.Remarks" placeholder="请输入备注信息" rows="2" @focus="onInputFocus"
							@blur="onInputBlur" @input="onFormChange"  class="rounded-textarea"></textarea>
					</view>
				</view>
			</view>

			<!-- 保存按钮 -->
			<view class="save-btn-container">
				<button class="custom-save-btn" @click="handleSave">
					保存收款
				</button>
			</view>
		</view>



		<!-- 客户选择弹窗（支持搜索和分页） -->
		<view v-if="showOrderModal" class="customer-modal">
			<!-- 遮罩层 -->
			<view class="modal-mask" @click="closeOrderModal"></view>

			<!-- 弹窗内容 -->
			<view class="modal-content">
				<!-- 弹窗头部 -->
				<view class="modal-header">
					<view class="header-title">选择客户</view>
					<view class="header-close" @click="closeOrderModal">
						<i class="iconfont icon-close"></i>
					</view>
				</view>

				<!-- 搜索框 -->
				<view class="search-container">
					<view class="search-box">
						<i class="iconfont icon-search"></i>
						<input v-model="orderSearchKey" placeholder="搜索客户名称或代码" @input="handleCustomerSearch"
							@confirm="handleCustomerSearch" />
						<i class="iconfont icon-clear" v-if="orderSearchKey" @click="clearOrderSearch"></i>
					</view>
				</view>

				<!-- 客户列表（带滚动加载） -->
				<scroll-view class="customer-list" scroll-y @scrolltolower="loadMoreOrders">
					<view class="customer-item" v-for="(order, index) in allOrders" :key="order.id"
						@click="selectOrder(order)"
						:class="{ 'selected': selectedOrder && order.id === selectedOrder.id }">
						<view class="customer-info">
							<view class="customer-name">{{ order.customer && order.customer.cus_name }}</view>
							<view class="customer-code"> {{ order.Serialnumber }}</view>
							<view class="customer-code"> {{ order.Order_amount | formatPrice }}</view>
						</view>
						<i class="iconfont icon-check" v-if="selectedOrder && order.id === selectedOrder.id"></i>
					</view>

					<!-- 加载状态 -->
					<view class="loading-more" v-if="isLoadingOrders">
						<view class="spinner"></view>
						<text>加载中...</text>
					</view>

					<!-- 无数据状态 -->
					<view class="no-data" v-if="!isLoadingOrders && allOrders.length === 0">
						<i class="iconfont icon-empty"></i>
						<text>未找到匹配的客户</text>
					</view>

					<!-- 已加载全部 -->
					<view class="load-all" v-if="!isLoadingOrders  && !hasMoreOrders">
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
            ReceiveData: {},

            // 订单选择相关
            showOrderModal: false,
            selectedOrder: {},
            allOrders: [],
            orderSearchKey: '',
            currentPage: 1,
            pageSize: 10,
            hasMoreOrders: true,
            isLoadingOrders: false,

            // 表单是否有焦点
            hasFocus: false,

            // 员工选择
            selectedEmployeeIndex: '',
            employeeList: [],

            // 支付方式
            selectedPaytypeIndex: '',
            pay_type: [],

            // ===== 返回拦截所需 =====
            hasChanged: false,
            isShowingModal: false,
        };
    },
    filters: {
        formatPrice(price) {
            if (price) {
                return '¥' + (price).toFixed(2);
            }
        }
    },
    onLoad(options) {
        console.log(options);

        if (Object.keys(options).length !== 0) {
            const receivedData = JSON.parse(decodeURIComponent(options.data));
            receivedData.Receive_date = this.formatDate(receivedData.Receive_date);
            this.ReceiveData = receivedData;
            this.selectedOrder = this.ReceiveData.Order;
            console.log(this.ReceiveData);
        }

        // 初始加载订单数据
        this.loadOrders();
        // 员工
        this.loademployee();
        // 参数
        this.loadparams();
    },

    // ============================================================
    //  返回拦截（与 onLoad 平级）
    // ============================================================
    onBackPress(options) {
        if (options.from === 'navigateBack') {
            return false;
        }
        if (!this.hasChanged) {
            return false;
        }
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
                    setTimeout(() => {
                        uni.navigateBack({ delta: 1 });
                    }, 100);
                }
            },
            complete: () => {
                this.isShowingModal = false;
            }
        });
        return true;
    },

    methods: {
        // ===== 修改标记 =====
        onFormChange() {
            this.hasChanged = true;
        },

        // ===== 加载订单数据（修复分页变量） =====
        loadOrders() {
            if (this.isLoadingOrders || !this.hasMoreOrders) return;
            this.isLoadingOrders = true;

            const postdata = {
                serchtxt: this.orderSearchKey,
                page: this.currentPage,
                limit: this.pageSize,
            };

            this.$request('/api/OrderList', postdata)
                .then((res) => {
                    const data = res.data || [];
                    this.allOrders =
                        this.currentPage === 1
                            ? data
                            : [...this.allOrders, ...data];
                    this.isLoadingOrders = false;

                    const total = res.count || 0;
                    const maxPage = Math.ceil(total / this.pageSize);
                    this.currentPage++;
                    if (this.currentPage > maxPage) {
                        this.hasMoreOrders = false;
                    }
                })
                .catch(() => {
                    this.isLoadingOrders = false;
                });
        },

        loademployee() {
            this.$request('/api/EmployeeList')
                .then((res) => {
                    this.employeeList = res.data;
                    if (Object.keys(this.ReceiveData).length !== 0) {
                        const Payee_id = this.ReceiveData.Payee_id;
                        this.selectedEmployeeIndex = this.employeeList.findIndex(
                            (item) => item.id === Payee_id
                        );
                    }
                });
        },

        // ===== 加载参数（支付方式） =====
        loadparams() {
            this.$request('/api/paramsCombo', { type: 'pay_type' })
                .then((res) => {
                    this.pay_type = res.data;
                    if (Object.keys(this.ReceiveData).length !== 0) {
                        const Pay_type_id = this.ReceiveData.Pay_type_id;
                        this.selectedPaytypeIndex = this.pay_type.findIndex(
                            (item) => item.id === Pay_type_id
                        );
                    }
                });
        },

        // ===== 订单列表相关 =====
        loadMoreOrders() {
            this.loadOrders();
        },

        handleCustomerSearch() {
            this.currentPage = 1;
            this.allOrders = [];
            this.hasMoreOrders = true;
            this.loadOrders();
        },

        clearOrderSearch() {
            this.orderSearchKey = '';
            this.handleCustomerSearch();
        },

        selectOrder(order) {
            this.selectedOrder = order;
            this.closeOrderModal();
            this.ReceiveData.order_id = order.id;
            this.onFormChange(); // 标记修改
        },

        closeOrderModal() {
            this.showOrderModal = false;
        },

        // ===== picker 选择事件 =====
        selectEmployee(e) {
            this.selectedEmployeeIndex = e.detail.value;
            this.ReceiveData.Payee_id = this.employeeList[this.selectedEmployeeIndex].id;
            this.onFormChange();
        },

        paytypeChange(e) {
            this.selectedPaytypeIndex = e.detail.value;
            this.ReceiveData.Pay_type_id = this.pay_type[this.selectedPaytypeIndex].id;
            this.onFormChange();
        },

        // ===== 日期绑定 =====
        bindDateChange(e) {
            this.ReceiveData.Receive_date = e.detail.value;
            this.onFormChange();
        },

        // ===== 日期格式化 =====
        formatDate(value) {
            if (!value) return '';
            const date = new Date(value);
            const year = date.getFullYear();
            const month = (date.getMonth() + 1).toString().padStart(2, '0');
            const day = date.getDate().toString().padStart(2, '0');
            return `${year}-${month}-${day}`;
        },

        // ===== 输入框焦点 =====
        onInputFocus() {
            this.hasFocus = true;
        },

        onInputBlur() {
            this.hasFocus = false;
        },

        // ===== 保存收款 =====
        handleSave() {
            const receive = this.ReceiveData;

            const postdata = {
                id: receive.id,
                Pay_type_id: receive.Pay_type_id,
                Payee_id: receive.Payee_id,
                order_id: receive.order_id,
                Receive_amount: receive.Receive_amount,
                Receive_date: this.formatDate(receive.Receive_date),
                Receive_num: receive.Receive_num,
                Remarks: receive.Remarks,
            };

            console.log(postdata);

            // 必填校验
            if (!postdata.order_id) {
                uni.showToast({ title: '请选择订单!', icon: 'none', duration: 2000 });
                return;
            }
            if (!postdata.Receive_num || postdata.Receive_num.trim() === '') {
                uni.showToast({ title: '请输入收款单号!', icon: 'none', duration: 2000 });
                return;
            }
            if (typeof postdata.Receive_amount !== 'number') {
                uni.showToast({ title: '请输入收款金额!', icon: 'none', duration: 2000 });
                return;
            }
            if (!postdata.Pay_type_id) {
                uni.showToast({ title: '请选择支付方式!', icon: 'none', duration: 2000 });
                return;
            }
            if (!postdata.Receive_date) {
                uni.showToast({ title: '请选择收款日期!', icon: 'none', duration: 2000 });
                return;
            }
            if (!postdata.Payee_id) {
                uni.showToast({ title: '请选择收款人!', icon: 'none', duration: 2000 });
                return;
            }

            uni.showLoading({ title: '保存中...', mask: true });

            this.$request('/api/ReceiveSave', postdata, 'POST')
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
						if(receive.id)
						{
							detailPage.$vm.updateData(receive);
						}
						else
						{
							detailPage.$vm.loadList();
						}
					}
									
					// 2. ★★★ 直接更新列表页的那一条数据 ★★★
					if (listPage && listPage.$vm && typeof listPage.$vm.updateData === 'function') {
						listPage.$vm.updateData(receive);
					}
					
                    setTimeout(() => {
                        uni.navigateBack({ delta: 1 }); // 返回列表页
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
		margin-bottom: 10px;
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
		padding: 10px;
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

	.module-title {
		font-size: 16px;
		font-weight: bold;
		color: #333;
		margin-bottom: 16px;
		padding: 10px 20px;
		border-bottom: 1px solid #ddd;
	}

	.module-title-product {
		font-size: 16px;
		font-weight: bold;
		color: #333;
		padding: 10px 20px;
		border-bottom: 1px solid #ddd;
		display: flex;
		align-items: center;
	}

	.module-label {
		width: 100px;
	}

	.module-input {
		flex: 1;
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

	.product-items {
		margin-top: 16px;
	}

	.empty-tip {
		text-align: center;
		padding: 20px;
		color: #999;
		font-size: 14px;
		background-color: #f9f9f9;
		border-radius: 8px;
	}

	.product-item {
		display: flex;
		align-items: center;
		padding: 12px 0;
		border-bottom: 1px solid #eee;
		width: 100%;
		box-sizing: border-box;
	}

	.product-item:last-child {
		border-bottom: none;
	}

	.product-image {
		width: 60px;
		height: 60px;
		border-radius: 8px;
		overflow: hidden;
		margin-right: 12px;
		flex-shrink: 0;
		border: 1px solid #ddd;
		margin-left: 5px;
	}

	.product-image image {
		width: 100%;
		height: 100%;
	}

	.product-info {
		flex: 1;
	}

	.product-name {
		font-size: 14px;
		color: #333;
		border-bottom: 1px solid #ddd;
		line-height: 25px;
	}

	.product-price {
		font-size: 12px;
		color: #666;
	}

	.product-quantity {
		width: 120px;
		text-align: center;
	}

	.quantity-control {
		display: flex;
		align-items: center;
		justify-content: center;
	}

	.quantity-btn {
		width: 28px;
		height: 28px;
		line-height: 28px;
		padding: 0;
		display: flex;
		align-items: center;
		justify-content: center;
		background-color: #f5f5f5;
		border: none;
		border-radius: 4px;
	}

	.quantity-control input {
		width: 40px;
		height: 28px;
		text-align: center;
		margin: 0 4px;
		border: 1px solid #ddd;
		border-radius: 4px;
	}

	.product-amount {
		width: 80px;
		text-align: right;
		font-size: 14px;
		color: #333;
		font-weight: 500;
	}

	.product-remove {
		width: 40px;
		text-align: center;
	}

	.remove-btn {
		background: transparent;
		border: none;
		padding: 0;
		margin: 0;
	}

	uni-button:after {
		border: none !important;
	}

	.action-buttons {
		display: flex;
		gap: 16px;
		margin-top: 20px;
	}

	.cancel-btn,
	.save-btn {
		flex: 1;
		height: 44px;
		border-radius: 8px;
		font-size: 16px;
	}

	.cancel-btn {
		background-color: #fff;
		color: #333;
		border: 1px solid #ddd;
	}

	.save-btn {
		background-color: #007aff;
		color: #fff;
		border: none;
	}

	.product-oper {
		flex: 1;
	}

	.product-oper-item {
		display: flex;
		align-items: center;
	}

	/* 底部弹出式产品选择器样式 */
	.picker-overlay {
		position: fixed;
		top: 0;
		left: 0;
		right: 0;
		bottom: 0;
		background-color: rgba(0, 0, 0, 0.5);
		z-index: 999;
		display: flex;
		justify-content: flex-end;
		animation: fadeIn 0.3s ease;
	}

	/* 淡入动画 */
	@keyframes fadeIn {
		from {
			opacity: 0;
		}

		to {
			opacity: 1;
		}
	}

	.picker-wrapper {
		width: 100%;
		background-color: #fff;
		border-top-left-radius: 16px;
		border-top-right-radius: 16px;
		max-height: 80vh;
		display: flex;
		flex-direction: column;
		transform: translateY(0);
		animation: slideUp 0.3s ease;
	}

	/* 上滑动画 */
	@keyframes slideUp {
		from {
			transform: translateY(100%);
		}

		to {
			transform: translateY(0);
		}
	}

	.picker-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 12px 16px;
		border-bottom: 1px solid #eee;
	}

	.picker-cancel {
		font-size: 16px;
		padding: 8px 16px;
		background: transparent;
		border: none;
		color: #666;
	}

	.picker-title {
		font-size: 18px;
		font-weight: 500;
		color: #333;
	}

	.picker-empty {
		width: 60px;
		/* 与取消按钮宽度一致，保持标题居中 */
	}

	.picker-search {
		display: flex;
		align-items: center;
		padding: 12px 16px;
		border-bottom: 1px solid #eee;
	}

	.search-icon {
		color: #999;
		margin-right: 8px;
	}

	.picker-search input {
		flex: 1;
		height: 36px;
		background-color: #f5f5f5;
		border-radius: 18px;
		padding: 0 16px;
		font-size: 14px;
		border: none;
		box-sizing: border-box;
	}

	.picker-content {
		flex: 1;
		overflow: hidden;
	}

	.picker-scroll {
		height: 100%;
	}

	.picker-item {
		padding: 0 16px;
	}

	.picker-item-inner {
		display: flex;
		align-items: center;
		padding: 14px 0;
		border-bottom: 1px solid #eee;
	}

	.picker-item:last-child .picker-item-inner {
		border-bottom: none;
	}

	/* 已选中产品样式 */
	.picker-item-selected {
		background-color: #f0f7ff;
	}

	.picker-item-image {
		width: 50px;
		height: 50px;
		border-radius: 8px;
		overflow: hidden;
		margin-right: 12px;
		flex-shrink: 0;
	}

	.picker-item-image image {
		width: 100%;
		height: 100%;
	}

	.picker-item-info {
		flex: 1;
	}

	.picker-item-name {
		font-size: 15px;
		color: #333;
		margin-bottom: 4px;
	}

	.picker-item-price {
		font-size: 13px;
		color: #007aff;
	}

	.add-product-btn {
		background-color: #007aff;
		color: #fff;
		border-radius: 20px;
		font-size: 14px;
		display: flex;
		align-items: center;
		justify-content: center;
		gap: 6px;
	}

	.uni-date-editor--x {
		height: 44px !important;
	}
</style>