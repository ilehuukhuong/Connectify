import { ChangeDetectorRef, Component, ElementRef, HostListener, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { take } from 'rxjs';
import { ConnectedMessage } from 'src/app/common/_models/connectedMessage';
import { Member } from 'src/app/common/_models/member';
import { Message } from 'src/app/common/_models/message';
import { Pagination } from 'src/app/common/_models/pagination';
import { User } from 'src/app/common/_models/user';
import { AccountService } from 'src/app/common/_services/account.service';
import { MembersService } from 'src/app/common/_services/members.service';
import { MessageService } from 'src/app/common/_services/message.service';
import { PresenceService } from 'src/app/common/_services/presence.service';
import { environment } from 'src/environments/environment';


@Component({
  selector: 'app-user-messages',
  templateUrl: './user-messages.component.html',
  styleUrls: ['./user-messages.component.css']
})
export class UserMessagesComponent implements OnInit, OnDestroy {
  @HostListener('scroll', ['$event'])
  @ViewChild('scrollMe') private myScrollContainer: ElementRef | undefined;
  @ViewChild('scrollConnected') private scrollableElementRef: ElementRef | undefined;
  @ViewChild('scrollConnectedMessage') private scrollable: ElementRef | undefined;
  username?: string;
  messageContent = '';
  private currentScrollHeight = 0;
  loading = false;
  loadingfile = false;
  loadinglocation = false;
  file64?: string;
  fileName?: string;
  mapURL?: string;
  zoomLevel: number = 14;
  private apiMapKey = environment.apiMapKey;
  selectedFile?: File;
  messages?: Message[];
  loadingOldMessages = false;
  member: Member = {} as Member;
  callUrl = environment.callUrl;
  isMessageMenuVisible: boolean = false;
  isEmojiMenuVisible: boolean = false;
  isChatinputMoreMenuVisible: boolean = false;
  isMessageBoxVisible = false;
  timeoutId: any;
  user!: User;
  connectedMessage: ConnectedMessage[]=[];
  pagination?: Pagination;
  pageNumber = 1;
  pageSize = 50;
  paginationListUserMessage?: Pagination;
  pageNumberListUserMessage = 1;
  pageSizeListUserMessage = 50;
  predicate = 'connected';
  members: Member[] =[];
  search = '';
  fullName = '';
  lastMessageId?: number;
  activeTab: string = 'chat';
  selectedMessageId?: number;
  hoveredMessageId: number | undefined;
  hoverTimeoutId?: number;
  constructor(public messageService: MessageService, private cdr: ChangeDetectorRef, private route: ActivatedRoute,
    public presenceService: PresenceService, private memberService: MembersService, public accountService: AccountService, private router: Router) {
    this.messageService.messageThread$.subscribe(
      messages => {
        this.messages = messages;
        this.loadConnectedMessages();
        
        if (this.messages.length > 0) {
          // Sắp xếp mảng tin nhắn theo ID tăng dần và lấy ID đầu tiên làm lastMessageId
          this.lastMessageId = this.messages.map(message => message.id).sort((a, b) => a - b)[0];
        }
        this.handleNewMessages();
      }
    );
    this.route.data.subscribe({
      next: data => {
        this.member = data['member'];
      }
    })
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        if (user) this.user = user;
      }
    });
    this.router.routeReuseStrategy.shouldReuseRoute = () => false;
    this.route.params.subscribe(params => {
      const username = params['username'];
      if (username) {
        this.username = username;
      }
    });
  }
  ngOnDestroy() {
    this.messageService.stopHubConnection();
  }
  ngOnInit() {
    this.messageService.createHubConnection(this.user, this.member.userName);
    this.loadLikes();
    this.loadMoreMessages();
  }
  onConnectedScroll(event: any) {
    const target = event.target;
    const atBottom = target.scrollHeight - target.scrollTop >= target.clientHeight;

    // Kiểm tra xem người dùng đã cuộn đến cuối danh sách chưa
    if (atBottom && !this.loading && (this.pagination && this.pageNumber < this.pagination.totalPages)) {

      this.pageNumber++;
      this.loadLikes();
    }
  }

  onListUserMessageScroll(event: any) {
    const target = event.target;
    const atBottom = target.scrollHeight - target.scrollTop >= target.clientHeight - 100;

    // Kiểm tra xem người dùng đã cuộn đến cuối danh sách chưa
    if (atBottom && !this.loading && (this.paginationListUserMessage && this.pageNumberListUserMessage < this.paginationListUserMessage.totalPages)) {
      this.loadMoreConnectedMessages();
    }
  }
  onScroll(event: any) {
    const element = event.target;
    this.currentScrollHeight = element.scrollHeight - element.scrollTop;
    // Kiểm tra nếu scroll lên trên cùng
    if (element.scrollTop === 0) {
      this.loadMoreMessages();
    }
  }
  scrollToBottom(): void {
    try {
      this.myScrollContainer!.nativeElement.scrollTop = this.myScrollContainer!.nativeElement.scrollHeight;
    } catch (err) { }
  }
  loadMoreConnectedMessages() {
    if (this.loading) return;
    this.loading = true;
    this.pageNumberListUserMessage++;
    this.messageService.getconnectedMessages(this.pageNumberListUserMessage, this.pageSizeListUserMessage, this.fullName).subscribe({
      next: response => {
        if (response.result) {
          this.connectedMessage = [...this.connectedMessage, ...response.result];
        }
        this.paginationListUserMessage = response.pagination;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    })
  }
  loadConnectedMessages() {
    this.pageNumberListUserMessage = 1;
    this.loading = true;
    this.messageService.getconnectedMessages(1, this.pageSizeListUserMessage, this.fullName).subscribe({
      next: response => {
        this.connectedMessage = response.result ?? [];
        this.paginationListUserMessage = response.pagination;
        this.loading = false;
        this.cdr.detectChanges();
        if (!this.loadingOldMessages) {
          // Chỉ cuộn xuống cuối nếu không đang tải tin nhắn cũ
          this.scrollToBottom();
        }
      }
    })
  }
  loadSearchLikeConnected() {
    this.pageNumber = 1;
    this.loading = true;
    this.memberService.getLikes(this.predicate, this.pageNumber, this.pageSize, this.search).subscribe({
      next: response => {
        this.members = response.result ?? [];
        this.pagination = response.pagination;
        this.loading = false;
        this.cdr.detectChanges();
        if (!this.loadingOldMessages) {
          // Chỉ cuộn xuống cuối nếu không đang tải tin nhắn cũ
          this.scrollToBottom();
        }
      }
    })
  }

  loadLikes() {
    this.loading = true;
    this.memberService.getLikes(this.predicate, this.pageNumber, this.pageSize, this.search).subscribe({
      next: response => {
        if (response.result) {
          this.members = [...this.members, ...response.result];
        }
        this.pagination = response.pagination;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }
  


  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.selectedFile = input.files[0];
      this.fileName = input.files[0].name;
      this.sendCombinedMessage();
      this.messageContent = '';
    }
  }

  async sendCombinedMessage() {
    if (!this.username) return;

    if (this.selectedFile) {
      this.loadingfile = true;
      try {
        await this.messageService.sendFileMessage(this.username, this.selectedFile);
        this.selectedFile = undefined;
        this.fileName = undefined;
      } catch (error) {
        console.error("Failed to send file message:", error);
      } finally {
        this.loadingfile = false;
      }
    } else {
      if (this.messageContent.trim() === '') return;  
      this.loading = true;
      try {
        await this.messageService.sendMessage(this.username, this.messageContent);
        this.messageContent = '';
      } catch (error) {
        console.error("Failed to send text message:", error);
      } finally {
        this.loading = false;
      }
    }
  }

  async sendLocationMessage() {
    if (!this.username) return;
    this.loadinglocation = true;
    try {
      await this.messageService.createLocationMessage(this.username).then();
    } catch (error) {
      console.error("Failed to send location message:", error);
    } finally {
      this.loadinglocation = false;
    }
  }
  getMapUrl(latitude: string, longitude: string): string {
    const apiKey = this.apiMapKey;
    const markerSize = this.calculateMarkerSize(this.zoomLevel);
    return `https://image.maps.ls.hereapi.com/mia/1.6/mapview?apiKey=${apiKey}&lat=${latitude}&lon=${longitude}&z=${this.zoomLevel}&poi=${latitude},
    ${longitude}&poisize=${markerSize}&w=600&h=400&ppi=320&poithm=2`;
  }
  increaseZoom() {
    if (this.zoomLevel < 20) this.zoomLevel++;
  }

  decreaseZoom() {
    if (this.zoomLevel > 0) this.zoomLevel--;
  }
  calculateMarkerSize(zoomLevel: number): number {
    const minZoom = 0;
    const maxZoom = 20;

    const minSize = 10;
    const maxSize = 50;

    return minSize + (maxSize - minSize) * (zoomLevel - minZoom) / (maxZoom - minZoom);
  }

  deleteMessage(id: number) {
    this.messageService.deleteMessage(id).subscribe({
      next: () => {
        this.toggleMessageBox(id);
        this.messages?.splice(this.messages.findIndex(m => m.id === id), 1);
        this.cdr.detectChanges();
      }
    });
  }
  unsendMessage(id: number) {
    this.messageService.unsendMessage(id).subscribe({
      next: () => {
        // this.messages?.splice(this.messages.findIndex(m => m.id === id), 1);
        this.toggleMessageBox(id);
        this.cdr.detectChanges();
      }
    });
  }

  callOpen() {
    window.open(this.callUrl + this.username, '_blank', 'height=600,width=800')
  }

  toggleMessageBox(id: number) {
    this.selectedMessageId = id;
    this.isMessageBoxVisible = !this.isMessageBoxVisible;
  }

  handleHover(id: number, isHovering: boolean) {
    if (this.hoverTimeoutId) {
      clearTimeout(this.hoverTimeoutId);
      this.hoverTimeoutId = undefined;
    }
  
    if (isHovering) {
      this.hoveredMessageId = id;
    } else {
      this.hoverTimeoutId = setTimeout(() => {
        if (this.hoveredMessageId === id) {
          this.hoveredMessageId = undefined;
          // Ẩn toggleMessageBox nếu nó đang hiển thị
          if (this.selectedMessageId === id && this.isMessageBoxVisible) {
            this.isMessageBoxVisible = false;
            this.selectedMessageId = undefined;
          }
        }
      }, 10000) as unknown as number; // Thời gian trễ để ẩn kí tự và toggleMessageBox
    }
  }


  toggleMessageMenu() {
    this.isMessageMenuVisible = !this.isMessageMenuVisible;
  }

  toggleChatinputMoreMenu() {
    this.isChatinputMoreMenuVisible = !this.isChatinputMoreMenuVisible;
  }

  // Gọi phương thức này khi người dùng muốn tải thêm tin nhắn
  loadMoreMessages() {
    if (!this.username || !this.lastMessageId || !this.messageService) return;

    this.loadingOldMessages = true;
    this.messageService.loadMoreMessages(this.username, this.lastMessageId).then(() => {
      this.loadingOldMessages = false;
    });
  }

  handleNewMessages(): void {
    setTimeout(() => {
      const currentScrollTop = this.myScrollContainer?.nativeElement.scrollTop;
      console.log('currentScrollTop = '+ currentScrollTop);
      const newScrollHeight = this.myScrollContainer?.nativeElement.scrollHeight;
      console.log('newScrollHeight = '+ newScrollHeight);
  
      // Nếu đang không tải tin nhắn cũ, cuộn xuống cuối
      if (!this.loadingOldMessages) {
        console.log('không tải tin nhắn cũ');
        console.log('newScrollHeight = '+ newScrollHeight);
        this.myScrollContainer!.nativeElement.scrollTop = newScrollHeight;
      } else {
        // Nếu đang tải tin nhắn cũ, cập nhật vị trí cuộn để giữ nguyên vị trí hiện tại của người dùng
        this.myScrollContainer!.nativeElement.scrollTop = newScrollHeight - this.currentScrollHeight + currentScrollTop;
        console.log('tải tin nhắn cũ');
        console.log('newScrollHeight = '+ newScrollHeight);
        console.log('currentScrollHeight = '+ this.currentScrollHeight);
        console.log('currentScrollTop = '+ currentScrollTop);
        console.log('scoll: ' + (newScrollHeight - this.currentScrollHeight + currentScrollTop));
      }
    });
  }

  setActiveTab(tabName: string) {
    this.activeTab = tabName;
  }
}