<template>
  <view class="search-page">
    <!-- 标题栏 -->
    <view class="navbar">
      <text class="nav-title">客户搜索</text>
      <button class="nav-close" @click="onCancel">取消</button>
    </view>
    
    <!-- 搜索区域 -->
    <view class="search-container">
      <!-- 输入项：客户 -->
      <view class="search-item">
        <text class="search-label">客户</text>
        <input 
          type="text" 
          v-model="searchForm.customerName" 
          placeholder="请输入客户名称" 
          class="search-input"
        />
      </view>
      
      <!-- 输入项：电话 -->
      <view class="search-item">
        <text class="search-label">电话</text>
        <input 
          type="number" 
          v-model="searchForm.phone" 
          placeholder="请输入联系电话" 
          class="search-input"
        />
      </view>
      
      <!-- 时间选择：创建时间 -->
      <view class="search-item">
        <text class="search-label">创建时间</text>
        <view class="date-range" @click="showDatePicker = true">
          <text class="date-text">
            {{ searchForm.createTimeStart ? searchForm.createTimeStart : '开始日期' }} 
            - 
            {{ searchForm.createTimeEnd ? searchForm.createTimeEnd : '结束日期' }}
          </text>
          <uni-icons type="calendar" size="24" color="#666"></uni-icons>
        </view>
      </view>
      
      <!-- 下拉项：行业 -->
      <view class="search-item dropdown-item" @click="showDropdown('industry')">
        <text class="search-label">行业</text>
        <view class="dropdown-content">
          <text>{{ selectedIndustry || '请选择' }}</text>
          <uni-icons type="arrowright" size="24" color="#666"></uni-icons>
        </view>
      </view>
      
      <!-- 下拉项：类型 -->
      <view class="search-item dropdown-item" @click="showDropdown('type')">
        <text class="search-label">类型</text>
        <view class="dropdown-content">
          <text>{{ selectedType || '请选择' }}</text>
          <uni-icons type="arrowright" size="24" color="#666"></uni-icons>
        </view>
      </view>
      
      <!-- 下拉项：级别 -->
      <view class="search-item dropdown-item" @click="showDropdown('level')">
        <text class="search-label">级别</text>
        <view class="dropdown-content">
          <text>{{ selectedLevel || '请选择' }}</text>
          <uni-icons type="arrowright" size="24" color="#666"></uni-icons>
        </view>
      </view>
      
      <!-- 下拉项：来源 -->
      <view class="search-item dropdown-item" @click="showDropdown('source')">
        <text class="search-label">来源</text>
        <view class="dropdown-content">
          <text>{{ selectedSource || '请选择' }}</text>
          <uni-icons type="arrowright" size="24" color="#666"></uni-icons>
        </view>
      </view>
      
      <!-- 下拉项：省份 -->
      <view class="search-item dropdown-item" @click="showDropdown('province')">
        <text class="search-label">省份</text>
        <view class="dropdown-content">
          <text>{{ selectedProvince || '请选择' }}</text>
          <uni-icons type="arrowright" size="24" color="#666"></uni-icons>
        </view>
      </view>
      
      <!-- 下拉项：城市 -->
      <view class="search-item dropdown-item" @click="showDropdown('city')" v-if="selectedProvince">
        <text class="search-label">城市</text>
        <view class="dropdown-content">
          <text>{{ selectedCity || '请选择' }}</text>
          <uni-icons type="arrowright" size="24" color="#666"></uni-icons>
        </view>
      </view>
      
      <!-- 下拉项：状态 -->
      <view class="search-item dropdown-item" @click="showDropdown('status')">
        <text class="search-label">状态</text>
        <view class="dropdown-content">
          <text>{{ selectedStatus || '请选择' }}</text>
          <uni-icons type="arrowright" size="24" color="#666"></uni-icons>
        </view>
      </view>
      
      <!-- 下拉项：归属 -->
      <view class="search-item dropdown-item" @click="showDropdown('owner')">
        <text class="search-label">归属</text>
        <view class="dropdown-content">
          <text>{{ selectedOwner || '请选择' }}</text>
          <uni-icons type="arrowright" size="24" color="#666"></uni-icons>
        </view>
      </view>
    </view>
    
    <!-- 已选条件 -->
    <view class="selected-filters" v-if="hasSelectedFilters">
      <view class="filter-tag" v-for="(filter, index) in selectedFilterTags" :key="index">
        <text>{{ filter.text }}</text>
        <uni-icons 
          type="close" 
          size="18" 
          color="#999" 
          @click="removeFilter(filter.type)"
        ></uni-icons>
      </view>
      <view class="clear-all" @click="clearAllFilters">
        清除全部
      </view>
    </view>
    
    <!-- 操作按钮 -->
    <view class="action-buttons">
      <button class="reset-btn" @click="resetForm">重置</button>
      <button class="search-btn" @click="doSearch">搜索</button>
    </view>
    
    <!-- 下拉选择弹窗 -->
    <view class="dropdown-popup" v-if="currentDropdown">
      <view class="popup-mask" @click="hideDropdown"></view>
      <view class="popup-content">
        <view class="popup-header">
          <text class="popup-title">{{ dropdownTitles[currentDropdown] }}</text>
          <button class="popup-clear" @click="clearDropdownSelection">清除</button>
        </view>
        <view class="dropdown-list">
          <view 
            class="dropdown-option" 
            v-for="(option, index) in getDropdownOptions(currentDropdown)" 
            :key="index"
            :class="{ 'selected': isOptionSelected(currentDropdown, option) }"
            @click="selectDropdownOption(currentDropdown, option)"
          >
            {{ option.label }}
            <uni-icons 
              type="checkmark" 
              size="24" 
              color="#007aff" 
              v-if="isOptionSelected(currentDropdown, option)"
            ></uni-icons>
          </view>
        </view>
      </view>
    </view>
    
    <!-- 日期选择器 -->
    <view class="date-picker-popup" v-if="showDatePicker">
      <view class="popup-mask" @click="showDatePicker = false"></view>
      <view class="popup-content">
        <view class="popup-header">
          <text class="popup-title">选择创建时间</text>
          <button class="popup-clear" @click="clearDateSelection">清除</button>
        </view>
        <view class="date-picker-container">
          <uni-datetime-picker 
            type="date"
            :value="searchForm.createTimeStart"
            @change="onStartDateChange"
            placeholder="开始日期"
            class="date-picker"
          ></uni-datetime-picker>
          <text class="date-separator">至</text>
          <uni-datetime-picker 
            type="date"
            :value="searchForm.createTimeEnd"
            @change="onEndDateChange"
            placeholder="结束日期"
            class="date-picker"
          ></uni-datetime-picker>
        </view>
        <view class="date-actions">
          <button class="date-btn cancel" @click="showDatePicker = false">取消</button>
          <button class="date-btn confirm" @click="confirmDateSelection">确定</button>
        </view>
      </view>
    </view>
    
    <!-- 搜索结果 -->
    <view class="search-results" v-if="showResults">
      <view class="results-header">
        <text>搜索结果 ({{ filteredCustomers.length }})</text>
      </view>
      
      <view class="customer-list">
        <view class="customer-item" v-for="(customer, index) in filteredCustomers" :key="index">
          <view class="customer-name">{{ customer.name }}</view>
          <view class="customer-info">
            <text>电话: {{ customer.phone }}</text>
            <text>行业: {{ getIndustryName(customer.industry) }}</text>
          </view>
          <view class="customer-details">
            <text>创建时间: {{ customer.createTime }}</text>
            <text>状态: {{ getStatusName(customer.status) }}</text>
          </view>
        </view>
      </view>
      
      <view class="no-results" v-if="filteredCustomers.length === 0">
        <uni-icons type="empty" size="100" color="#ccc"></uni-icons>
        <text>没有找到符合条件的客户</text>
      </view>
    </view>
  </view>
