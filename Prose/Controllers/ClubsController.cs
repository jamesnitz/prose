﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prose.Data;
using Prose.Models;
using Prose.Models.ClubViewModels;

namespace Prose.Controllers
{
    public class ClubsController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ApplicationDbContext _context;

        public ClubsController(ApplicationDbContext context, 
                                UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        // GET: Clubs
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            var clubs = await _context.Club.ToListAsync();

            ClubIndexViewModel ClubsVM = new ClubIndexViewModel
            {
                Clubs = clubs,
                CurrentUserId = user.Id
            };

            return View(ClubsVM);
        }

        // GET: Clubs/Details/5
        public async Task<IActionResult> Details(int? id)
        {

            var user = await GetCurrentUserAsync();

            if (id == null)
            {
                return NotFound();
            }

            //getting each club user that includes the club id passed from the view
            var members = await _context.ClubUser
                                    .Include(cu => cu.User)
                                    .Where(cu => cu.ClubId == id)
                                    .ToListAsync();

            List<ClubUser> clubUsers = new List<ClubUser>();

            clubUsers = members;
            
            var club = await _context.Club
                .FirstOrDefaultAsync(m => m.ClubId == id);

            Club clubWithMembers = new Club();

            clubWithMembers = club;

            clubWithMembers.ClubUsers = clubUsers;


            ClubDetailsViewModel clubDetailsViewModel = new ClubDetailsViewModel
            {
                Club = clubWithMembers,
                CurrentUserId = user.Id
            };

            if (club == null)
            {
                return NotFound();
            }

            return View(clubDetailsViewModel);
        }


        //Gets the user's list of member clubs
        public async Task<IActionResult> UserClubsList()
        {

            var user = await GetCurrentUserAsync();

            var ClubData = _context.ClubUser
                            .Include(c => c.Club)
                            .Include(c => c.User)
                            .Where(cu => cu.UserId == user.Id)
                            .ToList();

            ClubUserIndexViewModel ClubUserIndexViewModel = new ClubUserIndexViewModel
            {
                ClubUsers = ClubData,
                CurrentUserId = user.Id
            };

            if (ClubData == null)
            {
                return NotFound();
            }

            return View(ClubUserIndexViewModel);
    }

        // GET: Clubs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clubs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClubId,Name,Location,Description,MeetingFrequency")] Club club)
        {
            ModelState.Remove("UserId");

            var user = await GetCurrentUserAsync();

            if (ModelState.IsValid)
            {
                club.UserId = user.Id;
                _context.Add(club);
                await _context.SaveChangesAsync();
                var newClub = await _context.Club
                                    .Where(c => c == club)
                                    .FirstOrDefaultAsync();

                int id = newClub.ClubId;
                return RedirectToAction("Create", "ClubUsers", new { id });
            }
            return View(club);
        }

        // GET: Clubs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var user = await GetCurrentUserAsync();

            if (id == null)
            {
                return NotFound();
            }

            var club = await _context.Club.FindAsync(id);
            if (club == null || club.UserId != user.Id)
            {
                return NotFound();
            }
            return View(club);
        }

        // POST: Clubs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Club club)
        {
            ModelState.Remove("UserId");

            var user = await GetCurrentUserAsync();

            if (id != club.ClubId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    club.UserId = user.Id;
                    _context.Update(club);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClubExists(club.ClubId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(club);
        }

        // GET: Clubs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var user = await GetCurrentUserAsync();

            if (id == null)
            {
                return NotFound();
            }

            var club = await _context.Club
                .FirstOrDefaultAsync(m => m.ClubId == id);
            if (club == null || user.Id != club.UserId)
            {
                return NotFound();
            }

            return View(club);
        }

        // POST: Clubs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var club = await _context.Club.FindAsync(id);
            _context.Club.Remove(club);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClubExists(int id)
        {
            return _context.Club.Any(e => e.ClubId == id);
        }
    }
}
