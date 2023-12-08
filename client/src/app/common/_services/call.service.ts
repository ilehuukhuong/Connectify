import { EventEmitter, Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HttpTransportType } from '@microsoft/signalr';
import { BehaviorSubject, take } from 'rxjs';
import { User } from '../_models/user';
import { environment } from 'src/environments/environment';
import { CreateCallDto, Room } from '../_models/call';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class CallService {
  baseUrl = environment.apiUrl;
  hubUrl = environment.hubUrl;
  private hubConnection?: HubConnection;
  private callStatusSource = new BehaviorSubject<CreateCallDto[]>([]);
  callStatus$ = this.callStatusSource.asObservable();
  private incomingCallSource = new BehaviorSubject<boolean>(false);
  incomingCall$ = this.incomingCallSource.asObservable();
  screenUpdate = new EventEmitter<void>();
  autoCloseTabTimer: any;

  constructor(private toastr: ToastrService, private router: Router) { }

  createHubConnection(user: User, otherUsername: string) {
    // Tạo kết nối tới CallHub
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'call?user=' + otherUsername, {
        accessTokenFactory: () => user.token
        //, transport: HttpTransportType.WebSockets | HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling
      })
      .withAutomaticReconnect()
      .build();

    // Bắt đầu kết nối
    this.hubConnection.start()
      .catch(error => console.log(error))
      .finally(() => { });

          // Bắt đầu kết nối
    this.hubConnection.on("StartCall", () =>{
      this.autoCloseTabTimer = setTimeout(() => {
        this.missCall(otherUsername);
        this.router.navigate(['/end-call/' + otherUsername]);
        this.toastr.warning('User Busy');
      }, 20000);
    });


    this.hubConnection.on('AcceptCall', () => {
      this.incomingCallSource.next(false);
      clearTimeout(this.autoCloseTabTimer);
      console.log("AcceptCall");
    });

    this.hubConnection.on('UserBusy', () => {
      this.router.navigate(['/end-call/' + otherUsername]);
      this.toastr.info('Miss Call');
    });

    this.hubConnection.on('UserOffline', () => {
      this.router.navigate(['/end-call/' + otherUsername]);
      this.toastr.info('User Busy');
    });

    this.hubConnection.on('MicroUpdate', () => {
    });

    this.hubConnection.on('CameraUpdate', () => {
    });

    this.hubConnection.on('ScreenUpdate', () => {
    });
    this.hubConnection.on('MissCall', () => {
      console.log("Missed Call");
      this.toastr.warning('Missed Call');
      this.incomingCallSource.next(false);
    });
    this.hubConnection.on('IncomingCall', () => {
      console.log("IncomingCall");
      this.incomingCallSource.next(true);
      this.toastr.info('Calling!!');
    });

    this.hubConnection.on('EndCall', () => {
      this.router.navigate(['/end-call/' + otherUsername]);
      this.toastr.info('End Call');
    });
  }

  stopHubConnection() {
    if (this.hubConnection) {
      this.callStatusSource.next([]);
      this.hubConnection.stop();
    }
  }
  acceptCall(createCallDto: CreateCallDto) {
    this.hubConnection?.invoke('AcceptCall', createCallDto)
      .catch(error => console.log(error));
  }

  updateMicrophone(createCallDto: CreateCallDto) {
    this.hubConnection?.invoke('Micro', createCallDto)
      .catch(error => console.log(error));
  }

  updateCamera(createCallDto: CreateCallDto) {
    this.hubConnection?.invoke('Camera', createCallDto)
      .catch(error => console.log(error));
  }

  updateScreen(createCallDto: CreateCallDto) {
    this.hubConnection?.invoke('Screen', createCallDto)
      .catch(error => console.log(error));
  }

  declineCall(callerUsername: string) {
    this.hubConnection?.invoke('Decline', callerUsername)
      .catch(error => console.log(error));
  }

  endCall(createCallDto: CreateCallDto) {
    this.hubConnection?.invoke('EndCall', createCallDto)
      .catch();
  }
  missCall(recipientUsername: string) {
    this.hubConnection?.invoke('MissCall', recipientUsername)
      .catch(error => console.log(error));
  }

}
