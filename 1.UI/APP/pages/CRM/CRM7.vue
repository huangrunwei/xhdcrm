<template>
	<view class="customer-list-page">
		<!-- 顶部导航栏 -->
		<view class="navbar">
			<text class="nav-title">客户管理</text>
		</view>

		<!-- 搜索区域 -->
		<view class="search-bar">
			<view class="search-input-container">
				<uni-icons type="search" size="24" color="#999" class="search-icon"></uni-icons>
				<input type="text" v-model="searchForm.customerName" placeholder="搜索客户名称或电话..." class="search-input"
					@confirm="doSearch" />
				<uni-icons type="clear" size="20" color="#999" class="clear-icon" v-if="searchForm.customerName"
					@click="clearQuickSearch"></uni-icons>
			</view>
			<button class="expand-btn" @click="toggleAdvancedSearch">
				<uni-icons type="filter" size="24" color="#666"></uni-icons>
				<text>{{ showAdvancedSearch ? '收起' : '筛选' }}</text>
			</button>
		</view>

		<!-- 高级搜索区域 -->
		<view class="advanced-search" v-if="showAdvancedSearch">
			<view class="search-row">
				<!-- 电话输入 -->
				<view class="search-item">
					<text class="search-label">电话</text>
					<input type="number" v-model="searchForm.phone" placeholder="请输入联系电话" class="search-input" />
				</view>

				<!-- 创建时间选择 -->
				<view class="search-item time-item">
					<text class="search-label">创建时间</text>
					<view class="date-range" @click="showDatePicker = true">
						<text class="date-text">
							{{ searchForm.createTimeStart  }}
							-
							{{ searchForm.createTimeEnd  }}
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
				<uni-icons type="close" size="18" color="#999" @click.stop="removeFilter(filter.type)"></uni-icons>
			</view>
			<view class="clear-all" @click="clearAllFilters" v-if="selectedFilterTags.length > 0">
				清除全部
			</view>
		</view>

		<!-- 客户列表容器 -->
		<view class="customer-list">
			<scroll-view scroll-y="true" @scrolltolower="loadMore">
				<!-- 客户项 - 每个客户使用白色圆角框 -->
				<view class="customer-item" v-for="(customer, index) in customers" :key="index"
					@click="handleCustomerClick(customer)">
					<!-- 第一行：客户名称 -->
					<view class="item-row row-1">
						<text class="customer-name">{{ customer.name }}</text>
						<view class="customer-actions">

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
			</scroll-view>
		</view>

		<!-- 悬浮添加按钮 -->
		<button class="floating-add-btn" @click="addCustomer">
			<uni-icons type="plus" size="32" color="#fff"></uni-icons>
		</button>

		<!-- 下拉选择弹窗 -->
		<view class="dropdown-popup" v-if="currentDropdown">
			<view class="popup-mask" @click="hideDropdown"></view>
			<view class="popup-content">
				<view class="popup-header">
					<text class="popup-title">{{ dropdownTitles[currentDropdown] }}</text>
					<button class="popup-clear" @click="clearDropdownSelection">清除</button>
				</view>
				<view class="dropdown-list">
					<view class="dropdown-option" v-for="(option, index) in getDropdownOptions(currentDropdown)"
						:key="index" :class="{ 'selected': isOptionSelected(currentDropdown, option) }"
						@click="selectDropdownOption(currentDropdown, option)">
						{{ option.label }}
						<uni-icons type="checkmark" size="24" color="#007aff"
							v-if="isOptionSelected(currentDropdown, option)"></uni-icons>
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
					<uni-datetime-picker type="date" :value="searchForm.createTimeStart" @change="onStartDateChange"
						placeholder="开始日期" class="date-picker"></uni-datetime-picker>
					<text class="date-separator">至</text>
					<uni-datetime-picker type="date" :value="searchForm.createTimeEnd" @change="onEndDateChange"
						placeholder="结束日期" class="date-picker"></uni-datetime-picker>
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
				page: 1, // 当前页码
				pageSize: 10, // 每页条数
				total: 50, // 总数据量(实际项目中从接口获取)
				isLoading: false, // 是否正在加载
				hasMore: true, // 是否还有更多数据

				// 高级搜索显示状态
				showAdvancedSearch: false,

				// 下拉选项数据
				dropdownOptions: {
					industry: [{
							value: 'it',
							label: '信息技术'
						},
						{
							value: 'finance',
							label: '金融'
						},
						{
							value: 'retail',
							label: '零售'
						},
						{
							value: 'manufacturing',
							label: '制造业'
						},
						{
							value: 'service',
							label: '服务业'
						},
						{
							value: 'education',
							label: '教育'
						}
					],
					type: [{
							value: 'personal',
							label: '个人'
						},
						{
							value: 'enterprise',
							label: '企业'
						},
						{
							value: 'government',
							label: '政府'
						}
					],
					level: [{
							value: 'a',
							label: 'A级'
						},
						{
							value: 'b',
							label: 'B级'
						},
						{
							value: 'c',
							label: 'C级'
						},
						{
							value: 'd',
							label: 'D级'
						}
					],
					source: [{
							value: 'website',
							label: '官网'
						},
						{
							value: 'referral',
							label: '推荐'
						},
						{
							value: 'advertisement',
							label: '广告'
						},
						{
							value: 'exhibition',
							label: '展会'
						},
						{
							value: 'other',
							label: '其他'
						}
					],
					province: [{
							value: 'bj',
							label: '北京'
						},
						{
							value: 'sh',
							label: '上海'
						},
						{
							value: 'gd',
							label: '广东'
						},
						{
							value: 'zj',
							label: '浙江'
						},
						{
							value: 'js',
							label: '江苏'
						}
					],
					city: {
						bj: [{
							value: 'bj',
							label: '北京市'
						}],
						sh: [{
							value: 'sh',
							label: '上海市'
						}],
						gd: [{
								value: 'gz',
								label: '广州'
							},
							{
								value: 'sz',
								label: '深圳'
							},
							{
								value: 'zh',
								label: '珠海'
							}
						],
						zj: [{
								value: 'hz',
								label: '杭州'
							},
							{
								value: 'nb',
								label: '宁波'
							},
							{
								value: 'wz',
								label: '温州'
							}
						],
						js: [{
								value: 'nj',
								label: '南京'
							},
							{
								value: 'sz',
								label: '苏州'
							},
							{
								value: 'wx',
								label: '无锡'
							}
						]
					},
					status: [{
							value: 'active',
							label: '活跃'
						},
						{
							value: 'inactive',
							label: '不活跃'
						},
						{
							value: 'potential',
							label: '潜在'
						},
						{
							value: 'cooperative',
							label: '合作中'
						},
						{
							value: 'terminated',
							label: '已终止'
						}
					],
					owner: [{
							value: 'user1',
							label: '张三'
						},
						{
							value: 'user2',
							label: '李四'
						},
						{
							value: 'user3',
							label: '王五'
						},
						{
							value: 'user4',
							label: '赵六'
						}
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

				// 客户数据列表
				customers: [{
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
					},
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
				],

				// 筛选后的客户
				filteredCustomers: [],

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
			// 初始化显示所有客户
			this.filteredCustomers = [...this.allCustomers];
		},

		methods: {
			// 切换高级搜索显示/隐藏
			toggleAdvancedSearch: function() {
				this.showAdvancedSearch = !this.showAdvancedSearch;
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
					// 筛选客户
					self.filteredCustomers = self.allCustomers.filter(function(customer) {
						// 客户名称筛选
						if (self.searchForm.customerName &&
							customer.name.indexOf(self.searchForm.customerName) === -1 &&
							customer.phone.indexOf(self.searchForm.customerName) === -1) {
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
						if (self.searchForm.industry && customer.industry !== self.searchForm
							.industry) {
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
						if (self.searchForm.province && customer.province !== self.searchForm
							.province) {
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

				// 恢复显示所有客户
				this.filteredCustomers = [...this.allCustomers];
				this.isSearchCompleted = false;
			},

			// 清除快速搜索
			clearQuickSearch: function() {
				this.searchForm.customerName = '';
				this.doSearch();
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
			},

			// 加载更多数据
			loadMore() {
				uni.showToast({
					title: '加载中...'+this.$data.page,
					icon: 'loading',
					mask: true, // 遮罩层，防止用户操作
					duration: 2000
				});
			}
		}
	};
</script>

<style scoped>
	.customer-list-page {
		background-color: #f5f5f5;
		min-height: 100vh;
		padding-bottom: 120rpx;
		/* 为悬浮按钮预留空间 */
	}

	/* 顶部导航栏 */
	.navbar {
		display: flex;
		justify-content: center;
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

	/* 搜索栏 */
	.search-bar {
		display: flex;
		padding: 30rpx 24rpx;
		background-color: #fff;
		border-bottom: 1px solid #eee;
		align-items: center;
	}

	.search-input-container {
		flex: 1;
		display: flex;
		align-items: center;
		background-color: #f5f5f5;
		border-radius: 30rpx;
		padding: 0 20rpx;
		border: 1px solid #ddd;
	}

	.search-icon {
		margin-right: 10rpx;
	}

	.search-input {
		flex: 1;
		height: 100%;
		font-size: 28rpx;
		background-color: transparent;
	}

	.clear-icon {
		margin-left: 10rpx;
	}

	.expand-btn {
		display: flex;
		align-items: center;
		justify-content: center;
		width: 120rpx;
		height: 60rpx;
		background-color: #f5f5f5;
		color: #666;
		border-radius: 30rpx;
		font-size: 26rpx;
		margin-left: 15rpx;
		border: 1px solid #ddd;
	}

	.expand-btn uni-icons {
		margin-right: 5rpx;
	}

	/* 高级搜索区域 */
	.advanced-search {
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

	.reset-btn,
	.do-search-btn {
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

	/* 客户列表容器 */
	.customer-list {
		display: flex;
		
		gap: 20rpx;
		padding: 5px;
		height: 100vh;		
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
	.row-2,
	.row-3 {
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
	.floating-add-btn {
		position: fixed;
		right: 30rpx;
		bottom: 150rpx;
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

	/* 下拉弹窗 */
	.dropdown-popup,
	.date-picker-popup {
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

	uni-button:after {
		border: none !important;
	}
</style>