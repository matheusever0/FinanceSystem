using Equilibrium.Web.Models;

namespace Equilibrium.Web.Services
{
    public class TeamService
    {
        private readonly Random _random = new Random();

        public List<Team> DistributePlayersIntoTeams(List<Player> players, int maxPlayersPerTeam = 22)
        {
            if (players == null || !players.Any())
                return new List<Team>();

            // Ordenar jogadores por prioridade
            var orderedPlayers = OrderPlayersByPriority(players).ToList();

            // Calcular número de times necessários
            int totalPlayers = orderedPlayers.Count;
            int numberOfTeams = (int)Math.Ceiling((double)totalPlayers / maxPlayersPerTeam);

            // Inicializar times
            var teams = Enumerable.Range(1, numberOfTeams)
                .Select(i => new Team { TeamNumber = i })
                .ToList();

            // Distribuir jogadores
            DistributePlayers(teams, orderedPlayers);

            return teams;
        }

        private IEnumerable<Player> OrderPlayersByPriority(List<Player> players)
        {
            // Embaralhar inicialmente para garantir aleatoriedade entre jogadores com características idênticas
            var shuffledPlayers = players.OrderBy(_ => _random.Next()).ToList();

            // 1. Primeiro os que têm Prelive (ordenados por nível e energia)
            var prelivePlayers = shuffledPlayers.Where(p => p.Prelive)
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.MaxEnergia)
                .ToList();

            // Shuffle dentro do mesmo nível e energia para adicionar aleatoriedade controlada
            var preliveGroups = prelivePlayers
                .GroupBy(p => new { p.Level, p.MaxEnergia })
                .SelectMany(g => g.OrderBy(_ => _random.Next()));

            foreach (var player in preliveGroups)
            {
                yield return player;
            }

            // 2. Jogadores com Gira50x (que não têm Prelive)
            var gira50xPlayers = shuffledPlayers.Where(p => p.Gira50x && !p.Prelive)
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.MaxEnergia)
                .ToList();

            var gira50xGroups = gira50xPlayers
                .GroupBy(p => new { p.Level, p.MaxEnergia })
                .SelectMany(g => g.OrderBy(_ => _random.Next()));

            foreach (var player in gira50xGroups)
            {
                yield return player;
            }

