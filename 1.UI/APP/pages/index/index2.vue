<template>
  <view class="crm-home">
    <!-- 顶部导航 -->
    <view class="top-nav">
      <view class="search-bar">
        <i class="fa fa-search"></i>
        <input type="text" placeholder="搜索客户、任务或文档..." class="search-input" />
      </view>
      <view class="user-actions">
        <button class="action-btn" @click="showNotifications">
          <i class="fa fa-bell-o"></i>
          <span class="badge" v-if="notificationCount > 0">{{ notificationCount }}</span>
        </button>
        <button class="action-btn" @click="showSettings">
          <i class="fa fa-cog"></i>
        </button>
        <view class="user-profile" @click="showProfile">
          <image src="/static/avatar-default.png" class="user-avatar" mode="widthFix"></image>
        </view>
      </view>
    </view>

    <!-- 页面内容 -->
    <scroll-view class="main-content" scroll-y="true">
      <!-- 用户信息与日期 -->
      <view class="user-info-section">
        <view class="greeting">
          <text class="welcome">欢迎回来，王销售</text>
          <text class="current-date">{{ formattedDate }}</text>
        </view>
        <view class="performance-badge">
          <text class="badge-text">本月业绩: 85%</text>
          <view class="progress-bar">
            <view class="progress" :style="{ width: '85%' }"></view>
          </view>
        </view>
      </view>

      <!-- 快捷操作 -->
      <view class="quick-actions">
        <view class="section-header">
          <text class="section-title">常用功能</text>
        </view>
        <view class="actions-grid">
          <view class="action-card" @click="navigateTo('add-customer')">
            <view class="action-icon bg-blue">
              <i class="fa fa-user-plus"></i>
            </view>
            <text class="action-name">新增客户</text>
          </view>
          <view class="action-card" @click="navigateTo('add-opportunity')">
            <view class="action-icon bg-green">
              <i class="fa fa-handshake-o"></i>
            </view>
            <text class="action-name">新增商机</text>
          </view>
          <view class="action-card" @click="navigateTo('schedule-call')">
            <view class="action-icon bg-orange">
              <i class="fa fa-phone"></i>
            </view>
            <text class="action-name">安排通话</text>
          </view>
          <view class="action-card" @click="navigateTo('add-task')">
            <view class="action-icon bg-purple">
              <i class="fa fa-list-alt"></i>
            </view>
            <text class="action-name">创建任务</text>
          </view>
          <view class="action-card" @click="navigateTo('calendar')">
            <view class="action-icon bg-teal">
              <i class="fa fa-calendar"></i>
            </view>
            <text class="action-name">日程安排</text>
          </view>
          <view class="action-card" @click="navigateTo('quotes')">
            <view class="action-icon bg-red">
              <i class="fa fa-file-text-o"></i>
            </view>
            <text class="action-name">生成报价</text>
          </view>
        </view>
      </view>

      <!-- 关键指标 -->
      <view class="key-metrics">
        <view class="section-header">
          <text class="section-title">业绩指标</text>
          <text class="view-all" @click="navigateTo('metrics')">查看全部</text>
        </view>
        <view class="metrics-cards">
          <view class="metric-card">
            <text class="metric-label">总客户数</text>
            <text class="metric-value">187</text>
            <view class="metric-trend up">
              <i class="fa fa-arrow-up"></i>
              <text>12 新增</text>
            </view>
          </view>
          <view class="metric-card">
            <text class="metric-label">进行中商机</text>
            <text class="metric-value">24</text>
            <view class="metric-trend up">
              <i class="fa fa-arrow-up"></i>
              <text>3 新增</text>
            </view>
          </view>
          <view class="metric-card">
            <text class="metric-label">本月销售额</text>
            <text class="metric-value">¥98,600</text>
            <view class="metric-trend down">
              <i class="fa fa-arrow-down"></i>
              <text>5% 环比</text>
            </view>
          </view>
        </view>
      </view>

      <!-- 待办任务 -->
      <view class="pending-tasks">
        <view class="section-header">
          <text class="section-title">今日待办</text>
          <text class="view-all" @click="navigateTo('all-tasks')">全部任务</text>
        </view>
        <view class="tasks-container">
          <view class="task" v-for="(task, index) in pendingTasks" :key="index">
            <view class="task-checkbox" @click="toggleTask(index)">
              <i class="fa" :class="task.completed ? 'fa-check-square-o' : 'fa-square-o'"></i>
            </view>
            <view class="task-details">
              <text class="task-title" :class="task.completed ? 'completed' : ''">{{ task.title }}</text>
              <text class="task-related">{{ task.relatedTo }}</text>
            </view>
            <view class="task-time">
              <text class="time-text">{{ task.time }}</text>
            </view>
          </view>
        </view>
      </view>

      <!-- 重点客户 -->
      <view class="key-customers">
        <view class="section-header">
          <text class="section-title">重点客户</text>
          <text class="view-all" @click="navigateTo('all-customers')">客户列表</text>
        </view>
        <view class="customers-container">
          <view class="customer-card" v-for="(customer, index) in keyCustomers" :key="index">
            <image :src="customer.avatar" class="customer-avatar" mode="widthFix"></image>
            <view class="customer-info">
              <text class="customer-name">{{ customer.name }}</text>
              <text class="customer-company">{{ customer.company }}</text>
              <view class="customer-status">
                <view class="status-dot" :class="customer.statusColor"></view>
                <text class="status-text">{{ customer.status }}</text>
              </view>
            </view>
            <view class="customer-actions">
              <button class="contact-btn" @click="contactCustomer(customer.id)">
                <i class="fa fa-phone"></i>
              </button>
              <button class="more-btn" @click="showCustomerOptions(customer.id)">
                <i class="fa fa-ellipsis-v"></i>
              </button>
            </view>
          </view>
        </view>
      </view>
    </scroll-view>

    <!-- 底部导航 -->
    <view class="bottom-nav">
      <view class="nav-item active" @click="switchTab('home')">
        <i class="fa fa-home"></i>
        <text>首页</text>
      </view>
      <view class="nav-item" @click="switchTab('customers')">
        <i class="fa fa-users"></i>
        <text>客户</text>
      </view>
      <view class="nav-item add-btn" @click="navigateTo('quick-add')">
        <i class="fa fa-plus"></i>
      </view>
      <view class="nav-item" @click="switchTab('opportunities')">
        <i class="fa fa-briefcase"></i>
        <text>商机</text>
      </view>
      <view class="nav-item" @click="switchTab('reports')">
        <i class="fa fa-bar-chart"></i>
        <text>报表</text>
      </view>
    </view>
  </view>
