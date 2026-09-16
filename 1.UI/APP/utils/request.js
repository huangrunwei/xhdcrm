// 引入store实例
import store from '../store/index.js'

// 引入外部JS文件中的变量（保持你原来的 require 方式，不要改）
const envConfig = require('../config/env.js')

const baseUrl = envConfig.baseApi;

const request = (url = '', data = {}, type = 'GET', header1 = {}) => {
  const userInfo = uni.getStorageSync('userInfo');
  if (userInfo && userInfo.token) {
    header1.Authorization = `Bearer ${userInfo.token}`;
  }

  const headerinfo = {
    'Content-Type': 'application/json',
    ...header1
  };

  return new Promise((resolve, reject) => {
    uni.request({
      method: type,
      url: baseUrl + url,
      data: data,
      header: headerinfo,
      dataType: 'json',
	  sslVerify: false,  // 🚨 加上这一行，忽略证书校验
      success: (res) => {
        if (res.statusCode === 200) {
          const code = res.data.code;
          if (code === -9) {
            uni.removeStorageSync('isLogin');
            setTimeout(() => {
              uni.reLaunch({
                url: '../../pages/Login/Login'
              });
            }, 500);
            return reject(new Error('登录已过期！'));
          }
          resolve(res.data);
        } else {
          const errorMsg = res.data?.msg || `请求失败（${res.statusCode}）`;
          uni.showToast({
            title: errorMsg,
            icon: 'none',
            duration: 2000
          });
          reject(new Error(errorMsg));
        }
      },
      fail: (err) => {
        // 重点：这里能拿到最真实的错误原因
        let errMsg = '网络请求失败：' + (err.errMsg || '未知错误');
        uni.showToast({
          title: errMsg,
          icon: 'none',
          duration: 3000
        });
        console.error('请求异常详情:', err);
        reject(err);
      }
    });
  });
};

export default request;