
//货币格式

function toMoney(num) {
    if (num == null || num == "" || num == undefined) {
        return "0.00";
    } else {
        num = num.toString().replace(/\$|\,/g, '');
        if (isNaN(num))
            num = "0";
        sign = (num == (num = Math.abs(num)));
        num = Math.floor(num * 100 + 0.50000000001);
        cents = num % 100;
        num = Math.floor(num / 100).toString();
        if (cents < 10)
            cents = "0" + cents;
        for (var i = 0; i < Math.floor((num.length - (1 + i)) / 3) ; i++)
            num = num.substring(0, num.length - (4 * i + 3)) + ',' +
                num.substring(num.length - (4 * i + 3));

        return (((sign) ? '' : '-') + num + '.' + cents);
    }
}

//日期格式化

function formatTime(val) {
    if (!val) return "";

    var re = /-?\d+/;
    var m = re.exec(val);
    var d = new Date(parseInt(m[0]));
    // 按【2012-02-13 09:09:09】的格式返回日期    
    return d.format("yyyy-MM-dd hh:mm:ss");
}

//日期格式化

function formatTimebytype(val, datetype) {
    if (!val) return "";

    var re = /-?\d+/;
    var m = re.exec(val);
    var d = new Date(parseInt(m[0]));
    // 按【2012-02-13 09:09:09】的格式返回日期    
    return d.format(datetype);
}

// 用于格式化日期显示
Date.prototype.format = function (format) //author: meizz 
{
    var o = {
        "M+": this.getMonth() + 1, //month         
        "d+": this.getDate(),    //day         
        "h+": this.getHours(),   //hour         
        "m+": this.getMinutes(), //minute         
        "s+": this.getSeconds(), //second         
        "q+": Math.floor((this.getMonth() + 3) / 3),  //quarter         
        "S": this.getMilliseconds() //millisecond     
    };
    if (/(y+)/.test(format))
        format = format.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(format))
            format = format.replace(RegExp.$1, RegExp.$1.length == 1 ? o[k] : ("00" + o[k]).substr(("" + o[k]).length));
    return format;
};
//获取参数

function getparastr(strname) {
    var hrefstr, pos, parastr, para, tempstr;
    hrefstr = window.location.href;
    pos = hrefstr.indexOf("?");
    parastr = hrefstr.substring(pos + 1);
    para = parastr.split("&");
    tempstr = "";
    for (i = 0; i < para.length; i++) {
        tempstr = para[i];
        pos = tempstr.indexOf("=");
        if (tempstr.substring(0, pos) == strname) {
            return tempstr.substring(pos + 1);
        }
    }
    return null;
}

//获取参数

function getParastrByName(url, strname) {
    var hrefstr, pos, parastr, para, tempstr;
    hrefstr = url;
    pos = hrefstr.indexOf("?");
    parastr = hrefstr.substring(pos + 1);
    para = parastr.split("&");
    tempstr = "";
    for (i = 0; i < para.length; i++) {
        tempstr = para[i];
        pos = tempstr.indexOf("=");
        if (tempstr.substring(0, pos) == strname) {
            return tempstr.substring(pos + 1);
        }
    }
    return null;
}

//获取QueryString的数组

function getQueryString() {
    var result = location.search.match(new RegExp("[\?\&][^\?\&]+=[^\?\&]+", "g"));
    if (result == null) {
        return "";
    }
    for (var i = 0; i < result.length; i++) {
        result[i] = result[i].substring(1);
    }
    return result;
}

//根据QueryString参数名称获取值

function getQueryStringByName(name) {
    var result = location.search.match(new RegExp("[\?\&]" + name + "=([^\&]+)", "i"));
    if (result == null || result.length < 1) {
        return "";
    }
    return result[1];
}

//根据QueryString参数索引获取值

function getQueryStringByIndex(index) {
    if (index == null) {
        return "";
    }
    var queryStringList = getQueryString();
    if (index >= queryStringList.length) {
        return "";
    }
    var result = queryStringList[index];
    var startIndex = result.indexOf("=") + 1;
    result = result.substring(startIndex);
    return result;
}

//全局

