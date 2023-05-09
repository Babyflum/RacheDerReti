using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace rache_der_reti.Core.Pathfinding
{
    [Serializable]
    public class PathFinder
    {
        [JsonProperty]
        private readonly Grid mGrid;

        [JsonProperty]
        private static int sFunctionCallCounter;

        public PathFinder(Grid grid)
        {
            this.mGrid = grid;
        }


        public Path FindPath(Vector2 startLocation, Vector2 endLocation)
        {
            sFunctionCallCounter++;
            // System.Diagnostics.Debug.WriteLine(sFunctionCallCounter);
            Node startNode = mGrid.WorldToNode(startLocation);
            Node endNode = mGrid.WorldToNode(endLocation);
            /*System.Diagnostics.Debug.WriteLine($"endNode accessible? {endNode.IsAccessible}.");*/

            // check if endNode is accessible. If not find next accessible location.
            if (endNode == null || !endNode.IsAccessible)
            {
                // System.Diagnostics.Debug.WriteLine($"endNode is not accessible.");
                Vector2 alternativeEndLocation = FindNextAccessibleLocation(startLocation, endLocation);
                endNode = mGrid.WorldToNode(alternativeEndLocation);
                // System.Diagnostics.Debug.WriteLine($"alternativeLocation: {alternativeEndLocation}, alternativeNode accessible? {endNode.IsAccessible}");
            }

            if (startNode == null || endNode == null)
            {
                return new Path();
            }

            // Initialize the Open and Closed sets
            // TODO: consider capacity
            NodePriorityQueue openNodes = new NodePriorityQueue(64, sFunctionCallCounter);
            openNodes.Enqueue(startNode);
            startNode.G = 0;
            HashSet<Node> closedNodes = new HashSet<Node>();

            while (openNodes.Any())
            {
                // Find the lowest cost Node in the Open set
                Node currentNode = openNodes.Dequeue();
                closedNodes.Add(currentNode);

                // Found the path, return the solution
                if (currentNode == endNode)
                {
                    //openNodes.ResetHeap();
                    return BuildPath(startNode, endNode);
                }
                
                // Loop through each neighbor
                foreach (Node neighbor in mGrid.GetNeighbors(currentNode))
                {
                    // Skip the Node if it isn't walkable or is processed already
                    if (!neighbor.IsAccessible || closedNodes.Contains(neighbor))
                    {
                        continue;
                    }

                    float costToNeighbor = currentNode.G + ComputeHeuristicDistance(currentNode.mMapPosition, neighbor.mMapPosition);

                    if (costToNeighbor < neighbor.G || neighbor.mIndex == -1 || neighbor.mLastCallQueued < sFunctionCallCounter)
                    {
                        neighbor.G = costToNeighbor;
                        neighbor.H = ComputeHeuristicDistance(neighbor.mMapPosition, endNode.mMapPosition);
                        neighbor.F = neighbor.G + neighbor.H;
                        neighbor.ParentNode = currentNode;

                        if (neighbor.mIndex > -1 && neighbor.mLastCallQueued < sFunctionCallCounter)
                        {
                            neighbor.mIndex = -1;
                        }

                        // If this Node hasn't been processed yet, add it to the Open set
                        if (neighbor.mIndex == -1)
                        {
                            openNodes.Enqueue(neighbor);
                        }
                        else
                        {
                            openNodes.Heapify(neighbor);
                        }
                    }
                }
            }

            /*System.Diagnostics.Debug.WriteLine($"Did not find a path.");*/
            return new Path();
        }

        public Vector2 FindNextAccessibleLocation(Vector2 startLocation, Vector2 endLocation)
        {
            Node endNode = mGrid.WorldToNode(endLocation);

            // Check if endNode is accessible.
            if (endNode is { IsAccessible: true })
            {
                return endLocation;
            }

            // Try to find n
            Vector2 fromEndToStart = Vector2.Subtract(startLocation, endLocation);

            for (int i = 1; i <= 10; i++)
            {
                // Step a little bit towards start location
                Vector2 stepTowardsStart = endLocation + fromEndToStart * (0.1f * i);
                // Check if step location is accessible.
                Node stepNode = mGrid.WorldToNode(stepTowardsStart);
                if (stepNode == null)
                {
                    continue;
                }

                if (stepNode.IsAccessible)
                {
                    return stepTowardsStart;
                }
            }

            return startLocation;
        }


        private Path BuildPath(Node start, Node end)
        {
            Path path = new Path();
            Node currentNode = end;

            while (currentNode != start)
            {
                path.mPathPoints.Add(currentNode.mMapPosition);
                currentNode = currentNode.ParentNode;
            }

            path.mPathPoints.Add(currentNode.mMapPosition);

            // path.mPathPoints.Reverse();
            return path;
        }

        // Compute heuristic for Distance (right now with: Euclidean Distance).
        private static float ComputeHeuristicDistance(Vector2 from, Vector2 to)
        {
            return Vector2.Distance(from, to);
        }
    }
}