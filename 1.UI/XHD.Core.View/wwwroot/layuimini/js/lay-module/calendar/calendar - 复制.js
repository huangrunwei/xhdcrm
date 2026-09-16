// layui-calendar.js - 日历组件
layui.define(['jquery', 'layer', 'laydate', 'form'], function (exports) {
    "use strict";

    var $ = layui.jquery;
    var layer = layui.layer;
    var laydate = layui.laydate;
    var form = layui.form;

    var Calendar = function (options) {
        this.config = $.extend({
            elem: '',
            view: 'month', // month, week, day
            date: new Date(),
            tasks: [],
            events: {
                taskClick: null, // 点击任务
                dateClick: null, // 点击日期
                viewChange: null, // 视图切换
                prev: null, // 上一页
                next: null, // 下一页
                today: null // 今天
            }
        }, options);

        this.currentDate = new Date(this.config.date);
        this.currentView = this.config.view;
        this.tasks = this.config.tasks || [];
        this.currentTimeLineTimer = null;

        this.init();
    };

    Calendar.prototype = {
        constructor: Calendar,

        // 初始化
        init: function () {
            if (!this.config.elem) {
                console.error('Calendar: elem参数不能为空');
                return;
            }

            this.renderHTML();
            this.bindEvents();
            this.render();
            this.startCurrentTimeLineUpdate();
        },

        // 渲染HTML结构
        renderHTML: function () {
            var elem = $(this.config.elem);
            elem.addClass('layui-calendar-container');

            var html = `
            <!-- 日历头部 -->
            <div class="layui-calendar-header">
                <div class="layui-calendar-title" id="currentDate">2024年1月</div>
                <div class="layui-calendar-actions">
                    <div class="layui-calendar-nav-buttons">
                        <button class="layui-btn layui-btn-primary" id="prevBtn">
                            <i class="layui-icon">&#xe65a;</i>
                        </button>
                        <button class="layui-btn" id="todayBtn">今天</button>
                        <button class="layui-btn layui-btn-primary" id="nextBtn">
                            <i class="layui-icon">&#xe65b;</i>
                        </button>
                    </div>
                    <div class="layui-calendar-view-buttons">
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
            <div class="layui-calendar-view-container active" id="monthView">
                <div class="layui-calendar-month-view">
                    <div class="layui-calendar-month-weekdays">
                        <div class="layui-calendar-month-weekday">周日</div>
                        <div class="layui-calendar-month-weekday">周一</div>
                        <div class="layui-calendar-month-weekday">周二</div>
                        <div class="layui-calendar-month-weekday">周三</div>
                        <div class="layui-calendar-month-weekday">周四</div>
                        <div class="layui-calendar-month-weekday">周五</div>
                        <div class="layui-calendar-month-weekday">周六</div>
                    </div>
                    <div class="layui-calendar-month-days" id="monthDays"></div>
                </div>
            </div>

            <!-- 周视图 -->
            <div class="layui-calendar-view-container" id="weekView">
                <div class="layui-calendar-week-view">
                    <div class="layui-calendar-week-header">
                        <div class="layui-calendar-week-time-label">时间</div>
                        <div class="layui-calendar-week-day-header" id="weekDay0">周日</div>
                        <div class="layui-calendar-week-day-header" id="weekDay1">周一</div>
                        <div class="layui-calendar-week-day-header" id="weekDay2">周二</div>
                        <div class="layui-calendar-week-day-header" id="weekDay3">周三</div>
                        <div class="layui-calendar-week-day-header" id="weekDay4">周四</div>
                        <div class="layui-calendar-week-day-header" id="weekDay5">周五</div>
                        <div class="layui-calendar-week-day-header" id="weekDay6">周六</div>
                    </div>
                    <div class="layui-calendar-week-content">
                        <!-- 全天任务区域 -->
                        <div class="layui-calendar-week-all-day-label">全天任务</div>
                        <div class="layui-calendar-week-all-day-content" id="weekAllDay0"></div>
                        <div class="layui-calendar-week-all-day-content" id="weekAllDay1"></div>
                        <div class="layui-calendar-week-all-day-content" id="weekAllDay2"></div>
                        <div class="layui-calendar-week-all-day-content" id="weekAllDay3"></div>
                        <div class="layui-calendar-week-all-day-content" id="weekAllDay4"></div>
                        <div class="layui-calendar-week-all-day-content" id="weekAllDay5"></div>
                        <div class="layui-calendar-week-all-day-content" id="weekAllDay6"></div>

                        <div class="layui-calendar-week-time-slots" id="weekTimeSlots"></div>
                        <div class="layui-calendar-week-day-column" id="weekDayColumn0"></div>
                        <div class="layui-calendar-week-day-column" id="weekDayColumn1"></div>
                        <div class="layui-calendar-week-day-column" id="weekDayColumn2"></div>
                        <div class="layui-calendar-week-day-column" id="weekDayColumn3"></div>
                        <div class="layui-calendar-week-day-column" id="weekDayColumn4"></div>
                        <div class="layui-calendar-week-day-column" id="weekDayColumn5"></div>
                        <div class="layui-calendar-week-day-column" id="weekDayColumn6"></div>

                        <!-- 周视图当前时间线 -->
                        <div id="weekCurrentTimeLine" class="layui-calendar-current-time-line" style="display: none;"></div>
                    </div>
                </div>
            </div>

            <!-- 日视图 -->
            <div class="layui-calendar-view-container" id="dayView">
                <div class="layui-calendar-day-view">
                    <div class="layui-calendar-day-header">
                        <div class="layui-calendar-week-time-label">时间</div>
                        <div class="layui-calendar-week-day-header" id="dayTitle">2024年1月1日 周一</div>
                    </div>
                    <div class="layui-calendar-day-content">
                        <!-- 全天任务区域 -->
                        <div class="layui-calendar-day-all-day-label">全天任务</div>
                        <div class="layui-calendar-day-all-day-content" id="dayAllDayTasks"></div>

                        <div class="layui-calendar-day-time-slots" id="dayTimeSlots"></div>
                        <div class="layui-calendar-day-slot-column" id="daySlotColumn"></div>

                        <!-- 日视图当前时间线 -->
                        <div id="dayCurrentTimeLine" class="layui-calendar-current-time-line" style="display: none;"></div>
                    </div>
                </div>
            </div>
            `;

            elem.html(html);

            // 默认选中当前视图按钮
            this.setDefaultView();
        },

        // 绑定事件
        bindEvents: function () {
            var self = this;

            // 视图切换按钮
            $('.layui-calendar-view-buttons button').on('click', function () {
                $('.layui-calendar-view-buttons button').removeClass('layui-btn-normal').addClass('layui-btn-primary');
                $(this).removeClass('layui-btn-primary').addClass('layui-btn-normal');
                var view = $(this).data('view');
                self.setView(view);
                if (self.config.events.viewChange) {
                    self.config.events.viewChange(view);
                }
            });

            // 导航按钮
            $('#prevBtn').on('click', function () {
                self.navigate(-1);
                if (self.config.events.prev) {
                    self.config.events.prev(self.currentDate);
                }
            });

            $('#nextBtn').on('click', function () {
                self.navigate(1);
                if (self.config.events.next) {
                    self.config.events.next(self.currentDate);
                }
            });

            $('#todayBtn').on('click', function () {
                self.setDate(new Date());
                if (self.config.events.today) {
                    self.config.events.today();
                }
            });

            // 添加任务按钮
            $('#addTaskBtn').on('click', function () {
                // 触发外部事件，由外部处理弹窗
                if (self.config.events.dateClick) {
                    var date = new Date(self.currentDate);
                    date.setHours(9, 0, 0); // 默认时间设为上午9点
                    self.config.events.dateClick(date);
                }
            });
        },

        // 设置默认视图
        setDefaultView: function () {
            $('.layui-calendar-view-buttons button[data-view="' + this.currentView + '"]')
                .removeClass('layui-btn-primary')
                .addClass('layui-btn-normal');
        },

        // 开始更新时间线
        startCurrentTimeLineUpdate: function () {
            var self = this;

            if (self.currentTimeLineTimer) {
                clearInterval(self.currentTimeLineTimer);
            }

            self.currentTimeLineTimer = setInterval(function () {
                self.updateCurrentTimeLine();
            }, 1000);

            self.updateCurrentTimeLine();
        },

        // 更新时间线
        updateCurrentTimeLine: function () {
            if (this.currentView === 'week') {
                this.updateWeekCurrentTimeLine();
            } else if (this.currentView === 'day') {
                this.updateDayCurrentTimeLine();
            }
        },

        // 更新周视图当前时间线
        updateWeekCurrentTimeLine: function () {
            var now = new Date();
            var weekStart = this.getWeekStartDate(this.currentDate);
            var weekEnd = new Date(weekStart);
            weekEnd.setDate(weekEnd.getDate() + 7);

            if (now >= weekStart && now <= weekEnd) {
                var hour = now.getHours();
                var minute = now.getMinutes();
                var top = (hour * 61) + minute;

                var allDayAreaHeight = $('.layui-calendar-week-all-day-label').outerHeight() || 0;

                $('#weekCurrentTimeLine').css({
                    display: 'block',
                    top: (allDayAreaHeight + top) + 'px'
                });

                var timeText = this.formatTime(now);
                var timeElement = $('#weekCurrentTimeLine').find('.layui-calendar-current-time-line-text');
                if (timeElement.length === 0) {
                    $('#weekCurrentTimeLine').html('<div class="layui-calendar-current-time-line-text">' + timeText + '</div>');
                } else {
                    timeElement.text(timeText);
                }
            } else {
                $('#weekCurrentTimeLine').hide();
            }
        },

        // 更新日视图当前时间线
        updateDayCurrentTimeLine: function () {
            var now = new Date();
            var viewDate = this.currentDate;

            var isToday = now.getDate() === viewDate.getDate() &&
                now.getMonth() === viewDate.getMonth() &&
                now.getFullYear() === viewDate.getFullYear();

            if (isToday) {
                var hour = now.getHours();
                var minute = now.getMinutes();
                var top = (hour * 61) + minute;

                var allDayAreaHeight = $('.layui-calendar-day-all-day-label').outerHeight() || 0;

                $('#dayCurrentTimeLine').css({
                    display: 'block',
                    top: (allDayAreaHeight + top) + 'px'
                });

                var timeText = this.formatTime(now);
                var timeElement = $('#dayCurrentTimeLine').find('.layui-calendar-current-time-line-text');
                if (timeElement.length === 0) {
                    $('#dayCurrentTimeLine').html('<div class="layui-calendar-current-time-line-text">' + timeText + '</div>');
                } else {
                    timeElement.text(timeText);
                }
            } else {
                $('#dayCurrentTimeLine').hide();
            }
        },

        // 导航
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

            $('.layui-calendar-view-container').removeClass('active');
            $('#' + this.currentView + 'View').addClass('active');

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

            $('#currentDate').text(title);
        },

        // 渲染月视图
        renderMonthView: function () {
            var year = this.currentDate.getFullYear();
            var month = this.currentDate.getMonth();
            var today = new Date();
            today.setHours(0, 0, 0, 0);

            var firstDay = new Date(year, month, 1);
            var firstDayWeek = firstDay.getDay();

            var lastDay = new Date(year, month + 1, 0);
            var daysInMonth = lastDay.getDate();

            $('#monthDays').empty();

            var prevMonthLastDay = new Date(year, month, 0).getDate();
            for (var i = firstDayWeek - 1; i >= 0; i--) {
                var date = new Date(year, month - 1, prevMonthLastDay - i);
                this.addMonthDay(date, true);
            }

            for (var i = 1; i <= daysInMonth; i++) {
                var date = new Date(year, month, i);
                var isToday = date.getDate() === today.getDate() &&
                    date.getMonth() === today.getMonth() &&
                    date.getFullYear() === today.getFullYear();
                this.addMonthDay(date, false, isToday);
            }

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
            var dayElement = $('<div class="layui-calendar-month-day"></div>');

            if (isOtherMonth) {
                dayElement.addClass('other-month');
            }

            if (isToday) {
                dayElement.addClass('today');
            }

            var dayNumber = date.getDate();
            var dayHeader = $('<div class="layui-calendar-month-day-header"><span class="layui-calendar-month-day-number">' + dayNumber + '</span></div>');
            dayElement.append(dayHeader);

            var tasks = this.getTasksForDate(date);
            var dayTasks = tasks.filter(function (task) {
                if (task.allDay) return true;
                if (task.startDate !== task.endDate) return true;
                return false;
            });

            dayTasks.forEach(function (task) {
                var taskElement = self.createTaskElement(task, false);
                taskElement.on('click', function (e) {
                    e.stopPropagation();
                    if (self.config.events.taskClick) {
                        self.config.events.taskClick(task);
                    }
                });
                dayElement.append(taskElement);
            });

            dayElement.on('click', function (e) {
                if (!$(e.target).closest('.layui-calendar-task').length &&
                    !$(e.target).closest('.layui-calendar-month-multi-day-task').length) {
                    if (self.config.events.dateClick) {
                        var clickDate = new Date(date);
                        clickDate.setHours(9, 0, 0);
                        self.config.events.dateClick(clickDate);
                    }
                }
            });

            $('#monthDays').append(dayElement);
        },

        // 渲染周视图
        renderWeekView: function () {
            var self = this;
            var weekStart = this.getWeekStartDate(this.currentDate);
            var weekDates = [];

            for (var i = 0; i < 7; i++) {
                var date = new Date(weekStart);
                date.setDate(date.getDate() + i);
                weekDates.push(date);
            }

            var dayNames = ['周日', '周一', '周二', '周三', '周四', '周五', '周六'];
            weekDates.forEach(function (date, index) {
                var dayName = dayNames[date.getDay()];
                var month = date.getMonth() + 1;
                var day = date.getDate();
                $('#weekDay' + index).html(dayName + '<br>' + month + '/' + day);
            });

            for (var i = 0; i < 7; i++) {
                $('#weekAllDay' + i).empty();
            }

            weekDates.forEach(function (date, index) {
                var tasks = self.getTasksForDate(date);
                var dayTasks = tasks.filter(function (task) {
                    return task.allDay || task.startDate !== task.endDate;
                });

                dayTasks.forEach(function (task) {
                    var taskElement = self.createTaskElement(task);
                    taskElement.addClass('all-day');
                    taskElement.on('click', function (e) {
                        e.stopPropagation();
                        if (self.config.events.taskClick) {
                            self.config.events.taskClick(task);
                        }
                    });
                    $('#weekAllDay' + index).append(taskElement);
                });
            });

            $('#weekTimeSlots').empty();
            for (var hour = 0; hour < 24; hour++) {
                var timeSlot = $('<div class="layui-calendar-week-time-slot">' +
                    (hour < 10 ? '0' + hour : hour) + ':00</div>');
                $('#weekTimeSlots').append(timeSlot);
            }

            for (var i = 0; i < 7; i++) {
                $('#weekDayColumn' + i).empty();
                for (var hour = 0; hour < 24; hour++) {
                    var daySlot = $('<div class="layui-calendar-week-day-slot" data-hour="' + hour + '" data-date="' +
                        self.formatDate(weekDates[i]) + '"></div>');

                    daySlot.on('click', function (e) {
                        if (!$(e.target).closest('.layui-calendar-time-slot-task').length &&
                            !$(e.target).closest('.layui-calendar-week-multi-day-task').length) {
                            var hour = $(this).data('hour');
                            var dateStr = $(this).data('date');
                            var date = new Date(dateStr);
                            date.setHours(hour, 30, 0);
                            if (self.config.events.dateClick) {
                                self.config.events.dateClick(date);
                            }
                        }
                    });

                    $('#weekDayColumn' + i).append(daySlot);
                }
            }

            this.renderWeekTasks(weekDates);
        },

        // 渲染周视图的任务
        renderWeekTasks: function (weekDates) {
            var self = this;

            $('.layui-calendar-time-slot-task, .layui-calendar-week-multi-day-task').remove();

            var weekStart = weekDates[0];
            var weekEnd = weekDates[6];
            weekEnd.setHours(23, 59, 59, 999);

            var weekTasks = this.tasks.filter(function (task) {
                if (task.allDay) return false;
                if (task.startDate !== task.endDate) return false;

                var taskStart = new Date(task.startDate + (task.startTime ? ' ' + task.startTime : ' 00:00'));
                var taskEnd = new Date(task.endDate + (task.endTime ? ' ' + task.endTime : ' 23:59'));

                return (taskStart <= weekEnd && taskEnd >= weekStart);
            });

            weekTasks.sort(function (a, b) {
                var durationA = new Date(a.endDate + ' ' + (a.endTime || '23:59')) -
                    new Date(a.startDate + ' ' + (a.startTime || '00:00'));
                var durationB = new Date(b.endDate + ' ' + (b.endTime || '23:59')) -
                    new Date(b.startDate + ' ' + (b.startTime || '00:00'));
                return durationB - durationA;
            });

            var tasksByDay = {};
            weekDates.forEach(function (date, index) {
                tasksByDay[index] = [];
            });

            weekTasks.forEach(function (task) {
                var taskStart = new Date(task.startDate + ' ' + (task.startTime || '00:00'));
                var taskEnd = new Date(task.endDate + ' ' + (task.endTime || '23:59'));

                var taskDateStr = task.startDate;
                for (var i = 0; i < weekDates.length; i++) {
                    if (self.formatDate(weekDates[i]) === taskDateStr) {
                        tasksByDay[i].push({
                            task: task,
                            start: taskStart,
                            end: taskEnd
                        });
                        break;
                    }
                }
            });

            for (var dayIndex = 0; dayIndex < 7; dayIndex++) {
                var dayTasks = tasksByDay[dayIndex];
                if (dayTasks.length === 0) continue;

                self.layoutAndRenderDayTasks(dayIndex, dayTasks, weekDates);
            }
        },

        // 布局并渲染单天任务
        layoutAndRenderDayTasks: function (dayIndex, dayTasks, weekDates) {
            var self = this;

            dayTasks.sort(function (a, b) {
                return a.start - b.start;
            });

            var columns = [];

            dayTasks.forEach(function (dayTask) {
                var task = dayTask.task;
                var start = dayTask.start;
                var end = dayTask.end;

                var position = self.calculateTaskPosition(start, end);

                var columnIndex = -1;
                for (var i = 0; i < columns.length; i++) {
                    var column = columns[i];
                    var hasConflict = false;

                    for (var j = 0; j < column.length; j++) {
                        var existingTask = column[j];
                        if (!(end <= existingTask.start || start >= existingTask.end)) {
                            hasConflict = true;
                            break;
                        }
                    }

                    if (!hasConflict) {
                        columnIndex = i;
                        break;
                    }
                }

                if (columnIndex === -1) {
                    columnIndex = columns.length;
                    columns[columnIndex] = [];
                }

                columns[columnIndex].push({
                    start: start,
                    end: end,
                    task: task,
                    position: position
                });

                self.renderWeekSingleDayTask(task, start, end, weekDates, columnIndex, columns.length);
            });
        },

        // 计算任务位置
        calculateTaskPosition: function (startTime, endTime) {
            var timeSlotHeight = 60;
            var startHours = startTime.getHours();
            var startMinutes = startTime.getMinutes();
            var endHours = endTime.getHours();
            var endMinutes = endTime.getMinutes();

            var topPosition = (startHours * timeSlotHeight) + startHours + startMinutes;

            var startTotalMinutes = startHours * 60 + startMinutes;
            var endTotalMinutes = endHours * 60 + endMinutes;
            var height = (endTotalMinutes - startTotalMinutes) * (timeSlotHeight / 60);

            if (height < 20) height = 20;

            return {
                top: topPosition,
                height: height
            };
        },

        // 渲染周视图的单天任务
        renderWeekSingleDayTask: function (task, taskStart, taskEnd, weekDates, columnIndex, totalColumns) {
            var self = this;

            var taskDateStr = this.formatDate(taskStart);
            var dayIndex = -1;
            for (var i = 0; i < weekDates.length; i++) {
                if (this.formatDate(weekDates[i]) === taskDateStr) {
                    dayIndex = i;
                    break;
                }
            }

            if (dayIndex === -1) return;

            var position = this.calculateTaskPosition(taskStart, taskEnd);
            var dayColumn = $('#weekDayColumn' + dayIndex);
            var dayColumnWidth = dayColumn.width();

            var fixedIndent = 15;
            var baseTaskWidth = dayColumnWidth - fixedIndent * (totalColumns - 1);

            if (baseTaskWidth < 100) {
                baseTaskWidth = 100;
            }

            var taskWidth = baseTaskWidth - columnIndex * 5;
            var left = columnIndex * fixedIndent;

            var taskElement = $('<div class="layui-calendar-time-slot-task layui-calendar-task-color-' + task.color + '"></div>');
            taskElement.css({
                top: position.top + 'px',
                left: left + 'px',
                width: taskWidth + 'px',
                height: position.height + 'px',
                'z-index': 10 + columnIndex
            });

            var title = $('<div class="task-title">' + task.title + '</div>');
            taskElement.append(title);

            var timeText = this.formatTime(taskStart) + ' - ' + this.formatTime(taskEnd);
            var time = $('<div class="task-time">' + timeText + '</div>');
            taskElement.append(time);

            taskElement.on('click', function (e) {
                e.stopPropagation();
                if (self.config.events.taskClick) {
                    self.config.events.taskClick(task);
                }
            });

            dayColumn.append(taskElement);
        },

        // 渲染日视图
        renderDayView: function () {
            var self = this;
            var date = this.currentDate;

            var dayNames = ['周日', '周一', '周二', '周三', '周四', '周五', '周六'];
            var dayName = dayNames[date.getDay()];
            var year = date.getFullYear();
            var month = date.getMonth() + 1;
            var day = date.getDate();
            $('#dayTitle').html(year + '年' + month + '月' + day + '日 ' + dayName);

            $('#dayAllDayTasks').empty();

            var tasks = this.getTasksForDate(date);
            var allDayTasks = tasks.filter(function (task) {
                return task.allDay || task.startDate !== task.endDate;
            });

            allDayTasks.forEach(function (task) {
                var taskElement = self.createTaskElement(task);
                taskElement.addClass('all-day');
                taskElement.on('click', function (e) {
                    e.stopPropagation();
                    if (self.config.events.taskClick) {
                        self.config.events.taskClick(task);
                    }
                });
                $('#dayAllDayTasks').append(taskElement);
            });

            $('#dayTimeSlots').empty();
            for (var hour = 0; hour < 24; hour++) {
                var timeSlot = $('<div class="layui-calendar-day-time-slot">' +
                    (hour < 10 ? '0' + hour : hour) + ':00</div>');
                $('#dayTimeSlots').append(timeSlot);
            }

            $('#daySlotColumn').empty();
            for (var hour = 0; hour < 24; hour++) {
                var daySlot = $('<div class="layui-calendar-day-slot" data-hour="' + hour + '"></div>');

                daySlot.on('click', function (e) {
                    if (!$(e.target).closest('.layui-calendar-time-slot-task').length) {
                        var hour = $(this).data('hour');
                        var clickDate = new Date(date);
                        clickDate.setHours(hour, 30, 0);
                        if (self.config.events.dateClick) {
                            self.config.events.dateClick(clickDate);
                        }
                    }
                });

                $('#daySlotColumn').append(daySlot);
            }

            this.renderDayTasks(date);
        },

        // 渲染日视图的任务
        renderDayTasks: function (date) {
            var self = this;
            var dateStr = this.formatDate(date);

            $('.layui-calendar-time-slot-task').remove();

            var dayTasks = this.tasks.filter(function (task) {
                if (task.allDay) return false;
                if (task.startDate !== task.endDate) return false;

                var taskStartDate = task.startDate;
                var taskEndDate = task.endDate;

                return (taskStartDate <= dateStr && taskEndDate >= dateStr);
            });

            dayTasks.sort(function (a, b) {
                var durationA = new Date(a.endDate + ' ' + (a.endTime || '23:59')) -
                    new Date(a.startDate + ' ' + (a.startTime || '00:00'));
                var durationB = new Date(b.endDate + ' ' + (b.endTime || '23:59')) -
                    new Date(b.startDate + ' ' + (b.startTime || '00:00'));
                return durationB - durationA;
            });

            var tasksForLayout = [];
            dayTasks.forEach(function (task) {
                var taskStart = new Date(task.startDate + ' ' + task.startTime);
                var taskEnd = new Date(task.endDate + ' ' + task.endTime);

                var dayStart = new Date(date);
                dayStart.setHours(0, 0, 0, 0);

                var dayEnd = new Date(date);
                dayEnd.setHours(23, 59, 59, 999);

                var displayStart = taskStart < dayStart ? dayStart : taskStart;
                var displayEnd = taskEnd > dayEnd ? dayEnd : taskEnd;

                tasksForLayout.push({
                    task: task,
                    start: displayStart,
                    end: displayEnd
                });
            });

            self.layoutAndRenderDayTasksForDayView(tasksForLayout);
        },

        // 布局并渲染日视图任务
        layoutAndRenderDayTasksForDayView: function (tasksForLayout) {
            var self = this;

            tasksForLayout.sort(function (a, b) {
                return a.start - b.start;
            });

            var columns = [];

            tasksForLayout.forEach(function (taskData) {
                var task = taskData.task;
                var start = taskData.start;
                var end = taskData.end;

                var position = self.calculateTaskPosition(start, end);

                var columnIndex = -1;
                for (var i = 0; i < columns.length; i++) {
                    var column = columns[i];
                    var hasConflict = false;

                    for (var j = 0; j < column.length; j++) {
                        var existingTask = column[j];
                        if (!(end <= existingTask.start || start >= existingTask.end)) {
                            hasConflict = true;
                            break;
                        }
                    }

                    if (!hasConflict) {
                        columnIndex = i;
                        break;
                    }
                }

                if (columnIndex === -1) {
                    columnIndex = columns.length;
                    columns[columnIndex] = [];
                }

                columns[columnIndex].push({
                    start: start,
                    end: end,
                    task: task
                });

                self.renderDaySingleTask(task, start, end, columnIndex, columns.length);
            });
        },

        // 渲染日视图的单个任务
        renderDaySingleTask: function (task, displayStart, displayEnd, columnIndex, totalColumns) {
            var self = this;

            var position = this.calculateTaskPosition(displayStart, displayEnd);
            var dayColumn = $('#daySlotColumn');
            var dayColumnWidth = dayColumn.width();

            var fixedIndent = 15;
            var baseTaskWidth = dayColumnWidth - fixedIndent * (totalColumns - 1);

            if (baseTaskWidth < 100) {
                baseTaskWidth = 100;
            }

            var taskWidth = baseTaskWidth - 15 * columnIndex;
            var left = columnIndex * fixedIndent;

            var taskElement = $('<div class="layui-calendar-time-slot-task layui-calendar-task-color-' + task.color + '"></div>');
            taskElement.css({
                top: position.top + 'px',
                left: left + 'px',
                width: taskWidth + 'px',
                height: position.height + 'px',
                'z-index': 10 + columnIndex
            });

            var title = $('<div class="task-title">' + task.title + '</div>');
            taskElement.append(title);

            var timeText = this.formatTime(displayStart) + ' - ' + this.formatTime(displayEnd);
            var time = $('<div class="task-time">' + timeText + '</div>');
            taskElement.append(time);

            taskElement.on('click', function (e) {
                e.stopPropagation();
                if (self.config.events.taskClick) {
                    self.config.events.taskClick(task);
                }
            });

            $('#daySlotColumn').append(taskElement);
        },

        // 创建任务元素
        createTaskElement: function (task, showTime) {
            var taskElement = $('<div class="layui-calendar-task layui-calendar-task-color-' + task.color + '"></div>');

            var title = $('<div class="task-title">' + task.title + '</div>');
            taskElement.append(title);

            if (!task.allDay && task.startDate === task.endDate && showTime !== false) {
                var time = $('<div class="task-time">' + task.startTime + ' - ' + task.endTime + '</div>');
                taskElement.append(time);
            }

            return taskElement;
        },

        // 获取周开始日期
        getWeekStartDate: function (date) {
            var day = date.getDay();
            var weekStart = new Date(date);
            weekStart.setDate(date.getDate() - day);
            weekStart.setHours(0, 0, 0, 0);
            return weekStart;
        },

        // 获取某天的任务
        getTasksForDate: function (date) {
            var dateStr = this.formatDate(date);
            return this.tasks.filter(function (task) {
                var taskDate = task.startDate;
                var taskEndDate = task.endDate;
                return dateStr >= taskDate && dateStr <= taskEndDate;
            });
        },

        // 格式化日期
        formatDate: function (date) {
            var year = date.getFullYear();
            var month = ('0' + (date.getMonth() + 1)).slice(-2);
            var day = ('0' + date.getDate()).slice(-2);
            return year + '-' + month + '-' + day;
        },

        // 格式化时间
        formatTime: function (date) {
            var hours = ('0' + date.getHours()).slice(-2);
            var minutes = ('0' + date.getMinutes()).slice(-2);
            return hours + ':' + minutes;
        },

        // 公开方法：设置日期
        setDate: function (date) {
            this.currentDate = new Date(date);
            this.render();
            return this;
        },

        // 公开方法：设置视图
        setView: function (view) {
            this.currentView = view;
            this.render();
            return this;
        },

        // 公开方法：设置任务
        setTasks: function (tasks) {
            this.tasks = tasks || [];
            this.render();
            return this;
        },

        // 公开方法：添加任务
        addTask: function (task) {
            if (!task.id) {
                task.id = 'task_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
            }
            this.tasks.push(task);
            this.render();
            return this;
        },

        // 公开方法：更新任务
        updateTask: function (task) {
            for (var i = 0; i < this.tasks.length; i++) {
                if (this.tasks[i].id === task.id) {
                    this.tasks[i] = task;
                    break;
                }
            }
            this.render();
            return this;
        },

        // 公开方法：删除任务
        deleteTask: function (taskId) {
            this.tasks = this.tasks.filter(function (task) {
                return task.id !== taskId;
            });
            this.render();
            return this;
        },

        // 公开方法：获取任务
        getTasks: function () {
            return this.tasks;
        },

        // 公开方法：获取当前日期
        getCurrentDate: function () {
            return new Date(this.currentDate);
        },

        // 公开方法：获取当前视图
        getCurrentView: function () {
            return this.currentView;
        },

        // 公开方法：重新渲染
        reload: function () {
            this.render();
            return this;
        },

        // 公开方法：销毁
        destroy: function () {
            if (this.currentTimeLineTimer) {
                clearInterval(this.currentTimeLineTimer);
                this.currentTimeLineTimer = null;
            }

            $(this.config.elem).empty().removeClass('layui-calendar-container');
            return this;
        }
    };

    layui.link(layui.cache.base + 'calendar/calendar.css');

    exports('calendar', function (options) {
        return new Calendar(options);
    });
});