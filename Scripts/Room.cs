using Google.Protobuf;
using SocketProtocol;

namespace SocketServer
{
    public class Room(RoomPack _roomPack, Connection connection)
    {
        public RoomPack roomPack = _roomPack;
        public List<Connection> roomMemberConnectionList = [connection];

        public void Broadcast(MainPack mainPack, Connection sponseConnection)
        {
            foreach (var c in roomMemberConnectionList)
            {
                if (c != sponseConnection)
                {
                    c.SendMsg(mainPack.ToByteArray());
                }
            }
        }
    }
}