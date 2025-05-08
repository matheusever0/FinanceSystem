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

            // Calcular número de times necessários (garantindo que cada time não tenha mais que o máximo)
            int totalPlayers = orderedPlayers.Count;
            int numberOfTeams = (int)Math.Ceiling((double)totalPlayers / maxPlayersPerTeam);

            // Garantir pelo menos 1 time
            numberOfTeams = Math.Max(1, numberOfTeams);

            // Calcular o número exato de jogadores por time (não pode exceder maxPlayersPerTeam)
            int playersPerTeam = (int)Math.Floor((double)totalPlayers / numberOfTeams);
            int remainingPlayers = totalPlayers - (playersPerTeam * numberOfTeams);

            // Inicializar times
            var teams = Enumerable.Range(1, numberOfTeams)
                .Select(i => new Team { TeamNumber = i })
                .ToList();

            // Distribuir jogadores usando método melhorado
            DistributePlayersEvenly(teams, orderedPlayers, playersPerTeam, remainingPlayers, maxPlayersPerTeam);

            // Equilibrar características específicas e níveis após a distribuição inicial
            EnsureSpecialPlayersDistribution(teams);
            BalanceTeamLevels(teams);

            return teams;
        }

        private IEnumerable<Player> OrderPlayersByPriority(List<Player> players)
        {
            // Embaralhar inicialmente para garantir aleatoriedade entre jogadores com características idênticas
            var shuffledPlayers = players.OrderBy(_ => _random.Next()).ToList();

            // 1. Primeiro os que têm Prelive (ordenados por nível)
            var prelivePlayers = shuffledPlayers.Where(p => p.Prelive)
                .OrderByDescending(p => p.Level)
                .ToList();

            // Shuffle dentro do mesmo nível para adicionar aleatoriedade controlada
            var preliveGroups = prelivePlayers
                .GroupBy(p => p.Level)
                .SelectMany(g => g.OrderBy(_ => _random.Next()));

            foreach (var player in preliveGroups)
            {
                yield return player;
            }

            // 2. Jogadores com Gira50x (que não têm Prelive)
            var gira50xPlayers = shuffledPlayers.Where(p => p.Gira50x && !p.Prelive)
                .OrderByDescending(p => p.Level)
                .ToList();

            var gira50xGroups = gira50xPlayers
                .GroupBy(p => p.Level)
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
                .ToList();

            var descansoEspecialGroups = descansoEspecialPlayers
                .GroupBy(p => p.Level)
                .SelectMany(g => g.OrderBy(_ => _random.Next()));

            foreach (var player in descansoEspecialGroups)
            {
                yield return player;
            }

            // 4. Jogadores com apenas Descanso
            var descansoPlayers = shuffledPlayers.Where(p => p.Descanso && !p.Prelive && !p.Gira50x)
                .OrderByDescending(p => p.Level)
                .ToList();

            var descansoGroups = descansoPlayers
                .GroupBy(p => p.Level)
                .SelectMany(g => g.OrderBy(_ => _random.Next()));

            foreach (var player in descansoGroups)
            {
                yield return player;
            }

            // 5. Todos os outros jogadores
            var restPlayers = shuffledPlayers.Where(p => !p.Prelive && !p.Gira50x && !p.Descanso)
                .OrderByDescending(p => p.Level)
                .ToList();

            var restGroups = restPlayers
                .GroupBy(p => p.Level)
                .SelectMany(g => g.OrderBy(_ => _random.Next()));

            foreach (var player in restGroups)
            {
                yield return player;
            }
        }

        private void DistributePlayersEvenly(List<Team> teams, List<Player> orderedPlayers,
                                       int playersPerTeam, int remainingPlayers, int maxPlayersPerTeam)
        {
            // Dividir os jogadores em grupos baseados em suas características
            var prelivePlayers = orderedPlayers.Where(p => p.Prelive).ToList();
            var gira50xPlayers = orderedPlayers.Where(p => p.Gira50x && !p.Prelive).ToList();
            var descansoPlayers = orderedPlayers.Where(p => p.Descanso && !p.Prelive && !p.Gira50x).ToList();
            var normalPlayers = orderedPlayers.Where(p => !p.Prelive && !p.Gira50x && !p.Descanso).ToList();

            // Inicializar cada time com o número correto de jogadores
            // Primeiro, determinar quais times recebem um jogador extra
            var teamsWithExtraPlayer = new HashSet<int>();
            for (int i = 0; i < remainingPlayers; i++)
            {
                teamsWithExtraPlayer.Add(i % teams.Count);
            }

            // Distribuir os jogadores em serpentina, priorizando características
            var allPlayersGrouped = new List<List<Player>>
            {
                prelivePlayers,
                gira50xPlayers,
                descansoPlayers,
                normalPlayers
            };

            // Primeiro passo: distribuir jogadores Prelive, Gira50x e Descanso de forma equilibrada
            foreach (var playerGroup in allPlayersGrouped.Take(3))
            {
                DistributeSpecialPlayersEvenly(teams, playerGroup);
            }

            // Segundo passo: completar os times até o número máximo exato
            for (int i = 0; i < teams.Count; i++)
            {
                var team = teams[i];
                int targetSize = playersPerTeam + (teamsWithExtraPlayer.Contains(i) ? 1 : 0);

                // Verifique quantos jogadores ainda precisamos adicionar
                int playersNeeded = targetSize - team.Players.Count;

                if (playersNeeded > 0)
                {
                    // Encontre jogadores ainda não alocados
                    var availablePlayers = normalPlayers
                        .Where(p => !teams.Any(t => t.Players.Contains(p)))
                        .OrderByDescending(p => p.Level)
                        .Take(playersNeeded)
                        .ToList();

                    foreach (var player in availablePlayers)
                    {
                        team.Players.Add(player);
                        // Remover dos normalPlayers para não adicionar novamente
                        normalPlayers.Remove(player);
                    }
                }
            }

            // Terceiro passo: garantir que NENHUM time tenha mais jogadores que o máximo permitido
            foreach (var team in teams)
            {
                if (team.Players.Count > maxPlayersPerTeam)
                {
                    var excessPlayers = team.Players
                        .OrderBy(p => p.Level)
                        .Take(team.Players.Count - maxPlayersPerTeam)
                        .ToList();

                    foreach (var player in excessPlayers)
                    {
                        team.Players.Remove(player);
                    }
                }
            }
        }

        private void DistributeSpecialPlayersEvenly(List<Team> teams, List<Player> players)
        {
            if (teams.Count == 0 || players.Count == 0)
                return;

            // Ordenar os jogadores por Level
            var orderedPlayers = players
                .OrderByDescending(p => p.Level)
                .ToList();

            // Distribuição em serpentina para garantir equilíbrio de força
            bool reverse = false;
            var teamsOrdered = teams.OrderBy(t => t.TeamNumber).ToList();

            foreach (var player in orderedPlayers)
            {
                // Verificar se o jogador já está em algum time
                if (teams.Any(t => t.Players.Contains(player)))
                    continue;

                if (reverse)
                {
                    teamsOrdered.Reverse();
                }

                // Encontrar time com menos jogadores deste tipo
                var targetTeam = teamsOrdered
                    .OrderBy(t => t.Players.Count(p => p.GetType() == player.GetType()))
                    .ThenBy(t => t.Players.Count)
                    .First();

                targetTeam.Players.Add(player);

                if (reverse)
                {
                    teamsOrdered.Reverse();
                }
                reverse = !reverse;
            }
        }

        private void DistributePlayerGroupEvenly(List<Team> teams, List<Player> players)
        {
            if (teams.Count == 0 || players.Count == 0)
                return;

            // Distribuir usando método serpentina para equilibrar a força
            bool reverse = false;

            foreach (var player in players)
            {
                // Verificar se o jogador já está em algum time
                if (teams.Any(t => t.Players.Contains(player)))
                    continue;

                // Ordenar times por número de jogadores
                var orderedTeams = reverse
                    ? teams.OrderByDescending(t => t.Players.Count).ToList()
                    : teams.OrderBy(t => t.Players.Count).ToList();

                // Adicionar ao time com menos jogadores
                var targetTeam = orderedTeams.First();
                targetTeam.Players.Add(player);

                // Inverter a direção após cada distribuição para sistema de serpentina
                reverse = !reverse;
            }
        }

        private void EnsureMaxPlayersPerTeam(List<Team> teams, int maxPlayersPerTeam)
        {
            // Verificar se algum time excede o limite máximo
            foreach (var team in teams.Where(t => t.Players.Count > maxPlayersPerTeam))
            {
                // Identificar jogadores para remover, priorizando os com menor nível
                var playersToMove = team.Players
                    .OrderBy(p => p.Level)
                    .Take(team.Players.Count - maxPlayersPerTeam)
                    .ToList();

                // Encontrar times com menos jogadores
                foreach (var player in playersToMove)
                {
                    var teamWithMinPlayers = teams
                        .Where(t => t.Players.Count < maxPlayersPerTeam)
                        .OrderBy(t => t.Players.Count)
                        .FirstOrDefault();

                    if (teamWithMinPlayers != null)
                    {
                        team.Players.Remove(player);
                        teamWithMinPlayers.Players.Add(player);
                    }
                    else
                    {
                        // Se todos os times já estão no máximo, remover o jogador
                        team.Players.Remove(player);
                    }
                }
            }
        }

        private void EnsureSpecialPlayersDistribution(List<Team> teams)
        {
            if (teams.Count <= 1) return;

            // Equilibrar o número de jogadores Prelive entre times
            BalanceFeatureDistribution(teams, p => p.Prelive);

            // Equilibrar o número de jogadores Gira50x entre times
            BalanceFeatureDistribution(teams, p => p.Gira50x);

            // Equilibrar o número de jogadores com Descanso
            BalanceFeatureDistribution(teams, p => p.Descanso);

            // Equilibrar o número total de jogadores entre times
            BalanceTeamSizes(teams);
        }

        private void BalanceTeamSizes(List<Team> teams)
        {
            if (teams.Count <= 1) return;

            bool hasImbalance;
            int balanceAttempts = 0;

            do
            {
                hasImbalance = false;
                balanceAttempts++;

                // Obter tamanho máximo e mínimo dos times
                int maxSize = teams.Max(t => t.Players.Count);
                int minSize = teams.Min(t => t.Players.Count);

                // Se a diferença for maior que 1, rebalancear
                if (maxSize - minSize > 1)
                {
                    hasImbalance = true;

                    // Encontrar time com mais jogadores
                    var teamWithMax = teams.OrderByDescending(t => t.Players.Count).First();

                    // Encontrar time com menos jogadores
                    var teamWithMin = teams.OrderBy(t => t.Players.Count).First();

                    // Encontrar jogador adequado para transferir (preferencialmente sem características especiais)
                    var candidatesToMove = teamWithMax.Players
                        .OrderBy(p => p.Prelive ? 3 : (p.Gira50x ? 2 : (p.Descanso ? 1 : 0)))
                        .ThenBy(p => p.Level)
                        .Take(3)
                        .ToList();

                    if (candidatesToMove.Any())
                    {
                        var playerToMove = candidatesToMove.First();

                        // Mover o jogador
                        teamWithMax.Players.Remove(playerToMove);
                        teamWithMin.Players.Add(playerToMove);
                    }
                }

                // Limite de 5 tentativas para evitar loop infinito
            } while (hasImbalance && balanceAttempts < 5);
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
                        .OrderBy(p => p.Level)
                        .ToList();

                    if (candidatesToMove.Any())
                    {
                        var playerToMove = candidatesToMove.First();

                        // Encontrar um jogador para troca que não tenha a característica
                        var swapCandidates = teamWithMin.Players
                            .Where(p => !featurePredicate(p))
                            .OrderByDescending(p => p.Level)
                            .ToList();

                        if (swapCandidates.Any())
                        {
                            var playerToSwap = swapCandidates.First();

                            // Verificar se a troca não vai criar desbalanceamento de tamanho dos times
                            if (teamWithMax.Players.Count == teamWithMin.Players.Count)
                            {
                                // Fazer a troca
                                teamWithMax.Players.Remove(playerToMove);
                                teamWithMin.Players.Remove(playerToSwap);

                                teamWithMax.Players.Add(playerToSwap);
                                teamWithMin.Players.Add(playerToMove);
                            }
                        }
                        else
                        {
                            // Se não tiver jogador para troca, apenas move o jogador
                            // Mas apenas se isso não criar outro desbalanceamento
                            if (teamWithMax.Players.Count - teamWithMin.Players.Count > 0)
                            {
                                teamWithMax.Players.Remove(playerToMove);
                                teamWithMin.Players.Add(playerToMove);
                            }
                        }
                    }
                }

                // Limite de 5 tentativas para evitar loop infinito
            } while (hasImbalance && balanceAttempts < 5);
        }

        private void BalanceTeamLevels(List<Team> teams)
        {
            if (teams.Count <= 1) return;

            bool hasImbalance;
            int balanceAttempts = 0;

            do
            {
                hasImbalance = false;
                balanceAttempts++;

                // Calcular a força total (Level) de cada time
                var teamStrengths = teams.ToDictionary(
                    t => t.TeamNumber,
                    t => t.Players.Sum(p => p.Level)
                );

                // Encontrar o time mais forte e o mais fraco
                var maxStrength = teamStrengths.Values.Max();
                var minStrength = teamStrengths.Values.Min();

                // Se a diferença for significativa (mais de 10%), tentar equilibrar
                if ((maxStrength - minStrength) > (maxStrength * 0.1))
                {
                    hasImbalance = true;

                    var strongestTeam = teams.First(t => teamStrengths[t.TeamNumber] == maxStrength);
                    var weakestTeam = teams.First(t => teamStrengths[t.TeamNumber] == minStrength);

                    // Verificar se os times têm o mesmo número de jogadores
                    if (strongestTeam.Players.Count == weakestTeam.Players.Count)
                    {
                        // Encontrar jogador de maior level do time mais forte
                        var strongPlayer = strongestTeam.Players
                            .OrderByDescending(p => p.Level)
                            .First();

                        // Encontrar jogador de menor level do time mais fraco
                        var weakPlayer = weakestTeam.Players
                            .OrderBy(p => p.Level)
                            .First();

                        // Verificar se a troca melhora o equilíbrio
                        int newStrongTeamLevel = maxStrength - strongPlayer.Level + weakPlayer.Level;
                        int newWeakTeamLevel = minStrength - weakPlayer.Level + strongPlayer.Level;

                        // Fazer a troca apenas se realmente melhorar o equilíbrio
                        if (Math.Abs(newStrongTeamLevel - newWeakTeamLevel) < Math.Abs(maxStrength - minStrength))
                        {
                            // Preservar características especiais
                            bool strongHasPrelive = strongPlayer.Prelive;
                            bool weakHasPrelive = weakPlayer.Prelive;
                            bool strongHasGira50x = strongPlayer.Gira50x;
                            bool weakHasGira50x = weakPlayer.Gira50x;

                            // Não trocar se for gerar desbalanceamento de características
                            if (strongHasPrelive == weakHasPrelive && strongHasGira50x == weakHasGira50x)
                            {
                                // Fazer a troca
                                strongestTeam.Players.Remove(strongPlayer);
                                weakestTeam.Players.Remove(weakPlayer);

                                strongestTeam.Players.Add(weakPlayer);
                                weakestTeam.Players.Add(strongPlayer);
                            }
                        }
                    }
                }

                // Limite de 5 tentativas para evitar loop infinito
            } while (hasImbalance && balanceAttempts < 5);
        }

        public string ConvertTeamsToCSV(List<Team> teams)
        {
            var csvLines = new List<string>
            {
                "Time;Nickname;Level;Prelive;Gira50x;Descanso" // Cabeçalho atualizado
            };

            foreach (var team in teams)
            {
                // Ordenar jogadores por prioridade: Prelive, Gira50x, Descanso e Level
                var orderedPlayers = team.Players
                    .OrderByDescending(p => p.Prelive)
                    .ThenByDescending(p => p.Gira50x)
                    .ThenByDescending(p => p.Descanso)
                    .ThenByDescending(p => p.Level);

                foreach (var player in orderedPlayers)
                {
                    csvLines.Add($"{team.TeamNumber};{player.Nickname};{player.Level};{(player.Prelive ? "Sim" : "Não")};{(player.Gira50x ? "Sim" : "Não")};{(player.Descanso ? "Sim" : "Não")}");
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

                if (values.Length >= 5)
                {
                    try
                    {
                        players.Add(new Player
                        {
                            Nickname = values[0].Trim(),
                            Level = int.TryParse(values[1].Trim(), out var level) ? level : 1,
                            Gira50x = IsPositiveValue(values[2]),
                            Descanso = IsPositiveValue(values[3]),
                            Prelive = IsPositiveValue(values[4])
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