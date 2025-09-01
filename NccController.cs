/*************************************************************
 *          Project: NetCoreCMS                              *
 *              Web: http://dotnetcorecms.org                *
 *           Author: OnnoRokom Software Ltd.                 *
 *          Website: www.onnorokomsoftware.com               *
 *            Email: info@onnorokomsoftware.com              *
 *        Copyright: OnnoRokom Software Ltd.                 *
 *          License: BSD-3-Clause                            *
 *************************************************************/

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NetCoreCMS.Framework.Core.Models.ViewModels;
using NetCoreCMS.Framework.Core.Mvc.Cache;
using NetCoreCMS.Framework.Core.Mvc.Filters;
using NetCoreCMS.Framework.Core.Mvc.Models;
using NetCoreCMS.Framework.Core.Mvc.Views;
using NetCoreCMS.Framework.Core.Serialization;
using NetCoreCMS.Framework.Core.Services;
using NetCoreCMS.Framework.i18n;
using NetCoreCMS.Framework.Setup;
using NetCoreCMS.Framework.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NetCoreCMS.Framework.Core.Mvc.Controllers
{
    [ServiceFilter(typeof(NccGlobalExceptionFilter))]
    public class NccController : Controller
    {
        protected ILogger _logger;
        protected IHostingEnvironment Environment { get { return GlobalContext.HostingEnvironment; } }
        public string ErrorMessage { get; set; }
        public string ReturnUrl { get; set; }

        protected INccSettingsService Settings { get{
                object val;
                HttpContext.Items.TryGetValue("NCC_CONTROLLER_PROPERTY_SETTINGS", out val);
                return (INccSettingsService)val;
            }
        }

        public string ControllerName
        {
            get
            {
                object val;
                HttpContext.Items.TryGetValue("NCC_RAZOR_PAGE_PROPERTY_CONTROLLER_NAME", out val);
                return (string)val ?? "";
            }
        }

        public string WebSiteName
        {
            get
            {
                return GlobalContext.WebSite.WebSiteInfos.Where(x=>x.Language == CurrentLanguage).FirstOrDefault()?.Name;                
            }
        }

        public string AreaName
        {
            get
            {
                object val;
                HttpContext.Items.TryGetValue("NCC_RAZOR_PAGE_PROPERTY_AREA_NAME", out val);
                return (string)val ?? "";
            }
        }

        public string ModuleName
        {
            get
            {
                object val;
                HttpContext.Items.TryGetValue("NCC_RAZOR_PAGE_PROPERTY_MODULE_NAME", out val);
                return (string)val ?? "";
            }            
        }

        public string BaseUrl
        {
            get
            {
                var val = HttpContext.Request.IsHttps ? "https://" : "http://";
                val += HttpContext.Request.Host.Value;
                return val ?? "";
            }
        }

        public string PageUrl
        {
            get
            {
                object val;
                HttpContext.Items.TryGetValue("NCC_RAZOR_PAGE_PROPERTY_PAGE_URL", out val);
                return (string)val ?? "";
            }         
        }

        public List<AuthData> AuthDataList
        {
            get
            {
                object val;
                HttpContext.Items.TryGetValue("NCC_CONTROLLER_ACTION_AUTH_DATA", out val);
                return (List<AuthData>)val;
            }
        }

        protected INccUserService UserService
        {
            get
            {
                object val;
                HttpContext.Items.TryGetValue("NCC_CONTROLLER_PROPERTY_USER_SERVICE", out val);
                return (INccUserService)val;
            }
        }

        protected INccTranslator _T { get {
                object val;
                HttpContext.Items.TryGetValue("NCC_RAZOR_PAGE_PROPERTY_TRANSLATOR", out val);
                return (INccTranslator)val;                
            }
        }

        protected IMemoryCache Cache
        {
            get
            {
                object val;
                HttpContext.Items.TryGetValue("NCC_CONTROLLER_PROPERTY_CACHE", out val);
                return (IMemoryCache) val;
            }
        }

        public List<Menu> GetAdminMenus(long userId)
        {
            return GetUserMenus(userId, Menu.MenuType.Admin);
        }

        public List<Menu> GetWebSiteMenus(long userId)
        {
            return GetUserMenus(userId, Menu.MenuType.WebSite);
        }

        private List<Menu> GetUserMenus(long userId, Menu.MenuType menuType)
        {
            var menus = new List<Menu>();
            var user = UserService.Get(userId);
            if (user != null)
            {
                var permissions = user.Permissions.SelectMany(x => x.Permission.PermissionDetails);
                var menulist = permissions
                        .Where(y => y.MenuType == Enum.GetName(typeof(Menu.MenuType), menuType)
                           && user.ExtraDenies.Where(z => z.ModuleName == y.ModuleName
                              && z.Action != y.Action
                              && z.Controller != y.Controller).Count() == 0
                        ).Select(x => new Menu()
                        {
                            Area = GlobalContext.GetModuleAreaName(x.ModuleName),
                            Action = x.Action,
                            ModuleName = x.ModuleName,
                            //AuthDatas = x.AuthData,
                            Controller = x.Controller,
                            DisplayName = x.Name,
                            Type = menuType
                        });

                menus.AddRange(menulist);

                var extraAllowed = user.ExtraPermissions
                    .Where(y => y.MenuType == Enum.GetName(typeof(Menu.MenuType), menuType)
                           && user.ExtraDenies.Where(z => z.ModuleName == y.ModuleName
                              && z.Action != y.Action
                              && z.Controller != y.Controller).Count() == 0
                    )
                    .Select(x => new Menu()
                    {
                        Area = GlobalContext.GetModuleAreaName(x.ModuleName),
                        Action = x.Action,
                        ModuleName = x.ModuleName,
                        //AuthDatas = x.AuthData,
                        Controller = x.Controller,
                        DisplayName = x.Name,
                        Type = menuType
                    });

                menus.AddRange(extraAllowed);

            }
            return menus;
        }

        public object GetSessionValue(string key)
        {
            byte[] value;
            HttpContext.Session.TryGetValue(key, out value);
            if (value == null)
                return null;
            return NccObjectFormatter.BinaryDeserialize(value);
        }

        public void SetSessionValue(string key, object value)
        {
            HttpContext.Session.Set(key, NccObjectFormatter.BinarySerialize(value));
        }

        public void RemoveSessionValue(string key)
        {
            HttpContext.Session.Remove(key);
        }

        public object GetCache(string key)
        {
            object obj = null;
            Cache.TryGetValue(key, out obj);            
            return obj;
        }

        public void SetCache(string key, object value, bool neverRemove = false, MemoryCacheEntryOptions memoryCacheOptions = null)
        {
            if (neverRemove)
            {
                var opt = new MemoryCacheEntryOptions();
                opt.SetPriority(CacheItemPriority.NeverRemove);
                Cache.Set(key, value, opt);
            }
            else if(memoryCacheOptions != null)
            {
                Cache.Set(key, value, memoryCacheOptions);
            }
            else
            {   
                var cts = (CancellationTokenSource)GlobalContext.GlobalCache.Get(NccCacheKeys.CacheCancellationTokenKey);
                if (cts != null)
                {
                    Cache.Set(key, value, new CancellationChangeToken(cts.Token));
                }
                else
                {
                    var opt = new MemoryCacheEntryOptions();
                    opt.SetSlidingExpiration(new TimeSpan(0, 30, 0));
                    Cache.Set(key, value, opt);
                }
            }
        }

        public void RemoveCache(string key)
        {
            Cache.Remove(key);
        }
         
        public NccController()
        {
            
        }

        public string CurrentLanguage
        {
            get
            {
                object val;
                HttpContext.Items.TryGetValue("NCC_RAZOR_PAGE_PROPERTY_CURRENT_LANGUAGE", out val);
                return (string)val ?? "";
            }
        }
        
        public string ShowMessage(string message, MessageType messageType, bool appendMessage = false, bool showAfterRedirect = false, int durationSecond = 5, bool showCloseButton = true)
        {
            ViewBag.MessageDuration = durationSecond;
            ViewBag.MessageShowCloseButton = showCloseButton;

            if (showAfterRedirect)
            {
                TempData["MessageDuration"] = durationSecond;
                TempData["MessageShowCloseButton"] = showCloseButton;
            }

            switch (messageType)
            {
                case MessageType.Success:
                    if (appendMessage == true)
                    {
                        ViewBag.SuccessMessage += message;
                        if (showAfterRedirect)
                        {
                            TempData["SuccessMessage"] += message;
                        }
                    }
                    else
                    {
                        ViewBag.SuccessMessage = message;
                        if (showAfterRedirect)
                        {
                            TempData["SuccessMessage"] = message;
                        }
                    }
                    break;
                case MessageType.Info:
                    if (appendMessage == true)
                    {
                        ViewBag.InfoMessage += message;
                        if (showAfterRedirect)
                        {
                            TempData["InfoMessage"] += message;
                        }
                    }
                    else
                    {
                        ViewBag.InfoMessage = message;
                        if (showAfterRedirect)
                        {
                            TempData["InfoMessage"] = message;
                        }
                    }
                    break;
                case MessageType.Warning:
                    if (appendMessage == true)
                    {
                        ViewBag.WarningMessage += message;
                        if (showAfterRedirect)
                        {
                            TempData["WarningMessage"] += message;
                        }
                    }
                    else
                    {
                        ViewBag.WarningMessage = message;
                        if (showAfterRedirect)
                        {
                            TempData["WarningMessage"] += message;
                        }
                    }
                    break;
                case MessageType.Error:
                    if (appendMessage == true)
                    {
                        ViewBag.ErrorMessage += message;
                        if (showAfterRedirect)
                        {
                            TempData["ErrorMessage"] += message;
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = message;
                        if (showAfterRedirect)
                        {
                            TempData["ErrorMessage"] = message;
                        }
                    }
                    break;
                default:
                    var msg = "Invalid Message Type";
                    if (appendMessage == true)
                    {
                        ViewBag.ErrorMessage += msg;
                        if (showAfterRedirect)
                        {
                            TempData["ErrorMessage"] += msg;
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = msg;
                        if (showAfterRedirect)
                        {
                            TempData["ErrorMessage"] = msg;
                        }
                    }
                    break;
            }
            return "";
        }

        public IActionResult RedirectToErrorPage(string message, string returnUrl)
        {
            TempData["ErrorMessage"] = message;
            TempData["ReturnUrl"] = returnUrl;
            return RedirectToAction("CustomError", "Home");
        }
    }
}
