using Microsoft.AspNetCore.Mvc;
using Equilibrium.Web.Models;
using Equilibrium.Web.Services;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Equilibrium.Web.Controllers
{
    [Route("teamgenerator")]
    public class TeamGeneratorController : Controller
    {
        private readonly TeamService _teamService;

        public TeamGeneratorController()
        {
            _teamService = new TeamService();
        }

        [HttpGet("")]
        [HttpGet("index")]
        public IActionResult Index()
        {
            var viewModel = new TeamDistributionViewModel
            {
                MaxPlayersPerTeam = 22
            };
            return View(viewModel);
        }

        [HttpPost("generate")]
        public async Task<IActionResult> UploadCsvAndGenerateTeams(IFormFile csvFile, int maxPlayersPerTeam = 22)
        {
            var viewModel = new TeamDistributionViewModel
            {
                MaxPlayersPerTeam = maxPlayersPerTeam
            };

            if (csvFile == null || csvFile.Length == 0)
            {
                viewModel.ErrorMessage = "Nenhum arquivo foi selecionado.";
                return View("Index", viewModel);
            }

            try
            {
                string content;
                using (var reader = new StreamReader(csvFile.OpenReadStream(), Encoding.GetEncoding("ISO-8859-1")))
                {
                    content = await reader.ReadToEndAsync();
                }

                var players = _teamService.ParseCsvToPlayers(content);

                if (players.Count == 0)
                {
                    viewModel.ErrorMessage = "Nenhum jogador encontrado no arquivo CSV.";
                    return View("Index", viewModel);
                }

                // Gerar times diretamente
                viewModel.Teams = _teamService.DistributePlayersIntoTeams(players, maxPlayersPerTeam);
                viewModel.SuccessMessage = $"{players.Count} jogadores distribuídos em {viewModel.Teams.Count} time(s).";

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                viewModel.ErrorMessage = $"Erro ao processar o arquivo: {ex.Message}";
                return View("Index", viewModel);
            }
        }

        [HttpGet("sample")]
        public IActionResult DownloadSampleCsv()
        {
            string csvContent = "Nickname;Level;Gira50x;Descanso;Prelive\n" +
                               "Player1;85;Sim;Não;Não\n" +
                               "Player2;90;Não;Sim;Não\n" +
                               "Player3;75;Não;Não;Sim\n" +
                               "Player4;92;Sim;Não;Não\n" +
                               "Player5;88;Não;Sim;Não";

            byte[] bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(csvContent);

            return File(bytes, "text/csv", "modelo_jogadores.csv");
        }

        [HttpGet("export")]
        public IActionResult GetTeamsCsv(string teamsData)
        {
            if (string.IsNullOrEmpty(teamsData))
            {
                return BadRequest("Não há times para exportar.");
            }

            try
            {
                var teams = System.Text.Json.JsonSerializer.Deserialize<List<Team>>(teamsData);

                if (teams == null || teams.Count == 0)
                {
                    return BadRequest("Não há times para exportar.");
                }

                string csvContent = _teamService.ConvertTeamsToCSV(teams);
                byte[] bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(csvContent);

                return File(bytes, "text/csv", "times_distribuidos.csv");
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao gerar CSV: {ex.Message}");
            }
        }
    }
}