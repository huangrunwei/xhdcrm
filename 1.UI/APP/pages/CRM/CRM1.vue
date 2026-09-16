<template>
  <view class="goods-search-page">
    <!-- 搜索栏 -->
    <view class="search-bar">
      <view class="search-input-box">
        <uni-icons type="search" size="28" color="#999" class="search-icon"></uni-icons>
        <input 
          type="text" 
          v-model="searchText" 
          placeholder="搜索商品" 
          @confirm="doSearch"
          class="search-input"
        />
        <uni-icons 
          type="clear" 
          size="24" 
          color="#999" 
          v-if="searchText" 
          @click="searchText = ''"
          class="clear-icon"
        ></uni-icons>
      </view>
      <button class="cancel-btn" @click="onCancel">取消</button>
    </view>
    
    <!-- 筛选工具栏 -->
    <view class="filter-toolbar">
      <view 
        class="toolbar-item" 
        v-for="(item, index) in toolbarItems" 
        :key="index"
        @click="handleToolbarClick(index)"
      >
        <text :class="{ 'active': activeToolbarIndex === index }">{{ item.text }}</text>
        <uni-icons 
          :type="item.icon" 
          size="20" 
          color="#999"
          :class="{ 'active': activeToolbarIndex === index, 'rotate': item.rotate && activeToolbarIndex === index }"
        ></uni-icons>
      </view>
    </view>
    
    <!-- 筛选弹窗 - 分类 -->
    <view class="filter-popup" v-if="showCategoryFilter">
      <view class="popup-mask" @click="hideAllFilters"></view>
      <view class="popup-content category-popup">
        <view class="popup-header">
          <text class="popup-title">分类</text>
          <button class="reset-btn" @click="resetCategoryFilter">重置</button>
        </view>
        
        <view class="category-container">
          <view class="category-left">
            <view 
              class="category-item" 
              v-for="(category, index) in categories" 
              :key="index"
              :class="{ 'active': activeCategory === index }"
              @click="activeCategory = index"
            >
              {{ category.name }}
            </view>
          </view>
          
          <view class="category-right">
            <view class="subcategory-title">
              {{ categories[activeCategory] && categories[activeCategory].name || '全部分类' }}
            </view>
            <view class="subcategory-list">
              <view 
                class="subcategory-item" 
                v-for="(sub, i) in (categories[activeCategory] && categories[activeCategory].subcategories) || []" 
                :key="i"
                :class="{ 'selected': selectedSubcategories.indexOf(sub.id) !== -1 }"
                @click="toggleSubcategory(sub.id)"
              >
                {{ sub.name }}
                <uni-icons 
                  type="checkmark" 
                  size="24" 
                  color="#ff4400" 
                  v-if="selectedSubcategories.indexOf(sub.id) !== -1"
                ></uni-icons>
              </view>
            </view>
          </view>
        </view>
        
        <view class="popup-footer">
          <button class="cancel-filter" @click="hideAllFilters">取消</button>
          <button class="confirm-filter" @click="confirmCategoryFilter">确定</button>
        </view>
      </view>
    </view>
    
    <!-- 筛选弹窗 - 筛选 -->
    <view class="filter-popup" v-if="showAdvancedFilter">
      <view class="popup-mask" @click="hideAllFilters"></view>
      <view class="popup-content advanced-popup">
        <view class="popup-header">
          <text class="popup-title">筛选</text>
          <button class="reset-btn" @click="resetAdvancedFilter">重置</button>
        </view>
        
        <!-- 价格区间 -->
        <view class="filter-section">
          <text class="section-title">价格区间</text>
          <view class="price-range">
            <input 
              type="number" 
              v-model="priceRange.min" 
              placeholder="最低价" 
              class="price-input"
            />
            <text class="price-separator">-</text>
            <input 
              type="number" 
              v-model="priceRange.max" 
              placeholder="最高价" 
              class="price-input"
            />
          </view>
        </view>
        
        <!-- 销量筛选 -->
        <view class="filter-section">
          <text class="section-title">销量</text>
          <view class="filter-options">
            <view 
              class="filter-option" 
              v-for="(option, index) in salesOptions" 
              :key="index"
              :class="{ 'selected': selectedSales === option.value }"
              @click="selectedSales = option.value"
            >
              {{ option.text }}
              <uni-icons 
                type="checkmark" 
                size="24" 
                color="#ff4400" 
                v-if="selectedSales === option.value"
              ></uni-icons>
            </view>
          </view>
        </view>
        
        <!-- 好评筛选 -->
        <view class="filter-section">
          <text class="section-title">好评</text>
          <view class="filter-options">
            <view 
              class="filter-option" 
              v-for="(option, index) in ratingOptions" 
              :key="index"
              :class="{ 'selected': selectedRating === option.value }"
              @click="selectedRating = option.value"
            >
              {{ option.text }}
              <uni-icons 
                type="checkmark" 
                size="24" 
                color="#ff4400" 
                v-if="selectedRating === option.value"
              ></uni-icons>
            </view>
          </view>
        </view>
        
        <!-- 发货地 -->
        <view class="filter-section">
          <text class="section-title">发货地</text>
          <view class="filter-options">
            <view 
              class="filter-option" 
              v-for="(city, index) in cities" 
              :key="index"
              :class="{ 'selected': selectedCity === city.code }"
              @click="selectedCity = city.code"
            >
              {{ city.name }}
              <uni-icons 
                type="checkmark" 
                size="24" 
                color="#ff4400" 
                v-if="selectedCity === city.code"
              ></uni-icons>
            </view>
          </view>
        </view>
        
        <view class="popup-footer">
          <button class="cancel-filter" @click="hideAllFilters">取消</button>
          <button class="confirm-filter" @click="confirmAdvancedFilter">确定</button>
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
          @click="removeFilter(filter.type, filter.value)"
        ></uni-icons>
      </view>
      <view class="clear-all" @click="clearAllFilters" v-if="selectedFilterTags.length > 0">
        清除全部
      </view>
    </view>
    
    <!-- 商品列表 -->
    <view class="goods-list">
      <view class="goods-item" v-for="(item, index) in filteredGoods" :key="index">
        <view class="goods-image">
          <image :src="item.image" mode="aspectFill"></image>
          <view class="sales-tag" v-if="item.sales > 1000">
            {{ Math.floor(item.sales/1000) }}k+ 人已买
          </view>
        </view>
        <view class="goods-info">
          <text class="goods-title">{{ item.title }}</text>
          <view class="goods-rating">
            <uni-icons type="star" size="20" color="#FFD700"></uni-icons>
            <text class="rating-text">{{ item.rating.toFixed(1) }}</text>
            <text class="sales-text">({{ item.reviews }}评价)</text>
          </view>
          <text class="goods-price">¥{{ item.price.toFixed(2) }}</text>
          <view class="goods-shop">
            <text>{{ item.shop }}</text>
            <uni-icons type="arrowright" size="18" color="#999"></uni-icons>
          </view>
        </view>
      </view>
      
      <!-- 无结果状态 -->
      <view class="no-results" v-if="filteredGoods.length === 0 && hasSearched">
        <uni-icons type="empty" size="100" color="#ccc"></uni-icons>
        <text class="no-result-text">没有找到符合条件的商品</text>
        <button class="reset-all" @click="clearAllFilters">清除筛选条件</button>
      </view>
      
      <!-- 加载更多 -->
      <view class="load-more" v-if="filteredGoods.length > 0 && hasSearched">
        <text>加载更多...</text>
      </view>
    </view>
  </view>
