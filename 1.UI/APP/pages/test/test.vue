<template>
	<view class="password-strength-container">
		<!-- 密码输入框 -->
		<view class="input-group">
			<input 
				type="password" 
				v-model="password" 
				placeholder="请输入密码" 
				@input="checkPasswordStrength"
				class="password-input"
			/>
		</view>
		
		<!-- 密码强度指示器 -->
		<view class="strength-indicator" >
			<!-- 强度条 -->
			<view class="strength-bars">
				<view 
					class="strength-bar" 
					:class="{'weak': strength >= 1, 'medium': strength >= 2, 'strong': strength >= 3}"
				></view>
				<view 
					class="strength-bar" 
					:class="{'medium': strength >= 2, 'strong': strength >= 3}"
				></view>
				<view 
					class="strength-bar" 
					:class="{'strong': strength >= 3}"
				></view>
			</view>
			
			<!-- 强度文本和提示 -->
			<view class="strength-info">
				<text class="strength-text" :class="strengthClass">
					{{ strengthText }}
				</text>
				<text class="requirements-text">{{ requirementsText }}</text>
			</view>
		</view>
		
		<!-- 密码要求说明 -->
		<view class="requirements-list" v-if="password.length > 0">
			<view class="requirement-item" :class="{ met: hasMinLength }">
				<text class="icon">{{ hasMinLength ? '⚪' : '×' }}</text>
				<text>至少6个字符</text>
			</view>
			<view class="requirement-item" :class="{ met: hasLetters }">
				<text class="icon">{{ hasLetters ? '⚪' : '×' }}</text>
				<text>包含字母（a-z, A-Z）</text>
			</view>
			<view class="requirement-item" :class="{ met: hasNumbers }">
				<text class="icon">{{ hasNumbers ? '⚪' : '×' }}</text>
				<text>包含数字（0-9）</text>
			</view>
			<view class="requirement-item" :class="{ met: hasSymbols }">
				<text class="icon">{{ hasSymbols ? '⚪' : '×' }}</text>
				<text>包含符号（如 !@#$% 等）</text>
			</view>
		</view>
	</view>
</template>

<script>
export default {
	data() {
		return {
			password: '',
			strength: 0,        // 0-3，0表示未输入，1弱，2中，3强
			hasMinLength: false, // 是否满足最小长度
			hasLetters: false,   // 是否包含字母
			hasNumbers: false,   // 是否包含数字
			hasSymbols: false    // 是否包含符号
		};
	},
	computed: {
		// 强度文本
		strengthText() {
			switch(this.strength) {
				case 0: return '';
				case 1: return '弱';
				case 2: return '中';
				case 3: return '强';
				default: return '';
			}
		},
		// 强度文本样式
		strengthClass() {
			switch(this.strength) {
				case 1: return 'weak-text';
				case 2: return 'medium-text';
				case 3: return 'strong-text';
				default: return '';
			}
		},
		// 提示文本
		requirementsText() {
			if (this.strength === 3) {
				return '密码强度良好';
			} else if (this.password.length > 0) {
				return '请完善密码以提高安全性';
			}
			return '';
		}
	},
	methods: {
		// 检测密码强度
		checkPasswordStrength() {
			// 重置状态
			this.hasMinLength = false;
			this.hasLetters = false;
			this.hasNumbers = false;
			this.hasSymbols = false;
			
			// 检测长度
			if (this.password.length >= 6) {
				this.hasMinLength = true;
			}
			
			// 检测字母
			if (/[a-zA-Z]/.test(this.password)) {
				this.hasLetters = true;
			}
			
			// 检测数字
			if (/[0-9]/.test(this.password)) {
				this.hasNumbers = true;
			}
			
			// 检测符号（非字母数字）
			if (/[^a-zA-Z0-9]/.test(this.password)) {
				this.hasSymbols = true;
			}
			
			// 计算强度（满足的条件数量）
			let count = 0;
			if (this.hasMinLength) count++;
			if (this.hasLetters) count++;
			if (this.hasNumbers) count++;
			if (this.hasSymbols) count++;
			
			// 调整强度值（0-3）
			this.strength = count;
		}
	}
};
</script>

<style scoped>
.password-strength-container {
	padding: 20rpx;
}

.input-group {
	margin-bottom: 30rpx;
}

.password-input {
	width: 100%;
	height: 80rpx;
	line-height: 80rpx;
	padding: 0 20rpx;
	border: 2rpx solid #eee;
	border-radius: 10rpx;
	font-size: 32rpx;
}

.strength-indicator {
	margin-bottom: 20rpx;
}

.strength-bars {
	display: flex;
	gap: 10rpx;
	height: 12rpx;
	margin-bottom: 10rpx;
}

.strength-bar {
	flex: 1;
	background-color: #eee;
	border-radius: 6rpx;
	transition: background-color 0.3s ease;
}

.strength-bar.weak {
	background-color: #ff4d4f;
}

.strength-bar.medium {
	background-color: #faad14;
}

.strength-bar.strong {
	background-color: #52c41a;
}

.strength-info {
	display: flex;
	justify-content: space-between;
	font-size: 28rpx;
}

.strength-text {
	font-weight: bold;
}

.weak-text {
	color: #ff4d4f;
}

.medium-text {
	color: #faad14;
}

.strong-text {
	color: #52c41a;
}

.requirements-text {
	color: #888;
}

.requirements-list {
	background-color: #f9f9f9;
	border-radius: 10rpx;
	padding: 20rpx;
}

.requirement-item {
	display: flex;
	align-items: center;
	margin-bottom: 15rpx;
	font-size: 28rpx;
	color: #666;
}

.requirement-item:last-child {
	margin-bottom: 0;
}

.requirement-item.met {
	color: #52c41a;
}

.icon {
	margin-right: 15rpx;
	font-size: 30rpx;
}
</style>
