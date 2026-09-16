
$(function () {
    $.ajax({
        url: "/Report_Customer/ReportProvinces",
        type: "get",
        dataType: "json",
        success: function (data) {
            map(data);
        },
    });

    function map(data) {
        // 基于准备好的dom，初始化echarts实例
        var myChart = echarts.init(document.getElementById('map'));
        
        var geoCoordMap = {//市区坐标
            '黑龙江': [127.9688, 45.368],
            '内蒙古': [110.3467, 41.4899],
            "吉林": [125.8154, 44.2584],
            '北京': [116.4551, 40.2539],
            "辽宁": [123.1238, 42.1216],
            "河北": [114.4995, 38.1006],
            "天津": [118.4219, 39.4189],
            "山西": [112.3352, 37.9413],
            "陕西": [109.1162, 34.2004],
            "甘肃": [103.5901, 36.3043],
            "宁夏": [106.3586, 38.1775],
            "青海": [101.4038, 36.8207],
            "新疆": [87.9236, 43.5883],
            "西藏": [91.11, 29.97],
            "四川": [103.9526, 30.7617],
            "重庆": [108.384366, 30.439702],
            "山东": [117.1582, 36.8701],
            "河南": [113.4668, 34.6234],
            "江苏": [118.8062, 31.9208],
            "安徽": [117.29, 32.0581],
            "湖北": [114.3896, 30.6628],
            "浙江": [119.5313, 29.8773],
            "福建": [119.4543, 25.9222],
            "江西": [116.0046, 28.6633],
            "湖南": [113.0823, 28.2568],
            "贵州": [106.6992, 26.7682],
            "云南": [102.9199, 25.4663],
            "广东": [113.12244, 23.009505],
            "广西": [108.479, 23.1152],
            "海南": [110.3893, 19.8516],
            '上海': [121.4648, 31.2891]

        };
        var convertData = function (data) {
            var res = [];

            if (!data) {
                return res;
            }

            for (var i = 0; i < data.length; i++) {
                var geoCoord = geoCoordMap[data[i].name];
                if (geoCoord) {
                    res.push({
                        name: data[i].name,
                        value: geoCoord.concat(data[i].value)
                    });
                }
            }

            console.log(res);
            return res;
        };

        option = {
            // backgroundColor: '#404a59',
            title: {
                //text: '客户',
                
                left: 'center',
                textStyle: {
                    color: '#fff'
                }
            },
            //tooltip: {
            //    trigger: 'item'
            //},

            geo: {
                map: 'china',
                label: {
                    emphasis: {
                        show: false
                    }
                },
                roam: false,
                zoom: 1.2,
                itemStyle: {
                    normal: {
                        areaColor: 'rgba(2,37,101,.5)',
                        borderColor: 'rgba(112,187,252,.5)'
                    },
                    emphasis: {
                        areaColor: 'rgba(2,37,101,.8)'
                    }
                }
            },
            series: [
                {
                    //name: '标题名称',
                    type: 'effectScatter',
                    coordinateSystem: 'geo',
                    data: convertData(data),
                    rippleEffect: {
                        brushType:"stroke"
                    },
                    symbolSize: function (val) {
                        if (val[2] < 10)
                            return 15;
                        if (val[2] > 30)
                            return 30;
                        return val[2] ;
                    },
                    label: {
                        normal: {
                            formatter: function (params) {
                                return params.name + " : " + params.value[2];
                            },
                            position: 'right',
                            show: false
                        },
                        emphasis: {
                            show: true
                        }
                    },
                    itemStyle: {
                        normal: {
                            color: '#ffeb7b'
                        }
                    }
                }

            ]
        };

        myChart.setOption(option);
        window.addEventListener("resize", function () {
            myChart.resize();
        });
    }

})

