using SocketProtocol;

namespace SocketServer
{
    public class RoomRequest : BaseRequest
    {
        private readonly Dictionary<int, Room> roomDic = [];
        private readonly List<Room> roomList = [];

        public MainPack CreateRoom(MainPack mainPack, Connection connection)
        {
            try
            {
                // 已在房间中不能创建房间，直接返回
                if (roomDic.ContainsKey(connection.userPack.RoomId))
                {
                    mainPack.ReturnCode = ReturnCode.Fail;
                    return mainPack;
                }

                int roomId = IdGenerator.GenerateRoomId();
                connection.userPack.RoomId = roomId;

                // 新建房间包
                RoomPack roomPack = new()
                {
                    RoomId = roomId,
                    RoomName = mainPack.RoomPacks[0].RoomName,
                    RoomMemberCapacity = mainPack.RoomPacks[0].RoomMemberCapacity,
                    RoomMemberCount = 1,
                    RoomState = RoomState.Waiting
                };

                // 添加房间
                Room room = new(roomPack, connection);
                roomDic.Add(roomId, room);
                roomList.Add(room);

                //更新数据包
                mainPack.RoomPacks.Clear();
                mainPack.RoomPacks.Add(roomPack);
                mainPack.ReturnCode = ReturnCode.Success;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                mainPack.ReturnCode = ReturnCode.Fail;
            }
            return mainPack;
        }

        public MainPack FindRoom(MainPack mainPack, Connection connection)
        {
            try
            {
                // 更新数据包
                mainPack.RoomPacks.Clear();
                mainPack.RoomPacks.AddRange(roomList.Select(room => room.roomPack));
                mainPack.ReturnCode = ReturnCode.Success;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                mainPack.ReturnCode = ReturnCode.Fail;
            }
            return mainPack;
        }


        public MainPack JoinRoom(MainPack mainPack, Connection connection)
        {
            try
            {
                // 房间不存在，直接返回
                if (!roomDic.TryGetValue(mainPack.RoomPacks[0].RoomId, out Room room))
                {
                    mainPack.ReturnCode = ReturnCode.Fail;
                    return mainPack;
                }

                // 连接已在房间或房间已满，直接返回
                if (room.roomMemberConnectionList.Contains(connection) || room.roomPack.RoomMemberCount >= room.roomPack.RoomMemberCapacity)
                {
                    mainPack.ReturnCode = ReturnCode.Fail;
                    return mainPack;
                }

                // 加入房间
                connection.userPack.RoomId = mainPack.RoomPacks[0].RoomId;
                room.roomPack.RoomMemberCount++;
                room.roomMemberConnectionList.Add(connection);

                // 更新数据包
                mainPack.UserPack.Clear();
                mainPack.UserPack.AddRange(room.roomMemberConnectionList.Select(c => c.userPack));
                mainPack.RoomPacks.Clear();
                mainPack.RoomPacks.Add(room.roomPack);
                mainPack.ReturnCode = ReturnCode.Success;

                // 广播消息
                room.Broadcast(mainPack, connection);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                mainPack.ReturnCode = ReturnCode.Fail;
            }
            return mainPack;
        }


        public MainPack LeaveRoom(MainPack mainPack, Connection connection)
        {
            try
            {
                // 房间不存在，直接返回
                if (!roomDic.TryGetValue(connection.userPack.RoomId, out Room room))
                {
                    mainPack.ReturnCode = ReturnCode.Fail;
                    return mainPack;
                }

                // 离开房间
                room.roomPack.RoomMemberCount -= 1;
                room.roomMemberConnectionList.Remove(connection);
                Console.WriteLine("{0}离开房间{1}", connection.userPack.UserId, connection.userPack.RoomId);

                // 更新数据包
                mainPack.UserPack.Clear();
                mainPack.UserPack.Add(connection.userPack);

                // 房间没人则移除，还有人则广播消息
                if (room.roomMemberConnectionList.Count == 0)
                {
                    roomDic.Remove(room.roomPack.RoomId);
                    roomList.Remove(room);
                    mainPack.ReturnCode = ReturnCode.Success;
                    Console.WriteLine("房间{0}没人，自动移除", connection.userPack.RoomId);
                }
                else
                {
                    // 广播消息
                    mainPack.ReturnCode = ReturnCode.Success;
                    room.Broadcast(mainPack, connection);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                mainPack.ReturnCode = ReturnCode.Fail;
            }
            return mainPack;
        }

    }
}