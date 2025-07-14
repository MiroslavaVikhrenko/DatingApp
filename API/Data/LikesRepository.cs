using System;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class LikesRepository(AppDbContext context) : ILikesRepository
{
    public void AddLike(MemberLike like)
    {
        context.Likes.Add(like);
    }

    public void DeleteLike(MemberLike like)
    {
        context.Likes.Remove(like);
    }
    
    public async Task<IReadOnlyList<string>> GetCurrentMemberLikeIds(string memberId)
    {
        return await context.Likes
            .Where(x => x.SourceMemberId == memberId)
            .Select(x => x.TargetMemberId) //use projection (select) - who has user liked
            .ToListAsync();
    }

    public async Task<MemberLike?> GetMemberLike(string sourceMemberId, string targetMemberId)
    {
        return await context.Likes.FindAsync(sourceMemberId, targetMemberId);
    }

    public async Task<PaginatedResult<Member>> GetMemberLikes(LikesParams likesParams)
    {
        var query = context.Likes.AsQueryable(); // to build up an expression tree in switch 
        IQueryable<Member> result;

        switch (likesParams.Predicate)
        {
            case "liked":
                result = query
                    .Where(like => like.SourceMemberId == likesParams.MemberId)
                    .Select(like => like.TargetMember); // the list of users that the current user has liked
                break;
            case "likedBy":
                result = query
                    .Where(like => like.TargetMemberId == likesParams.MemberId)
                    .Select(like => like.SourceMember); // the list of users who liked the current user 
                break;
            default: // for mutual likes
                var likeIds = await GetCurrentMemberLikeIds(likesParams.MemberId);
                result = query
                    .Where(like => like.TargetMemberId == likesParams.MemberId
                        && likeIds.Contains(like.SourceMemberId))
                    .Select(like => like.SourceMember);
                break;
        }

        return await PaginationHelper.CreateAsync(result, likesParams.PageNumber, likesParams.PageSize);
    }

    public async Task<bool> SaveAllChanges()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
