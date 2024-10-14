using MTCG.Models;

namespace MTCG.Interfaces
{
    public interface IUserService
    {
        User Register(string username, string password);
        User GetUserByToken(string token);
        string Login(string username, string password);

    }
}