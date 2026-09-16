<template>
  <view class="customer-list-page">
    <!-- 顶部导航栏 -->
    <view class="navbar">
      <text class="nav-title">客户管理</text>
      <button class="add-btn" @click="addCustomer">
        <uni-icons type="plus" size="24" color="#fff"></uni-icons>
        <text>新增</text>
      </button>
    </view>
    
    <!-- 搜索区域折叠/展开控制 -->
    <view class="search-toggle" @click="toggleSearch">
      <text class="toggle-text">{{ showSearch ? '收起搜索' : '展开搜索' }}</text>
      <uni-icons 
        type="arrowup" 
        size="24" 
        color="#666" 
        :class="{ 'rotate': !showSearch }"
      ></uni-icons>
    </view>
    
    <!-- 多条件搜索区域 -->
    <view class="search-container" v-if="showSearch">
      <view class="search-row">
        <!-- 客户名称输入 -->
        <view class="search-item">
          <text class="search-label">客户</text>
          <input 
            type="text" 
            v-model="searchForm.customerName" 
            placeholder="请输入客户名称" 
            class="search-input"
          />
        </view>
        
        <!-- 电话输入 -->
        <view class="search-item">
          <text class="search-label">电话</text>
          <input 
            type="number" 
            v-model="searchForm.phone" 
            placeholder="请输入联系电话" 
            class="search-input"
          />
        </view>
      </view>
      
      <view class="search-row">
        <!-- 创建时间选择 -->
        <view class="search-item time-item">
          <text class="search-label">创建时间</text>
          <view class="date-range" @click="showDatePicker = true">
            <text class="date-text">
              {{ searchForm.createTimeStart || '开始日期' }} 
              - 
              {{ searchForm.createTimeEnd || '结束日期' }}
            </text>
            <uni-icons type="calendar" size="24" color="#666"></uni-icons>
          </view>
        </view>
      </view>
      
      <view class="search-row">
        <!-- 行业下拉 -->
        <view class="search-item dropdown-item" @click="showDropdown('industry')">
          <text class="search-label">行业</text>
          <view class="dropdown-content">
            <text>{{ selectedIndustry || '请选择' }}</text>
            <uni-icons type="arrowright" size="24" color="#666"></uni-icons>
          </view>
        </view>
        
        <!-- 类型下拉 -->
        <view class="search-item dropdown-item" @click="showDropdown('type')">
          <text class="search-label">类型</text>
          <view class="dropdown-content">
            <text>{{ selectedType || '请选择' }}</text>
            <uni-icons type="arrowright" size="24" color="#666"></uni-icons>
          </view>
        </view>
      </view>
      
      <view class="search-row">
        <!-- 级别下拉 -->
        <view class="search-item dropdown-item" @click="showDropdown('level')">
          <text class="search-label">级别</text>
          <view class="dropdown-content">
            <text>{{ selectedLevel || '请选择' }}</text>
            <uni-icons type="arrowright" size="24" color="#666"></uni-icons>
          </view>
        </view>
        
        <!-- 来源下拉 -->
        <view class="search-item dropdown-item" @click="showDropdown('source')">
          <text class="search-label">来源</text>
          <view class="dropdown-content">
            <text>{{ selectedSource || '请选择' }}</text>
            <uni-icons type="arrowright" size="24" color="#666"></uni-icons>
          </view>
        </view>
      </view>
      
      <view class="search-row">
        <!-- 省份下拉 -->
        <view class="search-item dropdown-item" @click="showDropdown('province')">
          <text class="search-label">省份</text>
          <view class="dropdown-content">
            <text>{{ selectedProvince || '请选择' }}</text>
            <uni-icons type="arrowright" size="24" color="#666"></uni-icons>
          </view>
        </view>
        
        <!-- 城市下拉 -->
        <view class="search-item dropdown-item" @click="showDropdown('city')" v-if="searchForm.province">
          <text class="search-label">城市</text>
          <view class="dropdown-content">
            <text>{{ selectedCity || '请选择' }}</text>
            <uni-icons type="arrowright" size="24" color="#666"></uni-icons>
          </view>
        </view>
      </view>
      
      <view class="search-row">
        <!-- 状态下拉 -->
        <view class="search-item dropdown-item" @click="showDropdown('status')">
          <text class="search-label">状态</text>
          <view class="dropdown-content">
            <text>{{ selectedStatus || '请选择' }}</text>
            <uni-icons type="arrowright" size="24" color="#666"></uni-icons>
          </view>
        </view>
        
        <!-- 归属下拉 -->
        <view class="search-item dropdown-item" @click="showDropdown('owner')">
          <text class="search-label">归属</text>
          <view class="dropdown-content">
            <text>{{ selectedOwner || '请选择' }}</text>
            <uni-icons type="arrowright" size="24" color="#666"></uni-icons>
          </view>
        </view>
      </view>
      
      <!-- 搜索操作按钮 -->
      <view class="search-actions">
        <button class="reset-btn" @click="resetSearch">重置</button>
        <button class="do-search-btn" @click="doSearch">搜索</button>
      </view>
    </view>
    
    <!-- 已选条件标签 -->
    <view class="selected-filters" v-if="hasSelectedFilters">
      <view class="filter-tag" v-for="(filter, index) in selectedFilterTags" :key="index">
        <text>{{ filter.text }}</text>
        <uni-icons 
          type="close" 
          size="18" 
          color="#999" 
          @click.stop="removeFilter(filter.type)"
        ></uni-icons>
      </view>
      <view class="clear-all" @click="clearAllFilters" v-if="selectedFilterTags.length > 0">
        清除全部
      </view>
    </view>
    
    <!-- 列表标题栏 -->
    <view class="list-header">
      <text class="header-item name-item">客户名称</text>
      <text class="header-item">联系电话</text>
      <text class="header-item">最后跟进时间</text>
      <text class="header-item operate-item">操作</text>
    </view>
    
    <!-- 客户列表 - 多层结构展示 -->
    <view class="customer-list">
      <!-- 客户组 -->
      <view class="customer-group" v-for="(group, groupIndex) in filteredCustomerGroups" :key="groupIndex">
        <view class="group-header" @click="toggleGroup(groupIndex)">
          <uni-icons 
            type="arrowright" 
            size="24" 
            color="#666" 
            :class="{ 'rotate': group.expanded }"
          ></uni-icons>
          <text class="group-name">{{ group.groupName }} ({{ group.customers.length }})</text>
          <text class="group-desc">{{ group.description }}</text>
        </view>
        
        <!-- 组内客户列表 -->
        <view class="group-customers" v-if="group.expanded">
          <view 
            class="customer-item" 
            v-for="(customer, customerIndex) in group.customers" 
            :key="customerIndex"
            @click="viewCustomerDetail(customer)"
          >
            <view class="customer-info name-item">
              <text class="customer-name">{{ customer.name }}</text>
              <text class="customer-tag" :class="'tag-' + customer.level">
                {{ getLevelName(customer.level) }}
              </text>
            </view>
            <text class="customer-info">{{ customer.phone }}</text>
            <text class="customer-info">{{ customer.lastFollowTime || '未跟进' }}</text>
            <view class="customer-info operate-item">
              <button class="operate-btn edit" @click.stop="editCustomer(customer)">
                <uni-icons type="edit" size="20" color="#007aff"></uni-icons>
              </button>
              <button class="operate-btn follow" @click.stop="followCustomer(customer)">
                <uni-icons type="chat" size="20" color="#00cc00"></uni-icons>
              </button>
            </view>
          </view>
        </view>
      </view>
      
      <!-- 无结果状态 -->
      <view class="no-results" v-if="isSearchCompleted && filteredCustomerGroups.length === 0">
        <uni-icons type="empty" size="100" color="#ccc"></uni-icons>
        <text>没有找到符合条件的客户</text>
        <button class="reset-btn" @click="resetSearch">重置筛选条件</button>
      </view>
      
      <!-- 加载状态 -->
      <view class="loading" v-if="isLoading">
        <uni-loading-icon size="24" color="#007aff"></uni-loading-icon>
        <text>加载中...</text>
      </view>
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
    
    <!-- 日期选择器弹窗 -->
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
      
      // 搜索区域显示状态
      showSearch: false,
      
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
      
      // 当前显示的下拉弹窗类型
      currentDropdown: '',
      
      // 日期选择器显示状态
      showDatePicker: false,
      
      // 客户组数据（多层结构）
      customerGroups: [
        {
          groupName: '重点客户',
          description: '合作中且级别较高的客户',
          expanded: true,
          customers: [
            {
              id: 1,
              name: '北京科技有限公司',
              phone: '13800138000',
              createTime: '2023-05-15',
              lastFollowTime: '2023-10-10',
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
              lastFollowTime: '2023-10-05',
              industry: 'finance',
              type: 'enterprise',
              level: 'a',
              source: 'referral',
              province: 'sh',
              city: 'sh',
              status: 'cooperative',
              owner: 'user2'
            }
          ]
        },
        {
          groupName: '潜在客户',
          description: '有合作意向但未达成合作的客户',
          expanded: true,
          customers: [
            {
              id: 3,
              name: '广州零售连锁',
              phone: '13700137000',
              createTime: '2023-07-05',
              lastFollowTime: '2023-10-08',
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
              lastFollowTime: '2023-09-28',
              industry: 'manufacturing',
              type: 'enterprise',
              level: 'b',
              source: 'exhibition',
              province: 'gd',
              city: 'sz',
              status: 'active',
              owner: 'user1'
            }
          ]
        },
        {
          groupName: '休眠客户',
          description: '长期未跟进或合作终止的客户',
          expanded: false,
          customers: [
            {
              id: 5,
              name: '杭州教育机构',
              phone: '13500135000',
              createTime: '2023-09-15',
              lastFollowTime: '2023-08-15',
              industry: 'education',
              type: 'enterprise',
              level: 'c',
              source: 'other',
              province: 'zj',
              city: 'hz',
              status: 'inactive',
              owner: 'user4'
            }
          ]
        }
      ],
      
      // 筛选后的客户组
      filteredCustomerGroups: [],
      
      // 加载状态
      isLoading: false,
      
      // 搜索完成状态
      isSearchCompleted: false
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
  
  onLoad: function() {
    // 初始化显示所有客户组
    this.filteredCustomerGroups = this.customerGroups.map(function(group) {
      return {
        ...group,
        customers: [...group.customers]
      };
    });
  },
  
  methods: {
    // 切换搜索区域显示/隐藏
    toggleSearch: function() {
      this.showSearch = !this.showSearch;
    },
    
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
      this.isLoading = true;
      this.isSearchCompleted = false;
      
      // 模拟网络请求延迟
      setTimeout(function() {
        // 深拷贝客户组数据
        var filteredGroups = JSON.parse(JSON.stringify(self.customerGroups));
        
        // 对每个客户组进行筛选
        filteredGroups.forEach(function(group) {
          group.customers = group.customers.filter(function(customer) {
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
        });
        
        // 过滤掉没有客户的组
        filteredGroups = filteredGroups.filter(function(group) {
          return group.customers.length > 0;
        });
        
        self.filteredCustomerGroups = filteredGroups;
        self.isLoading = false;
        self.isSearchCompleted = true;
      }, 500);
    },
    
    // 重置搜索条件
    resetSearch: function() {
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
      
      // 恢复显示所有客户组
      this.filteredCustomerGroups = this.customerGroups.map(function(group) {
        return {
          ...group,
          customers: [...group.customers]
        };
      });
      
      this.isSearchCompleted = false;
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
      
      // 重新搜索
      this.doSearch();
    },
    
    // 清除所有筛选条件
    clearAllFilters: function() {
      this.resetSearch();
    },
    
    // 切换客户组展开/折叠
    toggleGroup: function(groupIndex) {
      this.filteredCustomerGroups[groupIndex].expanded = !this.filteredCustomerGroups[groupIndex].expanded;
    },
    
    // 查看客户详情
    viewCustomerDetail: function(customer) {
      uni.navigateTo({
        url: '/pages/customer-detail/customer-detail?id=' + customer.id
      });
    },
    
    // 编辑客户
    editCustomer: function(customer) {
      uni.navigateTo({
        url: '/pages/customer-edit/customer-edit?id=' + customer.id
      });
    },
    
    // 跟进客户
    followCustomer: function(customer) {
      uni.navigateTo({
        url: '/pages/customer-follow/customer-follow?id=' + customer.id
      });
    },
    
    // 新增客户
    addCustomer: function() {
      uni.navigateTo({
        url: '/pages/customer-edit/customer-edit'
      });
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
    
    // 获取级别名称
    getLevelName: function(value) {
      return this.getOptionLabel('level', value);
    }
  }
};
</script>

<style scoped>
.customer-list-page {
  background-color: #f5f5f5;
  min-height: 100vh;
}

/* 顶部导航栏 */
.navbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  height: 90rpx;
  background-color: #007aff;
  padding: 0 24rpx;
}

