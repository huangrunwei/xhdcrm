<template>
  <view class="page-container">
    <!-- 页面标题 -->
    <view class="page-title">客户列表</view>
    
    <!-- 客户列表容器 -->
    <view class="customer-list">
      <!-- 客户项 - 每个客户使用白色圆角框 -->
      <view 
        class="customer-item" 
        v-for="(customer, index) in customers" 
        :key="index"
        @click="handleCustomerClick(customer)"
      >
        <!-- 第一行：客户名称 -->
        <view class="item-row row-1">
          <text class="customer-name">{{ customer.name }}</text>
          <view class="customer-actions">
            <button class="action-btn" @click.stop="handleEdit(customer)">
              <uni-icons type="edit" size="20" color="#007aff"></uni-icons>
            </button>
            <button class="action-btn" @click.stop="handleCall(customer)">
              <uni-icons type="phone" size="20" color="#00cc00"></uni-icons>
            </button>
          </view>
        </view>
        
        <!-- 第二行：联系电话 -->
        <view class="item-row row-2">
          <uni-icons type="contact" size="20" color="#666" class="row-icon"></uni-icons>
          <text class="row-content">{{ customer.phone }}</text>
        </view>
        
        <!-- 第三行：地址 -->
        <view class="item-row row-3">
          <uni-icons type="location" size="20" color="#666" class="row-icon"></uni-icons>
          <text class="row-content">{{ customer.address }}</text>
        </view>
      </view>
      
      <!-- 空状态提示 -->
      <view class="empty-state" v-if="customers.length === 0">
        <uni-icons type="empty" size="80" color="#ccc"></uni-icons>
        <text class="empty-text">暂无客户数据</text>
      </view>
    </view>
    
    <!-- 悬浮添加按钮 -->
    <button class="add-btn" @click="handleAddCustomer">
      <uni-icons type="plus" size="28" color="#fff"></uni-icons>
    </button>
  </view>
</template>

<script>
export default {
  data() {
    return {
      // 客户数据列表
      customers: [
        {
          id: 1,
          name: "北京科技有限公司",
          phone: "13800138000",
          address: "北京市海淀区中关村科技园区8号楼"
        },
        {
          id: 2,
          name: "上海贸易公司",
          phone: "13912345678",
          address: "上海市浦东新区陆家嘴环路1000号"
        },
        {
          id: 3,
          name: "广州餐饮连锁",
          phone: "13787654321",
          address: "广州市天河区天河路385号"
        },
        {
          id: 4,
          name: "深圳软件开发工作室",
          phone: "13611223344",
          address: "深圳市南山区科技园南区12栋"
        },
        {
          id: 5,
          name: "杭州电子商务有限公司",
          phone: "13555667788",
          address: "杭州市西湖区文三路998号"
        }
      ]
    };
  },
  methods: {
    // 点击客户项
    handleCustomerClick(customer) {
      uni.navigateTo({
        url: `/pages/customer-detail/customer-detail?id=${customer.id}`
      });
    },
    
    // 编辑客户
    handleEdit(customer) {
      uni.navigateTo({
        url: `/pages/customer-edit/customer-edit?id=${customer.id}`
      });
    },
    
    // 拨打电话
    handleCall(customer) {
      uni.makePhoneCall({
        phoneNumber: customer.phone
      });
    },
    
    // 添加新客户
    handleAddCustomer() {
      uni.navigateTo({
        url: "/pages/customer-edit/customer-edit"
      });
    }
  }
};
</script>

<style scoped>
.page-container {
  background-color: #f5f5f7;
  min-height: 100vh;
  padding: 20rpx;
  padding-bottom: 120rpx; /* 为底部悬浮按钮预留空间 */
}

/* 页面标题 */
.page-title {
  font-size: 34rpx;
  font-weight: bold;
  color: #333;
  padding: 20rpx 0;
  margin-bottom: 10rpx;
  text-align: center;
}

/* 客户列表容器 */
.customer-list {
  display: flex;
  flex-direction: column;
  gap: 20rpx;
}

/* 客户项样式 - 白色圆角框 */
.customer-item {
  background-color: #fff;
  border-radius: 16rpx;
  padding: 20rpx;
  box-shadow: 0 2rpx 10rpx rgba(0, 0, 0, 0.05);
}

/* 列表行通用样式 */
.item-row {
  display: flex;
  align-items: center;
  padding: 10rpx 0;
}

/* 第一行 - 客户名称和操作按钮 */
.row-1 {
  justify-content: space-between;
  border-bottom: 1px solid #f5f5f5;
}

.customer-name {
  font-size: 28rpx;
  font-weight: bold;
  color: #333;
}

.customer-actions {
  display: flex;
  gap: 15rpx;
}

.action-btn {
  width: 44rpx;
  height: 44rpx;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: transparent;
  padding: 0;
}

/* 第二行和第三行样式 */
.row-2, .row-3 {
  color: #666;
}

.row-icon {
  margin-right: 15rpx;
  flex-shrink: 0;
}

.row-content {
  font-size: 26rpx;
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* 空状态样式 */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 150rpx 0;
}

.empty-text {
  font-size: 28rpx;
  color: #999;
  margin-top: 30rpx;
}

/* 悬浮添加按钮 */
.add-btn {
  position: fixed;
  right: 30rpx;
  bottom: 30rpx;
  width: 90rpx;
  height: 90rpx;
  border-radius: 50%;
  background-color: #007aff;
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4rpx 10rpx rgba(0, 0, 0, 0.2);
  z-index: 99;
}
</style>
