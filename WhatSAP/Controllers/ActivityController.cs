﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhatSAP.Models;

namespace WhatSAP.Controllers
{
    [Route("activity")]
    public class ActivityController : Controller
    {
        private readonly WhatSAPContext _context;

        public ActivityController(WhatSAPContext context)
        {
            _context = context;
        }

        // GET: Activity
        [Route("")]
        public IActionResult Index(int page = 1, string sortBy = "")
        {
            var pageSize = 3;
            var totalActivities = _context.Activity.Count();
            var totalPages = totalActivities / pageSize;
            var previousPage = page - 1;
            var currentPage = page;
            var nextPage = page + 1;

            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalPage = totalPages;
            ViewBag.PreviousPage = previousPage;
            ViewBag.HasPreviousPage = previousPage > 0;
            ViewBag.NextPage = nextPage;
            ViewBag.HasNextPage = nextPage <= totalPages;
            ViewBag.PreviousPageIsEllipsis = false;

            //var activity = from ac in _context.Activity
            //               select ac;

            IEnumerable<Activity> activity = _context.Activity;

            switch (sortBy)
            {
                case "Name":
                    activity = activity.OrderBy(x => x.ActivityName);
                    break;
                case "Date":
                    activity = activity.OrderBy(x => x.ActivityDate);
                    break;
                case "Price":
                    activity = activity.OrderByDescending(x => x.Price);
                    break;
                case "PriceLow":
                    activity = activity.OrderBy(x => x.Price);
                    break;
                case "Rate":
                    activity = activity.OrderByDescending(x => x.Rate);
                    break;
                case "RateLow":
                    activity = activity.OrderBy(x => x.Rate);
                    break;
                default:
                    activity = activity.OrderByDescending(x => x.Rate);
                    break;
            }

            var result = activity.Skip(pageSize * (page - 1)).Take(pageSize).ToArray();
            return View(result);
        }

        [Route("details/")]
        public IActionResult Details()
        {
            return View();
        }

        // GET: Activity/Details/5
        [Route("details/{id}")]
        public IActionResult Details(long id)
        {
            var activity = _context.Activity.FirstOrDefault(x => x.ActivityId == id);
            var comment = _context.Comment.Where(x => x.ActivityId.Equals(id)).Include(c=>c.Customer).
                ToList();
            if (activity == null)
            {
                return NotFound();
            }

            var address = _context.Address.FirstOrDefault(x => x.AddressId == activity.AddressId);
            ViewBag.Address = address.Address2;
            ViewBag.Latitude = address.Latitude;
            ViewBag.Longitude = address.Longitude;

            return View(activity);
        }

        /*
        private bool ActivityExists(long id)
        {
            return _context.Activity.Any(e => e.ActivityId == id);
        }
        */

        [HttpGet, Route("search")]
        public IActionResult Search()
        {
            var categories = from c in _context.Category
                             select c;

            ViewData["Categories"] = categories.ToList();

            return View();
        }

        [HttpPost, Route("search")]
        public IActionResult Search(string keyword = "", DateTime? date = null, double price = 0, long categoryId = 0, int typeId=0)
        {
            var result = from a in _context.Activity
                         select a;

            var searchResult = new SearchResultModel();

            if (!String.IsNullOrEmpty(keyword))
            {
                result = result.Where(x => x.ActivityName.Contains(keyword)).OrderBy(x => x.Rate);
            }
            else if (price != 0)
            {
                result = result.Where(x => x.Price <= price).OrderBy(x => x.Rate);
            }
            else if (categoryId != 0)
            {
                result = result.Where(x => x.CategoryId == categoryId).OrderBy(x => x.Rate);
            }
            else if (date != null)
            {
                result = result.Where(x => x.ActivityDate == date).OrderBy(x => x.Rate);
            } else if(typeId != 0)
            {
                result = result.Where(x => x.TypeId == typeId).OrderBy(x => x.Rate);
            }

            ViewData["Categories"] = (from c in _context.Category
                                      select c).ToList();

            ViewBag.Search = true;
            searchResult.ActivityResults = result.ToList();

            return View(searchResult);
        }

        [Route("ActivityRequestDetail/{id}")]
        public ActionResult ActivityRequestDetail(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var items = _context.Activity.Where(x => x.ActivityId.Equals(id)).FirstOrDefault();

            if(items == null)
            {
                return NotFound();
            }

            return View(items);
        }

        

        //[Route("Delete/{id}")]
        //public async Task<IActionResult> Delete(long? id)
        //{
        //    if(id == null) { return NotFound(); }

        //    var activity = await _context.Activity
        //        .Include(c => c.Address)
        //        .FirstOrDefaultAsync(m => m.ActivityId == id);

        //    if(activity == null) { return NotFound(); }

        //    return View(activity);
        //}

        //[HttpDelete, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Delete(long id)
        //{
        //    var activity = await _context.Activity.FindAsync(id);
        //    var clientId = activity.ClientId;
        //    _context.Activity.Remove(activity);
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction(nameof(Index));
        //}
    }
}