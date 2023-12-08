import { ChangeDetectorRef, Component, OnInit , ViewChild, ElementRef} from '@angular/core';
import { Router } from '@angular/router';
import { ConnectedMessage } from 'src/app/common/_models/connectedMessage';
import { Member } from 'src/app/common/_models/member';
import { Message } from 'src/app/common/_models/message';
import { Pagination } from 'src/app/common/_models/pagination';
import { User } from 'src/app/common/_models/user';
import { AccountService } from 'src/app/common/_services/account.service';
import { MembersService } from 'src/app/common/_services/members.service';
import { PresenceService } from 'src/app/common/_services/presence.service';
import { MessageService } from 'src/app/common/_services/message.service';


@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  @ViewChild('scrollMe') private myScrollContainer: ElementRef | undefined;
  @ViewChild('scrollConnected') private scrollableElementRef: ElementRef | undefined;
  @ViewChild('scrollConnectedMessage') private scrollable: ElementRef | undefined;
  messages?: Message[];
  connectedMessage: ConnectedMessage[]=[];
  loading = false;
  member!: Member;
  users: User[] = [];
  userName: any;
  isMessageNavBoxVisible: boolean = false;
  isMessageMenuVisible: boolean = false;
  isEmojiMenuVisible: boolean = false;
  isChatinputMoreMenuVisible: boolean = false;
  isMessageBoxVisible = false;
  timeoutId: any;
  fullName = '';
  predicate = 'connected';
  loadingOldMessages = false;
  search = '';
  activeTab: string = 'chat';
  members: Member[] =[];
  pagination?: Pagination;
  pageNumber = 1;
  pageSize = 50;
  paginationListUserMessage?: Pagination;
  pageNumberListUserMessage = 1;
  pageSizeListUserMessage = 50;

  constructor(private messageService: MessageService, public presenceService: PresenceService, private cdr: ChangeDetectorRef, private memberService: MembersService, public accountService: AccountService, private router: Router) {
  }

  ngOnInit(): void {
    this.loadConnectedMessages();
    this.loadLikes();
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
  setActiveTab(tabName: string) {
    this.activeTab = tabName;
  }
  loadMoreConnectedMessages() {
    debugger
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
  scrollToBottom(): void {
    try {
      this.myScrollContainer!.nativeElement.scrollTop = this.myScrollContainer!.nativeElement.scrollHeight;
    } catch (err) { }
  }

  pageChanged(event: any) {
    if (this.pageNumber !== event.page) {
      this.pageNumber = event.page;
      this.ngOnInit();
    }
  }



  toggleMessageBox(visible: boolean) {
    // Delete the timer when the box is hidden before
    if (this.timeoutId) {
      clearTimeout(this.timeoutId);
    }

    if (visible) {
      this.isMessageBoxVisible = true;
    } else {
      // box hidden, delay 1s
      this.timeoutId = setTimeout(() => {
        this.isMessageBoxVisible = false;
      }, 1000);  // Delay time
    }
  }

  toggleMessageNavBox() {
    this.isMessageNavBoxVisible = !this.isMessageNavBoxVisible;
  }

  toggleMessageMenu() {
    this.isMessageMenuVisible = !this.isMessageMenuVisible;
  }

  toggleEmojiMenu() {
    this.isEmojiMenuVisible = !this.isEmojiMenuVisible;
  }

  toggleChatinputMoreMenu() {
    this.isChatinputMoreMenuVisible = !this.isChatinputMoreMenuVisible;
  }

}
