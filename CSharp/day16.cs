namespace AdventOfCode2022;

using NUnit.Framework;
using FluentAssertions;

using System.Collections.Generic;

using matthiasffm.Common.Collections;
using matthiasffm.Common.Math;

// TODO:
// - tooo slow, does a greedy / dynamic algorithm work here?

[TestFixture]
public class Day16
{
    private record Room(string Valve, int FlowRate, IDictionary<Room, int> Tunnels);

    private static IDictionary<string, Room> ParseData(string[] data)
    {
        var rawRooms = data.Select(s => s.Split(new [] { "Valve ",
                                                         " has flow rate=",
                                                         "; tunnels lead to valves ",
                                                         "; tunnel leads to valve ", },
                                                StringSplitOptions.RemoveEmptyEntries))
                           .Select(s => ParseRom(s))
                           .ToArray();

        var rooms = rawRooms.Select(r => new Room(r.Item1, r.Item2, new Dictionary<Room, int>{}))
                            .ToDictionary(r => r.Valve);

        for(int i = 0; i < rawRooms.Length; i++)
        {
            foreach(var valve in rawRooms[i].Item3)
            {
                rooms[rawRooms[i].Item1].Tunnels[rooms[valve]] = 1;
            }
        }

        return rooms;
    }

    private static (string, int, string[]) ParseRom(string[] specs) => (specs[0], int.Parse(specs[1]), specs[2].Split(", ").ToArray());

    // removes alle rooms where valves can't be opened (starting rooms remains) and
    // adds these tunnel and distances to the connected rooms with functioning valves
    private static void OptimizeRooms(IDictionary<string, Room> rooms, string startValve)
    {
        do
        {
            var roomToRemove = rooms.Values.First(r => r.FlowRate == 0 && r.Valve != startValve);

            foreach(var roomToOptimize in rooms.Values.Where(r => r.Tunnels.Any(t => t.Key.Valve == roomToRemove.Valve)))
            {
                ConsolidateDistancesToNeighbor(roomToOptimize, roomToRemove);
                roomToOptimize.Tunnels.Remove(roomToRemove);
            }

            rooms.Remove(roomToRemove.Valve);
        }
        while(rooms.Values.Any(r => r.FlowRate == 0 && r.Valve != startValve));
    }

    // the input only contains distances to direct neighbors, for the puzzle algorithm we
    // need the distances from each valve to _all_ other valves
    // before: rooms has min distance from each room to all direct neighbors
    // after:  rooms has min distance from each room to all other rooms with functioning valves
    private static void FillDistances(IDictionary<string, Room> rooms)
    {
        var nmbrRooms = rooms.Values.Count;

        int iteration = 0;
        do
        {
            foreach(var room in rooms.Values)
            {
                foreach(var neighbor in room.Tunnels.Keys.ToArray())
                {
                    ConsolidateDistancesToNeighbor(room, neighbor);
                }
            }
            iteration++;
        }
        while(!rooms.Values.All(r => r.Tunnels.Count == nmbrRooms - 1));

        // add 1 to each distance for opening valves step

        foreach(var room in rooms.Values)
        {
            foreach(var neighbor in room.Tunnels.Keys.ToArray())
            {
                room.Tunnels[neighbor]++;
            }
        }
    }

    // adds all distances from a neighbor to the distances of a room
    // before: room has distance to neighbor and min distance to some others
    // after:  room has distance to neighbor and min distance to some others and all reachable from neighbor
    private static void ConsolidateDistancesToNeighbor(Room room, Room neighbor)
    {
        var distanceToNeighbor = room.Tunnels[neighbor];

        foreach(var tunnel in neighbor.Tunnels.Where(t => t.Key != room))
        {
            if(room.Tunnels.ContainsKey(tunnel.Key))
            {
                room.Tunnels[tunnel.Key] = Math.Min(room.Tunnels[tunnel.Key],
                                                    distanceToNeighbor + tunnel.Value);
            }
            else
            {
                room.Tunnels[tunnel.Key] = distanceToNeighbor + tunnel.Value;
            }
        }
    }

    [Test]
    public void TestSamples()
    {
        var data = new[] {
            "Valve AA has flow rate=0; tunnels lead to valves DD, II, BB",
            "Valve BB has flow rate=13; tunnels lead to valves CC, AA",
            "Valve CC has flow rate=2; tunnels lead to valves DD, BB",
            "Valve DD has flow rate=20; tunnels lead to valves CC, AA, EE",
            "Valve EE has flow rate=3; tunnels lead to valves FF, DD",
            "Valve FF has flow rate=0; tunnels lead to valves EE, GG",
            "Valve GG has flow rate=0; tunnels lead to valves FF, HH",
            "Valve HH has flow rate=22; tunnel leads to valve GG",
            "Valve II has flow rate=0; tunnels lead to valves AA, JJ",
            "Valve JJ has flow rate=21; tunnel leads to valve II",
        };

        var rooms = ParseData(data);

        OptimizeRooms(rooms, "AA");
        FillDistances(rooms);

        Puzzle1(rooms, "AA", 30).Should().Be(1651);
        Puzzle2(rooms, "AA", 26).Should().Be(1707);
    }

    [Test]
    public void TestAocInput()
    {
        var data = FileUtils.ReadAllLines(this);
        var rooms = ParseData(data);

        OptimizeRooms(rooms, "AA");
        FillDistances(rooms);

        Puzzle1(rooms, "AA", 30).Should().Be(2114);
        // Puzzle2(rooms, "AA", 26).Should().Be(2666); // is correct but takes 3 minutes on build server, big no no
    }

