import { Component, OnInit } from '@angular/core';
import { Options } from 'ngx-slider-v2';
import { Member } from 'src/app/common/_models/member';
import { Pagination } from 'src/app/common/_models/pagination';
import { UserParams } from 'src/app/common/_models/userParams';
import { MembersService } from 'src/app/common/_services/members.service';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css']
})
export class ListsComponent implements OnInit {
  members: Member[] | undefined;
  predicate = 'liked';
  pageNumber = 1;
  pageSize = 8;
  pagination: Pagination | undefined;
  userParams: UserParams | undefined;
  search = '';
  showRecommendedFilters = false; 
  similaritySliderOptions: Options = {
    floor: 1,
    ceil: 10
  };
  similaritySliderValue = 10;


  constructor(private memberService: MembersService) {
    this.userParams = this.memberService.getUserParams();
    this.similaritySliderValue = this.userParams!.similarity;
   }

  ngOnInit(): void {
    this.loadLikes();
  }

  loadLikes() {
    this.showRecommendedFilters = false; 
    this.memberService.getLikes(this.predicate, this.pageNumber, this.pageSize, this.search).subscribe({
      next: response => {
        this.members = response.result;
        this.pagination = response.pagination;
        console.log(this.members);
      }
    })
  }
  loadRecommendedMembers() {
    this.showRecommendedFilters = true; 
    this.memberService.getRecommendedMembers(this.userParams!, this.pageNumber, this.pageSize).subscribe({
      next: response => {
        this.members = response.result;
        this.pagination = response.pagination;
        console.log(this.members);
      }
    })
  }

  pageChanged(event: any) {
    if (this.pageNumber !== event.page) {
      this.pageNumber = event.page;
      if (this.showRecommendedFilters) {
        this.loadRecommendedMembers();
      } else {
        this.loadLikes();
      }
    }
  }
  onSimilarityChange(newValue: number) {
    if (this.userParams) {
      this.userParams.similarity = newValue;
      this.loadRecommendedMembers();
    }
  }
  
}