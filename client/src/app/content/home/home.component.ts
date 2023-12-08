import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { AccountService } from 'src/app/common/_services/account.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  isLoggedIn = false;
  constructor(private accountService: AccountService) { }

  ngOnInit() {
    this.accountService.currentUser$.subscribe(user => {
      this.isLoggedIn = user !== null;
    });
  }
  
} 