namespace AdventOfCode2022;

using System.Numerics;

public static class Search2
{
    private class CostComparer<TElem, TCost> : IComparer<(TElem Elem, TCost Cost)>
        where TCost : notnull, IComparisonOperators<TCost, TCost, bool>, IAdditionOperators<TCost, TCost, TCost>
    {
        public int Compare((TElem Elem, TCost Cost) left, (TElem Elem, TCost Cost) right)
        {
            if(left.Cost == right.Cost)
            {
                return 0;
            }
            else
            {
                return left.Cost < right.Cost ? -1 : 1;
            }
        }
    }

    /// <summary>
    /// A*-Suche von start zu end unter Berücksichtigung der Distanz-Heuristik und Nachbarschaft von TPos.
    /// </summary>
    /// <param name="nodes">Liefert alle Knoten in der Karte/dem Graphen</param>
    /// <param name="start">Legt den Start-Knoten der Pfadsuche fest</param>
    /// <param name="finish">Legt den Ziel-Knoten der Pfadsuche und damit das Abbruchkriterium fest</param>
    /// <param name="Neighbors">diese Funktion muss alle direkten Nachbarn eines Knotens liefern</param>
    /// <param name="CalcCosts">diese Funktion liefert die tatsächlichen Kosten von einem Knoten zu einem Nachbarknoten</param>
    /// <param name="EstimateToFinish">diese Funktion liefert die geschätzten Kosten von einem Knoten zum Ziel-Knoten</param>
    /// <param name="maxCost">der Maximalwert von TCost zB int.MaxValue</param>
    /// <typeparam name="TCost">Typ der Distanz</typeparam>
    /// <typeparam name="TPos">Elementetyp von <paramref name="nodes"/></typeparam>
    /// <returns>die Knoten bilden den besten Pfad von <paramref name="start"/> zum <paramref name="finish"/></returns>
    public static IEnumerable<TPos> AStar<TPos, TCost>(IEnumerable<TPos> nodes,
                                                       TPos start,
                                                       Func<TPos, bool> GoalReached,
                                                       Func<TPos, IEnumerable<TPos>> Neighbors,
                                                       Func<TPos, TPos, TCost> CalcCosts,
                                                       Func<TPos, TCost> EstimateToFinish,
                                                       TCost maxCost)
        where TPos  : notnull
        where TCost : notnull, IComparisonOperators<TCost, TCost, bool>, IAdditionOperators<TCost, TCost, TCost>
    {
        ArgumentNullException.ThrowIfNull(Neighbors);
        ArgumentNullException.ThrowIfNull(CalcCosts);
        ArgumentNullException.ThrowIfNull(EstimateToFinish);

        // init Maps für Pfade und Kosten mit dem Startknoten

        var openSet  = new BinaryHeap<(TPos, TCost)>(1000, new CostComparer<TPos, TCost>());
        var posInOpenSet = new Dictionary<TPos, int>();

        var startPos = openSet.Insert((start, EstimateToFinish(start)));
        posInOpenSet.Add(start, startPos);

        var cameFrom = new Dictionary<TPos, TPos>();

        var minPathCosts = nodes.ToDictionary(coord => coord, _ => maxCost);
        minPathCosts[start] = default!;

        // besten Knoten aus openSet auswählen und mit dessen Kosten
        // minPathCosts und finishedPathCosts aktualisieren

        while(openSet.Count > 0)
        {
            // TODO: Fibonacci-Heap für openSet verwenden
            (var current, var cost) = openSet.ExtractMin();
            posInOpenSet.Remove(current);

            if(GoalReached(current))
            {
                var bestPath = new[] { current }.ToList();
                while(cameFrom.ContainsKey(current))
                {
                    current = cameFrom[current];
                    bestPath.Insert(0, current);
                }
                return bestPath;
            }
            else
            {
                foreach(var neighbor in Neighbors(current))
                {
                    var costToNeighbor = minPathCosts[current] + CalcCosts(current, neighbor);

                    if(costToNeighbor < minPathCosts[neighbor])
                    {
                        // dieser Pfad zu neighbor ist besser als alle bisherigen => merken

                        cameFrom[neighbor] = current;
                        minPathCosts[neighbor] = costToNeighbor;
                        
                        var estimanedCostToNeighbor = costToNeighbor + EstimateToFinish(neighbor);

                        if(!posInOpenSet.TryGetValue(neighbor, out var neighborPos))
                        {
                            openSet.Insert((neighbor, estimanedCostToNeighbor));
                        }
                        else
                        {
                            openSet.DecreaseElement(neighborPos, (neighbor, estimanedCostToNeighbor));
                        }
                    }
                }
            }
        }

        return Array.Empty<TPos>();
    }
}
