
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using XHD.Core.IServices;
using XHD.Core.Models;

namespace XHD.Core.View.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        private readonly ICRM_Customer_attaService _customerattaservice;
        private readonly ICRM_CustomerService _customerservice;
        private readonly IDBAuthService _dBAuthService;

        public UploadController(ICRM_Customer_attaService customerattaservice, ICRM_CustomerService customerservice, IDBAuthService dBAuthService)
        {
            _customerattaservice = customerattaservice;
            _customerservice = customerservice;
            _dBAuthService = dBAuthService;
        }

        [HttpPost]
        public async Task<string> Image()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            var out_trad_id = BitConverter.ToInt64(buffer, 0).ToString();


            var files = Request.Form.Files;
            if (files.Count == 0)
            {
                return "No File";
            }
            var file = files[0];
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (fileExtension == ".jpg" || fileExtension == ".png" || fileExtension == ".gif" || fileExtension == ".jpeg")
            {

            }
            else
            {
                return "File type error";
            }

            var UploadDir = $"Upload/Image/{DateTime.Now.ToString("yyyy-MM-dd")}";
            var fileDir = $"{Directory.GetCurrentDirectory()}/wwwroot/Upload/Image/{DateTime.Now.ToString("yyyy-MM-dd")}";
            if (!Directory.Exists(fileDir))
                Directory.CreateDirectory(fileDir);

            var filename = $"{out_trad_id}{fileExtension}";
            var fullpath = Path.Combine(fileDir, filename);
            using (var fs = new FileStream(fullpath, FileMode.Create, FileAccess.Write))
            {
                //file.CopyTo(fs);
                //fs.Close();
                await file.CopyToAsync(fs);
            }
            var url = $"../{UploadDir}/{filename}";

            JObject obj = new JObject();
            obj.Add("location", url);

            return obj.ToString();
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<string> FileUp()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            var out_trad_id = BitConverter.ToInt64(buffer, 0).ToString();


            var files = Request.Form.Files;
            if (files.Count == 0)
            {
                return "No File";
            }
            var file = files[0];
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (fileExtension == ".apk")
            {

            }
            else
            {
                return "File type error";
            }

            var UploadDir = $"Upload/File/{DateTime.Now.ToString("yyyy-MM-dd")}";
            var fileDir = $"{Directory.GetCurrentDirectory()}/wwwroot/Upload/File/{DateTime.Now.ToString("yyyy-MM-dd")}";
            if (!Directory.Exists(fileDir))
                Directory.CreateDirectory(fileDir);

            var filename = $"{out_trad_id}{fileExtension}";
            var fullpath = Path.Combine(fileDir, filename);
            using (var fs = new FileStream(fullpath, FileMode.Create, FileAccess.Write))
            {
                //file.CopyTo(fs);
                //fs.Close();
                await file.CopyToAsync(fs);
            }
            var url = $"../{UploadDir}/{filename}";

            JObject obj = new JObject();
            obj.Add("location", url);

            return obj.ToString();
        }


        /// <summary>
        /// 下载本地物理文件（指定文件名）
        /// </summary>
        /// <param name="fileName">客户端接收的文件名（如 "我的报告.pdf"）</param>
        /// <returns>文件流</returns>

        public async Task<IActionResult> DownloadCustomerAtta(string id)
        {
            Expression<Func<CRM_Customer_atta, bool>> exp = a => a.id == id;

            var attadata = _customerattaservice.Grid(exp);

            if (attadata.count == 0)
            {
                return NotFound("文件不存在");
            }

            var data = attadata.data[0];

            // 数据权限：非全量用户只能下载本人客户或公客的附件
            var empId = User.FindFirst(ClaimTypes.Sid)?.Value ?? string.Empty;
            var roleData = await _dBAuthService.GetDataAuth(empId);
            if (roleData.authtype == 0)
            {
                return Forbid();
            }

            if (roleData.authtype != 4)
            {
                var customerResult = await _customerservice.GridAsync(a => a.id == data.cus_id, 1, 1);
                if (customerResult.count == 0)
                {
                    return NotFound("文件不存在");
                }

                var customer = customerResult.data[0];
                var allowedEmpIds = roleData.empList ?? new List<string>();
                if (!allowedEmpIds.Contains(customer.emp_id) && customer.isPrivate != 1)
                {
                    return Forbid();
                }
            }

            // 防止 real_name/cus_id 中的路径穿越
            var uploadRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload", "customer"));
            var serverFilePath = Path.GetFullPath(Path.Combine(uploadRoot, data.cus_id ?? string.Empty, data.real_name ?? string.Empty));
            if (!serverFilePath.StartsWith(uploadRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                return NotFound("文件不存在");
            }

            // 2. 验证文件是否存在
            if (!System.IO.File.Exists(serverFilePath))
            {
                return NotFound("文件不存在");
            }

            // 2. 动态获取 Content-Type
            var contentTypeProvider = new FileExtensionContentTypeProvider();
            if (!contentTypeProvider.TryGetContentType(serverFilePath, out string? contentType))
            {
                contentType = "application/octet-stream"; // 未知类型默认二进制流
            }

            // 3. 读取文件流（使用 FileStream 确保资源释放）
            var fileStream = new FileStream(serverFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

            // 4. 设置响应头：指定下载文件名（支持中文，需编码）
            var contentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                // 编码文件名，避免中文乱码
                FileNameStar = Uri.EscapeDataString(data.file_name),
                FileName = Uri.EscapeDataString(data.file_name)
            };
            Response.Headers.TryAdd(HeaderNames.ContentDisposition, contentDisposition.ToString());

            return File(fileStream, contentType, enableRangeProcessing: true); // enableRangeProcessing 支持断点续传
        }
    }
}
