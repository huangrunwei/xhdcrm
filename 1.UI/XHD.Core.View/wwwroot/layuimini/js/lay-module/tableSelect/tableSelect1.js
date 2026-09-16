layui.define(['table', 'layer', 'jquery', 'form'], function (exports) {
    var table = layui.table;
    var layer = layui.layer;
    var $ = layui.jquery;
    var form = layui.form;

    var tableSelect = {
        config: {
            inputId: '',           // 绑定input ID
            data: [],              // 本地数据
            url: '',               // 接口地址
            ajaxMethod: 'GET',     // 请求方法
            ajaxParams: {},        // 请求参数
            ajaxDataKey: 'data',   // 接口数据字段
            initValue: '',         // 初始值
            popupTitle: '选择数据',// 弹窗标题
            columns: [],           // 表格列配置
            valueKey: 'id',        // value列名
            labelKey: 'name',      // 显示文本列名
            searchable: true,      // 是否显示搜索
            searchPlaceholder: '请输入关键词搜索',
            popupArea: ['600px', '450px'] // 弹窗尺寸
        },
        instances: {},      // 实例缓存
        loadStatus: {},     // 加载状态
        tableIns: {},       // 表格实例
        selectRow: {},      // 选中行缓存（核心：存储完整行数据）
        rawData: {},        // 原始数据缓存
        popupIndex: {}      // 弹窗索引

        // 构建输入框UI
        , _buildUI: function (opts) {
            var inputId = opts.inputId;
            var $input = $('#' + inputId);
            if ($input.parent('.table-select').length) return;

            // 容器+隐藏域
            var $container = $('<div class="table-select"></div>');
            var $hidden = $('<input type="hidden" id="' + inputId + '_val" value="">');
            $input.wrap($container).addClass('table-select-input').after($hidden);

            // 样式
            $input.attr('placeholder', '加载中...').css({
                cursor: 'pointer',
                background: '#f9f9f9',
                border: '1px solid #e6e6e6',
                borderRadius: '2px',
                padding: '0 30px 0 10px',
                height: '38px',
                lineHeight: '38px',
                width: '100%',
                boxSizing: 'border-box'
            });

            // 下拉箭头
            $container.css({
                position: 'relative',
                width: '300px'
            }).after('<span class="table-select-arrow" style="position:absolute;right:10px;top:50%;transform:translateY(-50%);font-size:12px;color:#999;pointer-events:none;">▼</span>');
        }

        // 加载数据
        , _loadData: function (opts) {
            var that = this;
            var inputId = opts.inputId;
            that.loadStatus[inputId] = 'loading';

            return new Promise(function (resolve, reject) {
                // 本地数据
                if (opts.data && opts.data.length > 0) {
                    that.rawData[inputId] = opts.data;
                    that.loadStatus[inputId] = 'loaded';
                    resolve(opts.data);
                    return;
                }

                // 接口数据
                if (!opts.url) {
                    layer.msg('请配置data或url', { icon: 2 });
                    reject('无数据来源');
                    return;
                }

                $.ajax({
                    url: opts.url,
                    type: opts.ajaxMethod,
                    data: opts.ajaxParams,
                    dataType: 'json',
                    success: function (res) {
                        var data = res[opts.ajaxDataKey] || [];
                        that.rawData[inputId] = data;
                        that.loadStatus[inputId] = 'loaded';
                        resolve(data);
                    },
                    error: function () {
                        that.loadStatus[inputId] = 'error';
                        layer.msg('接口请求失败', { icon: 2 });
                        reject('接口失败');
                    }
                });
            });
        }

        // 绑定事件
        , _bindEvent: function (opts, data) {
            var that = this;
            var inputId = opts.inputId;
            var $input = $('#' + inputId);
            var $hidden = $('#' + inputId + '_val');

            // 初始化值
            if (opts.initValue) {
                that._setSelected(opts, inputId, opts.initValue);
            }

            // 恢复输入框样式
            $input.attr('placeholder', '请选择').css({ background: '#fff' });

            // 点击输入框弹窗
            $input.off('click').on('click', function (e) {
                e.preventDefault();
                if (that.loadStatus[inputId] === 'loading') {
                    layer.msg('数据加载中...', { icon: 3 });
                    return;
                }
                if (that.loadStatus[inputId] === 'error') {
                    layer.msg('数据加载失败', { icon: 2 });
                    return;
                }

                // 弹窗内容
                var html = `<div class="table-select-wrap" style="padding:10px;">
                    ${opts.searchable ? `<div class="layui-form-item" style="margin-bottom:10px;">
                        <input type="text" id="search_${inputId}" class="layui-input" placeholder="${opts.searchPlaceholder}">
                    </div>` : ''}
                    <div style="height:350px;overflow:auto;">
                        <table id="table_${inputId}" lay-filter="tableFilter_${inputId}"></table>
                    </div>
                </div>`;

                // 打开弹窗（核心：使用layer内置按钮）
                that.popupIndex[inputId] = layer.open({
                    title: opts.popupTitle,
                    type: 1,
                    anim: 1,
                    area: opts.popupArea,
                    maxmin: true,
                    content: html,
                    btn: ['确定', '关闭'],
                    success: function (layero, index) {
                        // 渲染表格
                        that._renderTable(opts, inputId);

                        // 搜索事件
                        if (opts.searchable) {
                            $('#search_' + inputId).off('input').on('input', function () {
                                var keyword = $(this).val().trim().toLowerCase();
                                var filterData = that.rawData[inputId].filter(function (item) {
                                    return Object.values(item).some(function (val) {
                                        return val.toString().toLowerCase().includes(keyword);
                                    });
                                });
                                // 重载表格
                                table.reload('table_' + inputId, { data: filterData });
                                // 重新选中
                                setTimeout(function () {
                                    that._checkRadio(opts, inputId);
                                }, 200);
                            });
                        }
                    },
                    yes: function (index, layero) { // 确定按钮
                        var selected = that.selectRow[inputId];
                        if (!selected) {
                            layer.msg('请选择数据', { icon: 2 });
                            return;
                        }
                        // 回显值
                        $input.val(selected[opts.labelKey]);
                        $hidden.val(selected[opts.valueKey]);
                        layer.close(index);
                        layer.msg('选择成功', { icon: 1 });
                    },
                    btn2: function (index) { // 关闭按钮
                        layer.close(index);
                    }
                });
            });

            // 实例方法
            that.instances[inputId] = {
                getValue: function () {
                    return {
                        value: $hidden.val(),
                        text: $input.val()
                    };
                },
                reset: function () {
                    $input.val('');
                    $hidden.val('');
                    that.selectRow[inputId] = null;
                },
                setValue: function (val) { // 对外暴露的设置值方法
                    that._setSelected(opts, inputId, val);
                    // 如果弹窗已打开，重新选中
                    if (that.popupIndex[inputId]) {
                        setTimeout(function () {
                            that._checkRadio(opts, inputId);
                        }, 200);
                    }
                },
                refresh: function () {
                    $input.val('').attr('placeholder', '加载中...').css({ background: '#f9f9f9' });
                    $hidden.val('');
                    that.selectRow[inputId] = null;
                    return that.init(opts);
                }
            };

            return that.instances[inputId];
        }

        // 渲染表格（核心修复：Radio选中）
        , _renderTable: function (opts, inputId) {
            var that = this;
            var tableId = 'table_' + inputId;

            // 表格列配置（强制单选）
            var cols = [[
                { type: 'radio', width: 60 },
                ...opts.columns
            ]];

            // 渲染表格
            that.tableIns[inputId] = table.render({
                elem: '#' + tableId,
                data: that.rawData[inputId],
                cols: cols,
                page: false,
                limit: 999,
                done: function (res, curr, count) {
                    // 表格渲染完成后，强制选中指定行
                    setTimeout(function () {
                        that._checkRadio(opts, inputId);
                    }, 300); // 延长延迟，确保DOM完全加载
                }
            });

            // 监听单选事件
            table.on('radio(tableFilter_' + inputId + ')', function (obj) {
                that.selectRow[inputId] = obj.data;
            });
        }

        // 核心修复：强制选中Radio（解决选中不生效问题）
        , _checkRadio: function (opts, inputId) {
            var that = this;
            var selected = that.selectRow[inputId];
            if (!selected) return;

            var tableId = 'table_' + inputId;
            var valueKey = opts.valueKey;
            var targetValue = selected[valueKey].toString();

            // 方式1：直接操作DOM（最可靠）
            $('#' + tableId).find('tr').each(function () {
                var $radio = $(this).find('input[type="radio"]');
                if ($radio.val() === targetValue) {
                    $radio.prop('checked', true);
                    $(this).addClass('layui-table-click'); // 选中行高亮
                } else {
                    $radio.prop('checked', false);
                    $(this).removeClass('layui-table-click');
                }
            });

            // 方式2：强制刷新form（双保险）
            form.render('radio');

            // 方式3：通过Layui表格API选中（备选方案）
            var tableIns = that.tableIns[inputId];
            if (tableIns) {
                tableIns.setRowChecked({
                    index: that.rawData[inputId].findIndex(function (item) {
                        return item[valueKey].toString() === targetValue;
                    }),
                    checked: true
                });
            }
        }

        // 设置选中值（同步缓存+输入框+表格）
        , _setSelected: function (opts, inputId, targetValue) {
            var that = this;
            var $input = $('#' + inputId);
            var $hidden = $('#' + inputId + '_val');
            var data = that.rawData[inputId];

            // 查找匹配数据
            var selected = data.find(function (item) {
                return item[opts.valueKey].toString() === targetValue.toString();
            });

            if (selected) {
                // 同步缓存
                that.selectRow[inputId] = selected;
                // 同步输入框和隐藏域
                $input.val(selected[opts.labelKey]);
                $hidden.val(selected[opts.valueKey]);
            } else {
                that.selectRow[inputId] = null;
                $input.val('');
                $hidden.val('');
                layer.msg('值不存在', { icon: 2 });
            }
        }

        // 初始化入口
        , init: function (options) {
            var that = this;
            var opts = $.extend(true, {}, that.config, options);

            // 必传参数校验
            if (!opts.inputId) {
                layer.msg('inputId不能为空', { icon: 2 });
                return Promise.reject('inputId为空');
            }
            if (!opts.columns || opts.columns.length === 0) {
                layer.msg('columns不能为空', { icon: 2 });
                return Promise.reject('columns为空');
            }

            // 构建UI → 加载数据 → 绑定事件
            that._buildUI(opts);
            return that._loadData(opts).then(function (data) {
                return that._bindEvent(opts, data);
            });
        },

        // 获取实例
        getInstance: function (inputId) {
            return this.instances[inputId] || null;
        }
    };

    layui.link(layui.cache.base + 'tableSelect/tableSelect.css');

    exports('tableSelect', tableSelect);
});