</template>

<script>
export default {
  data() {
    return {
      notificationCount: 5,
      formattedDate: '',
      pendingTasks: [
        {
          title: '跟进新产品方案',
          relatedTo: '与 阿里巴巴 李总',
          time: '10:30',
          completed: false
        },
        {
          title: '发送合同修订版',
          relatedTo: '腾讯科技有限公司',
          time: '14:00',
          completed: false
        },
        {
          title: '准备季度汇报材料',
          relatedTo: '内部会议',
          time: '16:30',
          completed: false
        }
      ],
      keyCustomers: [
        {
          id: 1,
          name: '张总监',
          company: '百度科技',
          avatar: '/static/customer1.png',
          status: '需跟进',
          statusColor: 'red'
        },
        {
          id: 2,
          name: '李经理',
          company: '字节跳动',
          avatar: '/static/customer2.png',
          status: '谈判中',
          statusColor: 'orange'
        },
        {
          id: 3,
          name: '王总',
          company: '华为技术',
          avatar: '/static/customer3.png',
          status: '已签约',
          statusColor: 'green'
        }
      ]
    };
  },
  onLoad() {
    // 设置当前日期
    this.setFormattedDate();
  },
  methods: {
    setFormattedDate() {
      const now = new Date();
      const options = { 
        weekday: 'long', 
        year: 'numeric', 
        month: 'long', 
        day: 'numeric' 
      };
      this.formattedDate = now.toLocaleDateString('zh-CN', options);
    },
    navigateTo(page) {
      // 导航到指定页面
      uni.navigateTo({
        url: `/pages/${page}/${page}`
      });
    },
    switchTab(tab) {
      // 切换底部导航
      uni.switchTab({
        url: `/pages/${tab}/${tab}`
      });
    },
    toggleTask(index) {
      // 切换任务完成状态
      this.pendingTasks[index].completed = !this.pendingTasks[index].completed;
    },
    contactCustomer(id) {
      // 联系客户
      uni.navigateTo({
        url: `/pages/customer-contact/customer-contact?id=${id}`
      });
    },
    showCustomerOptions(id) {
      // 显示客户操作选项
      console.log('显示客户', id, '的操作选项');
    },
    showNotifications() {
      // 显示通知
      uni.navigateTo({
        url: '/pages/notifications/notifications'
      });
    },
    showSettings() {
      // 显示设置
      uni.navigateTo({
        url: '/pages/settings/settings'
      });
    },
    showProfile() {
      // 显示个人资料
      uni.navigateTo({
        url: '/pages/profile/profile'
      });
    }
  }
};
</script>

