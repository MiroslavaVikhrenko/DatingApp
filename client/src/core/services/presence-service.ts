import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { ToastService } from './toast-service';
import { User } from '../../types/user';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
} from '@microsoft/signalr';
import { Message } from '../../types/message';

@Injectable({
  providedIn: 'root',
})
export class PresenceService {
  private hubUrl = environment.hubUrl;
  private toast = inject(ToastService);
  hubConnection?: HubConnection;
  onlineUsers = signal<string[]>([]);

  createHubConnection(user: User) {
    // build connection
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'presence', {
        // pass token as query string parameter when it negotiates with API server
        // about what connection protocols it supports for real time communication
        accessTokenFactory: () => user.token,
      })
      .withAutomaticReconnect()
      .build();

    // start the connection => this returns a promise that resolves when the connection has successfully established
    // (or rejects with error)
    this.hubConnection.start().catch((error) => console.log(error));

    this.hubConnection.on('UserOnline', (userId) => {
      this.onlineUsers.update((users) => [...users, userId]);
    });

    this.hubConnection.on('UserOffline', (userId) => {
      this.onlineUsers.update((users) => users.filter((x) => x !== userId));
    });

    this.hubConnection.on('GetOnlineUsers', (userIds) => {
      this.onlineUsers.set(userIds);
    });

    this.hubConnection.on('NewMessageReceived', (message: Message) => {
      this.toast.info(message.senderDisplayName + ' has sent you a new message', 
          10000, message.senderImageUrl, `/members/${message.senderId}/messages`);
    })
  }

  stopHubConnection() {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      //returns a promise
      this.hubConnection.stop().catch((error) => console.log(error));
    }
  }
}
