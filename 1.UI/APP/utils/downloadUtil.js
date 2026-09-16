/**
 * 文件下载工具类
 */
export default class DownloadUtil {
	/**
	 * 下载文件并保存到本地
	 * @param {string} url - 文件下载地址
	 * @param {string} fileName - 保存的文件名（可选）
	 * @returns {Promise} - 返回Promise对象，成功时返回文件保存路径
	 */
	static downloadAndSaveFile(url, fileName = '') {
		return new Promise((resolve, reject) => {
			// 显示加载中提示
			uni.showLoading({
				title: '下载中...',
				mask: true
			})

			// 调用下载API
			uni.downloadFile({
				url: url,
				timeout: 60000, // 超时时间60秒
				success: (downloadResult) => {
					// 关闭加载提示
					uni.hideLoading()

					// 下载成功
					if (downloadResult.statusCode === 200) {
						// 如果未指定文件名，从URL中提取
						if (!fileName) {
							const urlArr = url.split('/')
							fileName = urlArr[urlArr.length - 1]
						}

						// 保存文件到本地
						uni.saveFile({
							tempFilePath: downloadResult.tempFilePath,
							filePath: `${wx.env.USER_DATA_PATH}/${fileName}`, // 微信小程序保存路径
							success: (saveResult) => {
								uni.showToast({
									title: '文件保存成功',
									icon: 'success',
									duration: 2000
								})
								resolve(saveResult.savedFilePath)
							},
							fail: (err) => {
								console.error('保存文件失败', err)
								uni.showToast({
									title: '保存文件失败',
									icon: 'none',
									duration: 2000
								})
								reject(err)
							}
						})
					} else {
						// 下载失败（非200状态码）
						console.error('文件下载失败，状态码：', downloadResult.statusCode)
						uni.showToast({
							title: '文件下载失败',
							icon: 'none',
							duration: 2000
						})
						reject(new Error(`下载失败，状态码: ${downloadResult.statusCode}`))
					}
				},
				fail: (err) => {
					// 关闭加载提示
					uni.hideLoading()

					console.error('下载请求失败', err)
					uni.showToast({
						title: '下载失败，请检查网络',
						icon: 'none',
						duration: 2000
					})
					reject(err)
				}
			})
		})
	}

	/**
	 * 打开已下载的文件
	 * @param {string} filePath - 文件本地路径
	 */
	static openFile(filePath) {
		uni.openDocument({
			filePath: filePath,
			showMenu: true,
			success: (res) => {
				console.log('文件打开成功')
			},
			fail: (err) => {
				console.error('文件打开失败', err)
				uni.showToast({
					title: '无法打开该文件',
					icon: 'none',
					duration: 2000
				})
			}
		})
	}
}