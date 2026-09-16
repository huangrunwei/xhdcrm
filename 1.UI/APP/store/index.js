// store/index.js
import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

const store = new Vuex.Store({
    state: {
        // 从本地存储获取登录状态，实现持久化
        isLogin: uni.getStorageSync('isLogin') || false,
        userInfo: uni.getStorageSync('userInfo') || {}
    },
    mutations: {
        // 登录成功
        loginSuccess(state, userInfo) {
            state.isLogin = true
            state.userInfo = userInfo
            // 同步到本地存储
            uni.setStorageSync('isLogin', true)
            uni.setStorageSync('userInfo', userInfo)
        },
        // 退出登录
        logout(state) {
            state.isLogin = false
            state.userInfo = {}
            // 清除本地存储
            uni.removeStorageSync('isLogin')
            uni.removeStorageSync('userInfo')
        }
    },
    actions: {
        // 可以在这里定义异步操作
    }
})

export default store
    