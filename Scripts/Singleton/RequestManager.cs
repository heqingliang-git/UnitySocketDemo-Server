using System.Reflection;
using Google.Protobuf;
using SocketProtocol;

namespace SocketServer
{
    public class RequestManager : Singleton<RequestManager>
    {
        public readonly Dictionary<RequestCode, BaseRequest> requestDic = [];

        public RequestManager()
        {
            requestDic.Add(RequestCode.User, new UserRequest());
            requestDic.Add(RequestCode.Room, new RoomRequest());
        }

        /// <summary>
        /// 处理接收到的数据包
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="connection"></param>
        public void HandleBuffer(byte[] buffer, Connection connection)
        {
            MainPack mainPack = MainPack.Parser.ParseFrom(buffer);
            if (requestDic.TryGetValue(mainPack.RequestCode, out BaseRequest request))
            {
                MethodInfo methodInfo = request.GetType().GetMethod(mainPack.ActionCode.ToString());
                if (methodInfo == null)
                {
                    return;
                }
                object ret = methodInfo.Invoke(request, [mainPack, connection]);
                if (ret != null)
                {
                    connection.SendMsg((ret as MainPack).ToByteArray());
                }
            }
        }
    }
}