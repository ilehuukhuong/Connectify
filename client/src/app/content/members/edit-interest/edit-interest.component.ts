import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { InterestsDto } from 'src/app/common/_models/interestsDto';
import { LookingForsDto } from 'src/app/common/_models/lookingForsDto';
import { Member } from 'src/app/common/_models/member';
import { User } from 'src/app/common/_models/user';
import { AdminService } from 'src/app/common/_services/admin.service';
import { MembersService } from 'src/app/common/_services/members.service';

@Component({
  selector: 'app-edit-interest',
  templateUrl: './edit-interest.component.html',
  styleUrls: ['./edit-interest.component.css']
})
export class EditInterestComponent implements OnInit {

  @Output() interestAdded = new EventEmitter<void>();
  searchinterestForm!: FormGroup;
  lookingFors: LookingForsDto[] = [];
  searchlookingForm!: FormGroup;
  user: User | null = null;
  member: Member | undefined;
  isVisible = false;
  interests: InterestsDto[] = []
  constructor(
    private memberService: MembersService,
    private toastr: ToastrService,
    private fb: FormBuilder,
    private adminService: AdminService) { }
  ngOnInit(): void {
    this.initializeSearchFormInterest()
  }
  loadMember() {
    if (!this.user) return;
    this.memberService.getMember(this.user.username).subscribe({
      next: member => {
        this.member = member
        this.isVisible = member.isVisible;
        this.lookingFors = member.lookingFors;
        this.interests = member.interests;
      }
    })
  }
  initializeSearchFormInterest() {
    this.searchinterestForm = this.fb.group({
      name: ['']
    });
  }
  onSearchInterest() {
    const searchValue = this.searchinterestForm.get('name')!.value;
    if (searchValue.trim() === '') {
      this.toastr.error('Please fill in the information');
      return;
    }
    this.loadInterest(searchValue);
  }
  loadInterest(name?: string) {
    this.adminService.getInterest(name).subscribe(response => {
      this.interests = response;
    }, error => {
    });
  }
  addInterest(id: number) {
    this.memberService.addInterest(id).subscribe(response => {
      this.toastr.success('Interest added successfully');
      this.interestAdded.emit();
      const index = this.interests.findIndex((lf) => lf.id === id);
      if (index !== -1) {
        this.interests.splice(index, 1);
      }
    }, error => {
    });
  }
  deleteInterest(id: number) {
    this.memberService.deleteInterest(id).subscribe(response => {
      this.toastr.success('Interest removed successfully');
      this.interestAdded.emit();
    }, error => {
      this.toastr.error(error);
    });
  }
}
