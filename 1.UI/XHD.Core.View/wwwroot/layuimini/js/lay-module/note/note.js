/**
 * 桌面便签组件 for Layui
 * 纯 UI 组件，不包含任何弹窗或业务逻辑
 * 依赖: layui, jquery, layer (仅用于消息提示，可选)
 */

layui.define(['jquery'], function (exports) {
    "use strict";

    var $ = layui.jquery;

    // 加载组件样式
    layui.link(layui.cache.base + 'note/note.css');

    var Note = function (options) {
        this.options = $.extend({
            container: '#note-desktop',       // 桌面容器
            noteWidth: 260,
            noteHeight: 200,
            // 事件回调
            onDelete: null,        // 删除前回调 (id, callback) -> callback(shouldDelete)
            onEdit: null,          // 编辑按钮回调 (id, noteData)
            onMove: null           // 移动结束回调 (id, left, top)
        }, options);
        this.notesData = [];
        this.maxZIndex = 10;
        this.$container = null;
        this.init();
    };

    Note.prototype = {
        constructor: Note,

        init: function () {
            this.$container = $(this.options.container);
            if (!this.$container.length) {
                console.error('容器不存在: ' + this.options.container);
                return;
            }
            this.bindResizeEvent();
        },

        // 设置数据 (全量替换)
        setData: function (data) {
            if (!Array.isArray(data)) data = [];
            this.notesData = data;
            this.updateMaxZIndex();
            this.render();
        },

        // 获取当前数据
        getData: function () {
            return this.notesData;
        },

        // 更新最大 zIndex
        updateMaxZIndex: function () {
            var max = 10;
            this.notesData.forEach(function (note) {
                if (note.zIndex && note.zIndex > max) max = note.zIndex;
            });
            this.maxZIndex = max;
        },

        // 便签置顶
        bringToTop: function (noteId) {
            var index = this.notesData.findIndex(function (n) { return n.id === noteId; });
            if (index !== -1) {
                this.maxZIndex++;
                this.notesData[index].zIndex = this.maxZIndex;
                this.updateNoteElementZIndex(noteId, this.maxZIndex);
                // 可选触发移动回调（位置没变但 zIndex 变了，可不触发，此处为了统一）
                if (this.options.onMove) {
                    this.options.onMove(noteId, this.notesData[index].left, this.notesData[index].top, this.notesData[index].zIndex);
                }
            }
        },

        updateNoteElementZIndex: function (noteId, zIndex) {
            var $note = this.$container.find('.note-card[data-id="' + noteId + '"]');
            if ($note.length) $note.css('z-index', zIndex);
        },

        // 获取随机位置 (供外部生成新便签位置使用)
        getRandomPosition: function () {
            var desktopW = this.$container.width();
            var desktopH = this.$container.height();
            var maxLeft = Math.max(0, desktopW - this.options.noteWidth - 20);
            var maxTop = Math.max(0, desktopH - this.options.noteHeight - 40);
            return {
                left: Math.floor(Math.random() * (maxLeft + 1)),
                top: Math.floor(Math.random() * (maxTop + 1))
            };
        },

        // 渲染所有便签
        render: function () {
            var self = this;
            this.$container.empty();
            this.notesData.forEach(function (note) {
                self.renderOneNote(note);
            });
        },

        // 渲染单个便签
        renderOneNote: function (note) {
            var self = this;
            var $note = $('<div class="note-card" data-id="' + note.id + '"></div>');
            $note.css({
                backgroundColor: note.color,
                left: note.left + 'px',
                top: note.top + 'px',
                zIndex: note.zIndex || 10
            });
            // 使用 layui 图标
            $note.html(`
                <div class="note-header">
                    <div class="note-actions">
                        <i class="layui-icon layui-icon-edit" title="编辑便签"></i>
                        <i class="layui-icon layui-icon-delete" title="删除便签"></i>
                    </div>
                </div>
                <div class="note-content">${this.escapeHtml(note.content)}</div>
            `);

            // 删除按钮点击 -> 触发外部回调
            $note.find('.layui-icon-delete').on('click', function (e) {
                e.stopPropagation();
                if (self.options.onDelete) {
                    self.options.onDelete(note.id, function (shouldDelete) {
                        if (shouldDelete) {
                            self.deleteNoteById(note.id);
                        }
                    });
                } else {
                    // 如果没有回调，默认直接删除（防止无反应）
                    self.deleteNoteById(note.id);
                }
            });

            // 编辑按钮点击 -> 触发外部回调
            $note.find('.layui-icon-edit').on('click', function (e) {
                e.stopPropagation();
                if (self.options.onEdit) {
                    self.options.onEdit(note.id, note);
                } else {
                    layui.layer.msg('未定义编辑回调', { icon: 2 });
                }
            });

            // 点击便签任意区域置顶（按钮区域除外）
            $note.on('mousedown', function (e) {
                if (!$(e.target).closest('.note-actions').length) {
                    self.bringToTop(note.id);
                }
            });

            this.enableDrag($note, note.id);
            this.$container.append($note);
        },

        // 启用拖拽
        enableDrag: function ($note, noteId) {
            var self = this;
            var header = $note.find('.note-header')[0];
            if (!header) return;

            var dragStartX = 0, dragStartY = 0;
            var noteStartLeft = 0, noteStartTop = 0;
            var dragging = false;

            var onMouseDown = function (e) {
                if ($(e.target).closest('.note-actions').length) return;
                e.preventDefault();
                dragging = true;
                dragStartX = e.clientX;
                dragStartY = e.clientY;
                var left = parseInt($note.css('left'));
                var top = parseInt($note.css('top'));
                noteStartLeft = left;
                noteStartTop = top;
                self.bringToTop(noteId);
                document.addEventListener('mousemove', onMouseMove);
                document.addEventListener('mouseup', onMouseUp);
                $note.css('cursor', 'grabbing');
            };

            var onMouseMove = function (e) {
                if (!dragging) return;
                var dx = e.clientX - dragStartX;
                var dy = e.clientY - dragStartY;
                var newLeft = noteStartLeft + dx;
                var newTop = noteStartTop + dy;

                var maxLeft = self.$container.width() - $note.outerWidth();
                var maxTop = self.$container.height() - $note.outerHeight();
                newLeft = Math.min(Math.max(0, newLeft), maxLeft);
                newTop = Math.min(Math.max(0, newTop), maxTop);

                $note.css({ left: newLeft + 'px', top: newTop + 'px' });
            };

            var onMouseUp = function (e) {
                if (!dragging) return;
                dragging = false;
                var finalLeft = parseInt($note.css('left'));
                var finalTop = parseInt($note.css('top'));
                var noteData = self.notesData.find(function (n) { return n.id === noteId; });
                if (noteData && (noteData.left !== finalLeft || noteData.top !== finalTop)) {
                    noteData.left = finalLeft;
                    noteData.top = finalTop;
                    if (self.options.onMove) {
                        self.options.onMove(noteId, finalLeft, finalTop);
                    }
                }
                document.removeEventListener('mousemove', onMouseMove);
                document.removeEventListener('mouseup', onMouseUp);
                $note.css('cursor', '');
            };

            header.addEventListener('mousedown', onMouseDown);
            $note.data('dragCleanup', function () {
                header.removeEventListener('mousedown', onMouseDown);
            });
        },

        // 删除便签（内部方法，不触发回调）
        deleteNoteById: function (id) {
            var idx = this.notesData.findIndex(function (n) { return n.id === id; });
            if (idx !== -1) {
                this.notesData.splice(idx, 1);
                this.render();
            }
        },

        // 外部调用：添加便签
        addNote: function (note) {
            this.notesData.push(note);
            this.updateMaxZIndex();
            this.render();
            setTimeout(function () { this.bringToTop(note.id); }.bind(this), 20);
        },

        // 外部调用：更新便签
        updateNote: function (id, updates) {
            var idx = this.notesData.findIndex(function (n) { return n.id === id; });
            if (idx !== -1) {
                Object.assign(this.notesData[idx], updates);
                this.render();
            }
        },

        // 窗口调整时限制位置
        clampAllPositions: function () {
            var self = this;
            var maxWidth = this.$container.width();
            var maxHeight = this.$container.height();
            var changed = false;
            this.notesData.forEach(function (note) {
                var newLeft = note.left;
                var newTop = note.top;
                if (newLeft + self.options.noteWidth > maxWidth) {
                    newLeft = Math.max(0, maxWidth - self.options.noteWidth - 10);
                }
                if (newTop + self.options.noteHeight > maxHeight) {
                    newTop = Math.max(0, maxHeight - self.options.noteHeight - 10);
                }
                if (newLeft !== note.left || newTop !== note.top) {
                    note.left = newLeft;
                    note.top = newTop;
                    changed = true;
                    if (self.options.onMove) self.options.onMove(note.id, newLeft, newTop);
                }
            });
            if (changed) this.render();
        },

        bindResizeEvent: function () {
            var self = this;
            var timer = null;
            $(window).on('resize', function () {
                if (timer) clearTimeout(timer);
                timer = setTimeout(function () {
                    self.clampAllPositions();
                }, 200);
            });
        },

        escapeHtml: function (str) {
            if (!str) return '';
            return str.replace(/[&<>]/g, function (m) {
                if (m === '&') return '&amp;';
                if (m === '<') return '&lt;';
                if (m === '>') return '&gt;';
                return m;
            });
        }
    };

    exports('note', Note);
});