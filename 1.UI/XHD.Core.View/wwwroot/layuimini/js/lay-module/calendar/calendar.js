// calendar.js - Layui日历组件（按需加载数据版本，周一为一周起始）
layui.define(['layer', 'laydate'], function (exports) {
    "use strict";

    var $ = layui.$;
    var layer = layui.layer;
    var laydate = layui.laydate;

    // 日历组件构造函数
    var Calendar = function (options) {
        this.options = $.extend({}, Calendar.DEFAULTS, options);
        this.elem = $(this.options.elem);
        if (!this.elem.length) {
            console.error('Calendar: elem is required');
            return;
        }
        this.currentDate = this.options.currentDate ? new Date(this.options.currentDate) : new Date();
        this.currentView = this.options.view || 'month';
        this.tasks = [];           // 任务数据，由外部通过setTasks注入
        this.eventHandlers = {};
        this.timeLineTimer = null;
        this.loading = false;      // 加载状态
        this.init();
    };

    // 默认配置
    Calendar.DEFAULTS = {
        elem: null,
        view: 'month',             // month, week, day
        currentDate: null,
        showTimeLine: true,        // 是否显示当前时间线
        autoUpdateTimeLine: true,  // 是否自动更新时间线
        onRangeChange: null,       // 视图范围变化时的回调，用于加载数据
        onTaskClick: null,
        onDayClick: null,
        onViewChange: null,
        onTaskAdded: null,
        onTaskUpdated: null,
        onTaskDeleted: null
    };

    Calendar.prototype = {
        // 辅助方法：判断是否为全天任务（兼容 true/false 和 1/0）
        isAllDay: function (task) {
            return task.allDay === true || task.allDay === 1;
        },

        // 初始化
        init: function () {
            this.renderContainer();
            this.bindEvents();
            this.notifyRangeChange();
        },

        // 渲染容器结构
        renderContainer: function () {
            var html = `
                <div class="calendar-container">
                    <div class="calendar-header">
                        <div class="calendar-title" id="currentDate"></div>
                        <div class="calendar-actions">
                            <div class="calendar-nav-buttons">
                                <button class="layui-btn layui-btn-primary" id="prevBtn"><i class="layui-icon">&#xe65a;</i></button>
                                <button class="layui-btn" id="todayBtn">今天</button>
                                <button class="layui-btn layui-btn-primary" id="nextBtn"><i class="layui-icon">&#xe65b;</i></button>
                            </div>
                            <div class="calendar-view-buttons">
                                <button class="layui-btn layui-btn-primary" data-view="month">月视图</button>
                                <button class="layui-btn layui-btn-primary" data-view="week">周视图</button>
                                <button class="layui-btn layui-btn-primary" data-view="day">日视图</button>
                            </div>
                        </div>
                    </div>
                    <!-- 月视图 -->
                    <div class="view-container active" id="monthView">
                        <div class="month-view">
                            <div class="month-weekdays">
                                <div class="month-weekday">周一</div><div class="month-weekday">周二</div><div class="month-weekday">周三</div>
                                <div class="month-weekday">周四</div><div class="month-weekday">周五</div><div class="month-weekday">周六</div>
                                <div class="month-weekday">周日</div>
                            </div>
                            <div class="month-days" id="monthDays"></div>
                        </div>
                    </div>
                    <!-- 周视图 -->
                    <div class="view-container" id="weekView">
                        <div class="week-view">
                            <div class="week-header">
                                <div class="week-time-label">时间</div>
                                <div class="week-day-header" id="weekDay0"></div><div class="week-day-header" id="weekDay1"></div>
                                <div class="week-day-header" id="weekDay2"></div><div class="week-day-header" id="weekDay3"></div>
                                <div class="week-day-header" id="weekDay4"></div><div class="week-day-header" id="weekDay5"></div>
                                <div class="week-day-header" id="weekDay6"></div>
                            </div>
                            <div class="week-content">
                                <div class="week-all-day-label">全天任务</div>
                                <div class="week-all-day-content" id="weekAllDay0"></div><div class="week-all-day-content" id="weekAllDay1"></div>
                                <div class="week-all-day-content" id="weekAllDay2"></div><div class="week-all-day-content" id="weekAllDay3"></div>
                                <div class="week-all-day-content" id="weekAllDay4"></div><div class="week-all-day-content" id="weekAllDay5"></div>
                                <div class="week-all-day-content" id="weekAllDay6"></div>
                                <div class="week-time-slots" id="weekTimeSlots"></div>
                                <div class="week-day-column" id="weekDayColumn0"></div><div class="week-day-column" id="weekDayColumn1"></div>
                                <div class="week-day-column" id="weekDayColumn2"></div><div class="week-day-column" id="weekDayColumn3"></div>
                                <div class="week-day-column" id="weekDayColumn4"></div><div class="week-day-column" id="weekDayColumn5"></div>
                                <div class="week-day-column" id="weekDayColumn6"></div>
                            </div>
                        </div>
                    </div>
                    <!-- 日视图 -->
                    <div class="view-container" id="dayView">
                        <div class="day-view">
                            <div class="day-header">
                                <div class="week-time-label">时间</div>
                                <div class="week-day-header" id="dayTitle"></div>
                            </div>
                            <div class="day-content">
                                <div class="day-all-day-label">全天任务</div>
                                <div class="day-all-day-content" id="dayAllDayTasks"></div>
                                <div class="day-time-slots" id="dayTimeSlots"></div>
                                <div class="day-slot-column" id="daySlotColumn"></div>
                            </div>
                        </div>
                    </div>
                    <div class="calendar-loading" style="display:none;">加载中...</div>
                </div>
            `;
            this.elem.html(html);
        },

        // 绑定UI事件
        bindEvents: function () {
            var self = this;
            $('.calendar-view-buttons button', this.elem).on('click', function () {
                var view = $(this).data('view');
                if (view === self.currentView) return;
                self.currentView = view;
                $('.calendar-view-buttons button', self.elem).removeClass('layui-btn-normal').addClass('layui-btn-primary');
                $(this).removeClass('layui-btn-primary').addClass('layui-btn-normal');
                if (self.options.onViewChange) self.options.onViewChange(self.currentView);
                self.triggerEvent('viewChange', self.currentView);
                self.notifyRangeChange();
            });
            $('#prevBtn', this.elem).on('click', function () { self.navigate(-1); });
            $('#nextBtn', this.elem).on('click', function () { self.navigate(1); });
            $('#todayBtn', this.elem).on('click', function () {
                self.currentDate = new Date();
                self.notifyRangeChange();
            });
            $('#addTaskBtn', this.elem).on('click', function () {
                if (self.options.onDayClick) self.options.onDayClick(self.currentDate);
                self.triggerEvent('dayClick', self.currentDate);
            });
        },

        // 导航到上/下一周期
        navigate: function (direction) {
            switch (this.currentView) {
                case 'month': this.currentDate.setMonth(this.currentDate.getMonth() + direction); break;
                case 'week': this.currentDate.setDate(this.currentDate.getDate() + direction * 7); break;
                case 'day': this.currentDate.setDate(this.currentDate.getDate() + direction); break;
            }
            this.notifyRangeChange();
        },

        // 通知外部数据范围变化
        notifyRangeChange: function () {
            if (this.loading) return;
            var range = this.getCurrentRange();
            if (!range) return;
            this.showLoading(true);
            if (typeof this.options.onRangeChange === 'function') {
                this.options.onRangeChange(range, this);
            } else {
                this.setTasks([]);
                this.showLoading(false);
            }
        },

        // 获取当前视图的时间范围
        getCurrentRange: function () {
            var view = this.currentView;
            var start, end;
            if (view === 'month') {
                var year = this.currentDate.getFullYear();
                var month = this.currentDate.getMonth();
                start = new Date(year, month, 1);
                end = new Date(year, month + 1, 0);
                end.setHours(23, 59, 59, 999);
            } else if (view === 'week') {
                var weekStart = this.getWeekStartDate(this.currentDate);
                start = new Date(weekStart);
                start.setHours(0, 0, 0, 0);
                end = new Date(weekStart);
                end.setDate(end.getDate() + 6);
                end.setHours(23, 59, 59, 999);
            } else if (view === 'day') {
                start = new Date(this.currentDate);
                start.setHours(0, 0, 0, 0);
                end = new Date(this.currentDate);
                end.setHours(23, 59, 59, 999);
            } else {
                return null;
            }
            return {
                view: view,
                startDate: this.formatDate(start),
                endDate: this.formatDate(end),
                start: start,
                end: end
            };
        },

        // 设置任务数据
        setTasks: function (tasks) {
            this.tasks = tasks || [];
            this.render();
            this.showLoading(false);
            if (this.options.showTimeLine) {
                setTimeout(function () { this.renderCurrentTimeLine(); }.bind(this), 100);
            }
        },

        // 获取当前任务数据
        getTasks: function () {
            return this.tasks.slice();
        },

        // 显示/隐藏加载动画
        showLoading: function (show) {
            var $loading = $('.calendar-loading', this.elem);
            if (show) {
                $loading.css({ display: 'block', position: 'absolute', top: '50%', left: '50%', transform: 'translate(-50%,-50%)', background: 'rgba(0,0,0,0.7)', color: '#fff', padding: '8px 16px', borderRadius: '4px', zIndex: 10000 });
            } else {
                $loading.hide();
            }
        },

        // 渲染整个日历
        render: function () {
            this.updateTitle();
            if (this.timeLineTimer) { clearTimeout(this.timeLineTimer); this.timeLineTimer = null; }
            $('.current-time-line, .current-time-line-label, .current-time-dot', this.elem).remove();

            $('.view-container', this.elem).removeClass('active');
            $('#' + this.currentView + 'View', this.elem).addClass('active');

            switch (this.currentView) {
                case 'month': this.renderMonthView(); break;
                case 'week': this.renderWeekView(); break;
                case 'day': this.renderDayView(); break;
            }
            $('.calendar-view-buttons button[data-view="' + this.currentView + '"]', this.elem)
                .removeClass('layui-btn-primary').addClass('layui-btn-normal');

            if (this.options.showTimeLine) {
                setTimeout(function () { this.renderCurrentTimeLine(); }.bind(this), 100);
            }
        },

        // 更新标题
        updateTitle: function () {
            var title = '', year = this.currentDate.getFullYear(), month = this.currentDate.getMonth() + 1, date = this.currentDate.getDate();
            if (this.currentView === 'month') title = year + '年' + month + '月';
            else if (this.currentView === 'week') {
                var weekStart = this.getWeekStartDate(this.currentDate);
                var weekEnd = new Date(weekStart);
                weekEnd.setDate(weekEnd.getDate() + 6);
                var startMonth = weekStart.getMonth() + 1;
                var endMonth = weekEnd.getMonth() + 1;
                if (weekStart.getFullYear() === weekEnd.getFullYear() && startMonth === endMonth)
                    title = weekStart.getFullYear() + '年' + startMonth + '月' + weekStart.getDate() + '日 - ' + weekEnd.getDate() + '日';
                else if (weekStart.getFullYear() === weekEnd.getFullYear())
                    title = weekStart.getFullYear() + '年' + startMonth + '月' + weekStart.getDate() + '日 - ' + endMonth + '月' + weekEnd.getDate() + '日';
                else
                    title = weekStart.getFullYear() + '年' + startMonth + '月' + weekStart.getDate() + '日 - ' + weekEnd.getFullYear() + '年' + endMonth + '月' + weekEnd.getDate() + '日';
            } else {
                var dayNames = ['周一', '周二', '周三', '周四', '周五', '周六', '周日'];
                title = year + '年' + month + '月' + date + '日 ' + dayNames[this.currentDate.getDay() === 0 ? 6 : this.currentDate.getDay() - 1];
            }
            $('#currentDate', this.elem).text(title);
        },

        // 月视图渲染
        renderMonthView: function () {
            var year = this.currentDate.getFullYear(), month = this.currentDate.getMonth();
            var today = new Date(); today.setHours(0, 0, 0, 0);
            var firstDay = new Date(year, month, 1);
            // 获取月份第一天是周几（周一=0,周日=6）
            var firstDayWeek = (firstDay.getDay() + 6) % 7;
            var lastDay = new Date(year, month + 1, 0);
            var daysInMonth = lastDay.getDate();
            $('#monthDays', this.elem).empty();
            // 上个月填充
            var prevMonthLastDay = new Date(year, month, 0).getDate();
            for (var i = firstDayWeek - 1; i >= 0; i--) {
                this.addMonthDay(new Date(year, month - 1, prevMonthLastDay - i), true);
            }
            // 本月
            for (var i = 1; i <= daysInMonth; i++) {
                var date = new Date(year, month, i);
                var isToday = date.getDate() === today.getDate() && date.getMonth() === today.getMonth() && date.getFullYear() === today.getFullYear();
                this.addMonthDay(date, false, isToday);
            }
            // 下个月填充至42格
            var totalCells = 42;
            var daysAdded = firstDayWeek + daysInMonth;
            for (var i = 1; i <= totalCells - daysAdded; i++) {
                this.addMonthDay(new Date(year, month + 1, i), true);
            }
        },
        addMonthDay: function (date, isOtherMonth, isToday) {
            var self = this;
            var dayElement = $('<div class="month-day"></div>');
            if (isOtherMonth) dayElement.addClass('other-month');
            if (isToday) dayElement.addClass('today');
            dayElement.append('<div class="month-day-header"><span class="month-day-number">' + date.getDate() + '</span></div>');
            var tasks = this.getTasksForDate(date);
            tasks.forEach(function (task) {
                var taskEl = self.createTaskElement(task);
                taskEl.on('click', function (e) { e.stopPropagation(); self.triggerEvent('taskClick', task); if (self.options.onTaskClick) self.options.onTaskClick(task); });
                dayElement.append(taskEl);
            });
            dayElement.on('click', function (e) {
                if (!$(e.target).closest('.calendar-task').length) {
                    var clickDate = new Date(date); clickDate.setHours(9, 0, 0);
                    self.triggerEvent('dayClick', clickDate);
                    if (self.options.onDayClick) self.options.onDayClick(clickDate);
                }
            });
            $('#monthDays', this.elem).append(dayElement);
        },

        // 获取周起始日期（周一）
        getWeekStartDate: function (date) {
            var d = new Date(date);
            var day = d.getDay(); // 0周日,1周一...6周六
            // 周一偏移量：周一为1，周日为0，需要将周日视为上一周结束
            var diff = (day === 0 ? 6 : day - 1);
            d.setDate(d.getDate() - diff);
            return d;
        },

        // 周视图渲染
        renderWeekView: function () {
            var self = this;
            var weekStart = this.getWeekStartDate(this.currentDate);
            var weekDates = [];
            for (var i = 0; i < 7; i++) { var d = new Date(weekStart); d.setDate(d.getDate() + i); weekDates.push(d); }
            var weekDayNames = ['周一', '周二', '周三', '周四', '周五', '周六', '周日'];
            weekDates.forEach(function (date, idx) {
                $('#weekDay' + idx, self.elem).html(weekDayNames[idx] + '<br>' + (date.getMonth() + 1) + '/' + date.getDate());
            });
            for (var i = 0; i < 7; i++) { $('#weekAllDay' + i, self.elem).empty(); }
            weekDates.forEach(function (date, idx) {
                var tasks = self.getTasksForDate(date).filter(function (t) {
                    return self.isAllDay(t) || t.startDate !== t.endDate;
                });
                tasks.forEach(function (task) {
                    var taskEl = self.createTaskElement(task).addClass('all-day');
                    taskEl.on('click', function (e) { e.stopPropagation(); self.triggerEvent('taskClick', task); if (self.options.onTaskClick) self.options.onTaskClick(task); });
                    $('#weekAllDay' + idx, self.elem).append(taskEl);
                });
            });
            $('#weekTimeSlots', this.elem).empty();
            for (var h = 0; h < 24; h++) { $('#weekTimeSlots', this.elem).append('<div class="week-time-slot">' + (h < 10 ? '0' + h : h) + ':00</div>'); }
            for (var i = 0; i < 7; i++) {
                $('#weekDayColumn' + i, this.elem).empty();
                for (var h = 0; h < 24; h++) {
                    var slot = $('<div class="week-day-slot" data-hour="' + h + '" data-date="' + this.formatDate(weekDates[i]) + '"></div>');
                    slot.on('click', (function (date, hour) {
                        return function (e) {
                            if (!$(e.target).closest('.time-slot-task').length) {
                                var clickDate = new Date(date); clickDate.setHours(hour, 30, 0);
                                self.triggerEvent('dayClick', clickDate);
                                if (self.options.onDayClick) self.options.onDayClick(clickDate);
                            }
                        };
                    })(weekDates[i], h));
                    $('#weekDayColumn' + i, this.elem).append(slot);
                }
            }
            this.renderWeekTasks(weekDates);
        },
        renderWeekTasks: function (weekDates) {
            var self = this;
            $('.time-slot-task, .week-multi-day-task', this.elem).remove();
            var weekStart = new Date(weekDates[0]); weekStart.setHours(0, 0, 0, 0);
            var weekEnd = new Date(weekDates[6]); weekEnd.setHours(23, 59, 59, 999);
            var weekTasks = this.tasks.filter(function (task) {
                if (self.isAllDay(task)) return false;
                if (task.startDate !== task.endDate) return false;
                var taskStart = new Date(task.startDate + ' ' + task.startTime);
                var taskEnd = new Date(task.endDate + ' ' + task.endTime);
                return (taskStart <= weekEnd && taskEnd >= weekStart);
            });
            var tasksByDate = {};
            weekDates.forEach(function (d) { tasksByDate[self.formatDate(d)] = []; });
            weekTasks.forEach(function (task) { if (tasksByDate[task.startDate]) tasksByDate[task.startDate].push(task); });
            Object.keys(tasksByDate).forEach(function (dateStr) {
                var tasks = tasksByDate[dateStr];
                if (!tasks.length) return;
                var grouped = self.groupOverlappingTasks(tasks);
                var dayIndex = -1;
                for (var i = 0; i < weekDates.length; i++) { if (self.formatDate(weekDates[i]) === dateStr) { dayIndex = i; break; } }
                if (dayIndex === -1) return;
                grouped.forEach(function (task) {
                    var taskStart = new Date(task.startDate + ' ' + task.startTime);
                    var taskEnd = new Date(task.endDate + ' ' + task.endTime);
                    var pos = self.calculateTaskPosition(taskStart, taskEnd, task.columnIndex || 0, task.totalColumns || 1);
                    var taskEl = $('<div class="time-slot-task task-color-' + task.color + '"></div>');
                    taskEl.css({ top: pos.top + 'px', height: pos.height + 'px', left: pos.left, width: pos.width });
                    taskEl.append('<div class="task-title">' + task.title + '</div><div class="task-time">' + self.formatTime(taskStart) + ' - ' + self.formatTime(taskEnd) + '</div>');
                    taskEl.on('click', function (e) { e.stopPropagation(); self.triggerEvent('taskClick', task); if (self.options.onTaskClick) self.options.onTaskClick(task); });
                    $('#weekDayColumn' + dayIndex, self.elem).append(taskEl);
                });
            });
        },

        // 日视图渲染
        renderDayView: function () {
            var self = this;
            var date = this.currentDate;
            var dayNames = ['周一', '周二', '周三', '周四', '周五', '周六', '周日'];
            var weekDayStr = dayNames[date.getDay() === 0 ? 6 : date.getDay() - 1];
            $('#dayTitle', this.elem).html(date.getFullYear() + '年' + (date.getMonth() + 1) + '月' + date.getDate() + '日 ' + weekDayStr);
            $('#dayAllDayTasks', this.elem).empty();
            var allDayTasks = this.getTasksForDate(date).filter(function (t) {
                return self.isAllDay(t) || t.startDate !== t.endDate;
            });
            allDayTasks.forEach(function (task) {
                var taskEl = self.createTaskElement(task).addClass('all-day');
                taskEl.on('click', function (e) { e.stopPropagation(); self.triggerEvent('taskClick', task); if (self.options.onTaskClick) self.options.onTaskClick(task); });
                $('#dayAllDayTasks', self.elem).append(taskEl);
            });
            $('#dayTimeSlots', this.elem).empty();
            for (var h = 0; h < 24; h++) { $('#dayTimeSlots', this.elem).append('<div class="day-time-slot">' + (h < 10 ? '0' + h : h) + ':00</div>'); }
            $('#daySlotColumn', this.elem).empty();
            for (var h = 0; h < 24; h++) {
                var slot = $('<div class="day-slot" data-hour="' + h + '"></div>');
                slot.on('click', (function (hour) {
                    return function (e) {
                        if (!$(e.target).closest('.time-slot-task').length) {
                            var clickDate = new Date(date); clickDate.setHours(hour, 30, 0);
                            self.triggerEvent('dayClick', clickDate);
                            if (self.options.onDayClick) self.options.onDayClick(clickDate);
                        }
                    };
                })(h));
                $('#daySlotColumn', this.elem).append(slot);
            }
            this.renderDayTasks(date);
        },
        renderDayTasks: function (date) {
            var self = this;
            var dateStr = this.formatDate(date);
            $('.time-slot-task', this.elem).remove();
            var dayTasks = this.tasks.filter(function (task) {
                if (self.isAllDay(task)) return false;
                if (task.startDate !== task.endDate) return false;
                return task.startDate <= dateStr && task.endDate >= dateStr;
            });
            if (!dayTasks.length) return;
            var grouped = this.groupOverlappingTasks(dayTasks);
            grouped.forEach(function (task) {
                var taskStart = new Date(task.startDate + ' ' + task.startTime);
                var taskEnd = new Date(task.endDate + ' ' + task.endTime);
                var dayStart = new Date(date); dayStart.setHours(0, 0, 0, 0);
                var dayEnd = new Date(date); dayEnd.setHours(23, 59, 59, 999);
                var displayStart = taskStart < dayStart ? dayStart : taskStart;
                var displayEnd = taskEnd > dayEnd ? dayEnd : taskEnd;
                var pos = self.calculateTaskPosition(displayStart, displayEnd, task.columnIndex || 0, task.totalColumns || 1);
                var taskEl = $('<div class="time-slot-task task-color-' + task.color + '"></div>');
                taskEl.css({ top: pos.top + 'px', height: pos.height + 'px', left: pos.left, width: pos.width });
                taskEl.append('<div class="task-title">' + task.title + '</div><div class="task-time">' + self.formatTime(displayStart) + ' - ' + self.formatTime(displayEnd) + '</div>');
                taskEl.on('click', function (e) { e.stopPropagation(); self.triggerEvent('taskClick', task); if (self.options.onTaskClick) self.options.onTaskClick(task); });
                $('#daySlotColumn', self.elem).append(taskEl);
            });
        },

        // 辅助方法
        createTaskElement: function (task) {
            var el = $('<div class="calendar-task task-color-' + task.color + '"></div>');
            el.append('<div class="task-title">' + task.title + '</div>');
            if (!this.isAllDay(task) && task.startDate === task.endDate && task.startTime && task.endTime) {
                el.append('<div class="task-time">' + task.startTime + ' - ' + task.endTime + '</div>');
            }
            return el;
        },
        getTasksForDate: function (date) {
            var self = this;
            var dateStr = this.formatDate(date);
            return this.tasks.filter(function (task) {
                if (self.isAllDay(task) || task.startDate !== task.endDate) {
                    var dateObj = new Date(dateStr + 'T00:00:00');
                    var taskStart = new Date(task.startDate + 'T00:00:00');
                    var taskEnd = new Date(task.endDate + 'T23:59:59');
                    return dateObj >= taskStart && dateObj <= taskEnd;
                }
                return dateStr === task.startDate;
            });
        },
        formatDate: function (date) {
            var y = date.getFullYear(), m = ('0' + (date.getMonth() + 1)).slice(-2), d = ('0' + date.getDate()).slice(-2);
            return y + '-' + m + '-' + d;
        },
        formatTime: function (date) {
            return ('0' + date.getHours()).slice(-2) + ':' + ('0' + date.getMinutes()).slice(-2);
        },
        groupOverlappingTasks: function (tasks) {
            if (!tasks.length) return [];
            tasks.sort(function (a, b) { return new Date(a.startDate + ' ' + a.startTime) - new Date(b.startDate + ' ' + b.startTime); });
            var columns = [], colMap = {};
            tasks.forEach(function (task) {
                var start = new Date(task.startDate + ' ' + task.startTime), end = new Date(task.endDate + ' ' + task.endTime);
                var placed = false;
                for (var i = 0; i < columns.length; i++) {
                    if (start >= columns[i]) { columns[i] = end; colMap[task.id] = i; placed = true; break; }
                }
                if (!placed) { columns.push(end); colMap[task.id] = columns.length - 1; }
            });
            tasks.forEach(function (t) { t.columnIndex = colMap[t.id] || 0; t.totalColumns = columns.length; });
            return tasks;
        },
        calculateTaskPosition: function (start, end, colIdx, totalCols) {
            var slotH = 60, totalH = slotH * 24;
            var startH = start.getHours() + start.getMinutes() / 60;
            var endH = end.getHours() + end.getMinutes() / 60;
            var top = (startH / 24) * totalH + startH;
            var height = ((endH - startH) / 24) * totalH;
            if (height < 20) height = 20;
            var colW = 100 / totalCols;
            var left = colIdx * colW;
            var width = colW;
            left += 1; width -= 2;
            return { top: top, height: height, left: left + '%', width: width + '%' };
        },

        // 时间线
        renderCurrentTimeLine: function () {
            if (!this.options.showTimeLine) return;
            var self = this, now = new Date(), today = new Date(); today.setHours(0, 0, 0, 0);
            var should = false, container = null;
            if (this.currentView === 'week' && this.isDateInCurrentWeek(today)) { should = true; container = $('.week-content', this.elem); }
            else if (this.currentView === 'day' && this.isSameDate(this.currentDate, today)) { should = true; container = $('.day-content', this.elem); }
            if (!should) return;
            var timePos = (now.getHours() * 60 + now.getMinutes()) * 1 + 71;
            var timeColW = 80;
            var line = $('<div class="current-time-line"></div>').css({ position: 'absolute', top: timePos + 'px', left: timeColW + 'px', right: 0, height: '2px', backgroundColor: '#FF5722', zIndex: 1000, pointerEvents: 'none' });
            var dot = $('<div class="current-time-dot"></div>').css({ position: 'absolute', top: timePos - 4 + 'px', left: timeColW - 5 + 'px', width: '10px', height: '10px', backgroundColor: '#FF5722', borderRadius: '50%', zIndex: 1001, pointerEvents: 'none' });
            var label = $('<div class="current-time-line-label"></div>').text(this.formatTime(now)).css({ position: 'absolute', top: timePos - 10 + 'px', left: timeColW + 5 + 'px', backgroundColor: '#FF5722', color: '#fff', padding: '2px 6px', borderRadius: '3px', fontSize: '11px', whiteSpace: 'nowrap', zIndex: 1002 });
            container.append(line, dot, label);
            if (this.options.autoUpdateTimeLine) {
                if (this.timeLineTimer) clearTimeout(this.timeLineTimer);
                this.timeLineTimer = setTimeout(function () { self.renderCurrentTimeLine(); }, 60000);
            }
        },
        isDateInCurrentWeek: function (date) {
            var ws = this.getWeekStartDate(this.currentDate);
            var we = new Date(ws);
            we.setDate(we.getDate() + 6);
            we.setHours(23, 59, 59, 999);
            return date >= ws && date <= we;
        },
        isSameDate: function (d1, d2) { return d1.getFullYear() === d2.getFullYear() && d1.getMonth() === d2.getMonth() && d1.getDate() === d2.getDate(); },

        // 任务增删改方法
        addTask: function (task) {
            if (!task.id) task.id = 'task_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
            this.tasks.push(task);
            this.render();
            this.triggerEvent('taskAdded', task);
            if (this.options.onTaskAdded) this.options.onTaskAdded(task);
            return task.id;
        },
        updateTask: function (task) {
            var idx = this.tasks.findIndex(function (t) { return t.id === task.id; });
            if (idx !== -1) { this.tasks[idx] = task; this.render(); this.triggerEvent('taskUpdated', task); if (this.options.onTaskUpdated) this.options.onTaskUpdated(task); return true; }
            return false;
        },
        deleteTask: function (id) {
            var len = this.tasks.length;
            this.tasks = this.tasks.filter(function (t) { return t.id !== id; });
            if (this.tasks.length < len) { this.render(); this.triggerEvent('taskDeleted', id); if (this.options.onTaskDeleted) this.options.onTaskDeleted(id); return true; }
            return false;
        },
        setView: function (view) {
            if (['month', 'week', 'day'].indexOf(view) !== -1) { this.currentView = view; this.notifyRangeChange(); return true; }
            return false;
        },
        setCurrentDate: function (date) {
            if (date instanceof Date) { this.currentDate = date; this.notifyRangeChange(); return true; }
            var d = new Date(date); if (!isNaN(d.getTime())) { this.currentDate = d; this.notifyRangeChange(); return true; }
            return false;
        },
        goToToday: function () { this.currentDate = new Date(); this.notifyRangeChange(); },

        // 事件系统
        triggerEvent: function (name, data) { if (this.eventHandlers[name]) this.eventHandlers[name].forEach(function (h) { h(data); }); },
        on: function (name, handler) { if (!this.eventHandlers[name]) this.eventHandlers[name] = []; this.eventHandlers[name].push(handler); },
        off: function (name, handler) { if (this.eventHandlers[name] && handler) { var idx = this.eventHandlers[name].indexOf(handler); if (idx !== -1) this.eventHandlers[name].splice(idx, 1); } else if (this.eventHandlers[name]) this.eventHandlers[name] = []; },
        destroy: function () {
            if (this.timeLineTimer) clearTimeout(this.timeLineTimer);
            this.elem.empty();
            this.triggerEvent('destroy', this);
        }
    };

    // 导出组件
    var calendar = {
        render: function (options) { return new Calendar(options); },
        formatDate: function (date) { var y = date.getFullYear(), m = ('0' + (date.getMonth() + 1)).slice(-2), d = ('0' + date.getDate()).slice(-2); return y + '-' + m + '-' + d; },
        formatTime: function (date) { return ('0' + date.getHours()).slice(-2) + ':' + ('0' + date.getMinutes()).slice(-2); }
    };
    layui.link(layui.cache.base + 'calendar/calendar.css');
    exports('calendar', calendar);
});