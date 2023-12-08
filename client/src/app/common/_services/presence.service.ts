import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HttpTransportType, HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject, take } from 'rxjs';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { MessageService } from './message.service';
import { MessagesComponent } from 'src/app/content/messages/messages.component';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnection?: HubConnection;
  private onlineUsersSource = new BehaviorSubject<string[]>([]);
  onlineUsers$ = this.onlineUsersSource.asObservable();
  callUrl = environment.callUrl;

  constructor(private toastr: ToastrService, private router: Router) { }

  createHubConnection(user: User) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'presence', {
        accessTokenFactory: () => user.token
        //, transport: HttpTransportType.WebSockets | HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch(error => console.log(error));

    this.hubConnection.on('UserIsOnline', username => {
      this.onlineUsers$.pipe(take(1)).subscribe({
        next: usernames => this.onlineUsersSource.next([...usernames, username])
      })
    })

    this.hubConnection.on('UserIsOffline', username => {
      this.onlineUsers$.pipe(take(1)).subscribe({
        next: usernames => this.onlineUsersSource.next(usernames.filter(x => x !== username))
      })
    })

    this.hubConnection.on('GetOnlineUsers', usernames => {
      this.onlineUsersSource.next(usernames);
    })

    this.hubConnection.on('NewMessageReceived', ({ username, knownAs }) => {
      this.toastr.info(knownAs + ' has sent you a new message! Click me to see it')
        .onTap
        .pipe(take(1))
        .subscribe({
          next: () => {
            this.router.navigateByUrl('/messages/' + username)
          }

        })
    })
    this.hubConnection.on('IncomingCall', ({ username, knownAs }) => {
      let timeoutId: any;

      const toast = this.toastr.info(knownAs + ' is calling you!', '', {
        timeOut: 20000 // Thời gian tự động ẩn thông báo là 30 giây
      });

      // Đặt hành động sau 30 giây
      timeoutId = setTimeout(() => {
        this.hubConnection?.invoke('Decline', username);
        this.toastr.warning('You missed ' + knownAs + ' call!')
      }, 20000);

      toast.onTap
        .pipe(take(1))
        .subscribe({
          next: () => {
            window.open(this.callUrl + username, '_blank', 'height=600,width=800');
            clearTimeout(timeoutId);  // Hủy bỏ hành động đã đặt nếu người dùng nhấp vào thông báo
          }
        });
    })
  }

  stopHubConnection() {
    this.hubConnection?.stop().catch(error => console.log(error));
  }
}