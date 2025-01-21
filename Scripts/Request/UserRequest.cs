using SocketProtocol;

namespace SocketServer
{
    public class UserRequest : BaseRequest
    {
        public MainPack QuickLogin(MainPack mainPack, Connection connection)
        {

            // 新建用户包
            int userId = IdGenerator.GenerateUserId();
            var userPack = new UserPack
            {
                UserId = userId,
                NickName = userId.ToString()
            };
            connection.userPack = userPack;

            // 告诉客户端自己的用户信息
            mainPack.UserPack.Clear();
            mainPack.UserPack.Add(userPack);
            mainPack.ReturnCode = ReturnCode.Success;

            return mainPack;
        }


        public MainPack UpdateNickName(MainPack mainPack, Connection connection)
        {
            if (mainPack.UserPack.Count > 0)
            {
                connection.userPack.NickName = mainPack.UserPack[0].NickName;
                mainPack.ReturnCode = ReturnCode.Success;
            }
            else
            {
                mainPack.ReturnCode = ReturnCode.Fail;
            }

            return mainPack;
        }
    }
}