</template>

<script>
export default {
  data() {
    return {
      // 搜索表单数据
      searchForm: {
        customerName: '',
        phone: '',
        createTimeStart: '',
        createTimeEnd: '',
        industry: '',
        type: '',
        level: '',
        source: '',
        province: '',
        city: '',
        status: '',
        owner: ''
      },
      
      // 下拉选项数据
      dropdownOptions: {
        industry: [
          { value: 'it', label: '信息技术' },
          { value: 'finance', label: '金融' },
          { value: 'retail', label: '零售' },
          { value: 'manufacturing', label: '制造业' },
          { value: 'service', label: '服务业' },
          { value: 'education', label: '教育' }
        ],
        type: [
          { value: 'personal', label: '个人' },
          { value: 'enterprise', label: '企业' },
          { value: 'government', label: '政府' }
        ],
        level: [
          { value: 'a', label: 'A级' },
          { value: 'b', label: 'B级' },
          { value: 'c', label: 'C级' },
          { value: 'd', label: 'D级' }
        ],
        source: [
          { value: 'website', label: '官网' },
          { value: 'referral', label: '推荐' },
          { value: 'advertisement', label: '广告' },
          { value: 'exhibition', label: '展会' },
          { value: 'other', label: '其他' }
        ],
        province: [
          { value: 'bj', label: '北京' },
          { value: 'sh', label: '上海' },
          { value: 'gd', label: '广东' },
          { value: 'zj', label: '浙江' },
          { value: 'js', label: '江苏' }
        ],
        city: {
          bj: [
            { value: 'bj', label: '北京市' }
          ],
          sh: [
            { value: 'sh', label: '上海市' }
          ],
          gd: [
            { value: 'gz', label: '广州' },
            { value: 'sz', label: '深圳' },
            { value: 'zh', label: '珠海' }
          ],
          zj: [
            { value: 'hz', label: '杭州' },
            { value: 'nb', label: '宁波' },
            { value: 'wz', label: '温州' }
          ],
          js: [
            { value: 'nj', label: '南京' },
            { value: 'sz', label: '苏州' },
            { value: 'wx', label: '无锡' }
          ]
        },
        status: [
          { value: 'active', label: '活跃' },
          { value: 'inactive', label: '不活跃' },
          { value: 'potential', label: '潜在' },
          { value: 'cooperative', label: '合作中' },
          { value: 'terminated', label: '已终止' }
        ],
        owner: [
          { value: 'user1', label: '张三' },
          { value: 'user2', label: '李四' },
          { value: 'user3', label: '王五' },
          { value: 'user4', label: '赵六' }
        ]
      },
      
      // 下拉弹窗标题
      dropdownTitles: {
        industry: '行业',
        type: '类型',
        level: '级别',
        source: '来源',
        province: '省份',
        city: '城市',
        status: '状态',
        owner: '归属'
      },
      
      // 当前显示的下拉弹窗
      currentDropdown: '',
      
      // 日期选择器显示状态
      showDatePicker: false,
      
      // 搜索结果显示状态
      showResults: false,
      
      // 客户数据
      allCustomers: [
        {
          id: 1,
          name: '北京科技有限公司',
          phone: '13800138000',
          createTime: '2023-05-15',
          industry: 'it',
          type: 'enterprise',
          level: 'a',
          source: 'website',
          province: 'bj',
          city: 'bj',
          status: 'cooperative',
          owner: 'user1'
        },
        {
          id: 2,
          name: '上海金融投资公司',
          phone: '13900139000',
          createTime: '2023-06-20',
          industry: 'finance',
          type: 'enterprise',
          level: 'a',
          source: 'referral',
          province: 'sh',
          city: 'sh',
          status: 'cooperative',
          owner: 'user2'
        },
        {
          id: 3,
          name: '广州零售连锁',
          phone: '13700137000',
          createTime: '2023-07-05',
          industry: 'retail',
          type: 'enterprise',
          level: 'b',
          source: 'advertisement',
          province: 'gd',
          city: 'gz',
          status: 'potential',
          owner: 'user3'
        },
        {
          id: 4,
          name: '深圳制造业工厂',
          phone: '13600136000',
          createTime: '2023-08-10',
          industry: 'manufacturing',
          type: 'enterprise',
          level: 'b',
          source: 'exhibition',
          province: 'gd',
          city: 'sz',
          status: 'active',
          owner: 'user1'
        },
        {
          id: 5,
          name: '杭州教育机构',
          phone: '13500135000',
          createTime: '2023-09-15',
          industry: 'education',
          type: 'enterprise',
          level: 'c',
          source: 'other',
          province: 'zj',
          city: 'hz',
          status: 'inactive',
          owner: 'user4'
        }
      ],
      
      // 筛选后的客户
      filteredCustomers: []
    };
  },
  
  computed: {
    // 已选条件标签
    selectedFilterTags: function() {
      var tags = [];
      
      // 客户名称
      if (this.searchForm.customerName) {
        tags.push({
          type: 'customerName',
          text: '客户: ' + this.searchForm.customerName
        });
      }
      
      // 电话
      if (this.searchForm.phone) {
        tags.push({
          type: 'phone',
          text: '电话: ' + this.searchForm.phone
        });
      }
      
      // 创建时间
      if (this.searchForm.createTimeStart || this.searchForm.createTimeEnd) {
        var timeText = '';
        if (this.searchForm.createTimeStart && this.searchForm.createTimeEnd) {
          timeText = this.searchForm.createTimeStart + '-' + this.searchForm.createTimeEnd;
        } else if (this.searchForm.createTimeStart) {
          timeText = this.searchForm.createTimeStart + '之后';
        } else {
          timeText = this.searchForm.createTimeEnd + '之前';
        }
        tags.push({
          type: 'createTime',
          text: '创建时间: ' + timeText
        });
      }
      
      // 行业
      if (this.searchForm.industry) {
        var industry = this.getOptionLabel('industry', this.searchForm.industry);
        tags.push({
          type: 'industry',
          text: '行业: ' + industry
        });
      }
      
      // 类型
      if (this.searchForm.type) {
        var type = this.getOptionLabel('type', this.searchForm.type);
        tags.push({
          type: 'type',
          text: '类型: ' + type
        });
      }
      
      // 级别
      if (this.searchForm.level) {
        var level = this.getOptionLabel('level', this.searchForm.level);
        tags.push({
          type: 'level',
          text: '级别: ' + level
        });
      }
      
      // 来源
      if (this.searchForm.source) {
        var source = this.getOptionLabel('source', this.searchForm.source);
        tags.push({
          type: 'source',
          text: '来源: ' + source
        });
      }
      
      // 省份
      if (this.searchForm.province) {
        var province = this.getOptionLabel('province', this.searchForm.province);
        tags.push({
          type: 'province',
          text: '省份: ' + province
        });
      }
      
      // 城市
      if (this.searchForm.city) {
        var city = this.getOptionLabel('city', this.searchForm.city);
        tags.push({
          type: 'city',
          text: '城市: ' + city
        });
      }
      
      // 状态
      if (this.searchForm.status) {
        var status = this.getOptionLabel('status', this.searchForm.status);
        tags.push({
          type: 'status',
          text: '状态: ' + status
        });
      }
      
      // 归属
      if (this.searchForm.owner) {
        var owner = this.getOptionLabel('owner', this.searchForm.owner);
        tags.push({
          type: 'owner',
          text: '归属: ' + owner
        });
      }
      
      return tags;
    },
    
    // 是否有已选条件
    hasSelectedFilters: function() {
      return this.selectedFilterTags.length > 0;
    },
    
    // 选中的行业名称
    selectedIndustry: function() {
      return this.getOptionLabel('industry', this.searchForm.industry);
    },
    
    // 选中的类型名称
    selectedType: function() {
      return this.getOptionLabel('type', this.searchForm.type);
    },
    
    // 选中的级别名称
    selectedLevel: function() {
      return this.getOptionLabel('level', this.searchForm.level);
    },
    
    // 选中的来源名称
    selectedSource: function() {
      return this.getOptionLabel('source', this.searchForm.source);
    },
    
    // 选中的省份名称
    selectedProvince: function() {
      return this.getOptionLabel('province', this.searchForm.province);
    },
    
    // 选中的城市名称
    selectedCity: function() {
      return this.getOptionLabel('city', this.searchForm.city);
    },
    
    // 选中的状态名称
    selectedStatus: function() {
      return this.getOptionLabel('status', this.searchForm.status);
    },
    
    // 选中的归属名称
    selectedOwner: function() {
      return this.getOptionLabel('owner', this.searchForm.owner);
    }
  },
  
  methods: {
    // 显示下拉选择框
    showDropdown: function(type) {
      this.currentDropdown = type;
    },
    
    // 隐藏下拉选择框
    hideDropdown: function() {
      this.currentDropdown = '';
    },
    
    // 获取下拉选项
    getDropdownOptions: function(type) {
      if (type === 'city' && this.searchForm.province) {
        return this.dropdownOptions.city[this.searchForm.province] || [];
      }
      return this.dropdownOptions[type] || [];
    },
    
    // 判断选项是否被选中
    isOptionSelected: function(type, option) {
      return this.searchForm[type] === option.value;
    },
    
    // 选择下拉选项
    selectDropdownOption: function(type, option) {
      // 如果选择了省份，需要重置城市
      if (type === 'province') {
        this.searchForm.city = '';
      }
      
      this.searchForm[type] = option.value;
      this.hideDropdown();
    },
    
    // 清除下拉选择
    clearDropdownSelection: function() {
      if (this.currentDropdown === 'province') {
        this.searchForm.province = '';
        this.searchForm.city = '';
      } else {
        this.searchForm[this.currentDropdown] = '';
      }
      this.hideDropdown();
    },
    
    // 开始日期变化
    onStartDateChange: function(e) {
      this.searchForm.createTimeStart = e.detail.value;
    },
    
    // 结束日期变化
    onEndDateChange: function(e) {
      this.searchForm.createTimeEnd = e.detail.value;
    },
    
    // 确认日期选择
    confirmDateSelection: function() {
      this.showDatePicker = false;
    },
    
    // 清除日期选择
    clearDateSelection: function() {
      this.searchForm.createTimeStart = '';
      this.searchForm.createTimeEnd = '';
    },
    
    // 执行搜索
    doSearch: function() {
      var self = this;
      this.filteredCustomers = this.allCustomers.filter(function(customer) {
        // 客户名称筛选
        if (self.searchForm.customerName && 
            customer.name.indexOf(self.searchForm.customerName) === -1) {
          return false;
        }
        
        // 电话筛选
        if (self.searchForm.phone && 
            customer.phone.indexOf(self.searchForm.phone) === -1) {
          return false;
        }
        
        // 创建时间筛选 - 开始时间
        if (self.searchForm.createTimeStart && 
            customer.createTime < self.searchForm.createTimeStart) {
          return false;
        }
        
        // 创建时间筛选 - 结束时间
        if (self.searchForm.createTimeEnd && 
            customer.createTime > self.searchForm.createTimeEnd) {
          return false;
        }
        
        // 行业筛选
        if (self.searchForm.industry && customer.industry !== self.searchForm.industry) {
          return false;
        }
        
        // 类型筛选
        if (self.searchForm.type && customer.type !== self.searchForm.type) {
          return false;
        }
        
        // 级别筛选
        if (self.searchForm.level && customer.level !== self.searchForm.level) {
          return false;
        }
        
        // 来源筛选
        if (self.searchForm.source && customer.source !== self.searchForm.source) {
          return false;
        }
        
        // 省份筛选
        if (self.searchForm.province && customer.province !== self.searchForm.province) {
          return false;
        }
        
        // 城市筛选
        if (self.searchForm.city && customer.city !== self.searchForm.city) {
          return false;
        }
        
        // 状态筛选
        if (self.searchForm.status && customer.status !== self.searchForm.status) {
          return false;
        }
        
        // 归属筛选
        if (self.searchForm.owner && customer.owner !== self.searchForm.owner) {
          return false;
        }
        
        return true;
      });
      
      this.showResults = true;
    },
    
    // 重置表单
    resetForm: function() {
      this.searchForm = {
        customerName: '',
        phone: '',
        createTimeStart: '',
        createTimeEnd: '',
        industry: '',
        type: '',
        level: '',
        source: '',
        province: '',
        city: '',
        status: '',
        owner: ''
      };
      this.showResults = false;
    },
    
    // 移除单个筛选条件
    removeFilter: function(type) {
      if (type === 'createTime') {
        this.searchForm.createTimeStart = '';
        this.searchForm.createTimeEnd = '';
      } else {
        this.searchForm[type] = '';
        
        // 如果移除省份，同时移除城市
        if (type === 'province') {
          this.searchForm.city = '';
        }
      }
      
      // 如果有搜索结果，重新搜索
      if (this.showResults) {
        this.doSearch();
      }
    },
    
    // 清除所有筛选条件
    clearAllFilters: function() {
      this.resetForm();
    },
    
    // 取消搜索
    onCancel: function() {
      uni.navigateBack();
    },
    
    // 根据值获取选项标签
    getOptionLabel: function(type, value) {
      if (!value) return '';
      
      if (type === 'city' && this.searchForm.province) {
        var cityOptions = this.dropdownOptions.city[this.searchForm.province] || [];
        for (var i = 0; i < cityOptions.length; i++) {
          if (cityOptions[i].value === value) {
            return cityOptions[i].label;
          }
        }
      } else {
        var options = this.dropdownOptions[type] || [];
        for (var j = 0; j < options.length; j++) {
          if (options[j].value === value) {
            return options[j].label;
          }
        }
      }
      
      return '';
    },
    
    // 获取行业名称
    getIndustryName: function(value) {
      return this.getOptionLabel('industry', value);
    },
    
    // 获取状态名称
    getStatusName: function(value) {
      return this.getOptionLabel('status', value);
    }
  }
};
</script>

