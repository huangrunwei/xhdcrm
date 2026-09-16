// 请求拦截器
const requestInterceptor = (config) => {
	// 可以在这里添加token等公共参数
	const token = uni.getStorageSync('token');
	if (token) {
		config.header.Authorization = `Bearer ${token}`;
	}

	// 显示加载中
	uni.showLoading({
		title: '加载中...',
		mask: true
	});

	return config;
};

// 响应拦截器
const responseInterceptor = (response) => {
	// 隐藏加载中
	uni.hideLoading();

	// 处理响应数据
	if (response.statusCode === 200) {
		// 假设后端返回格式为 { code, data, msg }
		if (response.data.code === 0) {
			return response.data.data;
		} else {
			// 业务错误
			uni.showToast({
				title: response.data.msg || '请求失败',
				icon: 'none',
				duration: 2000
			});
			return Promise.reject(response.data);
		}
	} else {
		// HTTP错误
		uni.showToast({
			title: `请求错误: ${response.statusCode}`,
			icon: 'none',
			duration: 2000
		});
		return Promise.reject(response);
	}
};

// 错误处理
const errorHandler = (error) => {
	uni.hideLoading();
	uni.showToast({
		title: '网络异常，请稍后再试',
		icon: 'none',
		duration: 2000
	});
	return Promise.reject(error);
};

// 封装请求方法
const request = (options) => {
	// 配置基础URL
	const baseUrl = uni.getStorageSync("globalurl");

	// 合并基础配置和请求配置
	const config = {
		url: `${baseUrl}${options.url}`,
		method: options.method || 'GET',
		data: options.data || {},
		header: {
			'Content-Type': 'application/json',
			'Access-Control-Allow-Origin': '*',
			'Access-Control-Allow-Methods': 'GET, POST, PUT, DELETE, OPTIONS',
			...options.header
		}
	};

	// 应用请求拦截器
	const interceptedConfig = requestInterceptor(config);

	// 发起请求
	return uni.request(interceptedConfig)
		.then(responseInterceptor)
		.catch(errorHandler);
};

// 简化常用请求方法
const api = {
	get(url, data = {}, header = {}) {
		return request({
			url,
			method: 'GET',
			data,
			header
		});
	},

	post(url, data = {}, header = {}) {
		return request({
			url,
			method: 'POST',
			data,
			header
		});
	},

	put(url, data = {}, header = {}) {
		return request({
			url,
			method: 'PUT',
			data,
			header
		});
	},

	delete(url, data = {}, header = {}) {
		return request({
			url,
			method: 'DELETE',
			data,
			header
		});
	}
};

export default api;