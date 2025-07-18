import { Component, computed, inject, input } from '@angular/core';
import { Member } from '../../../types/member';
import { RouterLink } from '@angular/router';
import { AgePipe } from '../../../core/pipes/age-pipe';
import { LikesService } from '../../../core/services/likes-service';
import { PresenceService } from '../../../core/services/presence-service';

@Component({
  selector: 'app-member-card',
  imports: [RouterLink, AgePipe],
  templateUrl: './member-card.html',
  styleUrl: './member-card.css'
})
export class MemberCard {
  private likeService = inject(LikesService);
  private presenceService = inject(PresenceService);
  member = input.required<Member>();
  // computed signal to check if the user has liked the member from membercard
  protected hasLiked = computed(() => this.likeService.likeIds().includes(this.member().id)); 
  // another computed signal => presence indicator for template to show if user is online
  protected isOnline = computed(() => this.presenceService.onlineUsers().includes(this.member().id));

  toggleLike(event: Event) {
    // to prevent redirecting to individula member's details page when liked/unliked
    event.stopPropagation();
    this.likeService.toggleLike(this.member().id)
  }
}
