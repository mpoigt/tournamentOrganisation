using Microsoft.AspNetCore.Mvc;
using TournamentApp.Services;

namespace TournamentApp.Controllers
{
    public class MatchesController : Controller
    {
        private readonly ITournamentService _tournamentService;
        private readonly IMatchService _matchService;

        public MatchesController(ITournamentService tournamentService, IMatchService matchService)
        {
            _tournamentService = tournamentService;
            _matchService = matchService;
        }

        public async Task<IActionResult> Edit(int id)
        {
            var match = await _matchService.GetMatchByIdAsync(id);
            if (match == null)
            {
                return NotFound();
            }

            var tournament = await _tournamentService.GetTournamentByIdAsync(match.TournamentId);
            ViewBag.Tournament = tournament;

            ViewBag.Match = match;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int? homeScore, int? awayScore, string isCompleted)
        {
            bool isCompletedBool = !string.IsNullOrEmpty(isCompleted) && isCompleted == "true";

            var match = await _matchService.GetMatchByIdAsync(id);
            if (match == null)
            {
                return NotFound();
            }

            var tournamentId = match.TournamentId;

            var success = await _matchService.UpdateMatchResultAsync(id, homeScore, awayScore, isCompletedBool);
            if (!success)
            {
                return NotFound();
            }

            return RedirectToAction("Matches", "Tournaments", new { id = tournamentId });
        }
    }
}