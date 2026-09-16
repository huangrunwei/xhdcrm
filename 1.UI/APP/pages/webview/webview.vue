<template>
	<view class="container">
		<web-view :src="url" @message="handleMessage"></web-view>
	</view>
</template>

<script>
	export default {
		data() {
			return {
				url: ''
			};
		},
		onLoad(options) {
			// 接收传递过来的url参数
			this.url = options.url;
		},
		methods: {
			handleMessage(e) {
				// 处理web-view发送过来的消息
				console.log('收到消息：', e.detail.data);
			}
		}
	};
</script>

<style>
	.container {
		width: 100%;
		height: 100%;
	}
</style>