</template>

<script>
export default {
  data() {
    return {
      // 搜索文本
      searchText: '',
      
      // 工具栏选项
      toolbarItems: [
        { text: '分类', icon: 'arrowdown', rotate: false },
        { text: '筛选', icon: 'arrowdown', rotate: false },
        { text: '综合', icon: 'arrowdown', rotate: true },
        { text: '销量', icon: 'arrowdown', rotate: true },
        { text: '价格', icon: 'arrowdown', rotate: true }
      ],
      
      // 激活的工具栏索引
      activeToolbarIndex: -1,
      
      // 筛选弹窗显示状态
      showCategoryFilter: false,
      showAdvancedFilter: false,
      
      // 分类数据
      categories: [
        {
          id: 'all',
          name: '全部分类',
          subcategories: []
        },
        {
          id: 'electronics',
          name: '电子产品',
          subcategories: [
            { id: 'phone', name: '手机' },
            { id: 'computer', name: '电脑' },
            { id: 'camera', name: '相机' },
            { id: 'audio', name: '音频设备' },
            { id: 'accessory', name: '配件' }
          ]
        },
        {
          id: 'clothing',
          name: '服装鞋帽',
          subcategories: [
            { id: 'men', name: '男装' },
            { id: 'women', name: '女装' },
            { id: 'shoes', name: '鞋靴' },
            { id: 'bag', name: '箱包' },
            { id: 'underwear', name: '内衣' }
          ]
        },
        {
          id: 'home',
          name: '家居用品',
          subcategories: [
            { id: 'furniture', name: '家具' },
            { id: 'kitchen', name: '厨房用品' },
            { id: 'bath', name: '卫浴' },
            { id: 'decor', name: '装饰' }
          ]
        }
      ],
      
      // 选中的分类索引
      activeCategory: 0,
      // 选中的子分类
      selectedSubcategories: [],
      
      // 价格范围
      priceRange: {
        min: '',
        max: ''
      },
      
      // 销量选项
      salesOptions: [
        { text: '全部销量', value: 'all' },
        { text: '销量从高到低', value: 'high' },
        { text: '销量从低到高', value: 'low' }
      ],
      selectedSales: 'all',
      
      // 好评选项
      ratingOptions: [
        { text: '全部好评', value: 'all' },
        { text: '好评率90%以上', value: '90+' },
        { text: '好评率95%以上', value: '95+' }
      ],
      selectedRating: 'all',
      
      // 城市选项
      cities: [
        { code: 'all', name: '全国' },
        { code: 'bj', name: '北京' },
        { code: 'sh', name: '上海' },
        { code: 'gz', name: '广州' },
        { code: 'sz', name: '深圳' },
        { code: 'hz', name: '杭州' }
      ],
      selectedCity: 'all',
      
      // 排序方式
      sortType: 'default', // default, sales, price_asc, price_desc
      
      // 已选筛选条件标签
      selectedFilterTags: [],
      
      // 商品数据
      allGoods: [
        {
          id: 1,
          title: '智能手机 5G全网通 128GB大存储 高清拍照游戏手机',
          price: 3299,
          image: 'https://picsum.photos/seed/phone1/300/300',
          rating: 4.7,
          reviews: 12568,
          sales: 56890,
          shop: '官方旗舰店',
          category: 'phone',
          city: 'sz'
        },
        {
          id: 2,
          title: '男士纯棉T恤 夏季新款 宽松休闲短袖上衣',
          price: 89,
          image: 'https://picsum.photos/seed/tshirt1/300/300',
          rating: 4.5,
          reviews: 8923,
          sales: 32560,
          shop: '男装专营店',
          category: 'men',
          city: 'sh'
        },
        {
          id: 3,
          title: '笔记本电脑 轻薄便携 商务办公本 学生游戏本',
          price: 4599,
          image: 'https://picsum.photos/seed/laptop1/300/300',
          rating: 4.6,
          reviews: 5621,
          sales: 12890,
          shop: '电脑数码旗舰店',
          category: 'computer',
          city: 'bj'
        },
        {
          id: 4,
          title: '无线蓝牙耳机 降噪长续航 运动跑步入耳式',
          price: 299,
          image: 'https://picsum.photos/seed/headphone1/300/300',
          rating: 4.4,
          reviews: 15689,
          sales: 89650,
          shop: '音频设备专营店',
          category: 'audio',
          city: 'gz'
        },
        {
          id: 5,
          title: '现代简约沙发 客厅家具 三人位布艺沙发组合',
          price: 2899,
          image: 'https://picsum.photos/seed/sofa1/300/300',
          rating: 4.8,
          reviews: 2356,
          sales: 4560,
          shop: '家居生活馆',
          category: 'furniture',
          city: 'hz'
        },
        {
          id: 6,
          title: '女士连衣裙 夏季新款 碎花雪纺长裙 收腰显瘦',
          price: 159,
          image: 'https://picsum.photos/seed/dress1/300/300',
          rating: 4.3,
          reviews: 9876,
          sales: 45210,
          shop: '女装旗舰店',
          category: 'women',
          city: 'sh'
        },
        {
          id: 7,
          title: '机械键盘 青轴红轴茶轴 电竞游戏办公专用',
          price: 329,
          image: 'https://picsum.photos/seed/keyboard1/300/300',
          rating: 4.9,
          reviews: 7654,
          sales: 23150,
          shop: '电脑外设专营店',
          category: 'accessory',
          city: 'sz'
        },
        {
          id: 8,
          title: '运动鞋 男女款 跑步鞋 轻便透气减震',
          price: 369,
          image: 'https://picsum.photos/seed/shoes1/300/300',
          rating: 4.6,
          reviews: 11234,
          sales: 67890,
          shop: '运动品牌旗舰店',
          category: 'shoes',
          city: 'gz'
        }
      ],
      
      // 筛选后的商品
      filteredGoods: [],
      
      // 是否已经进行过搜索
      hasSearched: false
    };
  },
  
  computed: {
    // 是否有已选条件 (Vue 2 语法)
    hasSelectedFilters: function() {
      return this.selectedFilterTags.length > 0;
    }
  },
  
  methods: {
    // 处理工具栏点击
    handleToolbarClick: function(index) {
      // 隐藏所有筛选弹窗
      this.hideAllFilters();
      
      // 处理排序相关
      if (index >= 2) {
        // 综合排序
        if (index === 2) {
          this.sortType = this.sortType === 'default' ? 'default_desc' : 'default';
        }
        // 销量排序
        else if (index === 3) {
          this.sortType = this.sortType === 'sales' ? 'sales_asc' : 'sales';
        }
        // 价格排序
        else if (index === 4) {
          this.sortType = this.sortType === 'price_asc' ? 'price_desc' : 'price_asc';
        }
        
        // 应用排序并更新列表
        this.applyFilters();
        
        // 更新激活状态
        this.activeToolbarIndex = index;
      } 
      // 分类筛选
      else if (index === 0) {
        this.showCategoryFilter = true;
        this.activeToolbarIndex = index;
      } 
      // 高级筛选
      else if (index === 1) {
        this.showAdvancedFilter = true;
        this.activeToolbarIndex = index;
      }
    },
    
    // 隐藏所有筛选弹窗
    hideAllFilters: function() {
      this.showCategoryFilter = false;
      this.showAdvancedFilter = false;
      this.activeToolbarIndex = -1;
    },
    
    // 切换子分类选择
    toggleSubcategory: function(id) {
      var index = this.selectedSubcategories.indexOf(id);
      if (index !== -1) {
        this.selectedSubcategories.splice(index, 1);
      } else {
        this.selectedSubcategories.push(id);
      }
    },
    
    // 确认分类筛选
    confirmCategoryFilter: function() {
      this.showCategoryFilter = false;
      
      // 清除之前的分类筛选标签
      this.selectedFilterTags = this.selectedFilterTags.filter(
        function(tag) {
          return tag.type !== 'category';
        }
      );
      
      // 添加新的分类筛选标签
      if (this.selectedSubcategories.length > 0) {
        var categoryName = this.categories[this.activeCategory].name;
        var subNames = this.categories[this.activeCategory].subcategories
          .filter(function(sub) {
            return this.selectedSubcategories.indexOf(sub.id) !== -1;
          }.bind(this))
          .map(function(sub) {
            return sub.name;
          });
          
        this.selectedFilterTags.push({
          type: 'category',
          text: categoryName + ': ' + subNames.join(','),
          value: this.selectedSubcategories.join(',')
        });
      }
      
      // 应用筛选
      this.applyFilters();
    },
    
    // 重置分类筛选
    resetCategoryFilter: function() {
      this.activeCategory = 0;
      this.selectedSubcategories = [];
    },
    
    // 确认高级筛选
    confirmAdvancedFilter: function() {
      this.showAdvancedFilter = false;
      
      // 清除之前的价格、销量、好评、城市筛选标签
      this.selectedFilterTags = this.selectedFilterTags.filter(
        function(tag) {
          return ['price', 'sales', 'rating', 'city'].indexOf(tag.type) === -1;
        }
      );
      
      // 添加价格筛选标签
      if (this.priceRange.min || this.priceRange.max) {
        var priceText = '';
        if (this.priceRange.min && this.priceRange.max) {
          priceText = this.priceRange.min + '-' + this.priceRange.max + '元';
        } else if (this.priceRange.min) {
          priceText = this.priceRange.min + '元以上';
        } else if (this.priceRange.max) {
          priceText = '低于' + this.priceRange.max + '元';
        }
        
        this.selectedFilterTags.push({
          type: 'price',
          text: '价格: ' + priceText,
          value: this.priceRange.min + '-' + this.priceRange.max
        });
      }
      
      // 添加销量筛选标签
      if (this.selectedSales !== 'all') {
        var salesText = this.salesOptions.find(function(option) {
          return option.value === this.selectedSales;
        }.bind(this)).text;
        
        this.selectedFilterTags.push({
          type: 'sales',
          text: '销量: ' + salesText,
          value: this.selectedSales
        });
      }
      
      // 添加好评筛选标签
      if (this.selectedRating !== 'all') {
        var ratingText = this.ratingOptions.find(function(option) {
          return option.value === this.selectedRating;
        }.bind(this)).text;
        
        this.selectedFilterTags.push({
          type: 'rating',
          text: '好评: ' + ratingText,
          value: this.selectedRating
        });
      }
      
      // 添加城市筛选标签
      if (this.selectedCity !== 'all') {
        var cityText = this.cities.find(function(city) {
          return city.code === this.selectedCity;
        }.bind(this)).name;
        
        this.selectedFilterTags.push({
          type: 'city',
          text: '发货地: ' + cityText,
          value: this.selectedCity
        });
      }
      
      // 应用筛选
      this.applyFilters();
    },
    
    // 重置高级筛选
    resetAdvancedFilter: function() {
      this.priceRange = { min: '', max: '' };
      this.selectedSales = 'all';
      this.selectedRating = 'all';
      this.selectedCity = 'all';
    },
    
    // 移除单个筛选条件
    removeFilter: function(type, value) {
      this.selectedFilterTags = this.selectedFilterTags.filter(function(tag) {
        return !(tag.type === type && tag.value === value);
      });
      
      // 根据类型重置对应的筛选条件
      if (type === 'category') {
        this.selectedSubcategories = [];
      } else if (type === 'price') {
        this.priceRange = { min: '', max: '' };
      } else if (type === 'sales') {
        this.selectedSales = 'all';
      } else if (type === 'rating') {
        this.selectedRating = 'all';
      } else if (type === 'city') {
        this.selectedCity = 'all';
      }
      
      // 应用筛选
      this.applyFilters();
    },
    
    // 清除所有筛选条件
    clearAllFilters: function() {
      this.searchText = '';
      this.selectedFilterTags = [];
      this.resetCategoryFilter();
      this.resetAdvancedFilter();
      this.sortType = 'default';
      this.activeToolbarIndex = -1;
      
      // 应用筛选
      this.applyFilters();
    },
    
    // 执行搜索
    doSearch: function() {
      this.hasSearched = true;
      this.applyFilters();
    },
    
    // 取消搜索
    onCancel: function() {
      uni.navigateBack();
    },
    
    // 应用所有筛选条件
    applyFilters: function() {
      var results = [].concat(this.allGoods);
      var self = this;
      
      // 文本搜索筛选
      if (this.searchText) {
        var searchStr = this.searchText.toLowerCase();
        results = results.filter(function(item) {
          return item.title.toLowerCase().indexOf(searchStr) !== -1 ||
                 item.shop.toLowerCase().indexOf(searchStr) !== -1;
        });
      }
      
      // 分类筛选
      if (this.selectedSubcategories.length > 0) {
        results = results.filter(function(item) {
          return self.selectedSubcategories.indexOf(item.category) !== -1;
        });
      }
      
      // 价格筛选
      if (this.priceRange.min) {
        var minPrice = Number(this.priceRange.min);
        results = results.filter(function(item) {
          return item.price >= minPrice;
        });
      }
      if (this.priceRange.max) {
        var maxPrice = Number(this.priceRange.max);
        results = results.filter(function(item) {
          return item.price <= maxPrice;
        });
      }
      
      // 销量筛选
      if (this.selectedSales === 'high') {
        results.sort(function(a, b) {
          return b.sales - a.sales;
        });
      } else if (this.selectedSales === 'low') {
        results.sort(function(a, b) {
          return a.sales - b.sales;
        });
      }
      
      // 好评筛选
      if (this.selectedRating === '90+') {
        results = results.filter(function(item) {
          return item.rating >= 4.5;
        });
      } else if (this.selectedRating === '95+') {
        results = results.filter(function(item) {
          return item.rating >= 4.7;
        });
      }
      
      // 发货地筛选
      if (this.selectedCity !== 'all') {
        results = results.filter(function(item) {
          return item.city === self.selectedCity;
        });
      }
      
      // 排序处理
      if (this.sortType === 'sales') {
        results.sort(function(a, b) {
          return b.sales - a.sales;
        });
      } else if (this.sortType === 'sales_asc') {
        results.sort(function(a, b) {
          return a.sales - b.sales;
        });
      } else if (this.sortType === 'price_asc') {
        results.sort(function(a, b) {
          return a.price - b.price;
        });
      } else if (this.sortType === 'price_desc') {
        results.sort(function(a, b) {
          return b.price - a.price;
        });
      }
      
      // 更新筛选结果
      this.filteredGoods = results;
    }
  }
};
</script>

