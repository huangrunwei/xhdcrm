// calendar.js - Layui日历组件（独立时间线版本）
layui.define(['layer', 'laydate'], function (exports) {
    "use strict";

    var $ = layui.$;
    var layer = layui.layer;
    var laydate = layui.laydate;

    // 日历组件构造函数
    var Calendar = function (options) {
        this.options = $.extend({}, Calendar.DEFAULTS, options);
        this.elem = $(this.options.elem);
        this.currentDate = this.options.currentDate ? new Date(this.options.currentDate) : new Date();
        this.currentView = this.options.view || 'month';
        this.tasks = this.options.tasks || [];
        this.eventHandlers = {};
        this.timeLineTimer = null;
        this.init();
    };

    // 默认配置
    Calendar.DEFAULTS = {
        elem: null,
        view: 'month',
        currentDate: null,
        tasks: [],
        storageKey: 'layui_calendar_tasks',
        useStorage: true,
        showTimeLine: true,          // 是否显示当前时间线
        autoUpdateTimeLine: true,    // 是否自动更新时间线
        onTaskClick: null,
        onDayClick: null,
        onViewChange: null,
        onTaskAdded: null,
        onTaskUpdated: null,
        onTaskDeleted: null
    };

    // 原型方法
    Calendar.prototype = {
        // 初始化
        init: function () {
            if (!this.elem.length) {
                console.error('Calendar: elem option is required');
                return;
            }

            this.renderContainer();
            this.bindEvents();
            this.initDatePickers();
            this.loadTasksFromStorage();
            this.render();

            // 初始化时间线
            this.renderCurrentTimeLine();

            // 触发初始化完成事件
            this.triggerEvent('init', this);
        },

        // 渲染容器
        renderContainer: function () {
            var html = `
                <div class="calendar-container">
                    <!-- 日历头部 -->
                    <div class="calendar-header">
                        <div class="calendar-title" id="currentDate">2024年1月</div>
                        <div class="calendar-actions">
                            <div class="calendar-nav-buttons">
                                <button class="layui-btn layui-btn-primary" id="prevBtn">
                                    <i class="layui-icon">&#xe65a;</i>
                                </button>
                                <button class="layui-btn" id="todayBtn">今天</button>
                                <button class="layui-btn layui-btn-primary" id="nextBtn">
                                    <i class="layui-icon">&#xe65b;</i>
                                </button>
                            </div>
                            <div class="calendar-view-buttons">
                                <button class="layui-btn layui-btn-primary" data-view="month">月视图</button>
                                <button class="layui-btn layui-btn-primary" data-view="week">周视图</button>
                                <button class="layui-btn layui-btn-primary" data-view="day">日视图</button>
                            </div>
                            <button class="layui-btn layui-btn-normal" id="addTaskBtn">
                                <i class="layui-icon">&#xe654;</i> 添加任务
                            </button>
                        </div>
                    </div>

                    <!-- 月视图 -->
                    <div class="view-container active" id="monthView">
                        <div class="month-view">
                            <div class="month-weekdays">
                                <div class="month-weekday">周日</div>
                                <div class="month-weekday">周一</div>
                                <div class="month-weekday">周二</div>
                                <div class="month-weekday">周三</div>
                                <div class="month-weekday">周四</div>
                                <div class="month-weekday">周五</div>
                                <div class="month-weekday">周六</div>
                            </div>
                            <div class="month-days" id="monthDays"></div>
                        </div>
                    </div>

                    <!-- 周视图 -->
                    <div class="view-container" id="weekView">
                        <div class="week-view">
                            <div class="week-header">
                                <div class="week-time-label">时间</div>
                                <div class="week-day-header" id="weekDay0">周日</div>
                                <div class="week-day-header" id="weekDay1">周一</div>
                                <div class="week-day-header" id="weekDay2">周二</div>
                                <div class="week-day-header" id="weekDay3">周三</div>
                                <div class="week-day-header" id="weekDay4">周四</div>
                                <div class="week-day-header" id="weekDay5">周五</div>
                                <div class="week-day-header" id="weekDay6">周六</div>
                            </div>
                            <div class="week-content">
                                <!-- 全天任务区域 -->
                                <div class="week-all-day-label">全天任务</div>
                                <div class="week-all-day-content" id="weekAllDay0"></div>
                                <div class="week-all-day-content" id="weekAllDay1"></div>
                                <div class="week-all-day-content" id="weekAllDay2"></div>
                                <div class="week-all-day-content" id="weekAllDay3"></div>
                                <div class="week-all-day-content" id="weekAllDay4"></div>
                                <div class="week-all-day-content" id="weekAllDay5"></div>
                                <div class="week-all-day-content" id="weekAllDay6"></div>

                                <div class="week-time-slots" id="weekTimeSlots"></div>
                                <div class="week-day-column" id="weekDayColumn0"></div>
                                <div class="week-day-column" id="weekDayColumn1"></div>
                                <div class="week-day-column" id="weekDayColumn2"></div>
                                <div class="week-day-column" id="weekDayColumn3"></div>
                                <div class="week-day-column" id="weekDayColumn4"></div>
                                <div class="week-day-column" id="weekDayColumn5"></div>
                                <div class="week-day-column" id="weekDayColumn6"></div>
                            </div>
                        </div>
                    </div>

                    <!-- 日视图 -->
                    <div class="view-container" id="dayView">
                        <div class="day-view">
                            <div class="day-header">
                                <div class="week-time-label">时间</div>
                                <div class="week-day-header" id="dayTitle">2024年1月1日 周一</div>
                            </div>
                            <div class="day-content">
                                <!-- 全天任务区域 -->
                                <div class="day-all-day-label">全天任务</div>
                                <div class="day-all-day-content" id="dayAllDayTasks"></div>

                                <div class="day-time-slots" id="dayTimeSlots"></div>
                                <div class="day-slot-column" id="daySlotColumn"></div>
                            </div>
                        </div>
                    </div>
                </div>
            `;

            this.elem.html(html);
        },

        // 绑定事件
        bindEvents: function () {
            var self = this;

            // 视图切换按钮
            $('.calendar-view-buttons button', this.elem).on('click', function () {
                $('.calendar-view-buttons button', self.elem).removeClass('layui-btn-normal').addClass('layui-btn-primary');
                $(this).removeClass('layui-btn-primary').addClass('layui-btn-normal');
                self.currentView = $(this).data('view');

                // 触发视图切换事件
                if (self.options.onViewChange) {
                    self.options.onViewChange(self.currentView);
                }
                self.triggerEvent('viewChange', self.currentView);

                self.render();
            });

            // 导航按钮
            $('#prevBtn', this.elem).on('click', function () {
                self.navigate(-1);
            });

            $('#nextBtn', this.elem).on('click', function () {
                self.navigate(1);
            });

            $('#todayBtn', this.elem).on('click', function () {
                self.currentDate = new Date();
                self.render();
            });

            // 添加任务按钮
            $('#addTaskBtn', this.elem).on('click', function () {
                if (self.options.onDayClick) {
                    self.options.onDayClick(self.currentDate);
                }
                self.triggerEvent('dayClick', self.currentDate);
            });
        },

        // 初始化日期选择器（供外部调用）
        initDatePickers: function () {
            // 此方法由外部调用
        },

        // 加载任务数据
        loadTasksFromStorage: function () {
            if (this.options.useStorage && this.options.storageKey) {
                var storedTasks = localStorage.getItem(this.options.storageKey);
                if (storedTasks) {
                    try {
                        this.tasks = JSON.parse(storedTasks);
                    } catch (e) {
                        console.error('Failed to parse stored tasks:', e);
                        this.tasks = [];
                    }
                }
            }
        },

        // 保存任务数据到存储
        saveTasksToStorage: function () {
            if (this.options.useStorage && this.options.storageKey) {
                localStorage.setItem(this.options.storageKey, JSON.stringify(this.tasks));
            }
        },

        // 导航到上/下一周期
        navigate: function (direction) {
            switch (this.currentView) {
                case 'month':
                    this.currentDate.setMonth(this.currentDate.getMonth() + direction);
                    break;
                case 'week':
                    this.currentDate.setDate(this.currentDate.getDate() + (direction * 7));
                    break;
                case 'day':
                    this.currentDate.setDate(this.currentDate.getDate() + direction);
                    break;
            }
            this.render();
        },

        // 渲染日历
        render: function () {
            this.updateTitle();

            // 清除旧的时间线计时器
            if (this.timeLineTimer) {
                clearTimeout(this.timeLineTimer);
                this.timeLineTimer = null;
            }

            // 移除旧的时间线
            $('.current-time-line', this.elem).remove();
            $('.current-time-line-label', this.elem).remove();

            // 隐藏所有视图，显示当前视图
            $('.view-container', this.elem).removeClass('active');
            $('#' + this.currentView + 'View', this.elem).addClass('active');

            switch (this.currentView) {
                case 'month':
                    this.renderMonthView();
                    break;
                case 'week':
                    this.renderWeekView();
                    break;
                case 'day':
                    this.renderDayView();
                    break;
            }

            // 更新按钮状态
            $('.calendar-view-buttons button[data-view="' + this.currentView + '"]', this.elem)
                .removeClass('layui-btn-primary')
                .addClass('layui-btn-normal');

            // 重新渲染时间线
            if (this.options.showTimeLine) {
                setTimeout(() => {
                    this.renderCurrentTimeLine();
                }, 100);
            }
        },

        // 更新标题
        updateTitle: function () {
            var title = '';
            var year = this.currentDate.getFullYear();
            var month = this.currentDate.getMonth() + 1;
            var date = this.currentDate.getDate();

            switch (this.currentView) {
                case 'month':
                    title = year + '年' + month + '月';
                    break;
                case 'week':
                    var weekStart = this.getWeekStartDate(this.currentDate);
                    var weekEnd = new Date(weekStart);
                    weekEnd.setDate(weekEnd.getDate() + 6);

                    var startMonth = weekStart.getMonth() + 1;
                    var endMonth = weekEnd.getMonth() + 1;

                    if (weekStart.getFullYear() === weekEnd.getFullYear() && startMonth === endMonth) {
                        title = weekStart.getFullYear() + '年' + startMonth + '月' +
                            weekStart.getDate() + '日 - ' + weekEnd.getDate() + '日';
                    } else if (weekStart.getFullYear() === weekEnd.getFullYear()) {
                        title = weekStart.getFullYear() + '年' +
                            startMonth + '月' + weekStart.getDate() + '日 - ' +
                            endMonth + '月' + weekEnd.getDate() + '日';
                    } else {
                        title = weekStart.getFullYear() + '年' + startMonth + '月' + weekStart.getDate() + '日 - ' +
                            weekEnd.getFullYear() + '年' + endMonth + '月' + weekEnd.getDate() + '日';
                    }
                    break;
                case 'day':
                    var dayNames = ['周日', '周一', '周二', '周三', '周四', '周五', '周六'];
                    title = year + '年' + month + '月' + date + '日 ' + dayNames[this.currentDate.getDay()];
                    break;
            }

            $('#currentDate', this.elem).text(title);
        },

        // 渲染月视图
        renderMonthView: function () {
            var year = this.currentDate.getFullYear();
            var month = this.currentDate.getMonth();
            var today = new Date();
            today.setHours(0, 0, 0, 0);

            // 获取月份的第一天
            var firstDay = new Date(year, month, 1);
            var firstDayWeek = firstDay.getDay();

            // 获取月份的最后一天
            var lastDay = new Date(year, month + 1, 0);
            var daysInMonth = lastDay.getDate();

            // 清空容器
            $('#monthDays', this.elem).empty();

            // 添加上个月的几天
            var prevMonthLastDay = new Date(year, month, 0).getDate();
            for (var i = firstDayWeek - 1; i >= 0; i--) {
                var date = new Date(year, month - 1, prevMonthLastDay - i);
                this.addMonthDay(date, true);
            }

            // 添加本月的所有天
            for (var i = 1; i <= daysInMonth; i++) {
                var date = new Date(year, month, i);
                var isToday = date.getDate() === today.getDate() &&
                    date.getMonth() === today.getMonth() &&
                    date.getFullYear() === today.getFullYear();
                this.addMonthDay(date, false, isToday);
            }

            // 添加下个月的几天
            var totalCells = 42;
            var daysAdded = firstDayWeek + daysInMonth;
            for (var i = 1; i <= totalCells - daysAdded; i++) {
                var date = new Date(year, month + 1, i);
                this.addMonthDay(date, true);
            }
        },

        // 添加月视图的一天
        addMonthDay: function (date, isOtherMonth, isToday) {
            var self = this;
            var dayElement = $('<div class="month-day"></div>');

            if (isOtherMonth) {
                dayElement.addClass('other-month');
            }

            if (isToday) {
                dayElement.addClass('today');
            }

            var dayNumber = date.getDate();
            var dayHeader = $('<div class="month-day-header"><span class="month-day-number">' + dayNumber + '</span></div>');
            dayElement.append(dayHeader);

            // 获取该日期的任务
            var tasks = this.getTasksForDate(date);

            // 显示任务
            tasks.forEach(function (task) {
                var taskElement = self.createTaskElement(task);
                taskElement.on('click', function (e) {
                    e.stopPropagation();
                    if (self.options.onTaskClick) {
                        self.options.onTaskClick(task);
                    }
                    self.triggerEvent('taskClick', task);
                });
                dayElement.append(taskElement);
            });

            // 添加点击事件
            dayElement.on('click', function (e) {
                if (!$(e.target).closest('.calendar-task').length) {
                    var clickDate = new Date(date);
                    clickDate.setHours(9, 0, 0);

                    if (self.options.onDayClick) {
                        self.options.onDayClick(clickDate);
                    }
                    self.triggerEvent('dayClick', clickDate);
                }
            });

            $('#monthDays', this.elem).append(dayElement);
        },

        // 渲染周视图
        renderWeekView: function () {
            var self = this;
            var weekStart = this.getWeekStartDate(this.currentDate);
            var weekDates = [];

            // 生成一周的日期
            for (var i = 0; i < 7; i++) {
                var date = new Date(weekStart);
                date.setDate(date.getDate() + i);
                weekDates.push(date);
            }

            // 更新周视图标题
            var dayNames = ['周日', '周一', '周二', '周三', '周四', '周五', '周六'];
            weekDates.forEach(function (date, index) {
                var dayName = dayNames[date.getDay()];
                var month = date.getMonth() + 1;
                var day = date.getDate();
                $('#weekDay' + index, self.elem).html(dayName + '<br>' + month + '/' + day);
            });

            // 清空全天任务区域
            for (var i = 0; i < 7; i++) {
                $('#weekAllDay' + i, self.elem).empty();
            }

            // 添加全天任务和跨天任务
            weekDates.forEach(function (date, index) {
                var tasks = self.getTasksForDate(date).filter(function (task) {
                    // 修复：包括全天任务和跨天任务
                    return task.allDay || task.startDate !== task.endDate;
                });

                tasks.forEach(function (task) {
                    var taskElement = self.createTaskElement(task);
                    taskElement.addClass('all-day');
                    taskElement.on('click', function (e) {
                        e.stopPropagation();
                        if (self.options.onTaskClick) {
                            self.options.onTaskClick(task);
                        }
                        self.triggerEvent('taskClick', task);
                    });
                    $('#weekAllDay' + index, self.elem).append(taskElement);
                });
            });

            // 生成时间列
            $('#weekTimeSlots', this.elem).empty();
            for (var hour = 0; hour < 24; hour++) {
                var timeSlot = $('<div class="week-time-slot">' +
                    (hour < 10 ? '0' + hour : hour) + ':00</div>');
                $('#weekTimeSlots', this.elem).append(timeSlot);
            }

            // 清空日期列
            for (var i = 0; i < 7; i++) {
                $('#weekDayColumn' + i, this.elem).empty();
                for (var hour = 0; hour < 24; hour++) {
                    var daySlot = $('<div class="week-day-slot" data-hour="' + hour + '" data-date="' +
                        this.formatDate(weekDates[i]) + '"></div>');

                    // 添加点击事件
                    daySlot.on('click', function (e) {
                        if (!$(e.target).closest('.time-slot-task').length) {
                            var hour = $(this).data('hour');
                            var dateStr = $(this).data('date');
                            var date = new Date(dateStr);
                            date.setHours(hour, 30, 0);

                            if (self.options.onDayClick) {
                                self.options.onDayClick(date);
                            }
                            self.triggerEvent('dayClick', date);
                        }
                    });

                    $('#weekDayColumn' + i, this.elem).append(daySlot);
                }
            }

            // 渲染周视图的任务
            this.renderWeekTasks(weekDates);
        },

        // 渲染周视图的任务
        renderWeekTasks: function (weekDates) {
            var self = this;

            // 清空之前的任务
            $('.time-slot-task, .week-multi-day-task', this.elem).remove();

            // 获取一周的时间范围（修复边界值问题）
            var weekStart = new Date(weekDates[0]);
            weekStart.setHours(0, 0, 0, 0); // 设置为周日的00:00:00

            var weekEnd = new Date(weekDates[6]);
            weekEnd.setHours(23, 59, 59, 999); // 设置为周六的23:59:59.999

            // 查找本周内的所有任务（排除跨天任务）
            var weekTasks = this.tasks.filter(function (task) {
                if (task.allDay) return false;

                // 排除跨天任务（已经在全天任务区域显示）
                if (task.startDate !== task.endDate) return false;

                var taskStart = new Date(task.startDate + ' ' + task.startTime);
                var taskEnd = new Date(task.endDate + ' ' + task.endTime);

                // 修复：任务在时间上与本周末尾有重叠就应该显示
                // 任务开始时间 <= 周结束时间 且 任务结束时间 >= 周开始时间
                return (taskStart <= weekEnd && taskEnd >= weekStart);
            });

            // 按日期分组任务
            var tasksByDate = {};
            weekDates.forEach(function (date, index) {
                var dateStr = self.formatDate(date);
                tasksByDate[dateStr] = [];
            });

            // 将任务分配到对应的日期
            weekTasks.forEach(function (task) {
                var taskDateStr = task.startDate;
                if (tasksByDate[taskDateStr]) {
                    tasksByDate[taskDateStr].push(task);
                }
            });

            // 为每个日期的任务进行分组以避免重叠
            Object.keys(tasksByDate).forEach(function (dateStr) {
                var tasks = tasksByDate[dateStr];
                if (tasks.length === 0) return;

                // 分组避免重叠
                var groupedTasks = self.groupOverlappingTasks(tasks);

                // 找到任务所在的日期列
                var dayIndex = -1;
                for (var i = 0; i < weekDates.length; i++) {
                    if (self.formatDate(weekDates[i]) === dateStr) {
                        dayIndex = i;
                        break;
                    }
                }

                if (dayIndex === -1) return;

                // 渲染每个任务
                groupedTasks.forEach(function (task) {
                    var taskStart = new Date(task.startDate + ' ' + task.startTime);
                    var taskEnd = new Date(task.endDate + ' ' + task.endTime);

                    // 计算任务在时间轴上的位置（包含列信息）
                    var position = self.calculateTaskPosition(
                        taskStart,
                        taskEnd,
                        task.columnIndex || 0,
                        task.totalColumns || 1
                    );

                    var taskElement = $('<div class="time-slot-task task-color-' + task.color + '"></div>');
                    taskElement.css({
                        top: position.top + 'px',
                        height: position.height + 'px',
                        left: position.left,
                        width: position.width
                    });

                    var title = $('<div class="task-title">' + task.title + '</div>');
                    taskElement.append(title);

                    var timeText = self.formatTime(taskStart) + ' - ' + self.formatTime(taskEnd);
                    var time = $('<div class="task-time">' + timeText + '</div>');
                    taskElement.append(time);

                    // 点击任务编辑
                    taskElement.on('click', function (e) {
                        e.stopPropagation();
                        if (self.options.onTaskClick) {
                            self.options.onTaskClick(task);
                        }
                        self.triggerEvent('taskClick', task);
                    });

                    $('#weekDayColumn' + dayIndex, self.elem).append(taskElement);
                });
            });
        },

        // 对任务进行分组，避免重叠
        groupOverlappingTasks: function (tasks) {
            if (!tasks || tasks.length === 0) return [];

            // 按开始时间排序
            tasks.sort(function (a, b) {
                var aStart = new Date(a.startDate + ' ' + (a.startTime || '00:00'));
                var bStart = new Date(b.startDate + ' ' + (b.startTime || '00:00'));
                return aStart - bStart;
            });

            var columns = [];
            var columnMap = {};

            tasks.forEach(function (task) {
                var taskStart = new Date(task.startDate + ' ' + (task.startTime || '00:00'));
                var taskEnd = new Date(task.endDate + ' ' + (task.endTime || '23:59'));

                // 找到可以放置的列
                var placed = false;
                for (var i = 0; i < columns.length; i++) {
                    var lastTaskEnd = columns[i];
                    if (taskStart >= lastTaskEnd) {
                        columns[i] = taskEnd;
                        columnMap[task.id] = i;
                        placed = true;
                        break;
                    }
                }

                if (!placed) {
                    columns.push(taskEnd);
                    columnMap[task.id] = columns.length - 1;
                }
            });

            tasks.forEach(function (task) {
                task.columnIndex = columnMap[task.id] || 0;
                task.totalColumns = columns.length;
            });

            return tasks;
        },

        // 计算任务在时间轴上的位置
        calculateTaskPosition: function (taskStart, taskEnd, columnIndex, totalColumns) {
            var timeSlotHeight = 60;
            var totalHeight = timeSlotHeight * 24;

            var startHours = taskStart.getHours() + taskStart.getMinutes() / 60;
            var endHours = taskEnd.getHours() + taskEnd.getMinutes() / 60;

            var topPosition = (startHours / 24) * totalHeight + startHours;
            var height = ((endHours - startHours) / 24) * totalHeight;

            if (height < 20) height = 20;

            var columnWidth = 100 / totalColumns;
            var left = columnIndex * columnWidth;
            var width = columnWidth;

            var margin = 2;
            left += margin / 2;
            width -= margin;

            return {
                top: topPosition,
                height: height,
                left: left + '%',
                width: width + '%'
            };
        },

        // 渲染日视图
        renderDayView: function () {
            var self = this;
            var date = this.currentDate;

            // 更新日视图标题
            var dayNames = ['周日', '周一', '周二', '周三', '周四', '周五', '周六'];
            var dayName = dayNames[date.getDay()];
            var year = date.getFullYear();
            var month = date.getMonth() + 1;
            var day = date.getDate();
            $('#dayTitle', this.elem).html(year + '年' + month + '月' + day + '日 ' + dayName);

            // 清空全天任务区域
            $('#dayAllDayTasks', this.elem).empty();

            // 添加全天任务和跨天任务
            var allDayTasks = this.getTasksForDate(date).filter(function (task) {
                return task.allDay || task.startDate !== task.endDate;
            });

            allDayTasks.forEach(function (task) {
                var taskElement = self.createTaskElement(task);
                taskElement.addClass('all-day');
                taskElement.on('click', function (e) {
                    e.stopPropagation();
                    if (self.options.onTaskClick) {
                        self.options.onTaskClick(task);
                    }
                    self.triggerEvent('taskClick', task);
                });
                $('#dayAllDayTasks', self.elem).append(taskElement);
            });

            // 生成时间列
            $('#dayTimeSlots', this.elem).empty();
            for (var hour = 0; hour < 24; hour++) {
                var timeSlot = $('<div class="day-time-slot">' +
                    (hour < 10 ? '0' + hour : hour) + ':00</div>');
                $('#dayTimeSlots', this.elem).append(timeSlot);
            }

            // 清空日列
            $('#daySlotColumn', this.elem).empty();
            for (var hour = 0; hour < 24; hour++) {
                var daySlot = $('<div class="day-slot" data-hour="' + hour + '"></div>');

                // 添加点击事件
                daySlot.on('click', function (e) {
                    if (!$(e.target).closest('.time-slot-task').length) {
                        var hour = $(this).data('hour');
                        var clickDate = new Date(date);
                        clickDate.setHours(hour, 30, 0);

                        if (self.options.onDayClick) {
                            self.options.onDayClick(clickDate);
                        }
                        self.triggerEvent('dayClick', clickDate);
                    }
                });

                $('#daySlotColumn', this.elem).append(daySlot);
            }

            // 渲染日视图的时间槽任务
            this.renderDayTasks(date);
        },

        // 渲染日视图的任务
        renderDayTasks: function (date) {
            var self = this;
            var dateStr = this.formatDate(date);

            // 清空之前的任务
            $('.time-slot-task', this.elem).remove();

            // 查找当天的所有任务
            var dayTasks = this.tasks.filter(function (task) {
                if (task.allDay) return false;
                if (task.startDate !== task.endDate) return false;
                return (task.startDate <= dateStr && task.endDate >= dateStr);
            });

            if (dayTasks.length === 0) return;

            var groupedTasks = self.groupOverlappingTasks(dayTasks);

            groupedTasks.forEach(function (task) {
                var taskStart = new Date(task.startDate + ' ' + task.startTime);
                var taskEnd = new Date(task.endDate + ' ' + task.endTime);

                var dayStart = new Date(date);
                dayStart.setHours(0, 0, 0, 0);

                var dayEnd = new Date(date);
                dayEnd.setHours(23, 59, 59, 999);

                var displayStart = taskStart < dayStart ? dayStart : taskStart;
                var displayEnd = taskEnd > dayEnd ? dayEnd : taskEnd;

                var position = self.calculateTaskPosition(
                    displayStart,
                    displayEnd,
                    task.columnIndex || 0,
                    task.totalColumns || 1
                );

                var taskElement = $('<div class="time-slot-task task-color-' + task.color + '"></div>');
                taskElement.css({
                    top: position.top + 'px',
                    height: position.height + 'px',
                    left: position.left,
                    width: position.width
                });

                var title = $('<div class="task-title">' + task.title + '</div>');
                taskElement.append(title);

                var timeText = self.formatTime(displayStart) + ' - ' + self.formatTime(displayEnd);
                var time = $('<div class="task-time">' + timeText + '</div>');
                taskElement.append(time);

                // 点击任务编辑
                taskElement.on('click', function (e) {
                    e.stopPropagation();
                    if (self.options.onTaskClick) {
                        self.options.onTaskClick(task);
                    }
                    self.triggerEvent('taskClick', task);
                });

                $('#daySlotColumn', self.elem).append(taskElement);
            });
        },

        // 创建任务元素
        createTaskElement: function (task) {
            var taskElement = $('<div class="calendar-task task-color-' + task.color + '"></div>');
            var title = $('<div class="task-title">' + task.title + '</div>');
            taskElement.append(title);

            // 添加时间显示（如果不是全天任务）
            if (!task.allDay && task.startTime && task.endTime) {
                var time = $('<div class="task-time">' + task.startTime + ' - ' + task.endTime + '</div>');
                taskElement.append(time);
            }

            return taskElement;
        },

        // 获取周开始日期（周日）
        getWeekStartDate: function (date) {
            var day = date.getDay();
            var diff = date.getDate() - day;
            var weekStart = new Date(date);
            weekStart.setDate(diff);
            return weekStart;
        },

        // 获取某天的任务（修复版）
        getTasksForDate: function (date) {
            var self = this;
            var dateStr = this.formatDate(date);
            return this.tasks.filter(function (task) {
                var taskStartDate = task.startDate;
                var taskEndDate = task.endDate;

                // 如果是全天任务或者跨天任务，检查是否在日期范围内
                if (task.allDay || task.startDate !== task.endDate) {
                    // 创建一个日期对象进行比较
                    var dateObj = new Date(dateStr + 'T00:00:00');
                    var taskStartObj = new Date(taskStartDate + 'T00:00:00');
                    var taskEndObj = new Date(taskEndDate + 'T23:59:59');

                    return dateObj >= taskStartObj && dateObj <= taskEndObj;
                }

                // 非全天单天任务，检查日期是否匹配
                return dateStr === taskStartDate;
            });
        },

        // 格式化日期为 YYYY-MM-DD
        formatDate: function (date) {
            var year = date.getFullYear();
            var month = ('0' + (date.getMonth() + 1)).slice(-2);
            var day = ('0' + date.getDate()).slice(-2);
            return year + '-' + month + '-' + day;
        },

        // 格式化时间为 HH:mm
        formatTime: function (date) {
            var hours = ('0' + date.getHours()).slice(-2);
            var minutes = ('0' + date.getMinutes()).slice(-2);
            return hours + ':' + minutes;
        },

        // 添加任务
        addTask: function (task) {
            if (!task.id) {
                task.id = 'task_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
            }
            this.tasks.push(task);
            this.saveTasksToStorage();
            this.render();

            // 触发任务添加事件
            if (this.options.onTaskAdded) {
                this.options.onTaskAdded(task);
            }
            this.triggerEvent('taskAdded', task);

            return task.id;
        },

        // 更新任务
        updateTask: function (task) {
            var updated = false;
            for (var i = 0; i < this.tasks.length; i++) {
                if (this.tasks[i].id === task.id) {
                    this.tasks[i] = task;
                    updated = true;
                    break;
                }
            }

            if (updated) {
                this.saveTasksToStorage();
                this.render();

                // 触发任务更新事件
                if (this.options.onTaskUpdated) {
                    this.options.onTaskUpdated(task);
                }
                this.triggerEvent('taskUpdated', task);

                return true;
            }
            return false;
        },

        // 删除任务
        deleteTask: function (id) {
            var originalLength = this.tasks.length;
            this.tasks = this.tasks.filter(function (task) {
                return task.id !== id;
            });

            if (this.tasks.length < originalLength) {
                this.saveTasksToStorage();
                this.render();

                // 触发任务删除事件
                if (this.options.onTaskDeleted) {
                    this.options.onTaskDeleted(id);
                }
                this.triggerEvent('taskDeleted', id);

                return true;
            }
            return false;
        },

        // 获取所有任务
        getTasks: function () {
            return this.tasks.slice(); // 返回副本
        },

        // 设置任务数据
        setTasks: function (tasks) {
            this.tasks = tasks || [];
            this.saveTasksToStorage();
            this.render();
        },

        // 清空所有任务
        clearTasks: function () {
            this.tasks = [];
            this.saveTasksToStorage();
            this.render();
        },

        // 设置当前视图
        setView: function (view) {
            if (['month', 'week', 'day'].includes(view)) {
                this.currentView = view;
                this.render();
                return true;
            }
            return false;
        },

        // 设置当前日期
        setCurrentDate: function (date) {
            if (date instanceof Date) {
                this.currentDate = date;
                this.render();
                return true;
            } else if (typeof date === 'string') {
                var parsedDate = new Date(date);
                if (!isNaN(parsedDate.getTime())) {
                    this.currentDate = parsedDate;
                    this.render();
                    return true;
                }
            }
            return false;
        },

        // 跳转到今天
        goToToday: function () {
            this.currentDate = new Date();
            this.render();
        },

        // =============== 时间线功能（独立时间线版本） ===============

        // 渲染当前时间线
        renderCurrentTimeLine: function () {
            // 如果不显示时间线，直接返回
            if (!this.options.showTimeLine) {
                return;
            }

            var self = this;

            // 移除旧的时间线
            $('.current-time-line', this.elem).remove();
            $('.current-time-line-label', this.elem).remove();
            $('.current-time-dot', this.elem).remove();

            var now = new Date();
            var today = new Date();
            today.setHours(0, 0, 0, 0);

            // 检查当前视图是否包含今天
            var shouldShowTimeLine = false;
            var container = null;

            switch (this.currentView) {
                case 'month':
                    // 月视图不显示时间线
                    break;

                case 'week':
                    shouldShowTimeLine = this.isDateInCurrentWeek(today);
                    if (shouldShowTimeLine) {
                        container = $('.week-content', this.elem);
                        this.renderWeekTimeLine(now, container);
                    }
                    break;

                case 'day':
                    shouldShowTimeLine = this.isSameDate(this.currentDate, today);
                    if (shouldShowTimeLine) {
                        container = $('.day-content', this.elem);
                        this.renderDayTimeLine(now, container);
                    }
                    break;
            }

            // 设置定时器，每分钟更新一次时间线
            if (shouldShowTimeLine && this.options.autoUpdateTimeLine) {
                if (this.timeLineTimer) {
                    clearTimeout(this.timeLineTimer);
                }

                this.timeLineTimer = setTimeout(function () {
                    self.renderCurrentTimeLine();
                }, 60000); // 每分钟更新一次
            }
        },

        // 检查日期是否在当前周
        isDateInCurrentWeek: function (date) {
            var weekStart = this.getWeekStartDate(this.currentDate);
            var weekEnd = new Date(weekStart);
            weekEnd.setDate(weekEnd.getDate() + 6);
            weekEnd.setHours(23, 59, 59, 999);

            return date >= weekStart && date <= weekEnd;
        },

        // 检查两个日期是否是同一天
        isSameDate: function (date1, date2) {
            return date1.getFullYear() === date2.getFullYear() &&
                date1.getMonth() === date2.getMonth() &&
                date1.getDate() === date2.getDate();
        },

        // 渲染周视图时间线（独立于单元格）
        renderWeekTimeLine: function (now, container) {
            if (container.length === 0) return;

            // 计算时间线在时间轴上的位置
            var currentHour = now.getHours();
            var currentMinute = now.getMinutes();
            var timePosition = (currentHour * 61 + currentMinute) * (60 / 60) + 71; // 每个时间槽60px，代表1小时，每分钟1px

            //console.log($(".week-all-day-label").height());

            // 获取时间列宽度（80px）
            var timeColumnWidth = 80;

            // 创建时间线容器
            var timeLineContainer = $('<div class="current-time-line"></div>');
            timeLineContainer.css({
                position: 'absolute',
                top: timePosition + 'px',
                left: timeColumnWidth + 'px', // 从时间列右侧开始
                right: '0', // 延伸到最右边
                height: '2px',
                backgroundColor: '#FF5722',
                zIndex: '1000',
                pointerEvents: 'none'
            });

            // 创建时间线圆点（放在时间列内）
            var timeDot = $('<div class="current-time-dot"></div>');
            timeDot.css({
                position: 'absolute',
                top: (timePosition - 4) + 'px',
                left: (timeColumnWidth - 5) + 'px', // 在时间列右侧边缘
                width: '10px',
                height: '10px',
                backgroundColor: '#FF5722',
                borderRadius: '50%',
                zIndex: '1001',
                pointerEvents: 'none'
            });

            // 创建时间标签（放在时间列内）
            var timeLabel = $('<div class="current-time-line-label"></div>');
            timeLabel.text(this.formatTime(now));
            timeLabel.css({
                position: 'absolute',
                top: (timePosition - 10) + 'px',
                left: (timeColumnWidth + 5) + 'px', // 从时间列右侧开始
                backgroundColor: '#FF5722',
                color: 'white',
                padding: '2px 6px',
                borderRadius: '3px',
                fontSize: '11px',
                fontWeight: 'bold',
                whiteSpace: 'nowrap',
                zIndex: '1002',
                pointerEvents: 'none'
            });

            // 添加到容器
            container.append(timeLineContainer);
            container.append(timeDot);
            container.append(timeLabel);
        },

        // 渲染日视图时间线（独立于单元格）
        renderDayTimeLine: function (now, container) {
            if (container.length === 0) return;

            // 计算时间线在时间轴上的位置
            var currentHour = now.getHours();
            var currentMinute = now.getMinutes();
            var timePosition = (currentHour * 61 + currentMinute) * (60 / 60) + 71; // 每个时间槽60px，代表1小时，每分钟1px

            // 获取时间列宽度（80px）
            var timeColumnWidth = 80;

            // 创建时间线容器
            var timeLineContainer = $('<div class="current-time-line"></div>');
            timeLineContainer.css({
                position: 'absolute',
                top: timePosition + 'px',
                left: timeColumnWidth + 'px', // 从时间列右侧开始
                right: '0', // 延伸到最右边
                height: '2px',
                backgroundColor: '#FF5722',
                zIndex: '1000',
                pointerEvents: 'none'
            });

            // 创建时间线圆点（放在时间列内）
            var timeDot = $('<div class="current-time-dot"></div>');
            timeDot.css({
                position: 'absolute',
                top: (timePosition - 4) + 'px',
                left: (timeColumnWidth - 5) + 'px', // 在时间列右侧边缘
                width: '10px',
                height: '10px',
                backgroundColor: '#FF5722',
                borderRadius: '50%',
                zIndex: '1001',
                pointerEvents: 'none'
            });

            // 创建时间标签（放在时间列内）
            var timeLabel = $('<div class="current-time-line-label"></div>');
            timeLabel.text(this.formatTime(now));
            timeLabel.css({
                position: 'absolute',
                top: (timePosition - 10) + 'px',
                left: (timeColumnWidth + 5) + 'px', // 从时间列右侧开始
                backgroundColor: '#FF5722',
                color: 'white',
                padding: '2px 6px',
                borderRadius: '3px',
                fontSize: '11px',
                fontWeight: 'bold',
                whiteSpace: 'nowrap',
                zIndex: '1002',
                pointerEvents: 'none'
            });

            // 添加到容器
            container.append(timeLineContainer);
            container.append(timeDot);
            container.append(timeLabel);
        },

        // =============== 事件系统 ===============

        // 触发事件
        triggerEvent: function (eventName, data) {
            if (this.eventHandlers[eventName]) {
                this.eventHandlers[eventName].forEach(function (handler) {
                    handler(data);
                });
            }
        },

        // 绑定事件
        on: function (eventName, handler) {
            if (!this.eventHandlers[eventName]) {
                this.eventHandlers[eventName] = [];
            }
            this.eventHandlers[eventName].push(handler);
        },

        // 解绑事件
        off: function (eventName, handler) {
            if (this.eventHandlers[eventName]) {
                if (handler) {
                    var index = this.eventHandlers[eventName].indexOf(handler);
                    if (index > -1) {
                        this.eventHandlers[eventName].splice(index, 1);
                    }
                } else {
                    this.eventHandlers[eventName] = [];
                }
            }
        },

        // 获取组件实例
        getInstance: function () {
            return this;
        },

        // 销毁组件
        destroy: function () {
            // 清除时间线计时器
            if (this.timeLineTimer) {
                clearTimeout(this.timeLineTimer);
                this.timeLineTimer = null;
            }

            // 移除事件监听
            $('.calendar-view-buttons button', this.elem).off('click');
            $('#prevBtn, #nextBtn, #todayBtn, #addTaskBtn', this.elem).off('click');

            // 清空容器
            this.elem.empty();

            // 触发销毁事件
            this.triggerEvent('destroy', this);
        }
    };

    // 对外暴露的render方法
    var calendar = {
        render: function (options) {
            var instance = new Calendar(options);
            return instance;
        },

        // 生成示例任务数据
        getSampleTasks: function () {
            var today = new Date();
            today.setHours(0, 0, 0, 0);

            var tomorrow = new Date(today);
            tomorrow.setDate(tomorrow.getDate() + 1);

            var nextWeek = new Date(today);
            nextWeek.setDate(nextWeek.getDate() + 7);

            return [
                {
                    id: 'task_1',
                    title: '团队会议',
                    startDate: this.formatDate(today),
                    startTime: '09:00',
                    endDate: this.formatDate(today),
                    endTime: '10:30',
                    color: 1,
                    allDay: 0,
                    description: '每周团队例会'
                },
                {
                    id: 'task_2',
                    title: '项目评审',
                    startDate: this.formatDate(today),
                    startTime: '14:00',
                    endDate: this.formatDate(today),
                    endTime: '16:00',
                    color: 2,
                    allDay: 0,
                    description: '项目进度评审会议'
                },
                {
                    id: 'task_3',
                    title: '生日聚会',
                    startDate: this.formatDate(tomorrow),
                    startTime: '18:00',
                    endDate: this.formatDate(tomorrow),
                    endTime: '21:00',
                    color: 7,
                    allDay: 0,
                    description: '朋友生日聚会'
                },
                {
                    id: 'task_4',
                    title: '项目截止日期',
                    startDate: this.formatDate(nextWeek),
                    endDate: this.formatDate(nextWeek),
                    color: 4,
                    allDay: 1,
                    description: '提交项目最终版本'
                },
                {
                    id: 'task_5',
                    title: '出差',
                    startDate: this.formatDate(new Date(today.getFullYear(), today.getMonth(), today.getDate() + 3)),
                    endDate: this.formatDate(new Date(today.getFullYear(), today.getMonth(), today.getDate() + 5)),
                    color: 5,
                    allDay: 1,
                    description: '前往上海出差'
                }
            ];
        },

        // 工具方法：格式化日期
        formatDate: function (date) {
            var year = date.getFullYear();
            var month = ('0' + (date.getMonth() + 1)).slice(-2);
            var day = ('0' + date.getDate()).slice(-2);
            return year + '-' + month + '-' + day;
        },

        // 工具方法：格式化时间
        formatTime: function (date) {
            var hours = ('0' + date.getHours()).slice(-2);
            var minutes = ('0' + date.getMinutes()).slice(-2);
            return hours + ':' + minutes;
        }
    };

    layui.link(layui.cache.base + 'calendar/calendar.css');

    exports('calendar', calendar);
});