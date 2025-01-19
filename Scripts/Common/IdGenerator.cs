using System;
using System.Threading;

namespace SocketServer
{
    public static class IdGenerator
    {
        private static int currentMaxUserId = 10000;
        private static int currentMaxRoomId = 10000;
        private static readonly object lockObj = new object();

        public static int GenerateUserId()
        {
            lock (lockObj)
            {
                currentMaxUserId += 1;
                return currentMaxUserId;
            }
        }

        public static int GenerateRoomId()
        {
            lock (lockObj)
            {
                currentMaxRoomId += 1;
                return currentMaxRoomId;
            }
        }
    }
}