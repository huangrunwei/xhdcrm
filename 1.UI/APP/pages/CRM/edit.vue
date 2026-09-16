<template>
	<view class="contact-edit-page">
		<!-- 顶部导航栏 -->
		<!-- <view class="navbar">
			<text class="navbar-title">编辑客户</text>
		</view> -->

		<!-- 表单内容区域 -->
		<view class="form-container">
			<!-- 表单字段 -->
			<view class="form-fields">
				<view class="module-title">客户信息</view>

				<view class="form-item">
					<view class="form-label">
						<text class="required">*</text>
						<text>客户名</text>
					</view>
					<view class="form-input">
						<input v-model="CustomerData.cus_name" placeholder="客户名" type="text" @focus="onInputFocus"
							@blur="onInputBlur" @input="onFormChange" class="rounded-input"></input>
					</view>
				</view>
				<view class="form-item">
					<view class="form-label">
						<text class="required">*</text>
						<text>电话</text>
					</view>
					<view class="form-input">
						<input v-model="CustomerData.cus_tel" placeholder="电话" type="text" @focus="onInputFocus"
							@blur="onInputBlur" @input="onFormChange" class="rounded-input"></input>
					</view>
				</view>
				<view class="form-item">
					<view class="form-label">
						<text>传真</text>
					</view>
					<view class="form-input">
						<input v-model="CustomerData.cus_fax" placeholder="传真" type="text" @focus="onInputFocus"
							@blur="onInputBlur" @input="onFormChange" class="rounded-input"></input>
					</view>
				</view>
				<view class="form-item">
					<view class="form-label">
						<text>网站</text>
					</view>
					<view class="form-input">
						<input v-model="CustomerData.cus_website" placeholder="网站" type="text" @focus="onInputFocus"
							@blur="onInputBlur" @input="onFormChange" class="rounded-input"></input>
					</view>
				</view>

				<view class="form-item">
					<view class="form-label">
						<span class="label-text">行业</span>
					</view>
					<view class="form-input">
						<picker class="custom-picker" v-model="selectedIndustryIndex" :range="IndustryData"
							:range-key="'params_name'" @change="industryChange">
							<view class="picker-display">
								<span v-if="selectedIndustryIndex !== ''" class="picker-value">
									{{ IndustryData[selectedIndustryIndex] && IndustryData[selectedIndustryIndex].params_name }}
								</span>
								<span v-else class="picker-placeholder">请选择</span>
								<i class="iconfont icon-arrow-down"></i>
							</view>
						</picker>
					</view>
				</view>
				<view class="form-item">
					<view class="form-label">
						<span class="label-text">类型</span>
					</view>
					<view class="form-input">
						<picker class="custom-picker" v-model="selectedCustypeIndex" :range="CustypeData"
							:range-key="'params_name'" @change="custypeChange">
							<view class="picker-display">
								<span v-if="selectedCustypeIndex !== ''" class="picker-value">
									{{ CustypeData[selectedCustypeIndex] && CustypeData[selectedCustypeIndex].params_name }}
								</span>
								<span v-else class="picker-placeholder">请选择</span>
								<i class="iconfont icon-arrow-down"></i>
							</view>
						</picker>
					</view>
				</view>
				<view class="form-item">
					<view class="form-label">
						<span class="label-text">级别</span>
					</view>
					<view class="form-input">
						<picker class="custom-picker" v-model="selectedCuslevelIndex" :range="CusLevelData"
							:range-key="'params_name'" @change="CusLevelChange">
							<view class="picker-display">
								<span v-if="selectedCuslevelIndex !== ''" class="picker-value">
									{{ CusLevelData[selectedCuslevelIndex] && CusLevelData[selectedCuslevelIndex].params_name }}
								</span>
								<span v-else class="picker-placeholder">请选择</span>
								<i class="iconfont icon-arrow-down"></i>
							</view>
						</picker>
					</view>
				</view>

				<view class="form-item">
					<view class="form-label">
						<span class="label-text">来源</span>
					</view>
					<view class="form-input">
						<picker class="custom-picker" v-model="selectedCussourceIndex" :range="CusSourceData"
							:range-key="'params_name'" @change="CusSourceChange">
							<view class="picker-display">
								<span v-if="selectedCussourceIndex !== ''" class="picker-value">
									{{ CusSourceData[selectedCussourceIndex] && CusSourceData[selectedCussourceIndex].params_name }}
								</span>
								<span v-else class="picker-placeholder">请选择</span>
								<i class="iconfont icon-arrow-down"></i>
							</view>
						</picker>
					</view>
				</view>

				<!-- <view class="form-item">
					<view class="form-label">
						<span class="label-text">省份</span>
					</view>
					<view class="form-input">
						<picker class="custom-picker" v-model="selectedProvincesIndex" :range="pay_type"
							:range-key="'params_name'" @change="paytypeChange">
							<view class="picker-display">
								<span v-if="selectedProvincesIndex !== ''" class="picker-value">
									{{ pay_type[selectedProvincesIndex].params_name }}
								</span>
								<span v-else class="picker-placeholder">请选择</span>
								<i class="iconfont icon-arrow-down"></i>
							</view>
						</picker>
					</view>
				</view> -->

				<!-- <view class="form-item">
					<view class="form-label">
						<span class="label-text">城市</span>
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
				</view> -->

				<view class="form-item">
					<view class="form-label">
						<text class="required">*</text>
						<span class="label-text">状态</span>
					</view>
					<view class="form-input">
						<picker class="custom-picker" v-model="selectedStausIndex" :range="StatusData"
							:range-key="'text'" @change="statusChange">
							<view class="picker-display">
								<span v-if="selectedStausIndex !== ''" class="picker-value">
									{{ StatusData[selectedStausIndex].text }}
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
						<span class="label-text">归属</span>
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
					<view class="form-label">描述</view>
					<view class="form-input">
						<textarea v-model="CustomerData.DesCripe" placeholder="请输入描述信息" rows="2" @focus="onInputFocus"
							@blur="onInputBlur" @input="onFormChange" class="rounded-textarea"></textarea>
					</view>
				</view>
				<!-- 备注 -->
				<view class="form-item form-item-textarea">
					<view class="form-label">地址</view>
					<view class="form-input">
						<textarea v-model="CustomerData.cus_add" placeholder="请输入地址信息" rows="2" @focus="onInputFocus"
							@blur="onInputBlur" @input="onFormChange" class="rounded-textarea"></textarea>
					</view>
				</view>

				<!-- 备注 -->
				<view class="form-item form-item-textarea">
					<view class="form-label">备注</view>
					<view class="form-input">
						<textarea v-model="CustomerData.Remarks" placeholder="请输入备注信息" rows="2" @focus="onInputFocus"
							@blur="onInputBlur" @input="onFormChange" class="rounded-textarea"></textarea>
					</view>
				</view>
			</view>

			<!-- 保存按钮 -->
			<view class="save-btn-container">
				<button class="custom-save-btn" @click="handleSave">
					保存客户
				</button>
			</view>
		</view>


	</view>
