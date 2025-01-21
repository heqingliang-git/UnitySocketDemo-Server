using SocketProtocol;

namespace SocketServer
{
    public class RoomRequest : BaseRequest
    {
        private readonly Dictionary<int, Room> roomDic = [];
        private readonly List<Room> roomList = [];

        public MainPack CreateRoom(MainPack mainPack, Connection connection)
        {
            // 未设置房间包
            // 已在房间中不能创建房间，直接返回
            if (mainPack.RoomPacks.Count == 0 || roomDic.ContainsKey(connection.userPack.RoomId))
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

            // 告诉客户端新建的房间信息及房间的成员（房主自己）
            mainPack.RoomPacks.Clear();
            mainPack.RoomPacks.Add(roomPack);
            mainPack.UserPack.Clear();
            mainPack.UserPack.Add(connection.userPack);
            mainPack.ReturnCode = ReturnCode.Success;

            return mainPack;
        }

        public MainPack FindRoom(MainPack mainPack, Connection connection)
        {
            // 告诉客户端所有房间的信息
            mainPack.RoomPacks.Clear();
            mainPack.RoomPacks.AddRange(roomList.Select(room => room.roomPack));
            mainPack.ReturnCode = ReturnCode.Success;

            return mainPack;
        }


        public MainPack JoinRoom(MainPack mainPack, Connection connection)
        {
            // 未设置房间包
            // 房间不存在，直接返回
            if (mainPack.RoomPacks.Count == 0 || !roomDic.TryGetValue(mainPack.RoomPacks[0].RoomId, out Room room))
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

            // 告诉客户端房间当前的成员（0号为房主）和当前房间的信息
            mainPack.UserPack.Clear();
            mainPack.UserPack.AddRange(room.roomMemberConnectionList.Select(c => c.userPack));
            mainPack.RoomPacks.Clear();
            mainPack.RoomPacks.Add(room.roomPack);
            mainPack.ReturnCode = ReturnCode.Success;

            // 广播消息
            room.Broadcast(mainPack, connection);
            return mainPack;
        }


        public MainPack LeaveRoom(MainPack mainPack, Connection connection)
        {
            // 用户还未登陆
            // 用户房间不存在，直接返回
            if (connection.userPack == null || !roomDic.TryGetValue(connection.userPack.RoomId, out Room room))
            {
                mainPack.ReturnCode = ReturnCode.Fail;
                return mainPack;
            }

            // 离开房间
            Console.WriteLine("{0}离开房间{1}", connection.userPack.UserId, connection.userPack.RoomId);
            room.roomPack.RoomMemberCount -= 1;
            room.roomMemberConnectionList.Remove(connection);
            connection.userPack.RoomId = 0;

            // 告诉客户端房间还剩的成员
            mainPack.UserPack.Clear();
            mainPack.UserPack.AddRange(room.roomMemberConnectionList.Select(c => c.userPack));
            mainPack.ReturnCode = ReturnCode.Success;

            // 房间没人则移除，还有人则广播消息
            if (room.roomMemberConnectionList.Count == 0)
            {
                Console.WriteLine("房间{0}没人，自动移除", connection.userPack.RoomId);
                roomDic.Remove(room.roomPack.RoomId);
                roomList.Remove(room);
            }
            else
            {
                // 广播
                room.Broadcast(mainPack, connection);
            }
            return mainPack;
        }

        public MainPack RoomChat(MainPack mainPack, Connection connection)
        {
            // 用户房间不存在，直接返回
            if (!roomDic.TryGetValue(connection.userPack.RoomId, out Room room))
            {
                mainPack.ReturnCode = ReturnCode.Fail;
                return mainPack;
            }

            // 告诉客户端谁发的消息
            mainPack.UserPack.Clear();
            mainPack.UserPack.Add(connection.userPack);
            mainPack.ReturnCode = ReturnCode.Success;

            // 广播消息
            room.Broadcast(mainPack, connection);

            return mainPack;
        }

        public MainPack StartGame(MainPack mainPack, Connection connection)
        {
            // 用户房间不存在，直接返回
            if (!roomDic.TryGetValue(connection.userPack.RoomId, out Room room))
            {
                mainPack.ReturnCode = ReturnCode.Fail;
                return mainPack;
            }

            // 房主才能开始游戏
            if (room.roomMemberConnectionList[0] != connection)
            {
                mainPack.ReturnCode = ReturnCode.Fail;
                return mainPack;
            }

            // 告诉客户端游戏开始
            mainPack.ReturnCode = ReturnCode.Success;

            // 广播消息
            room.Broadcast(mainPack, connection);

            return mainPack;
        }

    }
}