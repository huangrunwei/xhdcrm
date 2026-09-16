<template>
	<view>
		<!-- 顶部导航栏 -->
		<!-- <view class="navbar">
			<text class="navbar-title">跟进详情</text>
		</view> -->
		<view class="container">

			<!-- 跟进详情卡片 -->
			<view class="detail-card">
				<!-- 客户信息 -->
				<view class="info-item">
					<text class="label">客户：</text>
					<text class="value">{{ followData.customer&&followData.customer.cus_name }}</text>
				</view>

				<!-- 联系人信息 -->
				<view class="info-item">
					<text class="label">联系人：</text>
					<text class="value">{{ followData.contact&&followData.contact.C_name }}</text>
				</view>

				<!-- 跟进目的 -->
				<view class="info-item">
					<text class="label">跟进目的：</text>
					<text class="value">{{ followData.FollowAim&&followData.FollowAim.params_name}}</text>
				</view>

				<!-- 跟进方式 -->
				<view class="info-item">
					<text class="label">跟进方式：</text>
					<text class="value">{{ followData.FollowType&&followData.FollowType.params_name }}</text>
				</view>

				<!-- 跟进内容 -->
				<view class="info-item content-item">
					<text class="label">跟进内容：</text>
					<text class="value">{{ followData.follow_content }}</text>
				</view>

				<!-- 编辑按钮 -->
				<button class="edit-btn" @click="onEdit">编辑</button>
			</view>
		</view>
	</view>
</template>

<script>
export default {
    data() {
        return {
            // 跟进数据
            followData: {},
           
        };
    },
    onLoad(options) {
        // 将接收的字符串解析为JSON对象
        const receivedData = JSON.parse(decodeURIComponent(options.data));
        
        this.followData = receivedData;
        console.log('onLoad 加载数据:', this.followData);
    },

    methods: {
        updateData(data) {
            this.followData = data;
			console.log('详情页已更新数据:', data);
        },

        // ===== 编辑按钮点击事件 =====
        onEdit() {
            console.log('进入编辑模式');
            const cusinfo = encodeURIComponent(JSON.stringify(this.followData));
            // ★★★ 关键改动：使用 navigateTo 而不是 reLaunch ★★★
            // 这样编辑保存后返回，才会触发 onShow 刷新
            setTimeout(() => {
                uni.navigateTo({
                    url: '/pages/follow/edit?data=' + cusinfo,
                });
            }, 500);
        },
    },
};
</script>

<style scoped>
	.container {
		padding: 16px;
		background-color: #f5f5f5;
		min-height: 100vh;
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

	/* 详情卡片样式 */
	.detail-card {
		background-color: #ffffff;
		border-radius: 12px;
		padding: 20px;
		box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
	}

	/* 信息项样式 */
	.info-item {
		display: flex;
		padding: 12px 0;
		border-bottom: 1px solid #f0f0f0;
		align-items: flex-start;
	}

	/* 最后一项去掉下边框 */
	.info-item:last-child:not(.content-item) {
		border-bottom: none;
	}

	/* 标签样式 */
	.label {
		color: #666666;
		width: 100px;
		font-size: 14px;
		line-height: 22px;
	}

	/* 值样式 */
	.value {
		flex: 1;
		color: #333333;
		font-size: 14px;
		line-height: 22px;
	}

	/* 跟进内容项特殊样式 */
	.content-item {
		align-items: flex-start;
	}

	.content-item .value {
		white-space: pre-line;
	}

	/* 编辑按钮样式 */
	.edit-btn {
		width: 100%;
		height: 44px;
		line-height: 44px;
		background-color: #1677ff;
		color: #ffffff;
		border-radius: 8px;
		font-size: 16px;
		margin-top: 20px;
		border: none;
	}

	/* 按钮激活状态样式 */
	.edit-btn::after {
		border: none;
	}

	.edit-btn:active {
		background-color: #0e65d9;
	}
</style>