<style scoped>
.search-page {
  background-color: #f5f5f5;
  min-height: 100vh;
}

/* 标题栏 */
.navbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  height: 90rpx;
  background-color: #fff;
  padding: 0 24rpx;
  border-bottom: 1px solid #eee;
}

.nav-title {
  font-size: 34rpx;
  font-weight: bold;
  color: #333;
}

.nav-close {
  font-size: 30rpx;
  color: #666;
  background-color: transparent;
  padding: 0;
  height: auto;
  line-height: normal;
}

/* 搜索区域 */
.search-container {
  background-color: #fff;
  padding: 15rpx 24rpx;
}

.search-item {
  display: flex;
  align-items: center;
  padding: 20rpx 0;
  border-bottom: 1px solid #f5f5f5;
}

.search-item:last-child {
  border-bottom: none;
}

.search-label {
  width: 160rpx;
  font-size: 28rpx;
  color: #666;
}

.search-input {
  flex: 1;
  height: 60rpx;
  line-height: 60rpx;
  font-size: 28rpx;
  padding: 0 10rpx;
  border: 1px solid #eee;
  border-radius: 8rpx;
}

/* 日期选择 */
.date-range {
  flex: 1;
  display: flex;
  justify-content: space-between;
  align-items: center;
  height: 60rpx;
  padding: 0 10rpx;
  border: 1px solid #eee;
  border-radius: 8rpx;
}

