import { Component, inject, OnInit, signal, ViewChild } from '@angular/core';
import { MemberService } from '../../../core/services/member-service';
import { Member, MemberParams } from '../../../types/member';
import { MemberCard } from "../member-card/member-card";
import { PaginatedResult } from '../../../types/pagination';
import { Paginator } from "../../../shared/paginator/paginator";
import { FilterModal } from '../filter-modal/filter-modal';

@Component({
  selector: 'app-member-list',
  imports: [MemberCard, Paginator, FilterModal],
  templateUrl: './member-list.html',
  styleUrl: './member-list.css'
})
export class MemberList implements OnInit {
  @ViewChild('filterModal') modal!: FilterModal;
  private memberService = inject(MemberService);
  protected paginatedMembers = signal<PaginatedResult<Member> | null>(null);
  protected memberParams = new MemberParams();
  // to display accurate applied filters - avoid displaying filters that were selected on form but not submitted for API
  private updatedParams = new MemberParams(); // not part of 2-way binding 

  constructor() {
    const filters = localStorage.getItem('filters');
    if (filters) {
      this.memberParams = JSON.parse(filters);
      this.updatedParams = JSON.parse(filters);
    }
  }

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers() {
    this.memberService.getMembers(this.memberParams).subscribe({
      next: result => {
        this.paginatedMembers.set(result)
      }
    })
  }

  onPageChange(event: {pageNumber: number, pageSize: number}) {
    this.memberParams.pageSize = event.pageSize;
    this.memberParams.pageNumber = event.pageNumber;
    this.loadMembers();
  }

  openModal() {
    this.modal.open();
  }

  onClose() {
    console.log('Modal closed');
  }

  onFilterChange(data: MemberParams) {
    // create shallow copy so that they won't reference the same object in memory {...data}
    this.memberParams = {...data};
    this.updatedParams = {...data};
    this.loadMembers();
  }

  resetFilters() {
    this.memberParams = new MemberParams();
    this.updatedParams = new MemberParams();
    this.loadMembers();
  }

  // helper function to display chosen filters
  get displayMessage(): string {
    const defaultParams = new MemberParams();
    const filters: string[] = [];

    if (this.updatedParams.gender) {
      filters.push(this.updatedParams.gender + 's');
    } else {
      filters.push('Males, Females');
    }

    if (this.updatedParams.minAge !== defaultParams.minAge 
        || this.updatedParams.maxAge !== defaultParams.maxAge) {
          filters.push(` ages ${this.updatedParams.minAge}-${this.updatedParams.maxAge}`)
    }

    filters.push(this.updatedParams.orderBy === 'lastActive' ? 'Recently active' : 'Newest members');

    return filters.length > 0 ? `Selected: ${filters.join('  | ')}` : 'All members';
  }
}
