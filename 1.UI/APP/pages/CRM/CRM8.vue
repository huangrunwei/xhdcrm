<template>
  <view class="container">
    <!-- 顶部导航栏 -->
    <view class="navbar">
      <text class="navbar-title">客户列表</text>
    </view>

    <!-- 搜索区域 -->
    <view class="search-container">
      <!-- 基础搜索栏 -->
      <view class="basic-search">
        <view class="search-input">
          <icon type="search" size="18" color="#999"></icon>
          <input 
            type="text" 
            placeholder="输入客户名称或电话搜索" 
            v-model="keyword"
            @confirm="handleSearch"
          ></input>
        </view>
        <button class="expand-btn" @click="showAdvanced = !showAdvanced">
          <text>{{ showAdvanced ? '收起' : '高级搜索' }}</text>
          <icon type="arrow-down" size="16" :class="{ 'rotate': showAdvanced }"></icon>
        </button>
      </view>

      <!-- 高级搜索区域 -->
      <view class="advanced-search" v-if="showAdvanced">
        <view class="search-row">
          <!-- 客户输入框 -->
          <view class="search-item">
            <text class="label">客户</text>
            <input 
              type="text" 
              placeholder="请输入客户名称" 
              v-model="searchParams.customerName"
            ></input>
          </view>
          
          <!-- 电话输入框 -->
          <view class="search-item">
            <text class="label">电话</text>
            <input 
              type="number" 
              placeholder="请输入联系电话" 
              v-model="searchParams.phone"
            ></input>
          </view>
        </view>
        
        <view class="search-row">
          <!-- 创建时间选择 -->
          <view class="search-item">
            <text class="label">创建时间</text>
            <picker mode="date" start="2000-01-01" end="2099-12-31" v-model="searchParams.createTime">
              <view class="picker-view">
                {{ searchParams.createTime || '请选择日期' }}
                <icon type="arrow-right" size="16" color="#999"></icon>
              </view>
            </picker>
          </view>
          
          <!-- 行业下拉 -->
          <view class="search-item">
            <text class="label">行业</text>
            <picker mode="selector" :range="industryList" v-model="searchParams.industryIndex">
              <view class="picker-view">
                {{ industryList[searchParams.industryIndex] || '请选择行业' }}
                <icon type="arrow-right" size="16" color="#999"></icon>
              </view>
            </picker>
          </view>
        </view>
        
        <view class="search-row">
          <!-- 类型下拉 -->
          <view class="search-item">
            <text class="label">类型</text>
            <picker mode="selector" :range="typeList" v-model="searchParams.typeIndex">
              <view class="picker-view">
                {{ typeList[searchParams.typeIndex] || '请选择类型' }}
                <icon type="arrow-right" size="16" color="#999"></icon>
              </view>
            </picker>
          </view>
          
          <!-- 级别下拉 -->
          <view class="search-item">
            <text class="label">级别</text>
            <picker mode="selector" :range="levelList" v-model="searchParams.levelIndex">
              <view class="picker-view">
                {{ levelList[searchParams.levelIndex] || '请选择级别' }}
                <icon type="arrow-right" size="16" color="#999"></icon>
              </view>
            </picker>
          </view>
        </view>
        
        <view class="search-row">
          <!-- 来源下拉 -->
          <view class="search-item">
            <text class="label">来源</text>
            <picker mode="selector" :range="sourceList" v-model="searchParams.sourceIndex">
              <view class="picker-view">
                {{ sourceList[searchParams.sourceIndex] || '请选择来源' }}
                <icon type="arrow-right" size="16" color="#999"></icon>
              </view>
            </picker>
          </view>
          
          <!-- 省份下拉 -->
          <view class="search-item">
            <text class="label">省份</text>
            <picker mode="selector" :range="provinceList" v-model="searchParams.provinceIndex">
              <view class="picker-view">
                {{ provinceList[searchParams.provinceIndex] || '请选择省份' }}
                <icon type="arrow-right" size="16" color="#999"></icon>
              </view>
            </picker>
          </view>
        </view>
        
        <view class="search-row">
          <!-- 城市下拉 -->
          <view class="search-item">
            <text class="label">城市</text>
            <picker mode="selector" :range="cityList" v-model="searchParams.cityIndex">
              <view class="picker-view">
                {{ cityList[searchParams.cityIndex] || '请选择城市' }}
                <icon type="arrow-right" size="16" color="#999"></icon>
              </view>
            </picker>
          </view>
          
          <!-- 状态下拉 -->
          <view class="search-item">
            <text class="label">状态</text>
            <picker mode="selector" :range="statusList" v-model="searchParams.statusIndex">
              <view class="picker-view">
                {{ statusList[searchParams.statusIndex] || '请选择状态' }}
                <icon type="arrow-right" size="16" color="#999"></icon>
              </view>
            </picker>
          </view>
        </view>
        
        <view class="search-row">
          <!-- 归属下拉 -->
          <view class="search-item">
            <text class="label">归属</text>
            <picker mode="selector" :range="ownerList" v-model="searchParams.ownerIndex">
              <view class="picker-view">
                {{ ownerList[searchParams.ownerIndex] || '请选择归属' }}
                <icon type="arrow-right" size="16" color="#999"></icon>
              </view>
            </picker>
          </view>
        </view>
        
        <!-- 搜索按钮 -->
        <view class="search-btns">
          <button class="reset-btn" @click="resetSearch">重置</button>
          <button class="do-search-btn" @click="handleSearch">搜索</button>
        </view>
      </view>
    </view>

    <!-- 列表区域 - 使用scroll-view实现滚动控制 -->
    <scroll-view 
      class="list-container" 
      scroll-y="true"
      ref="listScroll"
      @scrolltolower="onReachBottom"
      @scroll="onScroll"
    >
      <view class="list-content">
        <view class="customer-item" v-for="(item, index) in customerList" :key="index">
          <view class="item-row">
            <text class="label">客户名：</text>
            <text class="content">{{ item.name }}</text>
          </view>
          <view class="item-row">
            <text class="label">电话：</text>
            <text class="content">{{ item.phone }}</text>
          </view>
          <view class="item-row">
            <text class="label">地址：</text>
            <text class="content">{{ item.address }}</text>
          </view>
          <view class="item-row extra-info">
            <text class="content">最后跟进：{{ item.lastFollowTime }}</text>
          </view>
        </view>
        
        <!-- 加载更多 -->
        <view class="load-more" v-if="loading">
          <loading :show="true" size="16"></loading>
          <text class="load-text">加载中...</text>
        </view>
        
        <!-- 没有更多数据 -->
        <view class="no-more" v-if="!hasMore && !loading">
          <text>没有更多数据了</text>
        </view>
        
        <!-- 空状态 -->
        <view class="empty-state" v-if="customerList.length === 0 && !loading">
          <image src="/static/images/empty.png" mode="widthFix" class="empty-img"></image>
          <text class="empty-text">暂无客户数据</text>
        </view>
      </view>
    </scroll-view>

    <!-- 右下角悬浮添加按钮 -->
    <button class="float-add-btn" @click="handleAdd">
      <icon type="plus" size="24" color="#fff"></icon>
    </button>
    
    <!-- 回到顶部按钮 - 滚动一定距离后显示 -->
    <button 
      class="back-to-top" 
      @click="scrollToTop"
      v-if="showBackToTop"
    >
      <icon type="arrowup" size="20" color="#fff"></icon>
    </button>
  </view>
