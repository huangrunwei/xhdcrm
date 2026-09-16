using System.Reflection;

var classes = Assembly.Load("XHD.Core.Models").GetTypes();
foreach (var item in classes)
{
    var classname=item.Name;
    //Console.WriteLine(item.Name);
    Console.WriteLine($"{classname}代码生成开始！");
    GeneratorIService(classname);
    GeneratorService(classname);
    GeneratorIRepository(classname);
    GeneratorRepository(classname);
    //GeneratorViewModels(classname);
    Console.WriteLine($"");
}

//Console.ReadLine();

//string[] list = {
//      "C_CarPark"
//    , "C_CarPark_Image"
//    , "C_CarPark_Order"
//    , "C_CarPark_Seat"
//    , "C_CarPark_WxApp"
//    , "C_Equipment_Camera"
//    , "C_Equipment_Host"
//    , "C_Equipment_MgrBox"
//    , "C_Equipment_sensor"
//    , "C_Member"
//    , "C_Member_CarNo"
//    , "C_Question"
//    , "C_Question_reply"
//    , "C_Suggestion"
//    , "C_Suggestion_reply"
//};
//string[] list = {
//    "C_Member"
//};

//foreach (var classname in list)
//{
//    Console.WriteLine($"{classname}代码生成开始！");
//    GeneratorIService(classname);
//    GeneratorService(classname);
//    GeneratorIRepository(classname);
//    GeneratorRepository(classname);
//    GeneratorViewModels(classname);
//    Console.WriteLine($"");
//}
Console.WriteLine($"全部代码生成结束！");
Console.ReadLine();

/// <summary>
/// 生成IService
/// </summary>
static void GeneratorIService(string classname)
{
    var basedir = Path.GetFullPath("../../../../../");

    //生成iserver
    var iServicesPath = $"{basedir}2.Application\\XHD.Core.Services\\IServices\\I{classname}Service.cs";

    //读取模板
    var content = ReadTemplate("IServicesTemplate.txt");

    //替换模板
    content = content.Replace("{className}", classname);

    //生成文件
    WriteAndSave(iServicesPath, content);
}

/// <summary>
/// 生成Service
/// </summary>
static void GeneratorService(string classname)
{
    var basedir = Path.GetFullPath("../../../../../");

    //生成iserver
    var iServicesPath = $"{basedir}2.Application\\XHD.Core.Services\\Services\\{classname}Service.cs";

    //读取模板
    var content = ReadTemplate("ServicesTemplate.txt");

    //替换模板
    content = content.Replace("{className}", classname);

    //生成文件
    WriteAndSave(iServicesPath, content);
}

/// <summary>
/// 生成IRepository
/// </summary>
static void GeneratorIRepository(string classname)
{
    var basedir = Path.GetFullPath("../../../../../");

    //生成iserver
    var iServicesPath = $"{basedir}3.Repository\\XHD.Core.Repository\\IRepository\\I{classname}Repository.cs";

    //读取模板
    var content = ReadTemplate("IRepositoryTemplate.txt");

    //替换模板
    content = content.Replace("{className}", classname);

    //生成文件
    WriteAndSave(iServicesPath, content);
}

/// <summary>
/// 生成Repository
/// </summary>
static void GeneratorRepository(string classname)
{
    var basedir = Path.GetFullPath("../../../../../");

    //生成iserver
    var iServicesPath = $"{basedir}3.Repository\\XHD.Core.Repository\\Repository\\{classname}Repository.cs";

    //读取模板
    var content = ReadTemplate("RepositoryTemplate.txt");

    //替换模板
    content = content.Replace("{className}", classname);

    //生成文件
    WriteAndSave(iServicesPath, content);
}

/// <summary>
/// 生成Repository
/// </summary>
static void GeneratorViewModels(string classname)
{
    var basedir = Path.GetFullPath("../../../../../");

    //生成iserver
    var iServicesPath = $"{basedir}4.Entity\\XHD.Core.ViewModels\\{classname}.cs";

    //读取模板
    var content = ReadTemplate("ViewModelsTemplate.txt");

    //替换模板
    content = content.Replace("{className}", classname);

    //生成文件
    WriteAndSave(iServicesPath, content);
}

/// <summary>
/// 从代码模板中读取内容
/// </summary>
/// <param name="templateName">模板名称，应包括文件扩展名称。比如：template.txt</param>
/// <returns></returns>
static string ReadTemplate(string templateName)
{
    var dir = Path.GetFullPath("../../../");
    var content = string.Empty;
    using (var stream = new FileStream($"{dir}/Template/{templateName}", FileMode.Open, FileAccess.Read))
    {
        if (stream != null)
        {
            using (var reader = new StreamReader(stream))
            {
                content = reader.ReadToEnd();
            }
        }
    }
    return content;
}

/// <summary>
/// 写文件
/// </summary>
/// <param name="fileName">文件完整路径</param>
/// <param name="content">内容</param>
static void WriteAndSave(string fileName, string content)
{
    if (File.Exists(fileName))
    {
        Console.WriteLine($"{fileName}文件存在，不能写入！");
        //文件存在，不处理，防止破坏代码
        return;
    }


    //实例化一个文件流--->与写入文件相关联
    using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
    {
        //实例化一个StreamWriter-->与fs相关联
        using (var sw = new StreamWriter(fs))
        {
            //开始写入
            sw.Write(content);
            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();
        }
    }

    Console.WriteLine($"{fileName}生成成功！");
}