<style scoped>
.goods-search-page {
  min-height: 100vh;
  background-color: #f5f5f5;
}

/* 搜索栏样式 */
.search-bar {
  display: flex;
  padding: 16rpx 24rpx;
  background-color: #fff;
  align-items: center;
}

.search-input-box {
  flex: 1;
  display: flex;
  align-items: center;
  background-color: #f5f5f5;
  border-radius: 30rpx;
  padding: 12rpx 20rpx;
}

.search-icon {
  margin-right: 10rpx;
}

.search-input {
  flex: 1;
  font-size: 28rpx;
  height: 40rpx;
  line-height: 40rpx;
}

.clear-icon {
  margin-left: 10rpx;
}

.cancel-btn {
  font-size: 30rpx;
  color: #333;
  background-color: transparent;
  padding: 0 20rpx;
  height: auto;
  line-height: normal;
}

/* 筛选工具栏 */
.filter-toolbar {
  display: flex;
  background-color: #fff;
  border-bottom: 1px solid #eee;
  overflow-x: auto;
  white-space: nowrap;
}

.toolbar-item {
  display: flex;
  align-items: center;
  padding: 0 20rpx;
  height: 80rpx;
  font-size: 28rpx;
  color: #333;
  flex-shrink: 0;
}

.toolbar-item text {
  margin-right: 6rpx;
}

