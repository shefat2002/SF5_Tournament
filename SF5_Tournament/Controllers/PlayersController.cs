using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SF5_Tournament.Data;
using SF5_Tournament.Models;

namespace SF5_Tournament.Controllers;

[Authorize]
public class PlayersController(ApplicationDbContext db) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var players = await db.Players.OrderBy(p => p.Name).ToListAsync();
        return View(players);
    }

    public IActionResult Create()
    {
        ViewBag.Characters = SF5Roster.Characters;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PlayerViewModel model)
    {
        ViewBag.Characters = SF5Roster.Characters;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var player = new Player
        {
            Name = model.Name.Trim(),
            MainCharacter = model.MainCharacter
        };

        db.Players.Add(player);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(model.Name), "A player with that name already exists.");
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var player = await db.Players.FindAsync(id);
        if (player is null)
        {
            return NotFound();
        }

        ViewBag.Characters = SF5Roster.Characters;
        return View(new PlayerViewModel { Id = player.Id, Name = player.Name, MainCharacter = player.MainCharacter });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PlayerViewModel model)
    {
        ViewBag.Characters = SF5Roster.Characters;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var player = await db.Players.FindAsync(model.Id);
        if (player is null)
        {
            return NotFound();
        }

        player.Name = model.Name.Trim();
        player.MainCharacter = model.MainCharacter;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(model.Name), "A player with that name already exists.");
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var player = await db.Players.FindAsync(id);
        if (player is null)
        {
            return NotFound();
        }

        db.Players.Remove(player);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Cannot delete a player who is enrolled in a tournament.";
        }

        return RedirectToAction(nameof(Index));
    }
}
