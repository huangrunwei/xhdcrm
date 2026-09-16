<template>
	<view class="attachment-page">
		<view class="page-title">附件列表</view>
		
		<!-- 附件列表容器 -->
		<view class="attachment-list">
			<!-- 单个附件项 -->
			<view class="attachment-item" v-for="(item, index) in attachments" :key="index">
				<!-- 左侧图标 -->
				<view class="attachment-icon">
					<uni-icons  type="paperclip" 
						size="24" 
						color="#606266"
					></uni-icons>
				</view>
				
				<!-- 附件名称 -->
				<view class="attachment-name">
					<text>{{ item.name }}</text>
				</view>
				
				<!-- 下载按钮 -->
				<view class="download-btn" @click="downloadFile(item.url)">
					<uni-icons type="download" size="20" color="#409EFF"></uni-icons>
					<text class="btn-text">下载</text>
				</view>
			</view>
		</view>
	</view>
</template>

<script>
export default {
	data() {
		return {
			// 附件列表数据
			attachments: [
				{
					name: "项目需求文档V2.5.1.docx",
					url: "/static/files/requirement.docx",
					type: "doc"
				},
				{
					name: "UI设计规范.pdf",
					url: "/static/files/ui_design.pdf",
					type: "pdf"
				},
				{
					name: "数据库设计方案.xlsx",
					url: "/static/files/db_design.xlsx",
					type: "xls"
				},
				{
					name: "产品原型图及说明.zip",
					url: "/static/files/prototype.zip",
					type: "zip"
				},
				{
					name: "会议记录20231015.txt",
					url: "/static/files/meeting_notes.txt",
					type: "txt"
				},
				{
					name: "非常长的文件名称示例用于测试文字超出隐藏效果.pdf",
					url: "/static/files/long_name_example.pdf",
					type: "pdf"
				}
			]
		};
	},
	methods: {
		// 根据文件类型获取对应的图标
		getIconType(fileType) {
			switch(fileType.toLowerCase()) {
				case 'doc':
				case 'docx':
					return 'doc';
				case 'xls':
				case 'xlsx':
					return 'excel';
				case 'pdf':
					return 'pdf';
				case 'zip':
				case 'rar':
					return 'zip';
				case 'txt':
					return 'text';
				default:
					return 'file';
			}
		},
		
		// 下载文件
		downloadFile(url) {
			// 这里实际项目中，这里应该调用文件下载的API
			uni.showToast({
				title: '开始下载文件',
				icon: 'none'
			});
			
			// 模拟下载过程
			setTimeout(() => {
				uni.showToast({
					title: '文件下载完成',
					icon: 'success'
				});
			}, 1500);
		}
	}
};
</script>

<style scoped>
.attachment-page {
	padding: 16px;
	background-color: #f5f7fa;
	min-height: 100vh;
}

.page-title {
	font-size: 18px;
	font-weight: 500;
	color: #303133;
	margin-bottom: 20px;
	padding-bottom: 10px;
	border-bottom: 1px solid #eee;
}

.attachment-list {
	background-color: #fff;
	border-radius: 8px;
	overflow: hidden;
}

.attachment-item {
	display: flex;
	align-items: center;
	padding: 14px 16px;
	border-bottom: 1px solid #f5f5f5;
}

.attachment-item:last-child {
	border-bottom: none;
}

.attachment-icon {
	width: 40px;
	text-align: center;
}

.attachment-name {
	flex: 1;
	margin: 0 16px;
	overflow: hidden;
}

.attachment-name text {
	display: block;
	white-space: nowrap;
	overflow: hidden;
	text-overflow: ellipsis;
	font-size: 14px;
	color: #606266;
}

.download-btn {
	display: flex;
	align-items: center;
	color: #409EFF;
	font-size: 14px;
}

.btn-text {
	margin-left: 4px;
}
</style>
