<template>
  <view class="crm-home">
    <!-- 顶部导航栏 -->
    <view class="top-nav">
      <view class="logo-area">
        <i class="fa fa-cubes"></i>
        <text class="app-name">智客CRM</text>
      </view>
      <view class="nav-actions">
        <button class="action-btn" @click="openSearch">
          <i class="fa fa-search"></i>
        </button>
        <button class="action-btn notification-btn" @click="openNotifications">
          <i class="fa fa-bell-o"></i>
          <span class="badge" v-if="unreadCount > 0">{{ unreadCount }}</span>
        </button>
        <view class="user-info" @click="openProfile">
          <image src="/static/avatar.png" class="user-avatar" mode="widthFix"></image>
        </view>
      </view>
    </view>

    <!-- 主内容区 -->
    <scroll-view class="main-content" scroll-y="true">
      <!-- 用户欢迎区 -->
      <view class="welcome-area">
        <text class="welcome-text">早上好，张经理</text>
        <text class="date-info">{{ currentDate }}</text>
      </view>

      <!-- 数据统计模块（优化数据对称性） -->
      <view class="stats-module">
        <view class="module-header">
          <text class="module-title">业务概览</text>
          <text class="see-more" @click="viewAllStats">查看全部</text>
        </view>
        <view class="stats-container">
          <view class="stat-item">
            <text class="stat-label">客户总数</text>
            <text class="stat-value">{{ totalCustomers }}</text>
            <text class="stat-change positive">+12</text>
          </view>
          <view class="stat-item">
            <text class="stat-label">今日新增</text>
            <text class="stat-value">{{ todayNew }}</text>
            <text class="stat-change neutral">-</text>
          </view>
          <view class="stat-item">
            <text class="stat-label">待跟进</text>
            <text class="stat-value">{{ pendingFollowup }}</text>
            <text class="stat-change neutral">-</text>
          </view>
          <view class="stat-item">
            <text class="stat-label">本月业绩</text>
            <text class="stat-value">¥{{ monthlySales }}</text>
            <text class="stat-change positive">+8%</text>
          </view>
        </view>
      </view>

      <!-- 宫格快捷方式 -->
      <view class="shortcut-module">
        <view class="module-header">
          <text class="module-title">快捷功能</text>
        </view>
        <view class="shortcut-grid">
          <view class="shortcut-item" @click="navigateTo('add-customer')">
            <view class="icon-container">
              <i class="fa fa-user-plus"></i>
            </view>
            <text class="shortcut-name">新增客户</text>
          </view>
          <view class="shortcut-item" @click="navigateTo('customer-list')">
            <view class="icon-container">
              <i class="fa fa-users"></i>
            </view>
            <text class="shortcut-name">客户列表</text>
          </view>
          <view class="shortcut-item" @click="navigateTo('add-opportunity')">
            <view class="icon-container">
              <i class="fa fa-lightbulb-o"></i>
            </view>
            <text class="shortcut-name">新增商机</text>
          </view>
          <view class="shortcut-item" @click="navigateTo('tasks')">
            <view class="icon-container">
              <i class="fa fa-tasks"></i>
            </view>
            <text class="shortcut-name">我的任务</text>
          </view>
          <view class="shortcut-item" @click="navigateTo('calendar')">
            <view class="icon-container">
              <i class="fa fa-calendar"></i>
            </view>
            <text class="shortcut-name">日程安排</text>
          </view>
          <view class="shortcut-item" @click="navigateTo('reports')">
            <view class="icon-container">
              <i class="fa fa-bar-chart"></i>
            </view>
            <text class="shortcut-name">业绩报表</text>
          </view>
          <view class="shortcut-item" @click="navigateTo('documents')">
            <view class="icon-container">
              <i class="fa fa-file-text-o"></i>
            </view>
            <text class="shortcut-name">合同文档</text>
          </view>
          <view class="shortcut-item" @click="navigateTo('settings')">
            <view class="icon-container">
              <i class="fa fa-cog"></i>
            </view>
            <text class="shortcut-name">系统设置</text>
          </view>
        </view>
      </view>

      <!-- 待办任务 -->
      <view class="tasks-module">
        <view class="module-header">
          <text class="module-title">今日待办</text>
          <text class="see-more" @click="viewAllTasks">全部任务</text>
        </view>
        <view class="tasks-list">
          <view class="task-item" v-for="(task, index) in todayTasks" :key="index">
            <view class="task-checkbox" @click="toggleTask(index)">
              <i class="fa" :class="task.completed ? 'fa-check-square-o' : 'fa-square-o'"></i>
            </view>
            <view class="task-content">
              <text class="task-title" :class="task.completed ? 'completed' : ''">{{ task.title }}</text>
              <text class="task-time">{{ task.time }}</text>
            </view>
            <view class="task-priority" :class="task.priority">
              <text class="priority-text">{{ task.priorityText }}</text>
            </view>
          </view>
        </view>
      </view>

      <!-- 最近客户 -->
      <view class="recent-customers">
        <view class="module-header">
          <text class="module-title">最近互动</text>
          <text class="see-more" @click="viewAllCustomers">更多客户</text>
        </view>
        <view class="customers-list">
          <view class="customer-item" v-for="(customer, index) in recentCustomers" :key="index">
            <image :src="customer.avatar" class="customer-avatar" mode="widthFix"></image>
            <view class="customer-info">
              <text class="customer-name">{{ customer.name }}</text>
              <text class="customer-company">{{ customer.company }}</text>
              <text class="interaction-type">{{ customer.interaction }}</text>
            </view>
            <button class="contact-btn" @click="contactCustomer(customer.id)">
              <i class="fa fa-comment"></i>
            </button>
          </view>
        </view>
      </view>
    </scroll-view>

    <!-- 悬浮添加按钮 -->
    <button class="floating-btn" @click="showQuickAdd">
      <i class="fa fa-plus"></i>
    </button>
  </view>