</template>

<script>
export default {
  data() {
    return {
      // 基础搜索关键词
      keyword: '',
      // 是否显示高级搜索
      showAdvanced: false,
      // 搜索参数
      searchParams: {
        customerName: '',
        phone: '',
        createTime: '',
        industryIndex: -1,
        typeIndex: -1,
        levelIndex: -1,
        sourceIndex: -1,
        provinceIndex: -1,
        cityIndex: -1,
        statusIndex: -1,
        ownerIndex: -1
      },
      // 下拉选项数据
      industryList: ['IT', '金融', '教育', '医疗', '零售', '制造业'],
      typeList: ['潜在客户', '意向客户', '签约客户', '流失客户'],
      levelList: ['A级', 'B级', 'C级', 'D级'],
      sourceList: ['网站咨询', '电话营销', '客户推荐', '线下活动', '其他'],
      provinceList: ['北京', '上海', '广东', '江苏', '浙江', '山东'],
      cityList: ['北京', '上海', '广州', '深圳', '杭州', '南京'],
      statusList: ['活跃', '休眠', '已合作', '已终止'],
      ownerList: ['张三', '李四', '王五', '赵六', '钱七'],
      // 客户列表数据
      customerList: [],
      // 分页参数
      page: 1,
      pageSize: 10,
      // 加载状态
      loading: false,
      // 是否还有更多数据
      hasMore: true,
      // 控制回到顶部按钮显示
      showBackToTop: false,
      // 滚动位置记录
      scrollTop: 0
    };
  },
  onLoad() {
    // 初始化加载数据
    this.loadCustomerList();
  },
  methods: {
    // 加载客户列表
    loadCustomerList() {
      this.loading = true;
      
      // 模拟API请求
      setTimeout(() => {
        // 生成模拟数据
        const newData = [];
        for (let i = 0; i < this.pageSize; i++) {
          const index = (this.page - 1) * this.pageSize + i;
          newData.push({
            name: `客户${index + 1}`,
            phone: `138${Math.floor(Math.random() * 100000000)}`,
            address: `${this.provinceList[Math.floor(Math.random() * this.provinceList.length)]}${this.cityList[Math.floor(Math.random() * this.cityList.length)]}区${Math.floor(Math.random() * 100)}号`,
            lastFollowTime: `2023-${Math.floor(Math.random() * 12) + 1}-${Math.floor(Math.random() * 28) + 1} ${Math.floor(Math.random() * 24)}:${Math.floor(Math.random() * 60).toString().padStart(2, '0')}`
          });
        }
        
        // 合并数据
        this.customerList = this.page === 1 ? newData : [...this.customerList, ...newData];
        
        // 模拟数据到底
        if (this.page >= 3) {
          this.hasMore = false;
        }
        
        this.loading = false;
      }, 1000);
    },
    
    // 下拉加载更多
    onReachBottom() {
      if (!this.loading && this.hasMore) {
        this.page++;
        this.loadCustomerList();
      }
    },
    
    // 处理搜索
    handleSearch() {
      // 重置分页
      this.page = 1;
      this.hasMore = true;
      // 执行搜索逻辑，这里简单模拟
      this.loadCustomerList();
      
      // 收起键盘
      uni.hideKeyboard();
    },
    
    // 重置搜索条件
    resetSearch() {
      this.searchParams = {
        customerName: '',
        phone: '',
        createTime: '',
        industryIndex: -1,
        typeIndex: -1,
        levelIndex: -1,
        sourceIndex: -1,
        provinceIndex: -1,
        cityIndex: -1,
        statusIndex: -1,
        ownerIndex: -1
      };
      this.keyword = '';
    },
    
    // 监听滚动事件
    onScroll(e) {
      this.scrollTop = e.detail.scrollTop;
      // 滚动超过500px显示回到顶部按钮
      this.showBackToTop = this.scrollTop > 500;
    },
    
    // 回到顶部
    scrollToTop() {
      this.$refs.listScroll.scrollTo({
        scrollTop: 0,
        duration: 300 // 滚动动画时长
      });
    },
    
    // 点击添加按钮
    handleAdd() {
      // 跳转到添加客户页面
      uni.navigateTo({
        url: '/pages/addCustomer/addCustomer'
      });
    }
  }
};
</script>

