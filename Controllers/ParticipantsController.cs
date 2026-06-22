using Microsoft.AspNetCore.Mvc;
using TournamentApp.Models;
using TournamentApp.Services;

namespace TournamentApp.Controllers
{
    public class ParticipantsController : Controller
    {
        private readonly IStatisticService _statisticService;
        private readonly IParticipantService _participantService;
        
        public ParticipantsController(IStatisticService statisticService, IParticipantService participantService)
        {
            _statisticService = statisticService;
            _participantService = participantService;
        }
        
        public async Task<IActionResult> Index()
        {
            var participants = await _participantService.GetAllParticipantsAsync();
            return View(participants);
        }
        
        public IActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Participant participant)
        {
            if (ModelState.IsValid)
            {
                await _participantService.CreateParticipantAsync(participant);
                return RedirectToAction(nameof(Index));
            }
            return View(participant);
        }
        
        public async Task<IActionResult> Edit(int id)
        {
            var participant = await _participantService.GetParticipantByIdAsync(id);
            if (participant == null)
            {
                return NotFound();
            }
            return View(participant);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Participant participant)
        {
            if (id != participant.Id)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                var success = await _participantService.UpdateParticipantAsync(id, participant);
                if (success)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return NotFound();
                }
            }
            return View(participant);
        }
        
        public async Task<IActionResult> Delete(int id)
        {
            var participant = await _participantService.GetParticipantByIdAsync(id);
            if (participant == null)
            {
                return NotFound();
            }
            return View(participant);
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _participantService.DeleteParticipantAsync(id);
            if (!success)
            {
                TempData["Error"] = "Невозможно удалить участника с завершенными матчами";
                return RedirectToAction(nameof(Index));
            }
            
            return RedirectToAction(nameof(Index));
        }
        
        public async Task<IActionResult> Statistics(int id)
        {
            var stats = await _statisticService.GetParticipantStatisticsAsync(id);
            var participant = await _participantService.GetParticipantByIdAsync(id);
            
            if (participant == null)
            {
                return NotFound();
            }
            
            ViewBag.Participant = participant;
            return View(stats);
        }
    }
} 