</template>

<script>
export default {
  data() {
    return {
      // 通知数量（保持简洁数字）
      unreadCount: 3,
      // 当前日期
      currentDate: '',
      // 统计数据（优化为长度对称的数值）
      totalCustomers: 286,      // 3位数字
      todayNew: 5,              // 1位数字，简洁明了
      pendingFollowup: 12,      // 2位数字
      monthlySales: '12.8万',   // 简化为带单位的短格式
      // 今日任务（标题长度尽量均衡）
      todayTasks: [
        {
          title: '阿里云合同确认',
          time: '10:30',
          priority: 'high',
          priorityText: '高',
          completed: false
        },
        {
          title: '腾讯新需求跟进',
          time: '14:00',
          priority: 'medium',
          priorityText: '中',
          completed: false
        },
        {
          title: '整理客户资料',
          time: '16:30',
          priority: 'low',
          priorityText: '低',
          completed: false
        }
      ],
      // 最近客户（信息长度优化）
      recentCustomers: [
        {
          id: 1001,
          name: '李总监',
          company: '百度',
          avatar: '/static/customer1.png',
          interaction: '昨天 通话'
        },
        {
          id: 1002,
          name: '张经理',
          company: '京东',
          avatar: '/static/customer2.png',
          interaction: '2天前 邮件'
        },
        {
          id: 1003,
          name: '王总',
          company: '字节',
          avatar: '/static/customer3.png',
          interaction: '3天前 拜访'
        }
      ]
    };
  },
  onLoad() {
    // 设置当前日期
    this.setCurrentDate();
  },
  methods: {
    // 设置当前日期
    setCurrentDate() {
      const options = { weekday: 'long', month: 'long', day: 'numeric' };
      this.currentDate = new Date().toLocaleDateString('zh-CN', options);
    },
    // 导航到指定页面
    navigateTo(page) {
      uni.navigateTo({
        url: `/pages/${page}/${page}`
      });
    },
    // 切换任务状态
    toggleTask(index) {
      this.todayTasks[index].completed = !this.todayTasks[index].completed;
    },
    // 联系客户
    contactCustomer(id) {
      uni.navigateTo({
        url: `/pages/customer-detail/customer-detail?id=${id}`
      });
    },
    // 查看全部统计
    viewAllStats() {
      uni.navigateTo({
        url: '/pages/stats-detail/stats-detail'
      });
    },
    // 查看全部任务
    viewAllTasks() {
      uni.navigateTo({
        url: '/pages/all-tasks/all-tasks'
      });
    },
    // 查看全部客户
    viewAllCustomers() {
      uni.navigateTo({
        url: '/pages/customer-list/customer-list'
      });
    },
    // 打开搜索
    openSearch() {
      uni.navigateTo({
        url: '/pages/search/search'
      });
    },
    // 打开通知
    openNotifications() {
      uni.navigateTo({
        url: '/pages/notifications/notifications'
      });
    },
    // 打开个人资料
    openProfile() {
      uni.navigateTo({
        url: '/pages/profile/profile'
      });
    },
    // 显示快速添加选项
    showQuickAdd() {
      console.log('显示快速添加选项');
    }
  }
};
</script>

