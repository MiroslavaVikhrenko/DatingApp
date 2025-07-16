import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ToastService {
  constructor() {
    this.createToastContainer();
  }

  // service = singleton
  private createToastContainer() {
    if (!document.getElementById('toast-container')) {
      const conatiner = document.createElement('div');
      conatiner.id = 'toast-container';
      conatiner.className = 'toast toast-bottom toast-end z-50'; //bottom right of the screen
      document.body.appendChild(conatiner);
    }
  }

  private createToastElement(
    message: string,
    alertClass: string,
    duration = 5000
  ) {
    const toastContainer = document.getElementById('toast-container');
    if (!toastContainer) return; // we will have, doing the check for typescript

    const toast = document.createElement('div');
    toast.classList.add('alert', alertClass, 'shadow-lg');
    toast.innerHTML = `
      <span>${message}</span>
      <button class="ml-4 btn btn-sm btn-ghost">x</button>
    `;
    toast.querySelector('button')?.addEventListener('click', () => {
      toastContainer.removeChild(toast);
    });

    toastContainer.append(toast);

    setTimeout(() => {
      if (toastContainer.contains(toast)) {
        toastContainer.removeChild(toast);
      }
    }, duration);
  }

  success(message: string, duration?: number) {
    this.createToastElement(message, 'alert-success', duration);
  }

  error(message: string, duration?: number) {
    this.createToastElement(message, 'alert-error', duration);
  }

  warning(message: string, duration?: number) {
    this.createToastElement(message, 'alert-warning', duration);
  }

  info(message: string, duration?: number) {
    this.createToastElement(message, 'alert-info', duration);
  }
}
