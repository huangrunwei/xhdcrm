<template>
	<view class="container">
		<view class="page-title">订单编辑</view>

		<!-- 订单基本基本信息模块 -->
		<view class="order-base-info module-card">
			<view class="module-title">订单基本信息</view>

			<view class="form-group">
				<view class="form-label">客户名称</view>
				<view class="form-control">
					<input type="text" v-model="orderInfo.customerName" placeholder="请输入客户名称" />
				</view>
			</view>

			<view class="form-group">
				<view class="form-label">订单日期</view>
				<view class="form-control">
					<input type="date" v-model="orderInfo.orderDate" />
				</view>
			</view>

			<view class="form-group">
				<view class="form-label">订单金额</view>
				<view class="form-control">
					<input type="number" v-model="orderInfo.totalAmount" placeholder="0.00" disabled />
				</view>
			</view>

			<view class="form-group">
				<view class="form-label">订单备注</view>
				<view class="form-control">
					<textarea v-model="orderInfo.remark" placeholder="请输入订单备注信息" rows="3"></textarea>
				</view>
			</view>
		</view>

		<!-- 产品列表模块 -->
		<view class="product-list module-card">
			<view class="module-header">
				<view class="module-title">产品清单</view>
				<!-- <button class="add-product-btn" @click="showProductSelector = true">
					<uni-icons type="plus" size="18" color="#fff"></uni-icons>
					添加产品
				</button> -->
			</view>

			<!-- 产品列表 -->
			<view class="product-items">
				<view v-if="orderProducts.length === 0" class="empty-tip">
					暂无产品，请点击添加产品按钮添加
				</view>

				<view v-for="(product, index) in orderProducts" :key="index" class="product-item">
					<view class="product-image">
						<image :src="product.imageUrl" mode="aspectFill"></image>
					</view>

					<view class="product-info">
						<view class="product-name">{{ product.name }}</view>
						<view class="product-price">单价: ¥{{ product.price.toFixed(2) }}</view>
					</view>

					<view class="product-quantity">
						<view class="quantity-control">
							<button class="quantity-btn" @click="decreaseQuantity(index)"
								:disabled="product.quantity <= 1">
								<uni-icons type="minus" size="16"></uni-icons>
							</button>
							<input type="number" v-model="product.quantity" @change="updateQuantity(index)" min="1" />
							<button class="quantity-btn" @click="increaseQuantity(index)">
								<uni-icons type="plus" size="16"></uni-icons>
							</button>
						</view>
					</view>

					<view class="product-amount">
						¥{{ (product.price * product.quantity).toFixed(2) }}
					</view>

					<view class="product-remove">
						<button class="remove-btn" @click="removeProduct(index)">
							<uni-icons type="trash" size="20" color="#ff4d4f"></uni-icons>
						</button>
					</view>
				</view>
			</view>
		
		<!-- 保存按钮 -->
		<view class="save-btn-container">
			<button class="custom-save-btn" @click="showProductSelector = true">
				保存跟进记录
			</button>
		</view>
		</view>

		<!-- 操作按钮 -->
		<view class="action-buttons">
			<button class="cancel-btn" @click="onCancel">取消</button>
			<button class="save-btn" @click="onSave">保存订单</button>
		</view>

		<!-- 产品选择器弹窗 - 底部弹出式，点击直接选择 -->
		<view v-if="showProductSelector" class="picker-overlay" @click="showProductSelector = false">
			<view class="picker-wrapper" @click.stop>
				<view class="picker-header">
					<button class="picker-cancel" @click="showProductSelector = false">取消</button>
					<view class="picker-title">选择产品</view>
					<view class="picker-empty"></view> <!-- 占位元素，保持标题居中 -->
				</view>

				<view class="picker-search">
					<uni-icons type="search" size="16" class="search-icon"></uni-icons>
					<input type="text" v-model="productSearch" placeholder="搜索产品..." />
				</view>

				<view class="picker-content">
					<scroll-view class="picker-scroll" scroll-y>
						<view class="picker-item" v-for="(product, index) in filteredProducts" :key="index"
							:class="{ 'picker-item-selected': isProductSelected(product.id) }"
							@click="selectAndAddProduct(product)">
							<view class="picker-item-inner">
								<view class="picker-item-image">
									<image :src="product.imageUrl" mode="aspectFill"></image>
								</view>
								<view class="picker-item-info">
									<view class="picker-item-name">{{ product.name }}</view>
									<view class="picker-item-price">¥{{ product.price.toFixed(2) }}</view>
								</view>
								<uni-icons v-if="isProductSelected(product.id)" type="checkmark" size="20"
									color="#007aff"></uni-icons>
							</view>
						</view>
					</scroll-view>
				</view>
			</view>
		</view>
	</view>
</template>

