using System.Collections.Generic;

namespace rache_der_reti.Core.Pathfinding
{
    internal class NodePriorityQueue
    {
        private readonly List<Node> mHeap;
        private readonly int mFunctionCallCounter;

        public NodePriorityQueue(int capacity, int functionCallCounter)
        {
            mHeap = new List<Node>();
            mHeap.EnsureCapacity(capacity);
            mFunctionCallCounter = functionCallCounter;
        }

        private static bool Compare(Node x, Node y)
        {
            return (x.F < y.F);
        }

        public Node Dequeue()
        {
            // exchange first and last element
            (mHeap[0], mHeap[^1]) = (mHeap[^1], mHeap[0]);
            mHeap[0].mIndex = 0;

            Node result = mHeap[^1];
            mHeap.RemoveAt(mHeap.Count - 1);

            // move element down the queue to restore MinHeap property
            int index = 0;

            while (index < mHeap.Count)
            {
                int leftIndex = 2 * index + 1;
                int rightIndex = 2 * index + 2;

                if (leftIndex >= mHeap.Count)
                {
                    break;
                }

                int minIndex;

                if (rightIndex >= mHeap.Count)
                {
                    minIndex = leftIndex;
                }

                else
                {
                    minIndex = Compare(mHeap[leftIndex], mHeap[rightIndex]) ? leftIndex : rightIndex;
                }

                if (Compare(mHeap[minIndex], mHeap[index]))
                {
                    (mHeap[minIndex], mHeap[index]) = (mHeap[index], mHeap[minIndex]);
                    (mHeap[minIndex].mIndex, mHeap[index].mIndex) = (mHeap[index].mIndex, mHeap[minIndex].mIndex);

                    index = minIndex;
                    continue;
                }

                break;
            }

            result.mIndex = -1;  // node is not in openSet anymore
            return result;
        }

        public void Enqueue(Node node)
        {
            // node might have an index from a previous call to A* so we reset it
            node.mIndex = -1;
            node.mLastCallQueued = mFunctionCallCounter;

            // add element to the end
            mHeap.Add(node);
            node.mIndex = mHeap.Count - 1;
            int index = mHeap.Count - 1;
            int nextIndex = (index - 1) / 2;

            // move element up the queue to restore MinHeap property
            while (nextIndex > -1)
            {
                if (Compare(mHeap[index], mHeap[nextIndex]))
                {
                    (mHeap[index], mHeap[nextIndex]) = (mHeap[nextIndex], mHeap[index]);
                    (mHeap[index].mIndex, mHeap[nextIndex].mIndex) = (mHeap[nextIndex].mIndex, mHeap[index].mIndex);

                    index = nextIndex;
                    nextIndex = (index - 1) / 2;

                    continue;
                }

                break;
            }
            // set node index to where it is in the openSet
            // node.mIndex = index;
        }

        public void Heapify(Node node)
        {
            int index = node.mIndex;
            if (index == 0)
            {
                return;
            }

            int parentIndex = (index - 1) / 2;

            // we never increase values, only decrease them, so we never need to swap downwards, only up
            // int childIndexLeft = index * 2 + 1;
            // int childIndexRight = index * 2 + 2;
            // move element up the queue to restore MinHeap property
            while (parentIndex > -1)
            {
                if (Compare(mHeap[index], mHeap[parentIndex]))
                {
                    (mHeap[index], mHeap[parentIndex]) = (mHeap[parentIndex], mHeap[index]);
                    (mHeap[index].mIndex, mHeap[parentIndex].mIndex) = (mHeap[parentIndex].mIndex, mHeap[index].mIndex);

                    index = parentIndex;
                    parentIndex = (index - 1) / 2;

                    continue;
                }

                break;
            }
            // set node index to where it is in the openSet
            // node.mIndex = index;
        }

        public bool Any()
        {
            return mHeap.Count > 0;
        }
    }
}
