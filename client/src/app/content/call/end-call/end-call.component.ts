import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Member } from 'src/app/common/_models/member';
import { User } from 'src/app/common/_models/user';
import { AccountService } from 'src/app/common/_services/account.service';

@Component({
  selector: 'app-end-call',
  templateUrl: './end-call.component.html',
  styleUrls: ['./end-call.component.css']
})
export class EndCallComponent {

  currentUser!: User;
  member: Member = {} as Member ;
  constructor(private route: ActivatedRoute, private accountService: AccountService) {
   this.accountService.currentUser$.subscribe(user => {
    if (user) {
      this.currentUser = user;
    }
  });
  this.route.data.subscribe({
    next: data => {
      this.member = data['member'];
    }
  })
}
  onClick() {
    window.close();
  }
}