<script>
	export default {
		data() {
			return {
				// 订单基本信息
				orderInfo: {
					customerName: '',
					orderDate: this.formatDate(new Date()),
					totalAmount: 0,
					remark: ''
				},

				// 订单产品列表
				orderProducts: [],

				// 所有可用产品
				allProducts: [{
						id: 1,
						name: '产品A',
						price: 199.99,
						imageUrl: '/static/images/product1.jpg'
					},
					{
						id: 2,
						name: '产品B',
						price: 299.99,
						imageUrl: '/static/images/product2.jpg'
					},
					{
						id: 3,
						name: '产品C',
						price: 399.99,
						imageUrl: '/static/images/product3.jpg'
					},
					{
						id: 4,
						name: '产品D',
						price: 499.99,
						imageUrl: '/static/images/product4.jpg'
					},
					{
						id: 5,
						name: '产品E',
						price: 599.99,
						imageUrl: '/static/images/product5.jpg'
					}
				],

				// 产品搜索关键词
				productSearch: '',

				// 显示产品选择器
				showProductSelector: false
			};
		},

		computed: {
			// 过滤后的产品列表
			filteredProducts() {
				if (!this.productSearch) return this.allProducts;

				const keyword = this.productSearch.toLowerCase();
				return this.allProducts.filter(product =>
					product.name.toLowerCase().includes(keyword)
				);
			}
		},

		methods: {
			// 格式化日期为YYYY-MM-DD
			formatDate(date) {
				const year = date.getFullYear();
				const month = (date.getMonth() + 1).toString().padStart(2, '0');
				const day = date.getDate().toString().padStart(2, '0');
				return `${year}-${month}-${day}`;
			},

			// 增加产品数量
			increaseQuantity(index) {
				this.orderProducts[index].quantity++;
				this.calculateTotalAmount();
			},

			// 减少产品数量
			decreaseQuantity(index) {
				if (this.orderProducts[index].quantity > 1) {
					this.orderProducts[index].quantity--;
					this.calculateTotalAmount();
				}
			},

			// 更新产品数量
			updateQuantity(index) {
				// 确保数量至少为1
				if (this.orderProducts[index].quantity < 1) {
					this.orderProducts[index].quantity = 1;
				}
				this.calculateTotalAmount();
			},

			// 从订单中移除产品
			removeProduct(index) {
				this.orderProducts.splice(index, 1);
				this.calculateTotalAmount();
			},

			// 检查产品是否已被选中(已添加到订单)
			isProductSelected(productId) {
				return this.orderProducts.some(item => item.id === productId);
			},

			// 选择并添加产品到订单，同时关闭选择器
			selectAndAddProduct(product) {
				// 检查产品是否已在订单中
				const existingIndex = this.orderProducts.findIndex(item => item.id === product.id);

				if (existingIndex > -1) {
					// 如果已存在，增加数量
					this.orderProducts[existingIndex].quantity++;
				} else {
					// 如果不存在，添加新产品
					this.orderProducts.push({
						...product,
						quantity: 1
					});
				}

				this.calculateTotalAmount();
				this.showProductSelector = false; // 选择后自动关闭
				this.productSearch = '';
			},

			// 计算订单总金额
			calculateTotalAmount() {
				this.orderInfo.totalAmount = this.orderProducts.reduce((total, product) => {
					return total + (product.price * product.quantity);
				}, 0);
			},

			// 取消编辑
			onCancel() {
				uni.navigateBack();
			},

			// 保存订单
			onSave() {
				// 简单验证
				if (!this.orderInfo.customerName) {
					uni.showToast({
						title: '请输入客户名称',
						icon: 'none'
					});
					return;
				}

				if (this.orderProducts.length === 0) {
					uni.showToast({
						title: '请至少添加一个产品',
						icon: 'none'
					});
					return;
				}

				// 这里可以添加保存订单的逻辑，比如调用API
				console.log('保存订单:', {
					...this.orderInfo,
					products: this.orderProducts
				});

				uni.showToast({
					title: '订单保存成功',
					icon: 'success'
				});

				// 保存成功后返回上一页
				setTimeout(() => {
					uni.navigateBack();
				}, 1500);
			}
		}
	};
</script>

<style scoped>
	.container {
		padding: 16px;
		background-color: #f5f5f5;
		min-height: 100vh;
		box-sizing: border-box;
	}

	.page-title {
		font-size: 20px;
		font-weight: bold;
		color: #333;
		margin-bottom: 20px;
		text-align: center;
	}

	.module-card {
		background-color: #fff;
		border-radius: 12px;
		padding: 16px;
		margin-bottom: 16px;
		box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
		box-sizing: border-box;
		width: 100%;
	}

	.module-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 16px;
	}

	.module-title {
		font-size: 16px;
		font-weight: bold;
		color: #333;
		margin-bottom: 16px;
	}

	.form-group {
		display: flex;
		margin-bottom: 16px;
		align-items: center;
		width: 100%;
		box-sizing: border-box;
	}

	.form-label {
		width: 25%;
		font-size: 14px;
		color: #666;
		padding-right: 10px;
		box-sizing: border-box;
	}

	.form-control {
		width: 75%;
		box-sizing: border-box;
	}

	/* 输入框样式 */
	.form-control input,
	.form-control textarea {
		width: 100%;
		padding: 14px 12px;
		border: 1px solid #ddd;
		border-radius: 6px;
		font-size: 15px;
		box-sizing: border-box;
		height: 48px;
		line-height: 20px;
	}

	/* 文本框特殊处理 */
	.form-control textarea {
		resize: none;
		height: auto;
		min-height: 100px;
	}

	.add-product-btn {
		background-color: #007aff;
		color: #fff;
		border-radius: 20px;
		padding: 6px 16px;
		font-size: 14px;
		display: flex;
		align-items: center;
		justify-content: center;
		gap: 6px;
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
		margin-bottom: 4px;
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
</style>