.toolbar-item.active text,
.toolbar-item.active uni-icons {
  color: #ff4400;
}

.toolbar-item .rotate {
  transform: rotate(180deg);
  transition: transform 0.3s;
}

/* 筛选弹窗 */
.filter-popup {
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
  max-height: 80vh;
  overflow-y: auto;
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

.reset-btn {
  font-size: 28rpx;
  color: #666;
  background-color: transparent;
  padding: 0;
  height: auto;
  line-height: normal;
}

.popup-footer {
  display: flex;
  padding: 20rpx;
  border-top: 1px solid #eee;
}

.cancel-filter, .confirm-filter {
  flex: 1;
  height: 80rpx;
  line-height: 80rpx;
  font-size: 30rpx;
  border-radius: 10rpx;
  margin: 0 10rpx;
}

.cancel-filter {
  background-color: #f5f5f5;
  color: #333;
}

.confirm-filter {
  background-color: #ff4400;
  color: #fff;
  border-color: #ff4400;
}

/* 分类弹窗 */
.category-popup .popup-content {
  display: flex;
  flex-direction: column;
}

.category-container {
  display: flex;
  flex: 1;
  overflow: hidden;
}

.category-left {
  width: 200rpx;
  background-color: #f5f5f5;
  overflow-y: auto;
}

.category-item {
  padding: 30rpx 20rpx;
  text-align: center;
  font-size: 28rpx;
  border-left: 4rpx solid transparent;
}

.category-item.active {
  background-color: #fff;
  border-left-color: #ff4400;
  color: #ff4400;
  font-weight: bold;
}

.category-right {
  flex: 1;
  padding: 20rpx;
  overflow-y: auto;
}

.subcategory-title {
  font-size: 28rpx;
  color: #666;
  margin-bottom: 20rpx;
  padding-left: 10rpx;
}

.subcategory-list {
  display: flex;
  flex-wrap: wrap;
}

.subcategory-item {
  width: 33.333%;
  padding: 15rpx 0;
  text-align: center;
  font-size: 28rpx;
  position: relative;
}

.subcategory-item.selected {
  color: #ff4400;
}

.subcategory-item.selected::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 50%;
  transform: translateX(-50%);
  width: 60%;
  height: 2rpx;
  background-color: #ff4400;
}