.date-text {
  font-size: 28rpx;
  color: #333;
}

/* 下拉项 */
.dropdown-item {
  cursor: pointer;
}

.dropdown-content {
  flex: 1;
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 28rpx;
  color: #333;
}

/* 已选条件 */
.selected-filters {
  display: flex;
  flex-wrap: wrap;
  padding: 15rpx 24rpx;
  background-color: #fff;
  gap: 15rpx;
  border-top: 1px solid #eee;
}

.filter-tag {
  display: flex;
  align-items: center;
  background-color: #e8f4fd;
  color: #007aff;
  border-radius: 20rpx;
  padding: 8rpx 15rpx;
  font-size: 26rpx;
}

.filter-tag uni-icons {
  margin-left: 8rpx;
}

.clear-all {
  font-size: 26rpx;
  color: #666;
  padding: 8rpx 15rpx;
  align-self: center;
}

/* 操作按钮 */
.action-buttons {
  display: flex;
  padding: 24rpx;
  background-color: #fff;
  gap: 20rpx;
}

.reset-btn, .search-btn {
  flex: 1;
  height: 80rpx;
  line-height: 80rpx;
  font-size: 30rpx;
  border-radius: 10rpx;
}

.reset-btn {
  background-color: #f5f5f5;
  color: #333;
  border: 1px solid #eee;
}

