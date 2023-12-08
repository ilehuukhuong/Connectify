import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs';
import { User } from 'src/app/common/_models/user';
import { AccountService } from 'src/app/common/_services/account.service';
import { MembersService } from 'src/app/common/_services/members.service';

@Component({
  selector: 'app-account-setvisibility',
  templateUrl: './account-setvisibility.component.html',
  styleUrls: ['./account-setvisibility.component.css']
})
export class AccountSetvisibilityComponent {
  user: User | null = null;
  isVisible = false;

  constructor(private memberService: MembersService, private accountService: AccountService, private toastr: ToastrService, private router: Router) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => this.user = user
    })
  }
  ngOnInit(): void {
    this.loadVisibility();
  }
  loadVisibility() {
    if (!this.user) return;
    this.memberService.getMember(this.user.username).subscribe({
      next: member => {
        this.isVisible = member.isVisible;
      }
    })
  }

  setVisibility(status: boolean) {
    this.isVisible = status;
    if (status) {
      this.memberService.setVisible().subscribe({
        next: () => {
          this.toastr.success('Set visibility successfully')
        },
        error: error => console.log('Failed to update visibility: ', error)
      });
    } else {
      this.memberService.setInvisible().subscribe({
        next: () => {
          this.toastr.success('Set invisibility successfully')
        },
        error: error => console.log('Failed to update visibility: ', error)
      });
    }
  }
}