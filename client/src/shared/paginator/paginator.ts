import { Component, computed, input, model, output} from '@angular/core';

@Component({
  selector: 'app-paginator',
  imports: [],
  templateUrl: './paginator.html',
  styleUrl: './paginator.css'
})
export class Paginator {
  // using different types of signals: model, computed
  pageNumber = model(1); //writable
  pageSize = model(10);
  totalCount = input(0); //no need to be writable
  totalPages = input(0);
  pageSizeOptions = input([5, 10, 20, 50]);

  pageChange = output<{pageNumber: number, pageSize: number}>();

  lastItemIndex = computed(() => {
    return Math.min(this.pageNumber() * this.pageSize(), this.totalCount()); // returns lower of these 2 numbers
  })

  onPageChange(newPage?: number, pageSize?: EventTarget | null) {
    if (newPage) this.pageNumber.set(newPage);
    if (pageSize) {
      const size = Number((pageSize as HTMLSelectElement).value)
      this.pageSize.set(size);
    } 

    this.pageChange.emit({
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize()
    })
  }
}