<style scoped>
.container {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  background-color: #f5f5f5;
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

/* 搜索区域样式 */
.search-container {
  padding: 10px;
  background-color: #fff;
  border-bottom: 1px solid #eee;
}

.basic-search {
  display: flex;
  align-items: center;
}

.search-input {
  flex: 1;
  display: flex;
  align-items: center;
  background-color: #f5f5f5;
  border-radius: 20px;
  padding: 8px 15px;
  margin-right: 10px;
}

.search-input input {
  flex: 1;
  margin-left: 5px;
  font-size: 14px;
  background-color: transparent;
}

.expand-btn {
  background-color: transparent;
  color: #1677ff;
  font-size: 14px;
  padding: 5px 10px;
  display: flex;
  align-items: center;
}

.expand-btn icon {
  margin-left: 3px;
  transition: transform 0.3s ease;
}

.expand-btn .rotate {
  transform: rotate(180deg);
}

/* 高级搜索样式 */
.advanced-search {
  margin-top: 10px;
  animation: fadeIn 0.3s ease;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(-10px); }
  to { opacity: 1; transform: translateY(0); }
}

.search-row {
  display: flex;
  margin-bottom: 10px;
}

.search-item {
  flex: 1;
  display: flex;
  flex-direction: column;
  margin-right: 10px;
}