/* 高级筛选弹窗 */
.filter-section {
  padding: 20rpx 24rpx;
  border-bottom: 1px solid #eee;
}

.section-title {
  font-size: 30rpx;
  color: #333;
  margin-bottom: 20rpx;
  display: block;
}

.price-range {
  display: flex;
  align-items: center;
  padding: 10rpx 0;
}

.price-input {
  flex: 1;
  height: 60rpx;
  border: 1px solid #eee;
  border-radius: 8rpx;
  padding: 0 15rpx;
  font-size: 28rpx;
}

.price-separator {
  padding: 0 15rpx;
  font-size: 28rpx;
  color: #999;
}

.filter-options {
  display: flex;
  flex-wrap: wrap;
}

.filter-option {
  width: 33.333%;
  padding: 15rpx 0;
  text-align: center;
  font-size: 28rpx;
  position: relative;
}

.filter-option.selected {
  color: #ff4400;
}

/* 已选条件 */
.selected-filters {
  display: flex;
  flex-wrap: wrap;
  padding: 15rpx 20rpx;
  background-color: #fff;
  border-bottom: 1px solid #eee;
  gap: 15rpx;
}

.filter-tag {
  display: flex;
  align-items: center;
  background-color: #f5f5f5;
  border-radius: 20rpx;
  padding: 8rpx 15rpx;
  font-size: 26rpx;
  color: #666;
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

/* 商品列表 */
.goods-list {
  padding: 15rpx;
}

.goods-item {
  display: flex;
  background-color: #fff;
  border-radius: 10rpx;
  overflow: hidden;
  margin-bottom: 15rpx;
}

.goods-image {
  width: 200rpx;
  height: 200rpx;
  position: relative;
}

.goods-image image {
  width: 100%;
  height: 100%;
}

.sales-tag {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  background-color: rgba(0, 0, 0, 0.6);
  color: #fff;
  font-size: 22rpx;
  padding: 5rpx;
  text-align: center;
}

.goods-info {
  flex: 1;
  padding: 15rpx;
  display: flex;
  flex-direction: column;
}

.goods-title {
  font-size: 28rpx;
  color: #333;
  line-height: 1.4;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  margin-bottom: 10rpx;
}

.goods-rating {
  display: flex;
  align-items: center;
  margin-bottom: 10rpx;
}

.rating-text {
  font-size: 24rpx;
  color: #ff9900;
  margin-left: 5rpx;
}

.sales-text {
  font-size: 22rpx;
  color: #999;
  margin-left: 10rpx;
}

.goods-price {
  font-size: 30rpx;
  color: #ff4400;
  font-weight: bold;
  margin-bottom: 10rpx;
}

.goods-shop {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 24rpx;
  color: #666;
  margin-top: auto;
}

/* 无结果状态 */
.no-results {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 200rpx 0;
}

.no-result-text {
  font-size: 30rpx;
  color: #999;
  margin-top: 30rpx;
}

.reset-all {
  margin-top: 40rpx;
  background-color: #ff4400;
  color: #fff;
  border-color: #ff4400;
  height: 70rpx;
  line-height: 70rpx;
  font-size: 28rpx;
  padding: 0 40rpx;
}

/* 加载更多 */
.load-more {
  text-align: center;
  padding: 30rpx 0;
  font-size: 28rpx;
  color: #999;
}
</style>
    