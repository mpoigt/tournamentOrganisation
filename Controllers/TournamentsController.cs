using Microsoft.AspNetCore.Mvc;
using TournamentApp.Constants;
using TournamentApp.DTOs;
using TournamentApp.Enums;
using TournamentApp.Models;
using TournamentApp.Services;

namespace TournamentApp.Controllers
{
    public class TournamentsController : Controller
    {
        private readonly ITournamentService _tournamentService;
        private readonly IParticipantService _parcipantService;
        private readonly IMatchService _matchService;
        
        public TournamentsController(ITournamentService tournamentService, IParticipantService parcipantService, IMatchService matchService)
        {
            _tournamentService = tournamentService;
            _parcipantService = parcipantService;
            _matchService = matchService;
        }
        
        public async Task<IActionResult> Index()
        {
            var tournaments = await _tournamentService.GetAllTournamentsAsync();
            return View(tournaments);
        }
        
        public async Task<IActionResult> Create()
        {
            var participants = await _parcipantService.GetAllParticipantsAsync();
            ViewBag.Participants = participants;
            return View(new CreateTournamentDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTournamentDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                dto.Name = dto.StartDate.ToString("dd.MM.yyyy");
                ModelState.Remove("Name");
            }

            if (ModelState.IsValid)
            {
                if (dto.ParticipantIds == null || 
                    dto.ParticipantIds.Count < ValidationConstants.MinParticipants || 
                    dto.ParticipantIds.Count > ValidationConstants.MaxParticipants)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Выберите от {ValidationConstants.MinParticipants} до {ValidationConstants.MaxParticipants} участников"
                    );
                    ViewBag.Participants = await _parcipantService.GetAllParticipantsAsync();
                    return View(dto);
                }

                try
                {
                    var tournament = new Tournament
                    {
                        Name = dto.Name,
                        StartDate = dto.StartDate,
                        Description = dto.Description,
                        MatchesPerOpponent = dto.MatchesPerOpponent,
                        Type = TournamentType.FromName(dto.Type),
                        Gender = TeamGender.FromName(dto.Gender),
                        IsThirdPlace = dto.IsThirdPlace,
                        PlayOffMatches = dto.PlayOffMatches
                    };

                    await _tournamentService.CreateTournamentAsync(tournament, dto.ParticipantIds, dto.TeamNames);

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ошибка при создании турнира: " + ex.Message);
                }
            }

            ViewBag.Participants = await _parcipantService.GetAllParticipantsAsync();
            return View(dto);
        }

        public async Task<IActionResult> Details(int id)
        {
            var tournament = await _tournamentService.GetTournamentByIdAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }
            
            return View(tournament);
        }
        
        public async Task<IActionResult> Standings(int id)
        {
            var tournament = await _tournamentService.GetTournamentByIdAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }
            
            var standings = await _tournamentService.GetTournamentStandingsAsync(id);
            ViewBag.Tournament = tournament;
            ViewBag.Standings = standings;
            return View();
        }
        
        public async Task<IActionResult> Matches(int id)
        {
            var matches = await _matchService.GetTournamentMatchesAsync(id);
            var tournament = await _tournamentService.GetTournamentByIdAsync(id);
            
            if (tournament == null)
            {
                return NotFound();
            }
            
            ViewBag.Tournament = tournament;
            return View(matches);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var tournament = await _tournamentService.GetTournamentByIdAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }

            var dto = new EditTournamentDTO
            {
                Id = tournament.Id,
                Name = tournament.Name,
                StartDate = tournament.StartDate,
                Description = tournament.Description,
                TypeDisplay = tournament.Type?.Name,
                GenderDisplay = tournament.Gender?.Name
            };

            if (tournament.TournamentParticipants != null)
            {
                foreach (var tp in tournament.TournamentParticipants)
                {
                    if (tp.Participant != null)
                    {
                        dto.Participants[tp.Participant.Name] = tp.TeamName;
                    }
                }
            }

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditTournamentDTO dto)
        {
            if (id != dto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingTournament = await _tournamentService.GetTournamentByIdAsync(id);
                if (existingTournament == null)
                {
                    return NotFound();
                }

                existingTournament.Name = dto.Name;
                existingTournament.StartDate = dto.StartDate;
                existingTournament.Description = dto.Description;

                var success = await _tournamentService.UpdateTournamentAsync(id, existingTournament);
                if (success)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return NotFound();
                }
            }

            var t = await _tournamentService.GetTournamentByIdAsync(id);
            if (t != null)
            {
                dto.TypeDisplay = t.Type?.Name;
                dto.GenderDisplay = t.Gender?.Name;

                if (t.TournamentParticipants != null)
                {
                    foreach (var tp in t.TournamentParticipants)
                    {
                        if (tp.Participant != null)
                        {
                            dto.Participants[tp.Participant.Name] = tp.TeamName;
                        }
                    }
                }
            }

            return View(dto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var tournament = await _tournamentService.GetTournamentByIdAsync(id);
                if (tournament == null)
                {

                    TempData["Error"] = $"Турнир с ID {id} не найден в базе данных.";
                    return RedirectToAction(nameof(Index));
                }

                return View(tournament);
            }
            catch (Exception ex)
            {

                TempData["Error"] = $"Ошибка при получении турнира: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _tournamentService.DeleteTournamentAsync(id);
                if (!success)
                {
                    TempData["Error"] = $"Не удалось удалить турнир с ID {id}. Возможно, он уже был удален.";
                    return RedirectToAction(nameof(Index));
                }
                
                TempData["Success"] = "Турнир успешно удален!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при удалении турнира: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneratePlayoff(int id, int playOffMatches)
        {
            var success = await _tournamentService.GeneratePlayoffAsync(id, playOffMatches);
            TempData["Success"] = "Плей-офф успешно сгенерирован!";
            return RedirectToAction(nameof(Matches), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateFinal(int id)
        {
            var success = await _tournamentService.GenerateFinalAsync(id);
            if (!success)
            {
                TempData["Error"] = "Не удалось создать финальный матч. Возможно, он уже был создан.";
            }
            else
            {
                TempData["Success"] = "Финальный матч успешно создан!";
            }
            
            return RedirectToAction(nameof(Matches), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateRandomGroupResults(int id)
        {
            var success = await _tournamentService.GenerateRandomGroupResultsAsync(id);
            if (!success)
            {
                TempData["Error"] = "Не удалось сгенерировать случайные результаты. Возможно, все матчи уже завершены.";
            }
            else
            {
                TempData["Success"] = "Случайные результаты для групповых матчей успешно сгенерированы!";
            }
            
            return RedirectToAction(nameof(Matches), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReGeneratePlayoff(int id, int playOffMatches)
        {
            var deleteTournament = await _tournamentService.DeletePlayoffAsync(id);
            var success = await _tournamentService.GeneratePlayoffAsync(id, playOffMatches);
            TempData["Success"] = "Плей-офф успешно перегенерирован!";
            return RedirectToAction(nameof(Matches), new { id });
        }


    }
} 