.nav-title {
  font-size: 34rpx;
  font-weight: bold;
  color: #fff;
}

.add-btn {
  display: flex;
  align-items: center;
  color: #fff;
  background-color: transparent;
  font-size: 28rpx;
  padding: 0 15rpx;
  height: 60rpx;
  line-height: 60rpx;
}

.add-btn uni-icons {
  margin-right: 8rpx;
}

/* 搜索区域折叠/展开控制 */
.search-toggle {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 60rpx;
  background-color: #fff;
  border-bottom: 1px solid #eee;
  cursor: pointer;
}

.toggle-text {
  font-size: 28rpx;
  color: #666;
  margin-right: 10rpx;
}

.rotate {
  transform: rotate(180deg);
  transition: transform 0.3s;
}

/* 搜索区域 */
.search-container {
  background-color: #fff;
  padding: 15rpx 24rpx;
  border-bottom: 1px solid #eee;
}

.search-row {
  display: flex;
  flex-wrap: wrap;
  margin-bottom: 10rpx;
}

.search-item {
  display: flex;
  align-items: center;
  width: 50%;
  padding: 15rpx 0;
}

.time-item {
  width: 100%;
}

.search-label {
  width: 120rpx;
  font-size: 26rpx;
  color: #666;
  padding-right: 10rpx;
  box-sizing: border-box;
}