<style scoped>
/* 基础样式 */
.crm-home {
  display: flex;
  flex-direction: column;
  height: 100%;
  background-color: #f7f8fa;
}

/* 顶部导航 */
.top-nav {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16rpx 24rpx;
  background-color: #ffffff;
  border-bottom: 1px solid #f0f2f5;
}

.search-bar {
  display: flex;
  align-items: center;
  background-color: #f0f2f5;
  border-radius: 30rpx;
  padding: 12rpx 20rpx;
  width: 60%;
}

.search-bar i {
  color: #8c8c8c;
  font-size: 28rpx;
  margin-right: 10rpx;
}

.search-input {
  flex: 1;
  font-size: 26rpx;
  background-color: transparent;
  border: none;
  outline: none;
  color: #333;
}

.user-actions {
  display: flex;
  align-items: center;
}

.action-btn {
  background-color: transparent;
  border: none;
  color: #666;
  font-size: 32rpx;
  margin-left: 24rpx;
  position: relative;
}

.badge {
  position: absolute;
  top: -8rpx;
  right: -8rpx;
  background-color: #ff3b30;
  color: white;
  font-size: 20rpx;
  width: 28rpx;
  height: 28rpx;
  border-radius: 50%;
  display: flex;
  justify-content: center;
  align-items: center;
}

.user-profile {
  margin-left: 24rpx;
}

.user-avatar {
  width: 64rpx;
  height: 64rpx;
  border-radius: 50%;
  border: 2rpx solid #eee;
}

/* 主内容区 */
.main-content {
  flex: 1;
  padding: 24rpx;
}

/* 用户信息区域 */
.user-info-section {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30rpx;
}

.greeting .welcome {
  font-size: 36rpx;
  font-weight: bold;
  color: #333;
}

.greeting .current-date {
  font-size: 24rpx;
  color: #666;
  display: block;
  margin-top: 8rpx;
}

.performance-badge {
  background-color: #fff;
  border-radius: 12rpx;
  padding: 16rpx;
  width: 240rpx;
  box-shadow: 0 2rpx 8rpx rgba(0, 0, 0, 0.05);
}

.badge-text {
  font-size: 24rpx;
  color: #333;
  display: block;
  margin-bottom: 10rpx;
}

.progress-bar {
  height: 10rpx;
  background-color: #f0f2f5;
  border-radius: 5rpx;
  overflow: hidden;
}

.progress {
  height: 100%;
  background-color: #00b42a;
  border-radius: 5rpx;
  transition: width 0.3s ease;
}

/* 快捷操作区域 */
.quick-actions {
  background-color: #fff;
  border-radius: 16rpx;
  padding: 24rpx;
  margin-bottom: 30rpx;
  box-shadow: 0 2rpx 8rpx rgba(0, 0, 0, 0.05);
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24rpx;
}

.section-title {
  font-size: 30rpx;
  font-weight: bold;
  color: #333;
}

.view-all {
  font-size: 24rpx;
  color: #165DFF;
}

.actions-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 20rpx;
}

.action-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 20rpx 0;
  border-radius: 12rpx;
  background-color: #f7f8fa;
  transition: all 0.2s ease;
}

.action-card:active {
  background-color: #eef0f5;
  transform: scale(0.98);
}

.action-icon {
  width: 80rpx;
  height: 80rpx;
  border-radius: 20rpx;
  display: flex;
  justify-content: center;
  align-items: center;
  margin-bottom: 16rpx;
  color: white;
  font-size: 40rpx;
}

.bg-blue {
  background-color: #165DFF;
}

.bg-green {
  background-color: #00b42a;
}

.bg-orange {
  background-color: #ff7d00;
}

.bg-purple {
  background-color: #722ed1;
}

.bg-teal {
  background-color: #0fc6c2;
}

.bg-red {
  background-color: #f53f3f;
}

.action-name {
  font-size: 24rpx;
  color: #333;
}

/* 关键指标区域 */
.key-metrics {
  background-color: #fff;
  border-radius: 16rpx;
  padding: 24rpx;
  margin-bottom: 30rpx;
  box-shadow: 0 2rpx 8rpx rgba(0, 0, 0, 0.05);
}

.metrics-cards {
  display: flex;
  justify-content: space-between;
  gap: 16rpx;
}

.metric-card {
  flex: 1;
  background-color: #f7f8fa;
  border-radius: 12rpx;
  padding: 20rpx;
  text-align: center;
}

