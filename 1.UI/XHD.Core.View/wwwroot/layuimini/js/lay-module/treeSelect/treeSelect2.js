// Layui树形下拉组件（新增“请选择”清除项）
layui.define(['tree', 'layer', 'jquery'], function (exports) {
    var tree = layui.tree;
    var layer = layui.layer;
    var $ = layui.jquery;

    var treeSelect = {
        // 默认配置
        config: {
            inputId: '',           // 绑定的input ID（必传）
            data: [],              // 本地数据（优先级 > url）
            url: '',               // AJAX地址（无data时生效）
            ajaxMethod: 'GET',     // AJAX方法
            ajaxParams: {},        // AJAX参数
            ajaxDataKey: 'data',   // 接口数据字段
            initValue: '',         // 初始选中ID（空则选中“请选择”）
            popupTitle: '选择节点',// 弹窗标题
            showCheckbox: true,    // 显示复选框
            checkStrictly: true    // 父子选中不关联
        },
        // 实例缓存（多实例隔离）
        instances: {},
        // 数据加载状态（key=inputId）
        loadStatus: {}, // 'loading'/'loaded'/'error'

        // -------------------------- 私有工具方法 --------------------------
        // 1. 同步构建前端UI（优先执行，无延迟）
        _buildUI: function (opts) {
            const inputId = opts.inputId;
            const $input = $('#' + inputId);

            // 避免重复构建UI
            if ($input.parent('.tree-select').length) return;

            // 构建容器+按钮+隐藏域（同步渲染）
            const $container = $('<div class="tree-select"></div>');
            const $hidden = $('<input type="hidden" id="' + inputId + '_id" value="">');
            const $btn = $('<button type="button" class="layui-btn tree-select-btn">选择</button>');

            // 重组DOM
            $input.wrap($container).addClass('tree-select-input');
            $input.after($hidden).after($btn);

            // 初始按钮禁用/提示
            $btn.attr('disabled', true).text('加载中...');
            $input.attr('placeholder', '数据加载中...');
        },

        // 2. 递归查找节点
        _findNode: function (treeData, targetId) {
            for (let i = 0; i < treeData.length; i++) {
                const node = treeData[i];
                if (node.id == targetId) return node;
                if (node.children && node.children.length > 0) {
                    const res = this._findNode(node.children, targetId);
                    if (res) return res;
                }
            }
            return null;
        },

        // 3. 递归设置节点选中状态
        _setNodeChecked: function (treeData, targetId) {
            treeData.forEach(node => {
                node.checked = (node.id === targetId);
                if (node.children && node.children.length > 0) {
                    this._setNodeChecked(node.children, targetId);
                }
            });
        },

        // 4. 异步加载数据（data/url二选一）
        _loadData: function (opts) {
            const inputId = opts.inputId;
            this.loadStatus[inputId] = 'loading';

            // 1. 优先使用本地data
            if (opts.data && opts.data.length > 0) {
                this.loadStatus[inputId] = 'loaded';
                // 插入“请选择”节点（核心修改1）
                const newData = [{ id: '', title: '请选择' }].concat(opts.data);
                return Promise.resolve(newData);
            }

            // 2. 无data则AJAX加载
            if (!opts.url) {
                this.loadStatus[inputId] = 'error';
                layer.msg('请配置data或url', { icon: 2 });
                return Promise.reject('缺少数据来源');
            }

            // AJAX请求
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: opts.url,
                    method: opts.ajaxMethod,
                    data: opts.ajaxParams,
                    dataType: 'json',
                    success: (res) => {
                        const treeData = res[opts.ajaxDataKey] || [];
                        this.loadStatus[inputId] = 'loaded';
                        // 插入“请选择”节点（核心修改1）
                        const newData = [{ id: '', title: '请选择' }].concat(treeData);
                        resolve(newData);
                    },
                    error: (xhr, msg) => {
                        this.loadStatus[inputId] = 'error';
                        reject(`数据加载失败：${msg}`);
                        layer.msg(`数据加载失败：${msg}`, { icon: 2 });
                    }
                });
            });
        },

        // 5. 数据加载完成后，绑定事件+回显初始值
        _bindData: function (opts, treeData) {
            const inputId = opts.inputId;
            const $input = $('#' + inputId);
            const $hidden = $('#' + inputId + '_id');
            const $btn = $input.siblings('.tree-select-btn');

            // 恢复按钮状态
            $btn.attr('disabled', false).text('选择');
            $input.attr('placeholder', '请选择');

            // 回显初始值（兼容“请选择”节点）
            if (opts.initValue === '') {
                // 初始值为空 → 选中“请选择”
                $input.val('请选择');
                $hidden.val('');
            } else if (opts.initValue) {
                const initNode = this._findNode(treeData, opts.initValue);
                if (initNode) {
                    $input.val(initNode.title);
                    $hidden.val(initNode.id);
                } else {
                    // 初始值无效 → 选中“请选择”
                    $input.val('请选择');
                    $hidden.val('');
                }
            }

            // 绑定选择按钮事件
            $btn.off('click').on('click', () => {
                if (this.loadStatus[inputId] === 'loading') {
                    layer.msg('数据加载中，请稍后', { icon: 3 });
                    return;
                }
                if (this.loadStatus[inputId] === 'error') {
                    layer.msg('数据加载失败，请刷新', { icon: 2 });
                    return;
                }

                const dataCopy = JSON.parse(JSON.stringify(treeData));
                // 选中状态回显（兼容“请选择”）
                if ($hidden.val()) {
                    this._setNodeChecked(dataCopy, $hidden.val());
                } else {
                    this._setNodeChecked(dataCopy, ''); // 选中“请选择”
                }

                // 弹出树形弹窗
                layer.open({
                    type: 1,
                    title: opts.popupTitle,
                    area: ['320px', '450px'],
                    shade: 0.1,
                    content: `<div class="tree-popup" id="treePopup_${inputId}"></div>`,
                    success: () => {
                        tree.render({
                            elem: `#treePopup_${inputId}`,
                            data: dataCopy,
                            showCheckbox: opts.showCheckbox,
                            checkStrictly: opts.checkStrictly,
                            click: (obj) => {
                                // 核心修改2：选择“请选择”时清空值
                                if (obj.data.id === '') {
                                    $input.val('');
                                    $hidden.val('');
                                } else {
                                    $input.val(obj.data.title);
                                    $hidden.val(obj.data.id);
                                }
                                layer.closeAll();
                            }
                        });
                    }
                });
            });

            // 构建实例方法
            const instance = {
                getValue: () => ({ id: $hidden.val(), title: $input.val() }),
                reset: () => {
                    // 重置 → 选中“请选择”
                    $input.val('请选择');
                    $hidden.val('');
                },
                setInitValue: (targetId) => {
                    if (targetId === '') {
                        // 设置为空 → 选中“请选择”
                        $input.val('请选择');
                        $hidden.val('');
                        layer.msg('已设置为：请选择', { icon: 1 });
                        return;
                    }
                    const node = this._findNode(treeData, targetId);
                    if (node) {
                        $input.val(node.title);
                        $hidden.val(node.id);
                        layer.msg(`已设置初始值：${node.title}`, { icon: 1 });
                    } else {
                        // 节点不存在 → 选中“请选择”
                        $input.val('请选择');
                        $hidden.val('');
                        layer.msg('节点不存在，已重置为“请选择”', { icon: 2 });
                    }
                },
                refresh: () => {
                    $btn.attr('disabled', true).text('加载中...');
                    $input.val('').attr('placeholder', '数据加载中...');
                    $hidden.val('');
                    return this.init(opts);
                },
                getRawData: () => treeData
            };

            this.instances[inputId] = instance;
            return instance;
        },

        // -------------------------- 对外暴露方法 --------------------------
        init: function (options) {
            const opts = $.extend(true, {}, this.config, options);
            if (!opts.inputId) {
                layer.msg('inputId不能为空', { icon: 2 });
                return Promise.reject('inputId为空');
            }

            // 先构建UI，后加载数据
            this._buildUI(opts);
            return this._loadData(opts).then((treeData) => {
                return this._bindData(opts, treeData);
            });
        },

        getInstance: function (inputId) {
            return this.instances[inputId] || null;
        }
    };

    layui.link(layui.cache.base + 'treeSelect/treeSelect.css');

    exports('treeSelect', treeSelect);
});