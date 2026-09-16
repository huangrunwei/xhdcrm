layui.define(['form', 'jquery'], function (exports) {
    var form = layui.form;
    var $ = layui.jquery;

    var Select = function () {
        this.config = {
            elem: '',          // 目标input的ID（必填）
            data: [],          // 本地数据源 [{value: '', text: ''}]
            ajax: {},          // Ajax配置 {url: '', method: '', data: {}}
            value: '',         // 默认选中值
            placeholder: '请选择', // 空选项文本
            success: null,     // 渲染成功回调 (val, text) => {}
            change: null       // 选中变化回调 (val, text) => {}
        };
    };

    // 配置参数
    Select.prototype.set = function (options) {
        $.extend(true, this.config, options);
        return this;
    };

    // 核心渲染方法（重点修复样式依赖）
    Select.prototype.render = function () {
        var that = this;
        var cfg = that.config;

        // 校验必传参数
        if (!cfg.elem) {
            console.error('[LayuiSelect] 请传入目标input的ID！');
            return that;
        }
        var $input = $('#' + cfg.elem);
        if (!$input.length) {
            console.error('[LayuiSelect] 未找到ID为 ' + cfg.elem + ' 的input元素！');
            return that;
        }

        // 1. 移除旧的select（防重复渲染）
        var selectId = 'layui-select-' + cfg.elem;
        $('#' + selectId).closest('.layui-form').remove();

        // 2. 核心：创建带layui-form的容器（Layui样式必须依赖该类）
        // 容器结构完全对齐Layui原生表单结构，确保样式继承
        var $formWrap = $('<div class="layui-form" style="margin:0;padding:0;"></div>'); // 核心类：layui-form
        var $formItem = $('<div class="layui-form-item" style="margin-bottom:0;"></div>');
        var $selectWrap = $('<div class="layui-input-inline" style="width:100%;"></div>'); // 保持宽度和原input一致
        var $select = $('<select id="' + selectId + '" lay-filter="select-' + cfg.elem + '" class="layui-select"></select>');

        // 3. 组装容器（严格对齐Layui原生结构）
        $selectWrap.append($select);
        $formItem.append($selectWrap);
        $formWrap.append($formItem);
        $input.after($formWrap); // 插入到input后方
        $input.hide(); // 隐藏原input

        // 渲染下拉选项
        var renderOptions = function (dataList) {
            $select.empty();
            // 添加默认空选项
            $select.append('<option value="">' + cfg.placeholder + '</option>');
            // 渲染数据选项
            if (Array.isArray(dataList) && dataList.length) {
                $.each(dataList, function (i, item) {
                    var selected = item.value === cfg.value ? 'selected' : '';
                    $select.append('<option value="' + item.value + '" ' + selected + '>' + item.text + '</option>');
                });
            }

            // 4. 强制渲染Layui样式（关键：必须调用form.render，且指定select）
            form.render('select', $formWrap.attr('lay-filter'));

            // 同步值到原input
            var val = $select.val();
            var text = $select.find('option:selected').text();
            $input.val(text).attr('data-value', val);

            // 成功回调
            if (typeof cfg.success === 'function') {
                cfg.success.call(that, val, text);
            }
        };

        // 监听选中变化
        form.on('select(select-' + cfg.elem + ')', function (data) {
            var text = data.elem[data.elem.selectedIndex].text;
            $input.val(text).attr('data-value', data.value).trigger('change');
            // 变化回调
            if (typeof cfg.change === 'function') {
                cfg.change.call(that, data.value, text);
            }
        });

        // 加载数据
        if (cfg.data && cfg.data.length) {
            renderOptions(cfg.data);
        } else if (cfg.ajax && cfg.ajax.url) {
            $.ajax({
                url: cfg.ajax.url,
                method: cfg.ajax.method || 'GET',
                data: cfg.ajax.data || {},
                dataType: 'json',
                beforeSend: function () {
                    $input.val('加载中...').attr('data-value', '');
                },
                success: function (res) {
                    if (res.code === 0 && Array.isArray(res.data)) {
                        renderOptions(res.data);
                    } else {
                        console.error('[LayuiSelect] Ajax返回格式错误，需满足 {code:0, data:[]}');
                        renderOptions([]);
                    }
                },
                error: function () {
                    $input.val('加载失败').attr('data-value', '');
                    renderOptions([]);
                }
            });
        } else {
            renderOptions([]);
        }

        return that;
    };

    // 获取选中值
    Select.prototype.getValue = function () {
        var $input = $('#' + this.config.elem);
        return {
            value: $input.attr('data-value') || '',
            text: $input.val() || ''
        };
    };

    // 更新数据
    Select.prototype.updateData = function (data, value) {
        this.config.data = data;
        if (value !== undefined) this.config.value = value;
        this.render();
        return this;
    };

    // 原生select渲染（仅用于对比）
    form.render('select');

    exports('layuiSelect', new Select());
});