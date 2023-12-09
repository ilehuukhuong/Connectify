import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import Peer from 'peerjs';
import { CreateCallDto } from 'src/app/common/_models/call';
import { Member } from 'src/app/common/_models/member';
import { User } from 'src/app/common/_models/user';
import { AccountService } from 'src/app/common/_services/account.service';
import { CallService } from 'src/app/common/_services/call.service';

@Component({
  selector: 'app-call',
  templateUrl: './call.component.html',
  styleUrls: ['./call.component.css']
})
export class CallComponent implements OnInit, OnDestroy {
  currentUser!: User;
  myPeer: any;
  peerId: string = ''; // Người dùng muốn gọi
  stream!: MediaStream;
  otherUserStream: MediaStream | undefined;
  enableVideo = true;
  enableAudio = true;
  @ViewChild('localVideo') localVideo!: ElementRef;
  @ViewChild('remoteVideo') remoteVideo!: ElementRef;
  @ViewChild('screenVideo') screenVideo!: ElementRef;
  @ViewChild('otherScreenVideo') otherScreenVideo!: ElementRef;
  @ViewChild('incomingCallModal') incomingCallModal!: ElementRef;
  currentCameraStream: MediaStream | undefined;
  showIncomingCallButtons: boolean = false;
  otherPeerId: any;
  isScreenSharing: boolean = false;
  screenStream: MediaStream | null = null;
  autoDenyTimer: any;
  autoCallUserTimer: any;
  isOtherScreenSharing = false;
  member: Member = {} as Member ;
  constructor(private accountService: AccountService, private callService: CallService, private route: ActivatedRoute, private router: Router) {
    this.accountService.currentUser$.subscribe(user => {
      if (user) {
        this.currentUser = user;
      }
    });
    this.route.params.subscribe(params => {
      const usernameToCall = params['username'];
      if (usernameToCall) {
        this.callService.createHubConnection(this.currentUser, usernameToCall);
      }
      if (usernameToCall) {
        this.otherPeerId = usernameToCall;
      }
    });
    this.route.data.subscribe({
      next: data => {
        this.member = data['member'];
      }
    })

    this.myPeer = new Peer(this.currentUser.username, {
      config: {
        'iceServers': [
          {
            urls: "stun:stun.l.google.com:19302",
          },
          {
            urls: "turn:numb.viagenie.ca",
            username: "webrtc@live.com",
            credential: "muazkh"
          }
        ]
      }
    });
    this.myPeer.on('open', (id: string) => {
      this.peerId = id; // Lưu lại peer ID của người dùng hiện tại
    });

  }
  ngOnInit(): void {
    this.createLocalStream();
    this.handleIncomingCalls();
    this.callService.incomingCall$.subscribe(incoming => {
      this.showIncomingCallButtons = incoming;
    });
    console.log(this.member);
  }

  async createLocalStream() {
      const mediaConstraints = {
        video: this.enableVideo,
        audio: this.enableAudio
      };
      console.log(this.enableVideo);
      console.log(this.enableAudio);
      try {
      this.stream = await navigator.mediaDevices.getUserMedia(mediaConstraints);
      if (this.localVideo) {
        this.localVideo.nativeElement.srcObject = this.stream;
        this.localVideo.nativeElement.load();
        await this.localVideo.nativeElement.play();
      }
    } catch (error) {
      console.error("Error getting user media:", error);
    }
  }
  enableOrDisableVideo() {
    this.enableVideo = !this.enableVideo;
    if (this.stream && this.stream.getVideoTracks().length > 0) {
      this.stream.getVideoTracks()[0].enabled = this.enableVideo;
    }
    const createCallDto = this.createCallDto();
    this.callService.updateCamera(createCallDto);
  }

  enableOrDisableAudio() {
    this.enableAudio = !this.enableAudio;
    if (this.stream && this.stream.getAudioTracks().length > 0) {
      this.stream.getAudioTracks()[0].enabled = this.enableAudio;
    }
    const createCallDto = this.createCallDto();
    this.callService.updateMicrophone(createCallDto);
  }

