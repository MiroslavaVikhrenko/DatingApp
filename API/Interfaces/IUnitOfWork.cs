using System;

namespace API.Interfaces;

public interface IUnitOfWork
{
    IMemberRepository MemberRepository { get; }
    IMessageRepository MessageRepository { get; }
    ILikesRepository LikesRepository { get; }
    IPhotoRepository PhotoRepository { get; }
    Task<bool> Complete(); // for saving changes no matter which repository was responsible for the updates
    bool HasChanges(); // to check the EF change tracker to see if there are any changes before attempt to save to db
}