</template>

<script>
	export default {
		data() {
			return {
				CustomerData: {},

				// 表单是否有焦点
				hasFocus: false,

				// 联系人选择相关（客户联动）
				selectedEmployeeIndex: '',
				employeeList: [],

				// 行业
				selectedIndustryIndex: '',
				IndustryData: [],

				// 类型
				selectedCustypeIndex: '',
				CustypeData: [],

				// 级别
				selectedCuslevelIndex: '',
				CusLevelData: [],

				// 来源
				selectedCussourceIndex: '',
				CusSourceData: [],

				// 状态
				selectedStausIndex: '',
				StatusData: [{
						"id": 0,
						"text": "私客"
					},
					{
						"id": 1,
						"text": "公客"
					}
				],

				hasChanged: false, // 标记表单是否有修改
				isShowingModal: false, // 新增：是否正在显示退出确认弹窗
			};
		},
		filters: {
			// 价格格式化过滤器
			formatPrice(price) {
				if (price) {
					return '¥' + (price).toFixed(2);
				}

			}
		},
		onLoad(options) {

			console.log(options);

			if (Object.keys(options).length !== 0) {
				// 将接收的字符串解析为JSON对象
				const receivedData = JSON.parse(decodeURIComponent(options.data));

				receivedData.Receive_date = this.formatDate(receivedData.Receive_date);

				this.CustomerData = receivedData;

				this.employeeList = this.CustomerData.employee;

				this.selectedStausIndex = this.StatusData.findIndex(item => item.id === receivedData.isPrivate);

				console.log(this.CustomerData);
			}

			//员工
			this.loademployee();

			//参数
			this.loadparams();

		},
		// 监听安卓物理返回、iOS手势返回、导航栏返回按钮
		onBackPress(options) {
			// 1. 如果是代码里主动调用的 uni.navigateBack()，直接放行，绝不拦截！
			if (options.from === 'navigateBack') {
			  return false; 
			}

			// 2. 如果没有数据修改，直接放行
			if (!this.hasChanged) {
			  return false; 
			}

			// 3. 如果弹窗正在显示，拦截本次返回，防止重复弹窗
			if (this.isShowingModal) {
			  return true;
			}

			// 4. 触发拦截逻辑
			this.isShowingModal = true;
			
			uni.showModal({
			  title: '提示',
			  content: '当前有未保存的修改，确定要退出吗？',
			  success: (res) => {
				if (res.confirm) {
				  // ⭐ 终极核心：先清除状态，再执行返回！
				  this.hasChanged = false; 
				  setTimeout(() => {
				  	uni.navigateBack({
				  		delta: 1
				  	});
				  }, 100);
				}
			  },
			  complete: () => {
				this.isShowingModal = false; 
			  }
			});
			
			return true; // 拦截物理返回
		},
		methods: {
			loademployee() {
				this.$request("/api/EmployeeList")
					.then(res => {
						//console.log(res);

						this.employeeList = res.data;

						console.log(this.employeeList);

						//console.log(Object.keys(this.CustomerData).length!=0);

						if (Object.keys(this.CustomerData).length !== 0) {

							const emp_id = this.CustomerData.emp_id;

							this.selectedEmployeeIndex = this.employeeList.findIndex(item => item.id === emp_id);
						}
					});
			},

			//加载参数
			loadparams() {
				this.$request("/api/paramsCombo", {
						"type": "cus_industry"
					})
					.then(res => {
						//console.log(res);

						this.IndustryData = res.data;

						//console.log(Object.keys(this.OrderData).length!=0);

						if (Object.keys(this.CustomerData).length !== 0) {

							const cus_industry_id = this.CustomerData.cus_industry_id;

							this.selectedIndustryIndex = this.IndustryData.findIndex(item => item.id ===
								cus_industry_id);
						}
					});

				this.$request("/api/paramsCombo", {
						"type": "cus_type"
					})
					.then(res => {
						//console.log(res);

						this.CustypeData = res.data;

						//console.log(Object.keys(this.OrderData).length!=0);

						if (Object.keys(this.CustomerData).length !== 0) {

							const cus_type_id = this.CustomerData.cus_type_id;

							this.selectedCustypeIndex = this.CustypeData.findIndex(item => item.id === cus_type_id);
						}
					});
				this.$request("/api/paramsCombo", {
						"type": "cus_level"
					})
					.then(res => {
						//console.log(res);

						this.CusLevelData = res.data;

						//console.log(Object.keys(this.OrderData).length!=0);

						if (Object.keys(this.CustomerData).length !== 0) {

							const cus_level_id = this.CustomerData.cus_level_id;

							this.selectedCuslevelIndex = this.CusLevelData.findIndex(item => item.id === cus_level_id);
						}
					});

				this.$request("/api/paramsCombo", {
						"type": "cus_source"
					})
					.then(res => {
						//console.log(res);

						this.CusSourceData = res.data;

						//console.log(Object.keys(this.OrderData).length!=0);

						if (Object.keys(this.CustomerData).length !== 0) {

							const cus_source_id = this.CustomerData.cus_source_id;

							this.selectedCussourceIndex = this.CusSourceData.findIndex(item => item.id ===
								cus_source_id);
						}
					});

			},

			// 选择客户
			selectEmployee(e) {
				this.selectedEmployeeIndex = e.detail.value;
				this.CustomerData.emp_id = this.employeeList[this.selectedEmployeeIndex].id;
				this.onFormChange(); 
			},

			industryChange(e) {
				this.selectedIndustryIndex = e.detail.value;
				this.CustomerData.cus_industry_id = this.IndustryData[this.selectedIndustryIndex].id;
				this.onFormChange(); 
			},

			custypeChange(e) {
				this.selectedCustypeIndex = e.detail.value;
				this.CustomerData.cus_type_id = this.CustypeData[this.selectedCustypeIndex].id;
				this.onFormChange(); 
			},

			CusLevelChange(e) {
				this.selectedCuslevelIndex = e.detail.value;
				this.CustomerData.cus_level_id = this.CusLevelData[this.selectedCuslevelIndex].id;
				this.onFormChange(); 
			},

			CusSourceChange(e) {
				this.selectedCussourceIndex = e.detail.value;
				this.CustomerData.cus_source_id = this.CusSourceData[this.selectedCussourceIndex].id;
				this.onFormChange(); 
			},

			statusChange(e) {
				this.selectedStausIndex = e.detail.value;
				this.CustomerData.isPrivate = this.StatusData[this.selectedStausIndex].id;
				this.onFormChange(); 
			},

			// 格式化日期为YYYY-MM-DD
			formatDate(value) {
				if (!value) return ''
				// 转换为Date对象
				const date = new Date(value)

				const year = date.getFullYear();
				const month = (date.getMonth() + 1).toString().padStart(2, '0');
				const day = date.getDate().toString().padStart(2, '0');
				return `${year}-${month}-${day}`;
			},


			// 输入框获得焦点
			onInputFocus() {
				this.hasFocus = true;
			},

			// 输入框失去焦点
			onInputBlur() {
				this.hasFocus = false;
			},
			// 任何表单改动都调用这个方法
			onFormChange() {
				this.hasChanged = true;
			},
			
			// 保存记录
			handleSave() {
				const cus = this.CustomerData;

				const postdata = {
					"id": cus.id,
					"City_id": cus.City_id,
					"cus_add": cus.cus_add,
					"cus_fax": cus.cus_fax,
					"cus_industry_id": cus.cus_industry_id,
					"cus_level_id": cus.cus_level_id,
					"cus_name": cus.cus_name,
					"cus_source_id": cus.cus_source_id,
					"cus_tel": cus.cus_tel,
					"cus_type_id": cus.cus_type_id,
					"cus_website": cus.cus_website,
					"DesCripe": cus.DesCripe,
					"emp_id": cus.emp_id,
					"isPrivate": cus.isPrivate,
					"Provinces_id": cus.Provinces_id,
					"Remarks": cus.Remarks,
					"xy": cus.xy,
					"x": cus.x,
					"y": cus.y
				};

				console.log(postdata);

				//return;

				if (!postdata.cus_name || postdata.cus_name.trim().length == 0) {
					uni.showToast({
						title: '请输入客户名!',
						icon: 'none',
						duration: 2000
					});

					return;
				}

				if (!postdata.cus_tel || postdata.cus_tel.trim().length == 0) {
					uni.showToast({
						title: '请输入电话!',
						icon: 'none',
						duration: 2000
					});

					return;
				}

				if (typeof postdata.isPrivate != "number") {
					uni.showToast({
						title: '请选择状态!',
						icon: 'none',
						duration: 2000
					});

					return;
				}

				if (!postdata.emp_id) {
					uni.showToast({
						title: '请选择归属!',
						icon: 'none',
						duration: 2000
					});

					return;
				}

				// return;
				// 模拟接口提交
				uni.showLoading({
					title: '保存中...',
					mask: true
				});


				//return;

				this.$request("/api/CustomerSave", postdata, "POST")
					.then(res => {
						console.log(res);

						uni.hideLoading();
						uni.showToast({
							title: '保存成功',
							icon: 'success',
							duration: 1500
						});
						
						// ★★★ 获取页面栈，更新前两个页面 ★★★
						const pages = getCurrentPages();
						const detailPage = pages[pages.length - 2]; // 详情页
						const listPage = pages[pages.length - 3];   // 列表页
				
						// 1. 更新详情页
						if (detailPage && detailPage.$vm && typeof detailPage.$vm.updateData === 'function') {
							if(cus.id)
							{
								detailPage.$vm.updateData(cus);
							}
							else
							{
								detailPage.$vm.loadCustomerList();
							}
						}
				
						// 2. ★★★ 直接更新列表页的那一条数据 ★★★
						if (listPage && listPage.$vm && typeof listPage.$vm.updateData === 'function') {
							listPage.$vm.updateData(cus);
						}
						
						setTimeout(() => {
							uni.navigateBack({
								delta: 1
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