  callUser() {
    if (this.myPeer && this.otherPeerId) {
      const call = this.myPeer.call(this.otherPeerId, this.stream);
      call.on('stream', (userVideoStream: MediaStream) => {
        this.currentCameraStream = userVideoStream;
        if (this.isScreenSharing) {
          // Hiển thị luồng màn hình trên thẻ video màn hình (screenVideo)
          this.screenVideo.nativeElement.srcObject = userVideoStream;
          this.screenVideo.nativeElement.load();
          this.screenVideo.nativeElement.play();
          this.isScreenSharing = false;  // Reset cờ sau khi xử lý luồng màn hình
        } else {
          // Hiển thị luồng video trên thẻ video từ xa (remoteVideo)
          clearTimeout(this.autoCallUserTimer);
          this.remoteVideo.nativeElement.srcObject = userVideoStream;
          this.remoteVideo.nativeElement.load();
          this.remoteVideo.nativeElement.play();
        }
      });
    } else {
      console.error("My Peer object or peer ID is null.");
    }
  }
  handleIncomingCalls() {
    this.myPeer.on('call', (call: any) => {
      call.answer(this.stream);
      call.on('stream', (userVideoStream: MediaStream) => {
        if (this.isScreenSharing) {
          this.otherScreenVideo.nativeElement.srcObject = userVideoStream;  // Chỉ định màn hình chia sẻ của B hiển thị ở otherScreenVideo
          this.otherScreenVideo.nativeElement.load();
          this.otherScreenVideo.nativeElement.play();
          this.isScreenSharing = false;  // Reset flag after handling screen stream

        } else {
          this.remoteVideo.nativeElement.srcObject = userVideoStream;  // Chỉ định màn hình video của B hiển thị ở remoteVideo
          this.remoteVideo.nativeElement.load();
          this.remoteVideo.nativeElement.play();
        }
      });
    });
  }

  endCall() {
    this.callService.endCall(this.otherPeerId);
  }
  acceptIncomingCall() {
    const createCallDto = this.createCallDto();
    this.callService.acceptCall(createCallDto)
    this.showIncomingCallButtons = false;
    this.callUser();
  }

  denyIncomingCall() {
    const callerUsername = this.otherPeerId;
    this.callService.declineCall(callerUsername)
    this.showIncomingCallButtons = false;
  }
  async getScreenStream() {
    try {
      const screenStream = await navigator.mediaDevices.getDisplayMedia({ video: true });
      return screenStream;
    } catch (error) {
      console.error('Error getting screen stream:', error);
      return null;
    }
  }
  async shareScreen() {
    const screenStream = await this.getScreenStream();
    this.isScreenSharing = true;
    if (screenStream) {
      this.hideScreenShare();
      this.screenVideo.nativeElement.classList.remove('hidden');  // Chỉ định màn hình chia sẻ của A hiển thị ở screenVideo
      if (this.myPeer && this.otherPeerId) {
        const call = this.myPeer.call(this.otherPeerId, screenStream);
        call.on('stream', (userVideoStream: MediaStream) => {
          this.otherUserStream = userVideoStream;
          this.screenVideo.nativeElement.srcObject = screenStream;  // Chỉ định màn hình chia sẻ của A hiển thị ở screenVideo
          this.screenVideo.nativeElement.load();
          this.screenVideo.nativeElement.play();
        });
        screenStream.getTracks().forEach(track => {
          track.onended = () => {
            this.stopScreenSharing();
            this.hideScreenShare();
          };
        });
        const createCallDto = this.createCallDto();
        this.callService.updateScreen(createCallDto);
      } else {
        console.error('My Peer object or peer ID is null.');
      }
    }
  }

  hideScreenShare() {
    this.screenVideo.nativeElement.classList.add('hidden');
  }
  showVideoCall() {
    this.screenVideo.nativeElement.classList.remove('hidden');
  }
  async stopScreenSharing() {
    const screenStream = this.screenVideo.nativeElement.srcObject as MediaStream;
    this.isScreenSharing = false;
    if (screenStream && screenStream instanceof MediaStream) {
      screenStream.getTracks().forEach((track) => {
        track.stop();
        this.isScreenSharing = false;
      });
    }
    this.callUser();
    this.showVideoCall();
    const createCallDto = this.createCallDto();
    this.callService.updateScreen(createCallDto);
  }
  toggleScreenSharing() {
    if (this.isScreenSharing) {
      this.stopScreenSharing();
    } else {
      this.shareScreen();
    }
  }
  private createCallDto(): CreateCallDto {
    return {
      recipientUsername: this.peerId,
      callerUsername: this.otherPeerId,
      isVoiceEnabled: this.enableAudio,
      isVideoEnabled: this.enableVideo,
      isScreenSharingEnabled: this.isScreenSharing
    };
  }
  ngOnDestroy(): void {
    if (this.myPeer && this.otherPeerId) {
      this.myPeer.destroy();
      this.callService.stopHubConnection();
    }
  }

}