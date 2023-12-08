import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { InterestsDto } from 'src/app/common/_models/interestsDto';
import { LookingForsDto } from 'src/app/common/_models/lookingForsDto';
import { Member } from 'src/app/common/_models/member';
import { User } from 'src/app/common/_models/user';
import { AdminService } from 'src/app/common/_services/admin.service';
import { MembersService } from 'src/app/common/_services/members.service';
@Component({
  selector: 'app-edit-lookingfor',
  templateUrl: './edit-lookingfor.component.html',
  styleUrls: ['./edit-lookingfor.component.css']
})
export class EditLookingforComponent implements OnInit {
  @Output() interestAdded = new EventEmitter<void>();
  lookingFors: LookingForsDto[] = [];
  searchlookingForm!: FormGroup;
  user: User | null = null;
  @Input() member: Member | undefined;
  interests: InterestsDto[] = []

  constructor(
    private memberService: MembersService,
    private toastr: ToastrService,
    private fb: FormBuilder,
    private adminService: AdminService) { }

  ngOnInit(): void {
    this.initializeSearchFormLookingFor();
  }
  initializeSearchFormLookingFor() {
    this.searchlookingForm = this.fb.group({
      name: ['']
    });
  }

  onSearchLookingFor() {
    const searchValue = this.searchlookingForm.get('name')!.value;
    if (searchValue.trim() === '') {
      this.toastr.error('Please fill in the information');
      return;
    }
    this.loadLookingFors(searchValue);
  }
  loadLookingFors(name?: string) {
    this.adminService.getLookingFors(name).subscribe(response => {
      this.lookingFors = response;
    }, _error => {
    });
  }
  addLookingFor(id: number) {
    this.memberService.addLookingFor(id).subscribe(_response => {
      this.toastr.success('Looking for added successfully');
      this.interestAdded.emit();
      const index = this.lookingFors.findIndex((lf) => lf.id === id);
      if (index !== -1) {
        this.lookingFors.splice(index, 1);
      }
    }, _error => {
    });
  }
 
}