    // You scan the cave and discover a network of pipes and pressure-release valves. Your device produces a report of each valve's flow rate if it
    // were opened (in pressure per minute) and the tunnels you could use to move between the valves.
    // You estimate it will take you one minute to open a single valve and one minute to follow any tunnel from one valve to another. All of the
    // valves begin closed. Making your way through the tunnels like this, you could open many or all of the valves by the time has elapsed. You
    // need to release as much pressure as possible.
    //
    // Puzzle == Work out the steps to release the most pressure in timeRemaining. What is the most pressure you can release?
    private static int Puzzle1(IDictionary<string, Room> rooms, string startValve, int timeRemaining)
    {
        return rooms[startValve].Tunnels
                                .Max(t => MaxPressure(rooms,
                                                      t.Key,
                                                      new HashSet<Room>(),
                                                      timeRemaining - t.Value,
                                                      0,
                                                      0));
    }

    private static int MaxPressure(IDictionary<string, Room> rooms,
                                   Room room,
                                   HashSet<Room> openValves,
                                   int time,
                                   int pressure,
                                   int dPressure)
    {
        if(time <= 1)
        {
            return pressure + time * dPressure;
        }

        // open valve

        dPressure += room.FlowRate;
        openValves = new HashSet<Room>(openValves)
        {
            room
        };

        // calc max possible flow for remaining closed valves in remaining timeframe

        var toVisit = TunnelsToVisit(room, openValves);
        if(!toVisit.Any())
        {
            return pressure + time * dPressure;
        }

        return toVisit.Max(v => MaxPressure(rooms,
                                            v,
                                            openValves,
                                            time - room.Tunnels[v],
                                            pressure + dPressure * room.Tunnels[v],
                                            dPressure));
    }

    // You're worried that even with an optimal approach, the pressure released won't be enough. What if you got one of the
    // elephants to help you? It would take you 4 minutes to teach an elephant how to open the right valves in the right
    // order, leaving you with only 26 minutes to actually execute your plan. Would having two of you working together be
    // better, even if it means having less time? (Assume that you teach the elephant before opening any valves yourself,
    // giving you both the same full 26 minutes.)
    //
    // Puzzle == With you and an elephant working together for 26 minutes, what is the most pressure you could release?
    private static int Puzzle2(IDictionary<string, Room> rooms, string startValve, int timeRemaining)
    {
        return rooms[startValve].Tunnels
                                .Variations()
                                .Max(t => MaxPressure2(rooms,
                                                       new HashSet<Room>(),
                                                       t.Item1.Key,
                                                       t.Item2.Key,
                                                       timeRemaining - t.Item1.Value,
                                                       timeRemaining - t.Item2.Value,
                                                       0,
                                                       0));
    }

    private static int MaxPressure2(IDictionary<string, Room> rooms,
                                    HashSet<Room> openValves,
                                    Room room1,
                                    Room room2,
                                    int time1,
                                    int time2,
                                    int pressure,
                                    int dPressure)
    {
        if(time1 <= 1 || time2 <= 1)
        {
            return pressure + Math.Max(time1, time2) * dPressure;
        }

        // open valves

        openValves = new HashSet<Room>(openValves)
        {
            room1,
            room2,
        };

        if(time1 > time2)
        {
            dPressure += room1.FlowRate;

            // room1 reached before room2 => calc max of all tunnels from room1

            var toVisit = TunnelsToVisit(room1, openValves);

            if(!toVisit.Any())
            {
                return pressure + time1 * dPressure + time2 * room2.FlowRate;
            }

            return toVisit.Max(v => MaxPressure2(rooms,
                                                 openValves,
                                                 v,
                                                 room2,
                                                 time1 - room1.Tunnels[v],
                                                 time2,
                                                 pressure + dPressure * Math.Min(room1.Tunnels[v], time1 - time2),
                                                 dPressure));
        }
        else if(time1 < time2)
        {
            dPressure += room2.FlowRate;

            // room2 reached before room1 => calc max of all tunnels from room2

            var toVisit = TunnelsToVisit(room2, openValves);

            if(!toVisit.Any())
            {
                return pressure + time1 * room1.FlowRate + time2 * dPressure;
            }

            return toVisit.Max(v => MaxPressure2(rooms,
                                                 openValves,
                                                 room1,
                                                 v,
                                                 time1,
                                                 time2 - room2.Tunnels[v],
                                                 pressure + dPressure * Math.Min(room2.Tunnels[v], time2 - time1),
                                                 dPressure));
        }
        else // time1 == time2
        {
            dPressure += room1.FlowRate + room2.FlowRate;

            // room1 and room2 reached at the same time => calc max of all possible variations of tunnels from room1 and room2

            System.Diagnostics.Debug.Assert(TunnelsToVisit(room1, openValves).Count() == TunnelsToVisit(room2, openValves).Count());
            var toVisit = TunnelsToVisit(room1, openValves).ToArray();

            if(toVisit.Length == 0)
            {
                return pressure + time1 * dPressure;
            }
            else if(toVisit.Length == 1)
            {
                var minDist = Math.Min(room1.Tunnels[toVisit[0]], room2.Tunnels[toVisit[0]]);
                return pressure + time1 * dPressure + (time1 - minDist) * toVisit[0].FlowRate;
            }

            return toVisit.Variations()
                          .Max(t => MaxPressure2(rooms,
                                                 openValves,
                                                 t.Item1,
                                                 t.Item2,
                                                 time1 - room1.Tunnels[t.Item1],
                                                 time2 - room2.Tunnels[t.Item2],
                                                 pressure + dPressure * Math.Min(room1.Tunnels[t.Item1], room2.Tunnels[t.Item2]),
                                                 dPressure));
        }
    }

    private static IEnumerable<Room> TunnelsToVisit(Room room, HashSet<Room> openValves) =>
        room.Tunnels.Keys.Where(t => t.FlowRate > 0 && !openValves.Contains(t));
}
