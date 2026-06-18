using Microsoft.AspNetCore.Mvc;
using TournamentApp.Services;

namespace TournamentApp.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly IStatisticService _statisticService;
        private readonly IParticipantService _participantService;

        public StatisticsController(IStatisticService statisticService, IParticipantService participantService)
        {
            _statisticService = statisticService;
            _participantService = participantService;
        }

        public async Task<IActionResult> Index()
        {
            var participants = await _participantService.GetAllParticipantsAsync();
            var participantStats = new List<Models.ParticipantStatistics>();
            
            foreach (var participant in participants)
            {
                var stats = await _statisticService.GetParticipantStatisticsAsync(participant.Id);
                participantStats.Add(stats);
            }
            
            ViewBag.ParticipantStatistics = participantStats;
            
            var headToHeadStats = await _statisticService.GetHeadToHeadStatisticsAsync();
            ViewBag.HeadToHeadStatistics = headToHeadStats;
            
            return View();
        }
        
        public async Task<IActionResult> Participant(int id)
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
        
        public async Task<IActionResult> HeadToHead()
        {
            var headToHeadStats = await _statisticService.GetHeadToHeadStatisticsAsync();
            return View(headToHeadStats);
        }
    }
} 