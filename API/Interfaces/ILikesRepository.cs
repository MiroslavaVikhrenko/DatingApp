using System;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface ILikesRepository
{
    Task<MemberLike?> GetMemberLike(string sourceMemberId, string targetMemberId);
    // predicate => this is going to define what kind of list needs to be returned: 
    // list of users you liked? list of users who liked you? mutual likes?
    Task<PaginatedResult<Member>> GetMemberLikes(LikesParams likesParams);
    // get a list of the IDs of the members that the current user has liked to display on UI
    Task<IReadOnlyList<string>> GetCurrentMemberLikeIds(string memberId);
    void DeleteLike(MemberLike like);
    void AddLike(MemberLike like);
    Task<bool> SaveAllChanges();
}
