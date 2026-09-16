// 封装Layui树形下拉组件（简化DOM版）
layui.define(['tree', 'layer', 'jquery'], function (exports) {
    var tree = layui.tree;
    var layer = layui.layer;
    var $ = layui.jquery;

    // 组件核心对象
    var treeSelect = {
        // 默认配置
        config: {
            inputId: '',           // 核心input的ID（必传）
            treeData: [],          // 树形数据（必传）
            initValue: '',         // 初始选中ID
            popupTitle: '选择节点',// 弹窗标题
            showCheckbox: true,    // 是否显示复选框
            checkStrictly: true,   // 父子节点是否关联选中
            onlyIconControl: false // 是否仅图标控制展开/折叠
        },
        // 存储实例（避免重复初始化）
        instances: {},

        // -------------------------- 私有工具函数 --------------------------
        // 递归查找节点
        _findTreeNode: function (treeData, targetId) {
            for (var i = 0; i < treeData.length; i++) {
                var node = treeData[i];
                if (node.id == targetId) return node;
                if (node.children && node.children.length > 0) {
                    var res = this._findTreeNode(node.children, targetId);
                    if (res) return res;
                }
            }
            return null;
        },

        // 递归设置节点选中状态
        _setTreeChecked: function (treeData, targetId) {
            for (var i = 0; i < treeData.length; i++) {
                var node = treeData[i];
                node.checked = (node.id == targetId);
                if (node.children && node.children.length > 0) {
                    this._setTreeChecked(node.children, targetId);
                }
            }
        },

        // 构建组件DOM结构（自动生成容器、按钮、隐藏域）
        _buildDom: function (inputId) {
            var $input = $('#' + inputId);
            // 避免重复构建
            if ($input.parent('.tree-select').length > 0) return;

            // 构建容器结构
            var container = $('<div class="tree-select"></div>');
            // 生成隐藏域（ID规则：inputId + "_id"）
            var hiddenInput = $('<input type="hidden" id="' + inputId + '_id" value="">');
            // 生成选择按钮
            var selectBtn = $('<button type="button" class="layui-btn tree-select-btn">选择</button>');

            // 重组DOM：input -> 容器包裹，追加按钮 + 隐藏域
            $input.wrap(container);
            $input.after(selectBtn).after(hiddenInput);
            // 给input添加样式类
            $input.addClass('tree-select-input');
        },

        // -------------------------- 核心方法 --------------------------
        /**
         * 初始化组件
         * @param {Object} options 配置项（覆盖默认配置）
         * @returns {Object} 组件实例
         */
        init: function (options) {
            // 1. 合并配置
            var opts = $.extend({}, this.config, options);
            if (!opts.inputId) {
                layer.msg('input的ID不能为空', { icon: 2 });
                return null;
            }
            if (this.instances[opts.inputId]) {
                layer.msg('该input已初始化树形下拉组件', { icon: 3 });
                return this.instances[opts.inputId];
            }

            // 2. 构建DOM结构（自动生成容器、按钮、隐藏域）
            this._buildDom(opts.inputId);

            // 3. 获取核心DOM元素
            var $input = $('#' + opts.inputId); // 显示名称的input
            var $hidden = $('#' + opts.inputId + '_id'); // 存储ID的隐藏域
            var $btn = $input.siblings('.tree-select-btn'); // 选择按钮

            // 4. 初始化值回显
            if (opts.initValue) {
                var initNode = this._findTreeNode(opts.treeData, opts.initValue);
                if (initNode) {
                    $input.val(initNode.title); // 回显名称
                    $hidden.val(initNode.id);   // 回显ID
                }
            }

            // 5. 绑定按钮点击事件
            $btn.off('click').on('click', function () {
                // 深拷贝数据，避免修改原数据的checked状态
                var treeDataCopy = JSON.parse(JSON.stringify(opts.treeData));
                if ($hidden.val()) {
                    treeSelect._setTreeChecked(treeDataCopy, $hidden.val());
                }

                // 弹出选择弹窗
                layer.open({
                    type: 1,
                    title: opts.popupTitle,
                    area: ['320px', '450px'],
                    shade: 0.1,
                    offset: '10px',
                    content: '<div class="tree-popup" id="treePopup_' + opts.inputId + '"></div>',
                    success: function () {
                        // 渲染树形结构
                        tree.render({
                            elem: '#treePopup_' + opts.inputId,
                            data: treeDataCopy,
                            showCheckbox: opts.showCheckbox,
                            onlyIconControl: opts.onlyIconControl,
                            checkStrictly: opts.checkStrictly,
                            click: function (obj) {
                                // 回填值并关闭弹窗
                                $input.val(obj.data.title);
                                $hidden.val(obj.data.id);
                                layer.closeAll();
                            }
                        });
                    }
                });
            });

            // 6. 存储实例（包含常用方法）
            var instance = {
                // 获取选中值（返回{id, title}）
                getValue: function () {
                    return {
                        id: $hidden.val(),
                        title: $input.val()
                    };
                },
                // 重置组件
                reset: function () {
                    $input.val('');
                    $hidden.val('');
                },
                // 手动设置值
                setValue: function (targetId) {
                    var node = treeSelect._findTreeNode(opts.treeData, targetId);
                    if (node) {
                        $input.val(node.title);
                        $hidden.val(node.id);
                    } else {
                        layer.msg('未找到指定ID的节点', { icon: 2 });
                    }
                }
            };

            this.instances[opts.inputId] = instance;
            return instance;
        },

        /**
         * 获取已初始化的实例
         * @param {String} inputId input的ID
         * @returns {Object|null} 实例对象
         */
        getInstance: function (inputId) {
            return this.instances[inputId] || null;
        }
    };


    layui.link(layui.cache.base + 'treeSelect/treeSelect.css');

    // 暴露组件
    exports('treeSelect', treeSelect);
});