.search-input {
  flex: 1;
  height: 60rpx;
  line-height: 60rpx;
  font-size: 26rpx;
  padding: 0 15rpx;
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
  padding: 0 15rpx;
  border: 1px solid #eee;
  border-radius: 8rpx;
}

.date-text {
  font-size: 26rpx;
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
  font-size: 26rpx;
  color: #333;
  padding-right: 15rpx;
}

/* 搜索操作按钮 */
.search-actions {
  display: flex;
  padding: 15rpx 0;
  gap: 20rpx;
}

.reset-btn, .do-search-btn {
  flex: 1;
  height: 70rpx;
  line-height: 70rpx;
  font-size: 28rpx;
  border-radius: 8rpx;
}

.reset-btn {
  background-color: #f5f5f5;
  color: #333;
  border: 1px solid #eee;
}

.do-search-btn {
  background-color: #007aff;
  color: #fff;
  border-color: #007aff;
}

/* 已选条件标签 */
.selected-filters {
  display: flex;
  flex-wrap: wrap;
  padding: 15rpx 24rpx;
  background-color: #fff;
  gap: 15rpx;
  border-bottom: 1px solid #eee;
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

/* 列表标题栏 */
.list-header {
  display: flex;
  background-color: #f5f5f5;
  padding: 0 24rpx;
  height: 70rpx;
  align-items: center;
  font-size: 26rpx;
  color: #666;
  border-bottom: 1px solid #eee;
}

.header-item {
  flex: 1;
  text-align: center;
}

.name-item {
  flex: 2;
  text-align: left;
}

.operate-item {
  width: 120rpx;
  flex: none;
  text-align: center;
}

/* 客户列表 */
.customer-list {
  background-color: #fff;
}

/* 客户组 */
.customer-group {
  border-bottom: 1px solid #eee;
}

.group-header {
  display: flex;
  align-items: center;
  padding: 15rpx 24rpx;
  background-color: #f9f9f9;
  cursor: pointer;
}

.group-name {
  font-size: 28rpx;
  font-weight: bold;
  color: #333;
  margin-left: 10rpx;
  flex: 1;
}

.group-desc {
  font-size: 24rpx;
  color: #999;
  padding-right: 20rpx;
}

/* 组内客户列表 */
.group-customers {
  background-color: #fff;
}

.customer-item {
  display: flex;
  align-items: center;
  padding: 15rpx 24rpx;
  border-bottom: 1px solid #f5f5f5;
}

.customer-item:last-child {
  border-bottom: none;
}

.customer-info {
  flex: 1;
  font-size: 26rpx;
  color: #333;
  text-align: center;
  word-break: break-all;
  padding: 5rpx 0;
}

.customer-info.name-item {
  text-align: left;
  display: flex;
  align-items: center;
}

.customer-name {
  flex: 1;
}

.customer-tag {
  font-size: 22rpx;
  padding: 2rpx 10rpx;
  border-radius: 12rpx;
  color: #fff;
  margin-left: 10rpx;
}

.tag-a {
  background-color: #ff4400;
}

.tag-b {
  background-color: #ff9900;
}

.tag-c {
  background-color: #00cc00;
}

.tag-d {
  background-color: #999;
}

.operate-btn {
  width: 50rpx;
  height: 50rpx;
  padding: 0;
  line-height: 50rpx;
  background-color: transparent;
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

.no-results .reset-btn {
  margin-top: 40rpx;
  width: 200rpx;
}

/* 加载状态 */
.loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 100rpx 0;
}

.loading text {
  font-size: 28rpx;
  color: #999;
  margin-top: 20rpx;
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

/* 适配不同屏幕 */
@media (max-width: 375px) {
  .search-item {
    width: 100%;
  }
  
  .customer-info {
    font-size: 24rpx;
  }
}
</style>
    