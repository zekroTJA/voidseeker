namespace RESTAPI.Hashing
{
    public interface IHasher
    {
        byte[] Create(string password);
        bool Validate(string password, byte[] hash);
    }
}
