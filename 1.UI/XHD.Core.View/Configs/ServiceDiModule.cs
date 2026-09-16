using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection; // 原生DI核心命名空间

namespace XHD.Core.View.Configs
{
    public static class ServiceDiModule
    {
        /// 
        /// <param name="services">原生DI服务集合</param>
        public static void AddService(this IServiceCollection services)
        {
            // 反射加载仓储程序集（与原Autofac一致，加载XHD.Core.Service程序集）
            // 说明：Assembly.Load参数是「程序集名称（DLL文件名，不含.dll）」，与命名空间无关
            Assembly ServiceAssembly = Assembly.Load("XHD.Core.Services");

            // 沿用原Autofac的筛选规则：类名以Service结尾、非抽象类、非接口
            var ServiceTypes = ServiceAssembly.GetTypes()
                .Where(t => t.FullName.EndsWith("Service")
                           && !t.IsAbstract
                           && !t.IsInterface);

            // 遍历筛选出的仓储类，原生DI批量注册（对应原Autofac的注册逻辑）
            foreach (var ServiceType in ServiceTypes)
            {
                // 找到仓储类实现的所有接口（与原AsImplementedInterfaces()一致）
                var implementedInterfaces = ServiceType.GetInterfaces();

                foreach (var @interface in implementedInterfaces)
                {
                    // 原生DI注册：接口→实现类，作用域生命周期（与原InstancePerLifetimeScope一致）
                    // 原生DI中，InstancePerLifetimeScope 对应 ServiceLifetime.Scoped
                    services.AddScoped(@interface, ServiceType);

                    // 原Autofac的PropertiesAutowired（属性注入）：原生DI默认不支持属性注入
                    // 补充：若必须使用属性注入，需手动配置（如下注释，按需启用）
                    // services.AddScoped(@interface, sp =>
                    // {
                    //     var instance = Activator.CreateInstance(ServiceType);
                    //     // 遍历属性，注入已注册的服务（简单属性注入实现）
                    //     foreach (var prop in ServiceType.GetProperties().Where(p => p.CanWrite))
                    //     {
                    //         var propService = sp.GetService(prop.PropertyType);
                    //         if (propService != null)
                    //         {
                    //             prop.SetValue(instance, propService);
                    //         }
                    //     }
                    //     return instance;
                    // });
                }
            }
        }
    }
}
