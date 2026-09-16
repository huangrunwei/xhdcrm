<template>
  <view class="crm-container">
    <!-- 顶部导航栏 -->
    <view class="top-nav">
      <view class="logo-area">
        <i class="fa fa-cubes"></i>
        <text class="app-title">客满盈CRM</text>
      </view>
      <view class="user-controls">
        <button class="nav-btn" @click="openSearch">
          <i class="fa fa-search"></i>
        </button>
        <button class="nav-btn notification-btn" @click="openNotifications">
          <i class="fa fa-bell-o"></i>
          <span class="notification-badge" v-if="unreadNotifications > 0">{{ unreadNotifications }}</span>
        </button>
        <view class="user-avatar" @click="openProfile">
          <image src="/static/user-avatar.png" mode="widthFix"></image>
        </view>
      </view>
    </view>

    <!-- 主内容区域 -->
    <scroll-view class="main-content" scroll-y="true">
      <!-- 用户欢迎区 -->
      <view class="welcome-panel">
        <view class="welcome-text">
          <text class="greeting">下午好，李销售</text>
          <text class="date-info">{{ currentDate }}</text>
        </view>
        <view class="performance-indicator">
          <text class="indicator-label">本月销售目标</text>
          <view class="progress-container">
            <view class="progress-bar" :style="{ width: progressPercent + '%' }"></view>
          </view>
          <text class="progress-text">{{ progressPercent }}% 已完成</text>
        </view>
      </view>

      <!-- 快捷功能区 -->
      <view class="quick-actions">
        <view class="section-header">
          <text class="section-title">快捷操作</text>
        </view>
        <view class="actions-grid">
          <view class="action-item" @click="navigateTo('add-customer')">
            <view class="action-icon">
              <i class="fa fa-user-plus"></i>
            </view>
            <text class="action-name">新增客户</text>
          </view>
          <view class="action-item" @click="navigateTo('add-opportunity')">
            <view class="action-icon">
              <i class="fa fa-lightbulb-o"></i>
            </view>
            <text class="action-name">新增商机</text>
          </view>
          <view class="action-item" @click="navigateTo('log-activity')">
            <view class="action-icon">
              <i class="fa fa-pencil"></i>
            </view>
            <text class="action-name">记录活动</text>
          </view>
          <view class="action-item" @click="navigateTo('schedule-meeting')">
            <view class="action-icon">
              <i class="fa fa-calendar-plus-o"></i>
            </view>
            <text class="action-name">安排会议</text>
          </view>
          <view class="action-item" @click="navigateTo('generate-report')">
            <view class="action-icon">
              <i class="fa fa-file-pdf-o"></i>
            </view>
            <text class="action-name">生成报表</text>
          </view>
          <view class="action-item" @click="navigateTo('product-catalog')">
            <view class="action-icon">
              <i class="fa fa-th-large"></i>
            </view>
            <text class="action-name">产品目录</text>
          </view>
        </view>
      </view>

      <!-- 数据概览 -->
      <view class="stats-overview">
        <view class="section-header">
          <text class="section-title">业务数据</text>
          <text class="view-more" @click="navigateTo('detailed-stats')">详情</text>
        </view>
        <view class="stats-grid">
          <view class="stat-card">
            <text class="stat-value">{{ totalCustomers }}</text>
            <text class="stat-label">总客户数</text>
            <text class="stat-change positive">+8 本周</text>
          </view>
          <view class="stat-card">
            <text class="stat-value">{{ openOpportunities }}</text>
            <text class="stat-label">活跃商机</text>
            <text class="stat-change positive">+3 本周</text>
          </view>
          <view class="stat-card">
            <text class="stat-value">¥{{ monthlySales.toLocaleString() }}</text>
            <text class="stat-label">本月销售额</text>
            <text class="stat-change negative">-2% 环比</text>
          </view>
        </view>
      </view>

      <!-- 待办任务 -->
      <view class="tasks-section">
        <view class="section-header">
          <text class="section-title">今日任务</text>
          <text class="view-more" @click="navigateTo('all-tasks')">全部</text>
        </view>
        <view class="tasks-list">
          <view class="task-item" v-for="(task, index) in todayTasks" :key="index">
            <view class="task-checkbox" @click="toggleTaskCompletion(index)">
              <i class="fa" :class="task.completed ? 'fa-check-square-o' : 'fa-square-o'"></i>
            </view>
            <view class="task-info">
              <text class="task-title" :class="task.completed ? 'completed-task' : ''">{{ task.title }}</text>
              <text class="task-meta">{{ task.relatedTo }} · {{ task.dueTime }}</text>
            </view>
            <view class="task-priority" :class="task.priority">
              <text class="priority-text">{{ task.priorityText }}</text>
            </view>
          </view>
        </view>
      </view>

      <!-- 最近客户 -->
      <view class="recent-customers">
        <view class="section-header">
          <text class="section-title">最近客户</text>
          <text class="view-more" @click="navigateTo('customer-list')">全部</text>
        </view>
        <view class="customers-list">
          <view class="customer-item" v-for="(customer, index) in recentCustomers" :key="index">
            <image :src="customer.avatar" class="customer-photo" mode="widthFix"></image>
            <view class="customer-details">
              <text class="customer-name">{{ customer.name }}</text>
              <text class="customer-company">{{ customer.company }}</text>
              <view class="customer-status">
                <text class="status-badge" :class="customer.statusClass">{{ customer.status }}</text>
                <text class="last-contact">{{ customer.lastContact }}</text>
              </view>
            </view>
            <button class="contact-btn" @click="contactCustomer(customer.id)">
              <i class="fa fa-comment"></i>
            </button>
          </view>
        </view>
      </view>
    </scroll-view>

    <!-- 悬浮添加按钮 -->
    <button class="floating-add-btn" @click="showQuickAddMenu">
      <i class="fa fa-plus"></i>
    </button>
  </view>
