/**
 * @name numberInput
 * @author HuangJunjie
 * @description layui 数值输入框扩展
 * @version 1.4.20210609
 */
layui.define(["jquery"], function (exports) {
    var $ = layui.jquery;
    var prefix = "number-input";

    /**
     * 链接外部样式文件
     * 由 码云 @hulangfy 同学建议
     */
    layui.link(layui.cache.base + 'numberInput/css/theme.css');

    /**
     * 创建样式列表
     * @param names
     * @returns {string}
     */
    function createClassList(flag, names) {
        if (typeof names == "string") names = [names];

        var classList = [];

        names.forEach(function (name) {
            classList.push([prefix, flag].join("-"));
            classList.push([flag, name].join("-"));
        });

        return classList.join(" ");
    }

    /**
     * 精度数字
     * @param value
     * @param precision
     */
    function radioNumber(value, precision) {
        if (precision === undefined) precision = 0;
        var floatVal = parseFloat(value) || 0;
        var radio = Math.pow(10, precision);

        return ((floatVal * radio) / radio).toFixed(precision);
    }

    /**
     * 初始化操作
     * @param id
     * @param opts
     */
    function init(id, opts) {
        if (!opts) opts = {};

        // 取出配置
        var max = parseFloat(opts.max) || 19961005;
        var min = parseFloat(opts.min) || 0;
        var precision = parseInt(opts.precision) || 0;
        var radio = Math.pow(10, precision);
        var step = opts.step ? parseFloat(opts.step) : ((parseFloat(opts.step) || 1) / radio);
        var skin = opts.skin || "default";
        var iconAdd = opts.iconAdd || "layui-icon layui-icon-addition";
        var iconSubtract = opts.iconSubtract || "layui-icon layui-icon-subtraction";
        var width = opts.width || 100;
        var allowEmpty = opts.allowEmpty || false;
        var autoSelect = opts.autoSelect || false;

        // 激活指定事件
        function activeEvent(name, event, dom, value, tree) {
            if (opts.event && typeof opts.event[name] == "function") {
                return opts.event[name](event, dom, value, tree);
            } else {
                return false;
            }
        }

        // 开始组装DOM
        activeEvent("beforeCreated");

        // 绑定元素
        var $input = $(id);
        var $target = $(id).parent();
        var $container = $('<div class="' + createClassList("container", skin) + '"></div>');
        var $subtract = $('<div class="' + createClassList("subtract", skin) + '"></div>').append($('<i class="' + iconSubtract + '"></i>'));
        var $add = $('<div class="' + createClassList("add", skin) + '"></div>').append($('<i class="' + iconAdd + '"></i>'));

        // DOM集合
        var $dom = {
            input: $input,
            subtract: $subtract,
            add: $add,
            container: $container,
        };

        // 初始化默认值
        var defaultValue = radioNumber(opts.value || $input.attr("value") || 0, precision);

        // 自动全选
        $input.on("focus", function (event) {
            activeEvent("focus", event, $input, $input.val(), $dom);
            if (autoSelect) $input.select()
        });
        $input.on("select", function (event) {
            activeEvent("select", event, $input, $input.val(), $dom);
        });

        // 处理逻辑
        $input
            .val(defaultValue)
            .css({
                width: width,
            })
            .on("input", function (event) {
                var val = $(this).val();
                var newVal = val;

                // 如果精度大于0
                if (precision > 0) {
                    newVal = val.replace(/[^\d\-\.]/g, "");
                    newVal = newVal.replace(/^\./, "");
                    newVal = newVal.replace(/\.{2,}$/g, ".");
                    newVal = newVal.replace(/\-\./, "-");
                    newVal = newVal.replace(new RegExp("\\.(\\d{" + precision + "})\\d+", "g"), ".$1");
                } else {
                    newVal = newVal.replace(/[^\d\-]/g, "");
                    if (min >= 0) {
                        newVal = newVal.replace(/^0([0-9])/, "$1");
                    } else {
                        newVal = newVal.replace(/\-0{2}/, "-0");
                        newVal = newVal.replace(/\-0([0-9])/, "-$1");
                    }
                }

                // 不允许输入以0开头的整数
                newVal = newVal.replace(/^0+/g, "0");
                $(this).val(newVal);

                // 如果最小值小于零则不允许输入负数
                if (min >= 0) {
                    newVal = newVal.replace(/\-/g, "");
                }

                // 只允许头部出现负号
                var values = newVal.split("");
                values.forEach(function (val, i) {
                    if (val === "-" && i !== 0) {
                        values.splice(i, 1);
                    }
                });

                newVal = values.join("");

                if (newVal) {
                    var floatVal = parseFloat(newVal);

                    if (floatVal > max) {
                        var maxVal = radioNumber(max, precision);
                        $(this).val(maxVal);
                        activeEvent("change", event, $input, maxVal, $dom);
                        activeEvent("input", event, $input, maxVal, $dom);
                        return;
                    }
                    if (floatVal < min) {
                        var minVal = radioNumber(min, precision);
                        $(this).val(minVal);
                        activeEvent("change", event, $input, minVal, $dom);
                        activeEvent("input", event, $input, minVal, $dom);
                        return;
                    }
                }

                $(this).val(newVal);
                activeEvent("change", event, $input, val, $dom);
                activeEvent("input", event, $input, val, $dom);
            })
            .on("blur", function (event) {
                var val = $(this).val();
                if (val.length === 0) {
                    if (allowEmpty) {
                        activeEvent("change", event, $input, val, $dom);
                        activeEvent("blur", event, $input, val, $dom);
                        return;
                    } else {
                        var newVal = radioNumber(min, precision);
                        $(this).val(newVal);
                        activeEvent("change", event, $input, newVal, $dom);
                        activeEvent("blur", event, $input, newVal, $dom);
                        return;
                    }
                } else {
                    var newVal = radioNumber(val, precision);
                    $(this).val(newVal);
                    activeEvent("change", event, $input, newVal, $dom);
                    activeEvent("blur", event, $input, newVal, $dom);
                    return;
                }
            })
            .on("keypress", function (event) {
                activeEvent("keypress", event, $input, $input.val(), $dom);
            }).on("mousewheel", function (event) {
            activeEvent("mousewheel", event, $input, $input.val(), $dom);
        });

        $subtract.on("click", function (event) {
            var val = $input.val();
            var floatVal = parseFloat(val);

            if (floatVal - step >= min) {
                var newVal = radioNumber(floatVal - step, precision);
                $input.val(newVal);
                if (floatVal != min) {
                    activeEvent("change", event, $subtract, newVal, $dom);
                } else {
                    activeEvent("toMin", event, $subtract, newVal, $dom);
                }
            } else {
                var newVal = radioNumber(min, precision);
                $input.val(newVal);
                if (floatVal == min) {
                    activeEvent("toMin", event, $subtract, newVal, $dom);
                } else {
                    activeEvent("change", event, $subtract, newVal, $dom);
                }
            }
        });

        $add.on("click", function (event) {
            var val = $input.val();

            var floatVal = parseFloat(val);

            if (floatVal + step <= max) {
                var newVal = radioNumber(floatVal + step, precision);
                $input.val(newVal);
                activeEvent("change", event, $add, newVal, $dom);
            } else {
                var newVal = radioNumber(max, precision);
                $input.val(newVal);
                if (floatVal == max) {
                    activeEvent("toMax", event, $add, newVal, $dom);
                } else {
                    activeEvent("change", event, $add, newVal, $dom);
                }
            }
        });

        $container.append($subtract).append($input).append($add);

        activeEvent("created", $dom);
        $target.empty().append($container);
        activeEvent("mounted", $dom);
    }

    exports("numberInput", {
        init: init,
    });
});