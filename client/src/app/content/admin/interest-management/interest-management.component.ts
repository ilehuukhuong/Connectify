import { Component, OnInit, TemplateRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { ToastrService } from 'ngx-toastr';
import { InterestsDto } from 'src/app/common/_models/interestsDto';
import { AdminService } from 'src/app/common/_services/admin.service';

@Component({
  selector: 'app-interest-management',
  templateUrl: './interest-management.component.html',
  styleUrls: ['./interest-management.component.css']
})
export class InterestManagementComponent implements OnInit {
  interests: InterestsDto[] = [];
  searchForm!: FormGroup;
  updateForm!: FormGroup;
  selectedInterest!: InterestsDto;
  bsModalRef!: BsModalRef;
  createForm!: FormGroup;

  constructor(private adminService: AdminService,
    private fb: FormBuilder,
    private toastr: ToastrService,
    private modalService: BsModalService) { }

  ngOnInit() {
    this.initializeSearchForm();
    this.initializeUpdateForm();
    this.initializeCreateForm();
  }
  
  //search
  initializeSearchForm() {
    this.searchForm = this.fb.group({
      name: ['']
    });
  }
  onSearch() {
    const searchValue = this.searchForm.get('name')!.value;
    if (searchValue.trim() === '') {
      this.toastr.error('Please fill in the information');
      return;
    }
    this.loadInterests(searchValue);
  }
  loadInterests(name?: string) {
    this.adminService.getInterest(name).subscribe(response => {
      this.interests = response;
    }, error => {
      this.toastr.error(error);
    });
  }

  //create
  initializeCreateForm() {
    this.createForm = this.fb.group({
      name: ['', Validators.required]
    });
  }
  openCreateModal(template: TemplateRef<any>) {
    this.bsModalRef = this.modalService.show(template);
  }
  createInterest() {
    const interestText = this.createForm.value.name;
    if (interestText.length > 200) {
      this.toastr.error('Your information is too long. Please limit it to 200 characters.');
      return;
    }
    const createValue = this.createForm.get('name')!.value;
    if (createValue.trim() === '') {
      this.toastr.error('Please fill in the information');
      return;
    }
    if (this.createForm.valid) {
      this.adminService.createInterest(this.createForm.value).subscribe(response => {
        this.toastr.success('Created successfully');
        this.createForm.reset(this.createForm.value);
        this.closeModal();
      }, error => {
        if (error.statusCode === 500) {
          this.toastr.error('There was an error updating your introduction. Please try again later.');
        } else {
          this.toastr.error(error);
        }
      });
    }
  }

  //update
  initializeUpdateForm() {
    this.updateForm = this.fb.group({
      name: ['']
    });
  }

  openUpdateModal(interest: InterestsDto, template: TemplateRef<any>) {
    this.selectedInterest = interest;
    this.updateForm.patchValue(interest);
    this.bsModalRef = this.modalService.show(template);
  }
  updateInterest() {
    const interestText = this.updateForm.value.name;
    if (interestText.length > 200) {
      this.toastr.error('Your information is too long. Please limit it to 200 characters.');
      return;
    }
    const updateValue = this.updateForm.get('name')!.value;
    if (updateValue.trim() === '') {
      this.toastr.error('Please fill in the information');
      return;
    }
    if (this.updateForm.valid) {
      const updatedInterest: InterestsDto = {
        ...this.selectedInterest,
        ...this.updateForm.value
      };
      this.adminService.updateInterest(updatedInterest).subscribe(response => {
        const index = this.interests.findIndex(lf => lf.id === updatedInterest.id);
        if (index !== -1) {
          this.interests[index] = updatedInterest;
        }
        this.toastr.success('Updated successfully!');
        this.closeModal();
      }, error => {
        if (error.statusCode === 500) {
          this.toastr.error('There was an error updating your introduction. Please try again later.');
        } else {
          error
        }
      });
    }
  }
  //delete
  onDelete(id: number) {
    this.adminService.deleteLookingFor(id).subscribe(response => {
      this.interests = this.interests.filter(lf => lf.id !== id);
      this.toastr.success('Deleted successfully!');
    }, error => {
      this.toastr.error(error);
    });
  }
  //close modal
  closeModal() {
    this.bsModalRef.hide();
  }
}
