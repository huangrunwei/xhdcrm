using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using XHD.Core.Common;
using XHD.Core.IRepository;
using XHD.Core.IServices;
using XHD.Core.Models;
using XHD.Core.View.Configs;

namespace XHD.Core.View.Controllers
{
    public class MyNoteController : Controller
    {
        private readonly IMy_NoteService _service;
        public MyNoteController(IMy_NoteService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<string> Grid()
        {
            var emp_id = User.FindFirst(ClaimTypes.Sid).Value;

            Expression<Func<My_Note, bool>> exp = a => a.emp_id == emp_id;

            var result = await _service.GridAsync(exp);

            return result.ToString();
        }

        public async Task<string> Save(My_Note model)
        {
            model.id = UUIDNext.Uuid.NewSequential().ToString();
            model.Note_time = DateTime.Now;
            model.emp_id = User.FindFirst(ClaimTypes.Sid).Value;

            await _service.AddAsync(model);

            return XHDResult.Success(model.id).ToString();
        }

        public async Task<string> Update(My_Note model)
        {
            Expression<Func<My_Note, My_Note>> expnote = a => new My_Note { 
                content=model.content,
                color=model.color
            };

            Expression<Func<My_Note, bool>> expwhere = a => a.id == model.id;

            await _service.UpdateAsync(expnote, expwhere);

            return XHDResult.Success(model.id).ToString();
        }

        public async Task<string> UpdateXY(My_Note model)
        {
            Expression<Func<My_Note, My_Note>> expnote = a => new My_Note
            {
                top= model.top,
                left = model.left
            };

            Expression<Func<My_Note, bool>> expwhere = a => a.id == model.id;

            await _service.UpdateAsync(expnote, expwhere);

            return XHDResult.Success(model.id).ToString();
        }

        public async Task<string> Delete(string id)
        {
            var result = 0;            

            result = await _service.DeleteAsync(id);


            return XHDResult.Success().ToString();
        }
    }
}
