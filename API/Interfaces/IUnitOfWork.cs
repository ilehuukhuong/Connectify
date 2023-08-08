namespace API.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository {get;}
        IMessageRepository MessageRepository {get;}
        ILikesRepository LikesRepository {get;}
        IGenderRepository GenderRepository {get;}
        ILookingForRepository LookingForRepository {get;}
        IInterestRepository InterestRepository {get;}
        ICityRepository CityRepository {get;}
        Task<bool> Complete();
        bool HasChanges();
    }
}