.search-item:last-child {
  margin-right: 0;
}

.label {
  font-size: 13px;
  color: #666;
  margin-bottom: 5px;
}

.search-item input {
  height: 36px;
  background-color: #f5f5f5;
  border-radius: 6px;
  padding: 0 10px;
  font-size: 14px;
}

.picker-view {
  height: 36px;
  background-color: #f5f5f5;
  border-radius: 6px;
  padding: 0 10px;
  font-size: 14px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.search-btns {
  display: flex;
  justify-content: center;
  margin-top: 15px;
  gap: 15px;
}

.reset-btn {
  width: 120px;
  height: 36px;
  line-height: 36px;
  background-color: #f5f5f5;
  color: #333;
  border-radius: 18px;
  font-size: 14px;
}

.do-search-btn {
  width: 120px;
  height: 36px;
  line-height: 36px;
  background-color: #1677ff;
  color: #fff;
  border-radius: 18px;
  font-size: 14px;
}

/* 列表区域样式 */
.list-container {
  flex: 1;
  width: 100%;
  overflow: hidden;
}

.list-content {
  padding: 10px;
}

.customer-item {
  background-color: #fff;
  border-radius: 10px;
  padding: 12px 15px;
  margin-bottom: 10px;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.05);
}

.item-row {
  display: flex;
  margin-bottom: 8px;
}

.item-row:last-child {
  margin-bottom: 0;
}

.item-row .label {
  color: #666;
  font-size: 14px;
  width: 70px;
}

.item-row .content {
  flex: 1;
  color: #333;
  font-size: 14px;
  word-break: break-all;
}

.extra-info {
  margin-top: 5px;
  padding-top: 5px;
  border-top: 1px dashed #eee;
}

.extra-info .content {
  color: #999;
  font-size: 13px;
}

/* 加载更多样式 */
.load-more {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 15px 0;
  color: #999;
  font-size: 14px;
}

.load-text {
  margin-left: 5px;
}

/* 没有更多数据 */
.no-more {
  text-align: center;
  padding: 15px 0;
  color: #999;
  font-size: 14px;
}

/* 空状态 */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 50px 0;
}

.empty-img {
  width: 120px;
  margin-bottom: 15px;
}

.empty-text {
  color: #999;
  font-size: 14px;
}

/* 悬浮添加按钮样式 */
.float-add-btn {
  position: fixed;
  right: 20px;
  bottom: 80px; /* 留出回到顶部按钮的位置 */
  width: 50px;
  height: 50px;
  border-radius: 50%;
  background-color: #1677ff;
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 10px rgba(22, 119, 255, 0.3);
  z-index: 999;
  padding: 0;
}

.float-add-btn::after {
  border: none;
}

/* 回到顶部按钮样式 */
.back-to-top {
  position: fixed;
  right: 20px;
  bottom: 20px;
  width: 44px;
  height: 44px;
  border-radius: 50%;
  background-color: #666;
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
  z-index: 999;
  padding: 0;
  opacity: 0;
  transform: translateY(20px);
  animation: fadeInUp 0.3s ease forwards;
}

.back-to-top::after {
  border: none;
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
    