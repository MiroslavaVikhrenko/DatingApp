using System;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    // lazy initialize repositories => they are only created when they're first accessed
    // UnitOfWork is scoped => http request comes in => we're using one of the methods from repositories
    // => then if it's not already initialized it will get initialized based on the code here

    private IMemberRepository? _memberRepository;
    private IMessageRepository? _messageRepository;
    private ILikesRepository? _likesRepository;
    private IPhotoRepository? _photoRepository;

    public IMemberRepository MemberRepository => _memberRepository ??= new MemberRepository(context);

    public IMessageRepository MessageRepository => _messageRepository ??= new MessageRepository(context);

    public ILikesRepository LikesRepository => _likesRepository ??= new LikesRepository(context);

    public IPhotoRepository PhotoRepository => _photoRepository ??= new PhotoRepository(context);


    public async Task<bool> Complete()
    {
        try
        {
            return await context.SaveChangesAsync() > 0; // check if smth was actually changed in db
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occured while saving changes", ex);
        }
    }

    public bool HasChanges()
    {
        return context.ChangeTracker.HasChanges();
    }
}