            // 3. Jogadores com Descanso QUE TAMBÉM TÊM Prelive ou Gira50x
            var descansoEspecialPlayers = shuffledPlayers.Where(p => p.Descanso && (p.Prelive || p.Gira50x) &&
                                                                 !preliveGroups.Contains(p) &&
                                                                 !gira50xGroups.Contains(p))
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.MaxEnergia)
                .ToList();

            var descansoEspecialGroups = descansoEspecialPlayers
                .GroupBy(p => new { p.Level, p.MaxEnergia })
                .SelectMany(g => g.OrderBy(_ => _random.Next()));

            foreach (var player in descansoEspecialGroups)
            {
                yield return player;
            }

            // 4. Jogadores com apenas Descanso
            var descansoPlayers = shuffledPlayers.Where(p => p.Descanso && !p.Prelive && !p.Gira50x)
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.MaxEnergia)
                .ToList();

            var descansoGroups = descansoPlayers
                .GroupBy(p => new { p.Level, p.MaxEnergia })
                .SelectMany(g => g.OrderBy(_ => _random.Next()));

            foreach (var player in descansoGroups)
            {
                yield return player;
            }

            // 5. Todos os outros jogadores
            var restPlayers = shuffledPlayers.Where(p => !p.Prelive && !p.Gira50x && !p.Descanso)
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.MaxEnergia)
                .ToList();

            var restGroups = restPlayers
                .GroupBy(p => new { p.Level, p.MaxEnergia })
                .SelectMany(g => g.OrderBy(_ => _random.Next()));

            foreach (var player in restGroups)
            {
                yield return player;
            }
        }

        private void DistributePlayers(List<Team> teams, List<Player> orderedPlayers)
        {
            // Para distribuição mais uniforme dos jogadores mais fortes,
            // vamos usar um algoritmo de lotação balanceada em vez de serpentina

            // Para garantir que o jogador mais forte vá para o último time criado
            // (quando há time ímpar), vamos ordenar os times em ordem reversa
            if (teams.Count % 2 != 0 && teams.Count >= 3)
            {
                // Inverte a ordem dos times para distribuir o jogador mais forte ao último time
                teams = teams.OrderByDescending(t => t.TeamNumber).ToList();
            }

            // Para cada jogador, encontrar o time com menor força total e adicionar a ele
            foreach (var player in orderedPlayers)
            {
                // Calcular a força de cada time (baseado na soma de níveis * energia máxima)
                var teamWithMinStrength = teams
                    .OrderBy(t => t.Players.Sum(p => p.Level * (p.MaxEnergia > 0 ? p.MaxEnergia : 1000)))
                    .ThenBy(t => t.Players.Count) // Em caso de empate, escolha o time com menos jogadores
                    .First();

                // Adicionar ao time com menor força
                teamWithMinStrength.Players.Add(player);
            }

            // Restaurar a ordem original dos times
            teams.Sort((t1, t2) => t1.TeamNumber.CompareTo(t2.TeamNumber));

            // Verificar distribuição especial de jogadores com características
            EnsureSpecialPlayersDistribution(teams);
        }

        private void EnsureSpecialPlayersDistribution(List<Team> teams)
        {
            // Para times ímpares, tentar colocar um jogador forte com Prelive no último time
            if (teams.Count % 2 != 0 && teams.Count >= 3)
            {
                var lastTeam = teams.Last();
                var otherTeams = teams.Take(teams.Count - 1).ToList();

                // Verificar se o último time NÃO tem jogadores com Prelive e os outros times têm
                if (!lastTeam.Players.Any(p => p.Prelive) && otherTeams.Any(t => t.Players.Any(p => p.Prelive)))
                {
                    // Encontrar o jogador com Prelive mais forte de outro time
                    var teamWithStrongPrelive = otherTeams
                        .Where(t => t.Players.Any(p => p.Prelive))
                        .OrderByDescending(t => t.Players.Where(p => p.Prelive)
                                                  .Max(p => p.Level * (p.MaxEnergia > 0 ? p.MaxEnergia : 1000)))
                        .First();

                    var strongestPrelivePlayer = teamWithStrongPrelive.Players
                        .Where(p => p.Prelive)
                        .OrderByDescending(p => p.Level * (p.MaxEnergia > 0 ? p.MaxEnergia : 1000))
                        .First();

                    // Encontrar um jogador do último time para trocar
                    var weakestPlayer = lastTeam.Players
                        .OrderBy(p => p.Level * (p.MaxEnergia > 0 ? p.MaxEnergia : 1000))
                        .First();

                    // Fazer a troca
                    teamWithStrongPrelive.Players.Remove(strongestPrelivePlayer);
                    lastTeam.Players.Remove(weakestPlayer);

                    teamWithStrongPrelive.Players.Add(weakestPlayer);
                    lastTeam.Players.Add(strongestPrelivePlayer);
                }
            }

            // Equilibrar o número de jogadores Prelive entre times
            BalanceFeatureDistribution(teams, p => p.Prelive);

            // Equilibrar o número de jogadores Gira50x entre times
            BalanceFeatureDistribution(teams, p => p.Gira50x);

            // Equilibrar o número de jogadores com Descanso que também têm Prelive ou Gira50x
            BalanceFeatureDistribution(teams, p => p.Descanso && (p.Prelive || p.Gira50x));
        }

        private void BalanceFeatureDistribution(List<Team> teams, Func<Player, bool> featurePredicate)
        {
            if (teams.Count <= 1) return;

            bool hasImbalance;
            int balanceAttempts = 0;

            do
            {
                hasImbalance = false;
                balanceAttempts++;

                // Obter contagem máxima e mínima da característica entre os times
                int maxCount = teams.Max(t => t.Players.Count(featurePredicate));
                int minCount = teams.Min(t => t.Players.Count(featurePredicate));

                // Se a diferença for maior que 1, rebalancear
                if (maxCount - minCount > 1)
                {
                    hasImbalance = true;

                    // Encontrar time com mais jogadores da característica
                    var teamWithMax = teams.OrderByDescending(t => t.Players.Count(featurePredicate)).First();

                    // Encontrar time com menos jogadores da característica
                    var teamWithMin = teams.OrderBy(t => t.Players.Count(featurePredicate)).First();

                    // Encontrar jogador adequado para transferir
                    var candidatesToMove = teamWithMax.Players
                        .Where(featurePredicate)
                        .OrderBy(p => p.Level * (p.MaxEnergia > 0 ? p.MaxEnergia : 1000))
                        .ToList();

                    if (candidatesToMove.Any())
                    {
                        var playerToMove = candidatesToMove.First();

                        // Encontrar um jogador para troca que não tenha a característica
                        var swapCandidates = teamWithMin.Players
                            .Where(p => !featurePredicate(p))
                            .OrderByDescending(p => p.Level * (p.MaxEnergia > 0 ? p.MaxEnergia : 1000))
                            .ToList();

                        if (swapCandidates.Any())
                        {
                            var playerToSwap = swapCandidates.First();

                            // Fazer a troca
                            teamWithMax.Players.Remove(playerToMove);
                            teamWithMin.Players.Remove(playerToSwap);

                            teamWithMax.Players.Add(playerToSwap);
                            teamWithMin.Players.Add(playerToMove);
                        }
                    }
                }

                // Limite de 3 tentativas para evitar loop infinito
            } while (hasImbalance && balanceAttempts < 3);
        }

        public string ConvertTeamsToCSV(List<Team> teams)
        {
            var csvLines = new List<string>
            {
                "Time;Nickname;Level;Gira50x;Descanso;Prelive;MaxEnergia" // Cabeçalho
            };

            foreach (var team in teams)
            {
                foreach (var player in team.Players)
                {
                    csvLines.Add($"{team.TeamNumber};{player.Nickname};{player.Level};{(player.Gira50x ? "Sim" : "Não")};{(player.Descanso ? "Sim" : "Não")};{(player.Prelive ? "Sim" : "Não")};{player.MaxEnergia}");
                }
            }

            return string.Join(Environment.NewLine, csvLines);
        }

        public List<Player> ParseCsvToPlayers(string csvContent)
        {
            var players = new List<Player>();
            var lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Pular o cabeçalho se existir
            bool hasHeader = lines[0].Contains("Nickname") || lines[0].Contains("nickname");
            int startIndex = hasHeader ? 1 : 0;

            for (int i = startIndex; i < lines.Length; i++)
            {
                var line = lines[i];
                var values = line.Split(';');

                if (values.Length >= 6)
                {
                    try
                    {
                        players.Add(new Player
                        {
                            Nickname = values[0].Trim(),
                            Level = int.TryParse(values[1].Trim(), out var level) ? level : 1,
                            Gira50x = IsPositiveValue(values[2]),
                            Descanso = IsPositiveValue(values[3]),
                            Prelive = IsPositiveValue(values[4]),
                            MaxEnergia = int.TryParse(values[5].Trim(), out var maxEnergia) ? maxEnergia : 0
                        });
                    }
                    catch
                    {
                        // Ignora linhas com erro
                        continue;
                    }
                }
            }

            return players;
        }

        private bool IsPositiveValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            value = value.Trim().ToLower();
            return value == "sim" || value == "s" || value == "yes" || value == "y" || value == "true" || value == "1";
        }
    }
}