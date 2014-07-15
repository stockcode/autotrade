using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace autotrade.util
{
    /// <summary>
    /// Same as Queue except Dequeue function blocks until there is an object to return.
    /// Note: This class does not need to be synchronized
    /// </summary>
    public class BlockingQueue<T>
    {
        private Queue<T> queue = new Queue<T>();

        private bool open = true;

        /// <summary>
        /// BlockingQueue Destructor (Close queue, resume any waiting thread).
        /// </summary>
        ~BlockingQueue()
        {
            Close();
        }

        /// <summary>
        /// Remove all objects from the Queue.
        /// </summary>
        public void Clear()
        {
            lock (queue)
            {
                queue.Clear();
            }
        }

        /// <summary>
        /// Remove all objects from the Queue, resume all dequeue threads.
        /// </summary>
        public void Close()
        {
            lock (queue)
            {
                open = false;
                queue.Clear();
                Monitor.PulseAll(queue);    // resume any waiting threads
            }
        }

        /// <summary>
        /// Removes and returns the object at the beginning of the Queue.
        /// </summary>
        /// <returns>Object in queue.</returns>
        public T Dequeue()
        {
            return Dequeue(Timeout.Infinite);
        }

        /// <summary>
        /// Removes and returns the object at the beginning of the Queue.
        /// </summary>
        /// <param name="timeout">time to wait before returning</param>
        /// <returns>Object in queue.</returns>
        public T Dequeue(TimeSpan timeout)
        {
            return Dequeue(timeout.Milliseconds);
        }

        /// <summary>
        /// Removes and returns the object at the beginning of the Queue.
        /// </summary>
        /// <param name="timeout">time to wait before returning (in milliseconds)</param>
        /// <returns>Object in queue.</returns>
        public T Dequeue(int timeout)
        {
            lock (queue)
            {
                while (open && (queue.Count == 0))
                {
                    if (!Monitor.Wait(queue, timeout))
                        throw new InvalidOperationException("Timeout");
                }
                if (open)
                    return queue.Dequeue();
                else
                    throw new InvalidOperationException("Queue Closed");
            }
        }

        /// <summary>
        /// Adds an object to the end of the Queue.
        /// </summary>
        /// <param name="obj">Object to put in queue</param>
        public void Enqueue(T obj)
        {
            lock (queue)
            {
                queue.Enqueue(obj);
                Monitor.Pulse(queue);
            }
        }

        /// <summary>
        /// Open Queue.
        /// </summary>
        public void Open()
        {
            lock (queue)
            {
                open = true;
            }
        }

        /// <summary>
        /// Gets flag indicating if queue has been closed.
        /// </summary>
        public bool Closed
        {
            get
            {
                return !open;
            }
        }
    }
}