.search-btn {
  background-color: #007aff;
  color: #fff;
  border-color: #007aff;
}

/* 下拉弹窗 */
.dropdown-popup, .date-picker-popup {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 999;
}

.popup-mask {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
}

.popup-content {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  background-color: #fff;
  border-top-left-radius: 20rpx;
  border-top-right-radius: 20rpx;
  max-height: 70vh;
}

.popup-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20rpx 24rpx;
  border-bottom: 1px solid #eee;
}

.popup-title {
  font-size: 32rpx;
  font-weight: bold;
  color: #333;
}

.popup-clear {
  font-size: 28rpx;
  color: #007aff;
  background-color: transparent;
  padding: 0;
  height: auto;
  line-height: normal;
}

.dropdown-list {
  max-height: calc(70vh - 80rpx);
  overflow-y: auto;
}

.dropdown-option {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20rpx 24rpx;
  font-size: 28rpx;
  border-bottom: 1px solid #f5f5f5;
}

.dropdown-option.selected {
  background-color: #f0f7ff;
  color: #007aff;
}

/* 日期选择器 */
.date-picker-container {
  display: flex;
  align-items: center;
  padding: 20rpx;
}

.date-picker {
  flex: 1;
}

.date-separator {
  padding: 0 20rpx;
  font-size: 28rpx;
  color: #666;
}