</template>

<script>
export default {
  data() {
    return {
      // 通知数量
      unreadNotifications: 3,
      // 当前日期
      currentDate: '',
      // 进度百分比
      progressPercent: 68,
      // 业务数据
      totalCustomers: 215,
      openOpportunities: 28,
      monthlySales: 156800,
      // 今日任务
      todayTasks: [
        {
          title: '跟进新产品方案反馈',
          relatedTo: '与 腾讯科技',
          dueTime: '14:30',
          priority: 'high',
          priorityText: '高',
          completed: false
        },
        {
          title: '发送修订后的报价单',
          relatedTo: '百度公司',
          dueTime: '16:00',
          priority: 'medium',
          priorityText: '中',
          completed: false
        },
        {
          title: '整理客户拜访记录',
          relatedTo: '阿里巴巴',
          dueTime: '17:30',
          priority: 'low',
          priorityText: '低',
          completed: false
        }
      ],
      // 最近客户
      recentCustomers: [
        {
          id: 1001,
          name: '张总监',
          company: '字节跳动',
          avatar: '/static/avatar1.png',
          status: '跟进中',
          statusClass: 'status-following',
          lastContact: '昨天 10:23'
        },
        {
          id: 1002,
          name: '王经理',
          company: '华为技术',
          avatar: '/static/avatar2.png',
          status: '已签约',
          statusClass: 'status-signed',
          lastContact: '3天前'
        },
        {
          id: 1003,
          name: '刘总',
          company: '小米集团',
          avatar: '/static/avatar3.png',
          status: '需求评估',
          statusClass: 'status-evaluating',
          lastContact: '上周'
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
      const options = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' };
      this.currentDate = new Date().toLocaleDateString('zh-CN', options);
    },
    // 导航到指定页面
    navigateTo(page) {
      uni.navigateTo({
        url: `/pages/${page}/${page}`
      });
    },
    // 切换任务完成状态
    toggleTaskCompletion(index) {
      this.todayTasks[index].completed = !this.todayTasks[index].completed;
    },
    // 联系客户
    contactCustomer(id) {
      uni.navigateTo({
        url: `/pages/customer-contact/customer-contact?id=${id}`
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
    // 显示快速添加菜单
    showQuickAddMenu() {
      console.log('显示快速添加菜单');
    }
  }
};
</script>

<style scoped>
/* 基础样式与自适应设置 */
.crm-container {
  display: flex;
  flex-direction: column;
  height: 100%;
  width: 100%;
  background-color: #f5f7fa;
  box-sizing: border-box;
}

/* 顶部导航栏 */
.top-nav {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16rpx 4%;
  background-color: #2d3e50;
  color: white;
  box-sizing: border-box;
}

.logo-area {
  display: flex;
  align-items: center;
}

.logo-area i {
  font-size: 36rpx;
  margin-right: 12rpx;
}

.app-title {
  font-size: 32rpx;
  font-weight: bold;
}

.user-controls {
  display: flex;
  align-items: center;
}

.nav-btn {
  background-color: transparent;
  border: none;
  color: white;
  font-size: 30rpx;
  margin-right: 30rpx;
  position: relative;
}

.notification-badge {
  position: absolute;
  top: -10rpx;
  right: -10rpx;
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

.user-avatar {
  width: 60rpx;
  height: 60rpx;
  border-radius: 50%;
  overflow: hidden;
}

.user-avatar image {
  width: 100%;
  height: 100%;
}

/* 主内容区域 */
.main-content {
  flex: 1;
  padding: 30rpx 4%;
  box-sizing: border-box;
}

/* 用户欢迎区 */
.welcome-panel {
  background-color: white;
  border-radius: 16rpx;
  padding: 30rpx;
  margin-bottom: 30rpx;
  box-shadow: 0 2rpx 10rpx rgba(0, 0, 0, 0.05);
}

.welcome-text {
  margin-bottom: 25rpx;
}

.greeting {
  font-size: 34rpx;
  font-weight: bold;
  color: #2d3e50;
}

.date-info {
  font-size: 26rpx;
  color: #7f8c8d;
  display: block;
  margin-top: 10rpx;
}

.performance-indicator {
  margin-top: 20rpx;
}

.indicator-label {
  font-size: 26rpx;
  color: #2d3e50;
  display: block;
  margin-bottom: 10rpx;
}

.progress-container {
  width: 100%;
  height: 12rpx;
  background-color: #f1f1f1;
  border-radius: 6rpx;
  overflow: hidden;
}

.progress-bar {
  height: 100%;
  background-color: #3498db;
  border-radius: 6rpx;
  transition: width 0.5s ease;
}

.progress-text {
  font-size: 24rpx;
  color: #3498db;
  display: block;
  margin-top: 10rpx;
  text-align: right;
}

/* 快捷功能区 */
.quick-actions {
  background-color: white;
  border-radius: 16rpx;
  padding: 30rpx;
  margin-bottom: 30rpx;
  box-shadow: 0 2rpx 10rpx rgba(0, 0, 0, 0.05);
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25rpx;
}

.section-title {
  font-size: 30rpx;
  font-weight: bold;
  color: #2d3e50;
}

.view-more {
  font-size: 24rpx;
  color: #3498db;
}

.actions-grid {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
}

.action-item {
  width: 31%;
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-bottom: 25rpx;
  padding: 20rpx 0;
  border-radius: 12rpx;
  background-color: #f9f9f9;
  box-sizing: border-box;
}

.action-icon {
  width: 70rpx;
  height: 70rpx;
  border-radius: 15rpx;
  background-color: #ebf5fb;
  color: #3498db;
  display: flex;
  justify-content: center;
  align-items: center;
  font-size: 36rpx;
  margin-bottom: 15rpx;
}

.action-name {
  font-size: 24rpx;
  color: #2d3e50;
  text-align: center;
}

/* 数据概览 */
.stats-overview {
  background-color: white;
  border-radius: 16rpx;
  padding: 30rpx;
  margin-bottom: 30rpx;
  box-shadow: 0 2rpx 10rpx rgba(0, 0, 0, 0.05);
}

.stats-grid {
  display: flex;
  justify-content: space-between;
  gap: 20rpx;
}

.stat-card {
  flex: 1;
  background-color: #f9f9f9;
  border-radius: 12rpx;
  padding: 20rpx;
  text-align: center;
}

.stat-value {
  font-size: 36rpx;
  font-weight: bold;
  color: #2d3e50;
  display: block;
  margin-bottom: 10rpx;
}

.stat-label {
  font-size: 24rpx;
  color: #7f8c8d;
  display: block;
  margin-bottom: 5rpx;
}

.stat-change {
  font-size: 22rpx;
  display: block;
}

.positive {
  color: #27ae60;
}

.negative {
  color: #e74c3c;
}

/* 待办任务 */
.tasks-section {
  background-color: white;
  border-radius: 16rpx;
  padding: 30rpx;
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
  width: 50rpx;
  height: 50rpx;
  display: flex;
  justify-content: center;
  align-items: center;
  color: #3498db;
  font-size: 30rpx;
}

.task-info {
  flex: 1;
  margin-left: 15rpx;
}

.task-title {
  font-size: 26rpx;
  color: #2d3e50;
  display: block;
}

.completed-task {
  text-decoration: line-through;
  color: #95a5a6;
}

.task-meta {
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

/* 最近客户 */
.recent-customers {
  background-color: white;
  border-radius: 16rpx;
  padding: 30rpx;
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

.customer-photo {
  width: 70rpx;
  height: 70rpx;
  border-radius: 50%;
  margin-right: 20rpx;
}

.customer-details {
  flex: 1;
}

.customer-name {
  font-size: 28rpx;
  font-weight: bold;
  color: #2d3e50;
  display: block;
}

.customer-company {
  font-size: 24rpx;
  color: #7f8c8d;
  display: block;
  margin-top: 5rpx;
}

.customer-status {
  display: flex;
  align-items: center;
  margin-top: 8rpx;
}

.status-badge {
  font-size: 22rpx;
  padding: 3rpx 12rpx;
  border-radius: 20rpx;
  display: inline-block;
}

.status-following {
  background-color: #ebf5fb;
  color: #3498db;
}

.status-signed {
  background-color: #eafaf1;
  color: #27ae60;
}

.status-evaluating {
  background-color: #fef5e7;
  color: #f39c12;
}

.last-contact {
  font-size: 22rpx;
  color: #7f8c8d;
  margin-left: 15rpx;
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

/* 悬浮添加按钮 */
.floating-add-btn {
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

/* 响应式调整 - 针对小屏设备 */
@media screen and (max-width: 320px) {
  .action-name {
    font-size: 22rpx;
  }
  
  .stat-value {
    font-size: 32rpx;
  }
  
  .task-title {
    font-size: 24rpx;
  }
}

/* 响应式调整 - 针对大屏设备 */
@media screen and (min-width: 414px) {
  .action-icon {
    width: 80rpx;
    height: 80rpx;
    font-size: 40rpx;
  }
  
  .stat-value {
    font-size: 40rpx;
  }
  
  .greeting {
    font-size: 38rpx;
  }
}
</style>
