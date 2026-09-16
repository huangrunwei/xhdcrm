<template>
	<view>
		<!-- 顶部导航栏 -->
		<!-- <view class="navbar">
			<text class="navbar-title">联系人详情</text>
		</view> -->
		<view class="container">
			<!-- 跟进详情卡片 -->
			<view class="detail-card">
				<!-- 联系人信息 -->
				<view class="info-item">
					<text class="label">姓名：</text>
					<text class="value">{{ contactData.C_name }}</text>
				</view>

				<!-- 客户信息 -->
				<view class="info-item">
					<text class="label">客户：</text>
					<text class="value">{{ contactData.customer&&contactData.customer.cus_name }}</text>
				</view>

				<view class="info-item">
					<text class="label">电话：</text>
					<text class="value">{{ contactData.C_tel }}</text>
					<!-- 电话图标（点击触发拨号） -->
					<view class="call-icon" @click="makePhoneCall(contactData.C_tel)">
						<!-- 使用 UniApp 内置图标（需确保项目已引入图标库） -->
						<uni-icons type="phone" size="20" color="#007AFF"></uni-icons>
					</view>
				</view>

				<view class="info-item">
					<text class="label">手机：</text>
					<text class="value">{{ contactData.C_mob }}</text>
					<!-- 电话图标（点击触发拨号） -->
					<view class="call-icon" @click="makePhoneCall(contactData.C_mob )">
						<!-- 使用 UniApp 内置图标（需确保项目已引入图标库） -->
						<uni-icons type="phone" size="20" color="#007AFF"></uni-icons>
					</view>
				</view>

				<view class="info-item">
					<text class="label">微信：</text>
					<text class="value">{{ contactData.C_weichat }}</text>
				</view>

				<view class="info-item">
					<text class="label">邮箱：</text>
					<text class="value">{{ contactData.C_email }}</text>
				</view>
				<view class="info-item">
					<text class="label">部门：</text>
					<text class="value">{{ contactData.C_department }}</text>
				</view>
				<view class="info-item">
					<text class="label">职务：</text>
					<text class="value">{{ contactData.C_position }}</text>
				</view>
				<view class="info-item">
					<text class="label">QQ：</text>
					<text class="value">{{ contactData.C_QQ }}</text>
				</view>
				<view class="info-item">
					<text class="label">生日：</text>
					<text class="value">{{ contactData.C_birthday }}</text>
				</view>
				<view class="info-item">
					<text class="label">爱好：</text>
					<text class="value">{{ contactData.C_hobby }}</text>
				</view>
				<view class="info-item">
					<text class="label">地址：</text>
					<text class="value">{{ contactData.C_add }}</text>
				</view>
				<view class="info-item">
					<text class="label">备注：</text>
					<text class="value">{{ contactData.C_remarks }}</text>
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
            contactData: {},            
        };
    },
    onLoad(options) {
        // 将接收的字符串解析为JSON对象
        const receivedData = JSON.parse(decodeURIComponent(options.data));
       
        // 直接赋值显示
        this.contactData = receivedData;
		
        console.log('onLoad 加载数据:', this.contactData);
    },
    
    methods: {
        updateData(data) {
            this.contactData = data;
        	console.log('详情页已更新数据:', data);
        },

        // 编辑按钮点击事件
        onEdit() {
            // 将JSON对象转为字符串（encodeURIComponent处理特殊字符）
            const cusinfo = encodeURIComponent(JSON.stringify(this.contactData));
            // 使用 navigateTo 而不是 reLaunch，这样编辑保存后可以返回并触发 onShow
            uni.navigateTo({
                url: '/pages/Contact/edit?data=' + cusinfo,
            });
        },

        makePhoneCall(phone) {
            if (!phone) {
                return;
            }
            uni.showModal({
                title: '提示',
                content: '确定拨号吗？',
                cancelText: '取消',
                confirmText: '确定',
                success: (res) => {
                    if (res.confirm) {
                        if (uni.getSystemInfoSync().platform === 'h5') {
                            const a = document.createElement('a');
                            a.href = `tel:${phone}`;
                            a.click();
                        } else {
                            uni.makePhoneCall({
                                phoneNumber: phone,
                                success: () => console.log('拨号请求已发起'),
                                fail: (err) => {
                                    console.error('拨号失败：', err);
                                    uni.showToast({
                                        title: '拨号失败，请重试',
                                        icon: 'none',
                                    });
                                },
                            });
                        }
                    }
                },
            });
        },
    },
};
</script>

<style scoped>
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

	.container {
		padding: 16px;
		background-color: #f5f5f5;
		min-height: 100vh;
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
	
	.call-icon
	{
		/* border:1px solid #ddd;
		border-radius: 5px;
		padding: 5px; */
	}
</style>