function initLayout() {
    var h = document.documentElement.clientHeight;
    var w = document.documentElement.clientWidth;

    $(".l-window-mask").height(h);
    $(".l-window-mask").width(w);

    var dialogwidth = $(".l-dialog").width();
    var dialogheight = $(".l-dialog").height();

    var offsettop = (h * 0.5 - dialogheight * 0.5) > 0 ? (h * 0.5 - dialogheight * 0.5) : 0;
    var offsetleft = (w * 0.5 - dialogwidth * 0.5) > 5 ? (w * 0.5 - dialogwidth * 0.5) : 5;
    $(".l-dialog").css({ 'top': offsettop, 'left': offsetleft });
}

function getCookie(name)//取cookies函数          
{
    var arr = document.cookie.match(new RegExp("(^| )" + name + "=([^;]*)(;|$)"));
    if (arr != null)
        return unescape(arr[2]);
    return null;
}

function SetCookie(name, value, time)//两个参数，一个是cookie的名子，一个是值  
{
    var exp = new Date();
    exp.setDate(exp.getDate() + time);
    document.cookie = name + "=" + escape(value) + ";expires=" + exp.toGMTString();
}

function delCookie(name)//删除cookie  
{
    var exp = new Date();
    exp.setTime(exp.getTime() - 1);
    var cval = getCookie(name);
    if (cval != null) document.cookie = name + "=" + cval + ";expires=" + exp.toGMTString();
}




//计算文件大小的文字描述,传入参数单位为字节

function formatUnits(size) {
    if (isNaN(size) || size == null) {
        size = 0;
    }

    if (size <= 0) return size + "bytes";

    var t1 = (size / 1024).toFixed(2);
    if (t1 < 0) {
        return "0KB";
    }

    if (t1 > 0 && t1 < 1024) {
        return t1 + "KB";
    }

    var t2 = (t1 / 1024).toFixed(2);
    if (t2 < 1024)
        return t2 + "MB";

    return (t2 / 1024).toFixed(2) + "GB";
}




function DateDiff(sDate) {    //sDate1和sDate2是2006-12-18格式  
    var oDate1, oDate2, iDays
    oDate1 = new Date()    //转换为12-18-2006格式  
    aDate = sDate.split("-")
    oDate2 = new Date(aDate[1] + '-' + aDate[2] + '-' + aDate[0])
    iDays = parseInt((oDate2 - oDate1) / 1000 / 60 / 60 / 24)    //把相差的毫秒数转换为天数  
    return iDays
}

function bytesToSize(bytes) {
    if (bytes === 0) return '0 B';
    var k = 1024, // or 1024
        sizes = ['B', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'],
        i = Math.floor(Math.log(bytes) / Math.log(k));
    return (bytes / Math.pow(k, i)).toPrecision(3) + ' ' + sizes[i];
}

function myHTMLEnCode(str) {
    if (!str) return "";
    if (str.length == 0) return "";
    var s = "";
    s = str.replace(/&/g, "&amp;");
    s = s.replace(/</g, "&lt;");
    s = s.replace(/>/g, "&gt;");
    s = s.replace(/ /g, "&nbsp;");
    s = s.replace(/\'/g, "&#39;");
    s = s.replace(/\"/g, "&quot;");
    //s = s.replace(/\n/g, "<br>");
    return s;
}

function myHTMLDeCode(str) {
    if (!str) return "";
    if (str.length == 0) return "";
    var s = "";
    s = str.replace(/&amp;/g, "&");
    s = s.replace(/&lt;/g, "<");
    s = s.replace(/&gt;/g, ">");
    s = s.replace(/&nbsp;/g, " ");
    s = s.replace(/&#39;/g, "\'");
    s = s.replace(/&quot;/g, "\"");
    s = s.replace(/<br>/g, "\n");
    return s;
}

function CusInfo(id,title) {
    top.cusInfo(id, title);
}

function encryptAES(plainText, secretKey) {
    // 将密钥转换为 CryptoJS 需要的格式
    var key = CryptoJS.enc.Utf8.parse(secretKey);

    // ECB模式加密
    var encrypted = CryptoJS.AES.encrypt(plainText, key, {
        mode: CryptoJS.mode.ECB,
        padding: CryptoJS.pad.Pkcs7
    });

    // 返回 Base64 字符串
    return encrypted.toString();
}