.date-actions {
  display: flex;
  padding: 20rpx;
  border-top: 1px solid #eee;
}

.date-btn {
  flex: 1;
  height: 70rpx;
  line-height: 70rpx;
  font-size: 28rpx;
  border-radius: 8rpx;
  margin: 0 10rpx;
}

.date-btn.cancel {
  background-color: #f5f5f5;
  color: #333;
}

.date-btn.confirm {
  background-color: #007aff;
  color: #fff;
  border-color: #007aff;
}

/* 搜索结果 */
.search-results {
  background-color: #fff;
  margin-top: 15rpx;
}

.results-header {
  padding: 20rpx 24rpx;
  border-bottom: 1px solid #eee;
  font-size: 28rpx;
  color: #666;
}

.customer-list {
  padding: 15rpx;
}

.customer-item {
  padding: 20rpx;
  border-bottom: 1px solid #f5f5f5;
}

.customer-item:last-child {
  border-bottom: none;
}

.customer-name {
  font-size: 30rpx;
  font-weight: bold;
  color: #333;
  margin-bottom: 10rpx;
}

.customer-info, .customer-details {
  display: flex;
  justify-content: space-between;
  font-size: 26rpx;
  color: #666;
  margin-bottom: 8rpx;
}

/* 无结果状态 */
.no-results {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 150rpx 0;
}

.no-results text {
  font-size: 30rpx;
  color: #999;
  margin-top: 30rpx;
}
</style>
    