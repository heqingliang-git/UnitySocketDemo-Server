using SocketProtocol;

namespace SocketServer
{
    public class UserRequest : BaseRequest
    {
        public MainPack QuickLogin(MainPack mainPack, Connection connection)
        {
            try
            {
                // 新建用户包
                int userId = IdGenerator.GenerateUserId();
                var userPack = new UserPack
                {
                    UserId = userId,
                    NickName = userId.ToString()
                };
                connection.userPack = userPack;

                // 更新数据包
                mainPack.UserPack.Clear();
                mainPack.UserPack.Add(userPack);
                mainPack.ReturnCode = ReturnCode.Success;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                mainPack.ReturnCode = ReturnCode.Fail;
            }
            return mainPack;
        }


        public MainPack UpdateNickName(MainPack mainPack, Connection connection)
        {
            try
            {
                connection.userPack.NickName = mainPack.UserPack[0].NickName;
                mainPack.ReturnCode = ReturnCode.Success;
            }
            catch
            {
                mainPack.ReturnCode = ReturnCode.Fail;
            }
            return mainPack;
        }
    }
}