.metric-label {
  font-size: 24rpx;
  color: #666;
  display: block;
  margin-bottom: 10rpx;
}

.metric-value {
  font-size: 36rpx;
  font-weight: bold;
  color: #333;
  display: block;
  margin-bottom: 10rpx;
}

.metric-trend {
  font-size: 22rpx;
  display: flex;
  align-items: center;
  justify-content: center;
}

.up {
  color: #00b42a;
}

.down {
  color: #f53f3f;
}

.metric-trend i {
  margin-right: 5rpx;
}

/* 待办任务区域 */
.pending-tasks {
  background-color: #fff;
  border-radius: 16rpx;
  padding: 24rpx;
  margin-bottom: 30rpx;
  box-shadow: 0 2rpx 8rpx rgba(0, 0, 0, 0.05);
}

.tasks-container {
  display: flex;
  flex-direction: column;
  gap: 16rpx;
}

.task {
  display: flex;
  align-items: center;
  padding: 16rpx;
  border-radius: 12rpx;
  background-color: #f7f8fa;
}

.task-checkbox {
  width: 50rpx;
  height: 50rpx;
  display: flex;
  justify-content: center;
  align-items: center;
  color: #165DFF;
  font-size: 30rpx;
}

.task-details {
  flex: 1;
  margin-left: 10rpx;
}

.task-title {
  font-size: 26rpx;
  color: #333;
  display: block;
}

.completed {
  text-decoration: line-through;
  color: #999;
}

.task-related {
  font-size: 22rpx;
  color: #666;
  display: block;
  margin-top: 4rpx;
}

.task-time {
  background-color: #e8f3ff;
  color: #165DFF;
  padding: 5rpx 15rpx;
  border-radius: 20rpx;
  font-size: 22rpx;
}

/* 重点客户区域 */
.key-customers {
  background-color: #fff;
  border-radius: 16rpx;
  padding: 24rpx;
  margin-bottom: 30rpx;
  box-shadow: 0 2rpx 8rpx rgba(0, 0, 0, 0.05);
}

.customers-container {
  display: flex;
  flex-direction: column;
  gap: 16rpx;
}

.customer-card {
  display: flex;
  align-items: center;
  padding: 16rpx;
  border-radius: 12rpx;
  background-color: #f7f8fa;
  transition: background-color 0.2s ease;
}

.customer-card:active {
  background-color: #eef0f5;
}

.customer-avatar {
  width: 80rpx;
  height: 80rpx;
  border-radius: 50%;
  margin-right: 20rpx;
}

.customer-info {
  flex: 1;
}

.customer-name {
  font-size: 28rpx;
  font-weight: bold;
  color: #333;
  display: block;
}

.customer-company {
  font-size: 24rpx;
  color: #666;
  display: block;
  margin-top: 4rpx;
}

.customer-status {
  display: flex;
  align-items: center;
  margin-top: 6rpx;
}

.status-dot {
  width: 16rpx;
  height: 16rpx;
  border-radius: 50%;
  margin-right: 8rpx;
}

.red {
  background-color: #f53f3f;
}

.orange {
  background-color: #ff7d00;
}

.green {
  background-color: #00b42a;
}

.status-text {
  font-size: 22rpx;
  color: #666;
}

.customer-actions {
  display: flex;
}

.contact-btn, .more-btn {
  width: 56rpx;
  height: 56rpx;
  border-radius: 50%;
  display: flex;
  justify-content: center;
  align-items: center;
  background-color: white;
  border: 1px solid #eee;
  color: #666;
  font-size: 26rpx;
  margin-left: 10rpx;
}

.contact-btn {
  color: #165DFF;
  border-color: #e8f3ff;
}

/* 底部导航 */
.bottom-nav {
  display: flex;
  justify-content: space-around;
  align-items: center;
  height: 120rpx;
  background-color: white;
  border-top: 1px solid #f0f2f5;
  padding: 0 20rpx;
}

.nav-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #999;
  font-size: 22rpx;
  flex: 1;
}

.nav-item i {
  font-size: 36rpx;
  margin-bottom: 8rpx;
}

.nav-item.active {
  color: #165DFF;
}

.add-btn {
  width: 80rpx;
  height: 80rpx;
  border-radius: 50%;
  background-color: #165DFF;
  color: white;
  display: flex;
  justify-content: center;
  align-items: center;
  font-size: 40rpx;
  margin-top: -40rpx;
  box-shadow: 0 4rpx 12rpx rgba(22, 93, 255, 0.3);
}
</style>