<style scoped>
/* 基础样式保持不变 */
.crm-home {
  display: flex;
  flex-direction: column;
  height: 100%;
  background-color: #f5f7fa;
  box-sizing: border-box;
}

/* 顶部导航栏 */
.top-nav {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 18rpx 4%;
  background-color: #2c3e50;
  color: white;
  box-sizing: border-box;
}

.logo-area {
  display: flex;
  align-items: center;
}

.logo-area i {
  font-size: 36rpx;
  margin-right: 15rpx;
}

.app-name {
  font-size: 32rpx;
  font-weight: bold;
}

.nav-actions {
  display: flex;
  align-items: center;
}

.action-btn {
  background-color: transparent;
  border: none;
  color: white;
  font-size: 30rpx;
  margin-right: 25rpx;
  position: relative;
}

.notification-btn {
  position: relative;
}

.badge {
  position: absolute;
  top: -10rpx;
  right: 15rpx;
  background-color: #e74c3c;
  color: white;
  font-size: 20rpx;
  width: 26rpx;
  height: 26rpx;
  border-radius: 50%;
  display: flex;
  justify-content: center;
  align-items: center;
}

.user-info {
  width: 60rpx;
  height: 60rpx;
  border-radius: 50%;
  overflow: hidden;
}

.user-avatar {
  width: 100%;
  height: 100%;
}

/* 主内容区 */
.main-content {
  flex: 1;
  padding: 30rpx 4%;
  box-sizing: border-box;
}

/* 欢迎区 */
.welcome-area {
  margin-bottom: 30rpx;
}

.welcome-text {
  font-size: 36rpx;
  font-weight: bold;
  color: #2c3e50;
}

.date-info {
  font-size: 26rpx;
  color: #7f8c8d;
  display: block;
  margin-top: 10rpx;
}

/* 数据统计模块（优化了对齐） */
.stats-module {
  background-color: white;
  border-radius: 16rpx;
  padding: 25rpx;
  margin-bottom: 30rpx;
  box-shadow: 0 2rpx 10rpx rgba(0, 0, 0, 0.05);
}

.module-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25rpx;
}

.module-title {
  font-size: 30rpx;
  font-weight: bold;
  color: #2c3e50;
}

.see-more {
  font-size: 24rpx;
  color: #3498db;
}

.stats-container {
  display: flex;
  justify-content: space-between;
  gap: 15rpx;
}

.stat-item {
  flex: 1;
  text-align: center;
  padding: 15rpx 0;
}

.stat-label {
  font-size: 24rpx;
  color: #7f8c8d;
  display: block;
  margin-bottom: 10rpx;
}

.stat-value {
  font-size: 34rpx;
  font-weight: bold;
  color: #2c3e50;
  display: block;
  margin-bottom: 5rpx;
  min-height: 36rpx; /* 确保数值区域高度一致 */
}

.stat-change {
  font-size: 22rpx;
  display: block;
}

.positive {
  color: #27ae60;
}

.neutral {
  color: #95a5a6;
}

/* 宫格快捷方式（保持原样） */
.shortcut-module {
  background-color: white;
  border-radius: 16rpx;
  padding: 25rpx;
  margin-bottom: 30rpx;
  box-shadow: 0 2rpx 10rpx rgba(0, 0, 0, 0.05);
}

.shortcut-grid {
  display: flex;
  flex-wrap: wrap;
}

.shortcut-item {
  width: 25%;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 20rpx 0;
  box-sizing: border-box;
}

