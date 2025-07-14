import { Component, computed, inject, input } from '@angular/core';
import { Member } from '../../../types/member';
import { RouterLink } from '@angular/router';
import { AgePipe } from '../../../core/pipes/age-pipe';
import { LikesService } from '../../../core/services/likes-service';

@Component({
  selector: 'app-member-card',
  imports: [RouterLink, AgePipe],
  templateUrl: './member-card.html',
  styleUrl: './member-card.css'
})
export class MemberCard {
  private likeService = inject(LikesService);
  member = input.required<Member>();
  // computed signal to check if the user has liked the member from membercard
  protected hasLiked = computed(() => this.likeService.likeIds().includes(this.member().id)); 

  toggleLike(event: Event) {
    // to prevent redirecting to individula member's details page when liked/unliked
    event.stopPropagation();
    this.likeService.toggleLike(this.member().id).subscribe({
      next: () => {
        if (this.hasLiked()) {
          // unlike
          this.likeService.likeIds.update(ids => ids.filter(x => x !== this.member().id))
        } else {
          // like
          this.likeService.likeIds.update(ids => [...ids, this.member().id])
        }
      }
    })
  }
}
