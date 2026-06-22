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
  
            var headToHeadStats = await _statisticService.GetHeadToHeadStatisticsAsync();
            var viewModel = new ViewModels.StatisticsIndexViewModel
            {
                ParticipantStats = participantStats,
                HeadToHeadStats = headToHeadStats,

                Top10Scored = participantStats
                    .OrderByDescending(s => s.TotalGoalsScored)
                    .Take(10)
                    .ToList(),

                Top10Conceded = participantStats
                    .OrderByDescending(s => s.TotalGoalsConceded)
                    .Take(10)
                    .ToList()
            };

            return View(viewModel);
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