.icon-container {
  width: 70rpx;
  height: 70rpx;
  border-radius: 15rpx;
  background-color: #f1f8ff;
  color: #3498db;
  display: flex;
  justify-content: center;
  align-items: center;
  font-size: 36rpx;
  margin-bottom: 15rpx;
}

.shortcut-name {
  font-size: 24rpx;
  color: #2c3e50;
  text-align: center;
}

/* 待办任务（优化了文本长度） */
.tasks-module {
  background-color: white;
  border-radius: 16rpx;
  padding: 25rpx;
  margin-bottom: 30rpx;
  box-shadow: 0 2rpx 10rpx rgba(0, 0, 0, 0.05);
}

.tasks-list {
  display: flex;
  flex-direction: column;
  gap: 15rpx;
}

.task-item {
  display: flex;
  align-items: center;
  padding: 20rpx;
  border-radius: 12rpx;
  background-color: #f9f9f9;
}

.task-checkbox {
  width: 40rpx;
  height: 40rpx;
  display: flex;
  justify-content: center;
  align-items: center;
  color: #3498db;
  font-size: 30rpx;
}

.task-content {
  flex: 1;
  margin-left: 20rpx;
}

.task-title {
  font-size: 26rpx;
  color: #2c3e50;
  display: block;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 100%;
}

.completed {
  text-decoration: line-through;
  color: #95a5a6;
}

.task-time {
  font-size: 22rpx;
  color: #7f8c8d;
  display: block;
  margin-top: 5rpx;
}

.task-priority {
  padding: 5rpx 15rpx;
  border-radius: 20rpx;
  font-size: 22rpx;
}

.priority-text {
  color: white;
}

.high {
  background-color: #e74c3c;
}

.medium {
  background-color: #f39c12;
}

.low {
  background-color: #27ae60;
}

/* 最近客户（优化了文本长度） */
.recent-customers {
  background-color: white;
  border-radius: 16rpx;
  padding: 25rpx;
  margin-bottom: 30rpx;
  box-shadow: 0 2rpx 10rpx rgba(0, 0, 0, 0.05);
}

.customers-list {
  display: flex;
  flex-direction: column;
  gap: 15rpx;
}

.customer-item {
  display: flex;
  align-items: center;
  padding: 20rpx;
  border-radius: 12rpx;
  background-color: #f9f9f9;
}

.customer-avatar {
  width: 70rpx;
  height: 70rpx;
  border-radius: 50%;
  margin-right: 20rpx;
}

.customer-info {
  flex: 1;
}

.customer-name {
  font-size: 28rpx;
  font-weight: bold;
  color: #2c3e50;
  display: block;
}

.customer-company {
  font-size: 24rpx;
  color: #7f8c8d;
  display: block;
  margin-top: 5rpx;
}

.interaction-type {
  font-size: 22rpx;
  color: #3498db;
  display: block;
  margin-top: 5rpx;
}

.contact-btn {
  width: 56rpx;
  height: 56rpx;
  border-radius: 50%;
  background-color: #ebf5fb;
  color: #3498db;
  border: none;
  display: flex;
  justify-content: center;
  align-items: center;
  font-size: 26rpx;
}

/* 悬浮按钮 */
.floating-btn {
  position: fixed;
  right: 4%;
  bottom: 40rpx;
  width: 90rpx;
  height: 90rpx;
  border-radius: 50%;
  background-color: #3498db;
  color: white;
  border: none;
  display: flex;
  justify-content: center;
  align-items: center;
  font-size: 40rpx;
  box-shadow: 0 5rpx 15rpx rgba(52, 152, 219, 0.3);
  z-index: 99;
}

/* 响应式调整 */
@media screen and (max-width: 320px) {
  .shortcut-item {
    width: 33.333%;
  }
  
  .stat-value {
    font-size: 30rpx;
  }
  
  .task-title {
    font-size: 24rpx;
  }
}

@media screen and (min-width: 414px) {
  .shortcut-item {
    padding: 25rpx 0;
  }
  
  .icon-container {
    width: 80rpx;
    height: 80rpx;
    font-size: 40rpx;
  }
  
  .shortcut-name {
    font-size: 26rpx;
  }
}
</style>
